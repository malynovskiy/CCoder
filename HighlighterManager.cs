using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Diagnostics;
using System.Xml;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Collections;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Data;

namespace CCoder
{
    public class HighlightManager
    {
        private static HighlightManager instance = new HighlightManager();
        public static HighlightManager Instance { get { return instance; } }

        public IDictionary<string, Highlighter> Highlighters { get; private set; }

        private HighlightManager()
        {
            Highlighters = new Dictionary<string, Highlighter>();

            var resourceStream = Application.GetResourceStream(
                new Uri("pack://application:,,,/AurelienRibon.Ui.SyntaxHighlightBox;component/resources/syntax.xsd"));

            var schemaStream = resourceStream.Stream;
            XmlSchema schema = XmlSchema.Read(schemaStream, (s, e) => {
                Debug.WriteLine("Xml schema validation error : " + e.Message);
            });

            XmlReaderSettings readerSettings = new XmlReaderSettings();
            readerSettings.Schemas.Add(schema);
            readerSettings.ValidationType = ValidationType.Schema;

            foreach (var res in GetResources("Resources/Highlighters/(.+?)[.]xml"))
            {
                XDocument xmldoc = null;
                try
                {
                    XmlReader reader = XmlReader.Create(res.Value, readerSettings);
                    xmldoc = XDocument.Load(reader);
                }
                catch (XmlSchemaValidationException ex)
                {
                    Debug.WriteLine("Xml validation error at line " + ex.LineNumber + " for " + res.Key + " :");
                    Debug.WriteLine("Warning : if you cannot find the issue in the xml file, verify the xsd file.");
                    Debug.WriteLine(ex.Message);
                    return;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    return;
                }

                XElement root = xmldoc.Root;
                String name = root.Attribute("name").Value.Trim();

                Highlighters.Add(name, new Highlighter(root));
            }
        }

        /// Returns a dictionary of the assembly resources (not embedded).
        /// Uses a regex filter for the resource paths.
        private IDictionary<string, UnmanagedMemoryStream> GetResources(string filter)
        {
            var asm = Assembly.GetCallingAssembly();
            string resName = asm.GetName().Name + ".g.resources";
            Stream manifestStream = asm.GetManifestResourceStream(resName);
            ResourceReader reader = new ResourceReader(manifestStream);

            IDictionary<string, UnmanagedMemoryStream> ret = new Dictionary<string, UnmanagedMemoryStream>();
            foreach (DictionaryEntry res in reader)
            {
                string path = (string)res.Key;
                UnmanagedMemoryStream stream = (UnmanagedMemoryStream)res.Value;
                if (Regex.IsMatch(path, filter))
                    ret.Add(path, stream);
            }
            return ret;
        }

        public  class Highlighter
        {
            private List<HighlightWordRule> wordRules;
            private List<HighlightLineRule> lineRules;
            private List<HighlightRegexRule> regexRules;

            public Highlighter(XElement root)
            {
                wordRules = new List<HighlightWordRule>();
                lineRules = new List<HighlightLineRule>();
                regexRules = new List<HighlightRegexRule>();

                foreach (XElement elem in root.Elements())
                {
                    switch(elem.Name.ToString())
                    {
                        case "HighlightWordRule":
                            wordRules.Add(new HighlightWordRule(elem));
                            break;
                        case "HighlightLineRule":
                            lineRules.Add(new HighlightLineRule(elem));
                            break;
                        case "HighlightRegexRule":
                            regexRules.Add(new HighlightRegexRule(elem));
                            break;
                    }
                }
            }

            // TODO(Maksym): Revisit this later in case of performance improvements
            public int Highlight(FormattedText text)
            {
                // WORDS RULES
                Regex wordsRgx = new Regex("[a-zA-Z_][a-zA-Z0-9_]*");
                foreach (Match m in wordsRgx.Matches(text.Text))
                {
                    foreach (HighlightWordRule rule in wordRules)
                    {
                        foreach (string word in rule.Words)
                        {
                            if (rule.Options.IgnoreCase)
                            {
                                if (m.Value.Equals(word, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    text.SetForegroundBrush(rule.Options.Foreground, m.Index, m.Length);
                                    text.SetFontWeight(rule.Options.FontWeight, m.Index, m.Length);
                                    text.SetFontStyle(rule.Options.FontStyle, m.Index, m.Length);
                                }
                            }
                            else
                            {
                                if (m.Value == word)
                                {
                                    text.SetForegroundBrush(rule.Options.Foreground, m.Index, m.Length);
                                    text.SetFontWeight(rule.Options.FontWeight, m.Index, m.Length);
                                    text.SetFontStyle(rule.Options.FontStyle, m.Index, m.Length);
                                }
                            }
                        }
                    }
                }

                // REGEX RULES
                foreach (HighlightRegexRule rule in regexRules)
                {
                    Regex regexRgx = new Regex(rule.Expression);
                    foreach (Match m in regexRgx.Matches(text.Text))
                    {
                        text.SetForegroundBrush(rule.Options.Foreground, m.Index, m.Length);
                        text.SetFontWeight(rule.Options.FontWeight, m.Index, m.Length);
                        text.SetFontStyle(rule.Options.FontStyle, m.Index, m.Length);
                    }
                }

                // LINE RULES
                foreach (HighlightLineRule rule in lineRules)
                {
                    Regex lineRgx = new Regex(Regex.Escape(rule.LineStart) + ".*");
                    foreach (Match m in lineRgx.Matches(text.Text))
                    {
                        text.SetForegroundBrush(rule.Options.Foreground, m.Index, m.Length);
                        text.SetFontWeight(rule.Options.FontWeight, m.Index, m.Length);
                        text.SetFontStyle(rule.Options.FontStyle, m.Index, m.Length);
                    }
                }

                return -1;
            }
        }

        private class HighlightWordRule
        {
            public List<string> Words { get; private set; }
            public HighlightOptions Options { get; private set; }

            public HighlightWordRule(XElement rule)
            {
                // probably need handle rule == null
                Words = new List<string>();
                Options = new HighlightOptions(rule);

                string wordString = rule.Element("Words").Value;
                string[] words = Regex.Split(wordString, "\\s+");

                foreach(string word in words)
                {
                    if (!string.IsNullOrWhiteSpace(word))
                        Words.Add(word.Trim());
                }
            }
        }

        private class HighlightLineRule
        {
            public string LineStart { get; private set; }
            public HighlightOptions Options { get; private set; }

            public HighlightLineRule(XElement rule)
            {
                // probably need handle rule == null
                LineStart = rule.Element("LineStart").Value.Trim();
                Options = new HighlightOptions(rule);
            }
        }

        private class HighlightRegexRule
        {
            public string Expression { get; private set; }
            public HighlightOptions Options { get; private set; }

            public HighlightRegexRule(XElement rule)
            {
                Expression = rule.Element("Expression").Value.Trim();
                Options = new HighlightOptions(rule);
            }
        }

        private class HighlightOptions
        {
            public bool IgnoreCase { get; private set; }
            public Brush Foreground { get; private set; }
            public FontWeight FontWeight { get; private set; }
            public FontStyle FontStyle { get; private set; }

            public HighlightOptions(XElement rule)
            {
                // probably need handle rule == null
                string ignoreCaseStr = rule.Element("IgnoreCase").Value.Trim();
                string foregroundStr = rule.Element("Foreground").Value.Trim();
                string fontWeightStr = rule.Element("FontWeight").Value.Trim();
                string fontStyleStr = rule.Element("FontStyle").Value.Trim();

                IgnoreCase = bool.Parse(ignoreCaseStr);
                Foreground = (Brush)new BrushConverter().ConvertFrom(foregroundStr);
                FontWeight = (FontWeight)new FontWeightConverter().ConvertFrom(fontWeightStr);
                FontStyle = (FontStyle)new FontStyleConverter().ConvertFrom(fontStyleStr);
            }
        }
    }
}
