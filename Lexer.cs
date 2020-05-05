using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace CCoder
{
    class Lexer
    {
        public enum Token
        {
            token_eof = -1,

            token_def = -2,
            token_extern = -3,
            
            token_identifier = -4,
            token_number = -5
        };

        public static String mIdentifierString;
        public static double mNumericValue;

		private static Char currentChar;

		Lexer(){}

        public static int getToken()
        {
			/*currentChar = ' ';

			// skip all spaces
			while (Char.IsWhiteSpace(currentChar))
			{
				std::cout << currentChar;
				currentChar = getChar();
			}
			// processing words that starts with alphabetical symbol
			if (isalpha(currentChar))
			{
				identifier_string = currentChar;
				while (isalnum(currentChar = getchar()))
					identifier_string += currentChar;


				if (identifier_string == "def")
				{
					SetConsoleTextAttribute(hConsole, Color::color_def);
					std::cout << identifier_string;
					return tok_def;
				}
				if (identifier_string == "extern")
				{
					SetConsoleTextAttribute(hConsole, Color::color_extern);
					std::cout << identifier_string;
					return tok_extern;
				}

				SetConsoleTextAttribute(hConsole, Color::color_identifier);
				std::cout << identifier_string;
				return tok_identifier;
			}

			// processing numbers
			if (isdigit(currentChar) || currentChar == '.')
			{
				std::string numberString;
				do
				{
					numberString += currentChar;
					currentChar = getchar();
				} while (isdigit(currentChar) || currentChar == '.');

				SetConsoleTextAttribute(hConsole, Color::color_number);
				std::cout << numberString;

				numeric_value = strtod(numberString.c_str(), 0);
				return tok_number;
			}

			// processing comments
			if (currentChar == '#')
			{
				std::string commentString;
				do
				{
					commentString += currentChar;
					currentChar = getchar();
				} while (currentChar != EOF && currentChar != '\n' currentChar != '\r');

				SetConsoleTextAttribute(hConsole, Color::color_comment);
				std::cout << commentString;

				if (currentChar != EOF)
					return getToken();
			}

			// end of the file
			if (currentChar == EOF)
				return tok_eof;

			int thisChar = currentChar;
			currentChar = getchar();

			SetConsoleTextAttribute(hConsole, Color::color_default);
			std::cout << currentChar;

			return thisChar;*/
			return 0;
		}
    }
}
