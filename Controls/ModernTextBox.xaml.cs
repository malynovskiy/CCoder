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
using System.ComponentModel;

namespace CCoder.Controls
{
    /// <summary>
    /// Interaction logic for ModernTextBox.xaml
    /// </summary>
    public partial class ModernTextBox : TextBox
    {
        public HighlightingManager.Highlighter Highlighter { get; set; }

        private DrawingElement renderCanvas;
        private DrawingElement lineNumbersCanvas;

        private ScrollViewer scrollViewer;

        private double lineHeight;
        private int totalLinesCount;
        private double blockHeight;
        private int maxLineCountInBlock;

        private List<InnerTextBlock> textBlocks;
        private int blocksRunningCount;

        public double LineHeight
        {
            get { return lineHeight; }
            set
            {
                if(value != lineHeight)
                {
                    lineHeight = value;
                    blockHeight = maxLineCountInBlock * value;
                    TextBlock.SetLineStackingStrategy(this, LineStackingStrategy.BlockLineHeight);
                    TextBlock.SetLineHeight(this, lineHeight);
                }
            }
        }

        public int MaxLineCountInBlock  
        {
            get { return maxLineCountInBlock; }
            set
            {
                maxLineCountInBlock = value > 0 ? value : 0;
                blockHeight = value * LineHeight;
            }
        }

        public static readonly DependencyProperty IsLineNumbersMarginVisibleProperty = DependencyProperty.Register(
            "IsLineNumbersMarginVisible", typeof(bool), typeof(ModernTextBox), new PropertyMetadata(true));

        public bool IsLineNumbersMarginVisible
        {
            get { return (bool)GetValue(IsLineNumbersMarginVisibleProperty); }
            set { SetValue(IsLineNumbersMarginVisibleProperty, value); }
        }

        public static readonly DependencyProperty TextFromFileProperty = DependencyProperty.Register(
            "TextFromFile", typeof(string), typeof(ModernTextBox), new PropertyMetadata(string.Empty));

        public string TextFromFile
        {
            get { return (string)GetValue(TextFromFileProperty); }
            set 
            { 
                SetValue(TextFromFileProperty, value);
                Text = value;
            }
        }

        public ModernTextBox()
        {
            InitializeComponent();

            MaxLineCountInBlock = 100;
            LineHeight = FontSize * 1.3;
            totalLinesCount = 1;

            blocksRunningCount = 0;

            textBlocks = new List<InnerTextBlock>();

            Loaded += (s, e) =>
            {
                renderCanvas = (DrawingElement)Template.FindName("PART_CodeRenderCanvas", this);
                lineNumbersCanvas = (DrawingElement)Template.FindName("PART_LineNumbersCanvas", this);
                scrollViewer = (ScrollViewer)Template.FindName("PART_ContentHost", this);

                lineNumbersCanvas.Width = GetFormattedTextWidth(string.Format("{0:0000}", totalLinesCount)) + 5;

                scrollViewer.ScrollChanged += onScrollChanged;

                InvalidateBlocks(0);
                InvalidateVisual();
            };

            SizeChanged += (s, e) => {
                if (e.HeightChanged == false)
                    return;
                UpdateTextBlocks();
                InvalidateVisual();
            };

            TextChanged += (s, e) => {
                UpdateTotalLineCount();
                InvalidateBlocks(e.Changes.First().Offset);
                InvalidateVisual();
            };
        }

        private void onScrollChanged(object sender, ScrollChangedEventArgs eventArgs)
        {
            if (eventArgs.VerticalChange != 0)
                UpdateTextBlocks();
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            while (blocksRunningCount > 0)
                Console.WriteLine("Waiting for other threads!\n");

            DrawTextBlocks();
            base.OnRender(drawingContext);
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

        // Formats and Highlights the text of a block.
        private void FormatTextBlock(InnerTextBlock currentBlock, InnerTextBlock previousBlock)
        {
            currentBlock.FormattedText = GetFormattedText(currentBlock.RawText);
            if (Highlighter != null)
            {
                ThreadPool.QueueUserWorkItem(p =>
                {
                    blocksRunningCount++;
                    Highlighter.Highlight(currentBlock.FormattedText);
                    blocksRunningCount--;
                });
            }
        }

        private void InvalidateBlocks(int changeOffset)
        {
            InnerTextBlock blockChanged = null;
            for (int i = 0; i < textBlocks.Count; i++)
            {
                if (textBlocks[i].CharStartIndex <= changeOffset && changeOffset <= textBlocks[i].CharEndIndex + 1)
                {
                    blockChanged = textBlocks[i];
                    break;
                }
            }

            if (blockChanged == null && changeOffset > 0)
                blockChanged = textBlocks.Last();

            int fvline = blockChanged != null ? blockChanged.LineStartIndex : 0;
            int lvline = GetIndexOfLastVisibleLine();
            int fvchar = blockChanged != null ? blockChanged.CharStartIndex : 0;
            int lvchar = Utilities.GetLastCharIndexFromLineIndex(Text, lvline);

            if (blockChanged != null)
                textBlocks.RemoveRange(textBlocks.IndexOf(blockChanged), textBlocks.Count - textBlocks.IndexOf(blockChanged));

            int localLineCount = 1;
            int charStart = fvchar;
            int lineStart = fvline;
            for (int i = fvchar; i < Text.Length; i++)
            {
                if (Text[i] == '\n')
                {
                    localLineCount += 1;
                }
                if (i == Text.Length - 1)
                {
                    string blockText = Text.Substring(charStart);
                    InnerTextBlock block = new InnerTextBlock(
                        charStart,
                        i, lineStart,
                        lineStart + Utilities.GetLineCount(blockText) - 1,
                        LineHeight);
                    block.RawText = block.GetSubString(Text);
                    block.LineNumbers = GetFormattedLineNumbers(block.LineStartIndex, block.LineEndIndex);
                    block.IsLast = true;

                    foreach (InnerTextBlock b in textBlocks)
                        if (b.LineStartIndex == block.LineStartIndex)
                            throw new Exception();

                    textBlocks.Add(block);
                    FormatTextBlock(block, textBlocks.Count > 1 ? textBlocks[textBlocks.Count - 2] : null);
                    break;
                }
                if (localLineCount > maxLineCountInBlock)
                {
                    InnerTextBlock block = new InnerTextBlock(
                        charStart,
                        i,
                        lineStart,
                        lineStart + maxLineCountInBlock - 1,
                        LineHeight);
                    block.RawText = block.GetSubString(Text);
                    block.LineNumbers = GetFormattedLineNumbers(block.LineStartIndex, block.LineEndIndex);

                    foreach (InnerTextBlock b in textBlocks)
                        if (b.LineStartIndex == block.LineStartIndex)
                            throw new Exception();

                    textBlocks.Add(block);
                    FormatTextBlock(block, textBlocks.Count > 1 ? textBlocks[textBlocks.Count - 2] : null);

                    charStart = i + 1;
                    lineStart += maxLineCountInBlock;
                    localLineCount = 1;

                    if (i > lvchar)
                        break;
                }
            }
        }

        private void DrawTextBlocks()
        {
            if (!IsLoaded || renderCanvas == null || lineNumbersCanvas == null)
                return;

            var dc = renderCanvas.GetContext();
            var dc2 = lineNumbersCanvas.GetContext();
            for (int i = 0; i < textBlocks.Count; i++)
            {
                InnerTextBlock block = textBlocks[i];
                Point blockPos = block.Position;
                double top = blockPos.Y - VerticalOffset;
                double bottom = top + blockHeight;
                if (top < ActualHeight && bottom > 0)
                {
                    try
                    {
                        dc.DrawText(block.FormattedText, new Point(2 - HorizontalOffset, block.Position.Y - VerticalOffset));
                        if (IsLineNumbersMarginVisible)
                        {
                            lineNumbersCanvas.Width = GetFormattedTextWidth(string.Format("{0:0000}", totalLinesCount)) + 5;
                            dc2.DrawText(block.LineNumbers, new Point(lineNumbersCanvas.ActualWidth, 1 + block.Position.Y - VerticalOffset));
                        }
                    }
                    catch
                    {
                        // Don't know why this exception is raised sometimes.
                        // Reproduce steps:
                        // - Sets a valid syntax highlighter on the box.
                        // - Copy a large chunk of code in the clipboard.
                        // - Paste it using ctrl+v and keep these buttons pressed.
                    }
                }
            }
            dc.Close();
            dc2.Close();
        }

        private void UpdateTotalLineCount()
        {
            totalLinesCount = Utilities.GetLineCount(Text);
        }

        // Returns a formatted text object from the given string
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

        // Returns a string containing a list of numbers separated with newlines.
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

        // Returns the width of a text once formatted.
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

        // Returns the index of the last visible text line.
		public int GetIndexOfLastVisibleLine()
        {
            double height = VerticalOffset + ViewportHeight;
            int guessedLine = (int)(height / lineHeight);
            return guessedLine > totalLinesCount - 1 ? totalLinesCount - 1 : guessedLine;
        }

        private class InnerTextBlock
        {
            public string RawText { get; set; }
            public FormattedText FormattedText { get; set; }
            public FormattedText LineNumbers { get; set; }
            public int CharStartIndex { get; private set; }
            public int CharEndIndex { get; private set; }
            public int LineStartIndex { get; private set; }
            public int LineEndIndex { get; private set; }
            public Point Position { get { return new Point(0, LineStartIndex * lineHeight); } }
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

            public string GetSubString(string text)
            {
                return text.Substring(CharStartIndex, CharEndIndex - CharStartIndex + 1);
            }

            public override string ToString()
            {
                return string.Format("L:{0}/{1} C:{2}/{3} {4}",
                    LineStartIndex,
                    LineEndIndex,
                    CharStartIndex,
                    CharEndIndex,
                    FormattedText.Text);
            }
        }
    }
}
