using System;

namespace LBC
{
    class Tokenizer
    {
        public const char TAB = '\t';
        public const char CR = '\r';
        public const char LF = '\n';
        public const char EOF = (char)0x1A;

        // Look ahead character
        private char look;
        public char Look { get { return look; } }

        public char Token;        // Current token
        public string Value;      // Value of current token

        // Definition of keyword tokens
        private readonly string[] KWList = { "IF", "ELSE", "ENDIF", "WHILE", "ENDWHILE", "READ", "WRITE", "VAR", "BEGIN", "END", "PROGRAM" };
        private const string KWCode = "xileweRWvbep";

        // Read new character from input stream 
        public void GetChar()
        {
            look = Console.ReadKey().KeyChar;
        }

        // Recognize an alpha character
        public bool IsAlpha(char c)
        {
            // return "ABCDEFGHIJKLMNOPQRSTUVWXYZ".IndexOf(Char.ToUpper(c)) > -1;
            return char.IsLetter(c);
        }

        // Recognize an addop
        public bool IsDigit(char c)
        {
            // return "0123456789".IndexOf(c) > -1;
            return char.IsDigit(c);
        }

        // Recognize an alphanumeric
        public bool IsAlNum(char c)
        {
            // return IsAlpha(c) || IsDigit(c);
            return char.IsLetterOrDigit(c);
        }

        // Recognize an addop
        public bool IsAddop(char c)
        {
            return "+-".IndexOf(c) > -1;
        }

        // Recognize a mulop
        public bool IsMulop(char c)
        {
            return "*/".IndexOf(c) > -1;
        }

        // Recognize a Boolean orop
        public bool IsOrop(char c)
        {
            return "|~".IndexOf(c) > -1;
        }

        // Recognize a relop
        public bool IsRelop(char c)
        {
            return "=#<>".IndexOf(c) > -1;
        }

        // Recognize white space
        public bool IsWhite(char c)
        {
            return (" " + TAB).IndexOf(c) > -1;
        }

        // Skip over leading white space
        private void SkipWhite()
        {
            while (IsWhite(look))
                GetChar();
        }

        // Recognize and skip over a new line
        public void NewLine()
        {
            while (look == CR)
            {
                GetChar();
                if (look == LF) GetChar();
                SkipWhite();
            }
        }

        // Match a specific input character
        public void Match(char c)
        {
            NewLine();
            if (look == c)
            {
                GetChar();
            }
            else
            {
                ErrorHandler.Expected("'" + c + "'");
            }
            SkipWhite();
        }

        // Table lookup
        // Return entry index of given string, else -1 if not matched
        public int Lookup(string[] table, string s)
        {
            int index = 0;
            foreach (string entry in table)
            {
                if (entry == s) return index;
                ++index;
            }
            return -1;
        }

        // Get an identifier
        public void GetName()
        {
            NewLine();
            if (!IsAlpha(look)) ErrorHandler.Expected("Name");
            Value = "";
            while (IsAlNum(look))
            {
                Value += char.ToUpper(look);
                GetChar();
            }
            SkipWhite();
        }

        // Get a number
        public int GetNum()
        {
            int value = 0;
            NewLine();
            if (!IsDigit(look)) ErrorHandler.Expected("Integer");
            while (IsDigit(look))
            {
                value = value * 10 + look - '0';
                GetChar();
            }
            return value;
        }

        // Get an identifier and scan it for keywords
        public void Scan()
        {
            GetName();
            Token = KWCode[Lookup(KWList, Value) + 1];
        }

        // Match a specific input string
        public void MatchString(string s)
        {
            if (Value != s) ErrorHandler.Expected("\"" + s + "\"");
        }

        public Tokenizer()
        {
            GetChar();
            Scan();
        }


    }

}