using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCoder
{
    public class Utilities
    {
		public static int GetLineCount(String text)
		{
			int count = 1;
			for (int i = 0; i < text.Length; ++i)
			{
				if (text[i] == '\n')
					count += 1;
			}
			return count;
		}

		public static int GetFirstCharIndexFromLineIndex(string text, int lineIndex)
		{
			if (text == null)
				throw new ArgumentNullException("text");
			if (lineIndex <= 0)
				return 0;

			int currentLineIndex = 0;
			for (int i = 0; i < text.Length - 1; ++i)
			{
				if (text[i] == '\n')
				{
					currentLineIndex += 1;
					if (currentLineIndex == lineIndex)
						return Math.Min(i + 1, text.Length - 1);
				}
			}

			return Math.Max(text.Length - 1, 0);
		}

		public static int GetLastCharIndexFromLineIndex(string text, int lineIndex)
		{
			if (text == null)
				throw new ArgumentNullException("text");
			if (lineIndex < 0)
				return 0;

			int currentLineIndex = 0;
			for (int i = 0; i < text.Length - 1; i++)
			{
				if (text[i] == '\n')
				{
					if (currentLineIndex == lineIndex)
						return i;
					currentLineIndex += 1;
				}
			}

			return Math.Max(text.Length - 1, 0);
		}
	}
}
