using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.TextFormatting;
using System.Windows.Shapes;

namespace CCoder.Controls
{
    /// <summary>
    /// Interaction logic for ModernTextBox.xaml
    /// </summary>
    public partial class ModernTextBox : TextBox
    {
        private class InnerTextBlock
        {
            public string RawText { get; set; }
            public FormattedText FormattedText { get; set; }
            public FormattedText LineNumbers { get; set; }

            public int CharStartIndex { get; private set; }
            public int CharEndIndex { get; private set; }

            public int LineStartIndex { get; set; }
            public int LineEndIndex { get; set; }

            public Point Position
            {
                get
                {
                    return new Point(0, LineStartIndex * lineHeight);
                }
            }

            public bool IsLast { get; set; }
            public int Code { get; set; }

            private double lineHeight;

            public InnerTextBlock(int charStart, int charEnd, int lineStart, int lineEnd, double lineHeight)
            {
                CharStartIndex = charStart;
                CharEndIndex = charEnd;
                LineStartIndex = lineStart;
                LineEndIndex = lineEnd;
                this.lineHeight = lineHeight;
                IsLast = false;
            }

            public string GetSubString(string str)
            {
                return str.Substring(CharStartIndex, CharEndIndex - CharStartIndex + 1);
            }

            /* not sure why we need this
            public override string ToString()
            {
                return string.Format("L:{0}/{1} C:{2}/{3} {4}",
                    LineStartIndex,
                    LineEndIndex,
                    CharStartIndex,
                    CharEndIndex,
                    FormattedText.Text);
            }*/
        }

        public ModernTextBox()
        {
            InitializeComponent();

            maxLineCountInBlock = 100;
            lineHeight = FontSize * 1.3;
            totalLinesCount = 1;

            textBlocks = new List<InnerTextBlock>();

            Loaded += (s, e) =>
            {
                renderCanvas = (DrawingElement)Template.FindName("PART_CodeRenderCanvas", this);
                lineNumbersCanvas = (DrawingElement)Template.FindName("PART_0", this);
                scrollViewer = (ScrollViewer)Template.FindName("PART_", this);

                lineNumbersCanvas.Width = GetFormattedTextWidth(string.Format("{0:0000}", totalLinesCount)) + 5;

                scrollViewer.ScrollChanged += onScrollChanged;
            };
        }

        private void onScrollChanged(object sender, ScrollChangedEventArgs eventArgs)
        {
            if (eventArgs.VerticalChange != 0)
                UpdateTextBlocks();
            InvalidateVisual();
        }

        private void UpdateTextBlocks()
        {
            if (textBlocks.Count == 0)
                return;

            // While something is visible after last block...
            while (!textBlocks.Last().IsLast && 
                textBlocks.Last().Position.Y + blockHeight - VerticalOffset < ActualHeight)
            {
                int firstLineIndex = textBlocks.Last().LineEndIndex + 1;
                int lastLineIndex = firstLineIndex + maxLineCountInBlock - 1;
                lastLineIndex = lastLineIndex <= totalLinesCount - 1 ? lastLineIndex : totalLinesCount - 1;

                int firstCharIndex = textBlocks.Last().CharEndIndex + 1;
                int lastCharIndex = Utilities.GetLastCharIndexFromLineIndex(Text, lastLineIndex);

                if(lastCharIndex <= firstCharIndex)
                {
                    textBlocks.Last().IsLast = true;
                    return;
                }

                InnerTextBlock block = new InnerTextBlock(
                    firstCharIndex,
                    lastCharIndex,
                    textBlocks.Last().LineEndIndex + 1,
                    lastLineIndex,
                    lineHeight);
                block.RawText = block.GetSubString(Text);
                block.LineNumbers = GetFormattedLineNumbers(block.LineStartIndex, block.LineEndIndex);
                textBlocks.Add(block);
                FormatTextBlock(block, textBlocks.Count > 1 ? textBlocks[textBlocks.Count - 2] : null);
            }   
        }

        private void FormatTextBlock(InnerTextBlock currentBlock, InnerTextBlock previousBlock)
        {
            currentBlock.FormattedText = GetFormattedText(currentBlock.RawText);
            if (CurrentHighlighter != null)
            {
                ThreadPool.QueueUserWorkItem(p =>
                {
                    int previousCode = previousBlock != null ? previousBlock.Code : -1;
                    //currentBlock.Code = CurrentHighlighter.Highlight(currentBlock.FormattedText, previousCode);
                });
            }
        }

        private FormattedText GetFormattedText(string text)
        {
            FormattedText ft = new FormattedText(
                text,
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
                FontSize,
                Brushes.Black);

            ft.Trimming = TextTrimming.None;
            ft.LineHeight = lineHeight;

            return ft;
        }

        private FormattedText GetFormattedLineNumbers(int firstIndex, int lastIndex)
        {
            string text = "";
            for (int i = firstIndex + 1; i <= lastIndex + 1; ++i)
                text += i.ToString() + "\n";
            text = text.Trim();

            FormattedText formattedText = new FormattedText(
                text,
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
                FontSize,
                new SolidColorBrush(Color.FromRgb(0x21, 0xA1, 0xD8)));
            formattedText.Trimming = TextTrimming.None;
            formattedText.LineHeight = lineHeight;
            formattedText.TextAlignment = TextAlignment.Right;

            return formattedText;
        }

        private double GetFormattedTextWidth(string text)
        {
            FormattedText formatedText = new FormattedText(
                text,
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
                FontSize,
                Brushes.Black);

            formatedText.Trimming = TextTrimming.None;
            formatedText.LineHeight = lineHeight;

            return formatedText.Width;
        }

        private DrawingElement renderCanvas;
        private DrawingElement lineNumbersCanvas;

        private ScrollViewer scrollViewer;

        private double lineHeight;
        private int totalLinesCount;
        private double blockHeight;
        private int maxLineCountInBlock;

        private List<InnerTextBlock> textBlocks;
    }
}
