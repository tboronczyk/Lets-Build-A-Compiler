using System;

namespace LBC
{
    class Tokenizer
    {
        // Character constants
        public const char TAB = '\t';
        public const char CR = '\r';
        public const char LF = '\n';
        public const char EOF = (char)0x1A;

        // Look ahead character
        private char look;
        public char Look { get { return look; } }

        // Current token
        private char token;
        public char Token { get { return token; } }

        // Value of current token
        private string value;
        public string Value {get {return value; } }
        
        // Definition of keyword tokens
        private readonly string[] KWList = { "IF", "ELSE", "ENDIF", "WHILE", "ENDWHILE", "READ", "WRITE", "VAR", "END" };
        private const string KWCode = "xileweRWve";

        // Skip a comment field
        public void SkipComment()
        {
            while (look != '}')
            {
                GetChar();
                if (look == '{') SkipComment();
            }
            GetChar();
        }

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
            // return (" {" + TAB + CR + LF).IndexOf(c) > -1;
            return char.IsWhiteSpace(c);
        }

        // Skip over leading white space
        private void SkipWhite()
        {
            while (IsWhite(look))
            {
                if (look == '{')
                    SkipComment();
                else
                    GetChar();
            }
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
            SkipWhite();
            if (!IsAlpha(look)) ErrorHandler.Expected("Name");
            token = 'x';
            value = "";
            do
            {
                value += char.ToUpper(look);
                GetChar();
            }
            while (IsAlNum(look));
        }

        // Get a number
        public void GetNum()
        {
            SkipWhite();
            if (!IsDigit(look)) ErrorHandler.Expected("Integer");
            token = '#';
            value = "";
            do
            {
                value += look;
                GetChar();
            }
            while (IsDigit(look));
        }

        // Get an operator
        public void GetOp()
        {
            SkipWhite();
            token = look;
            value = look.ToString();
            GetChar();
        }

        // Get the next input token
        public void Next()
        {
            SkipWhite();
            if (IsAlpha(look)) GetName();
            else if (IsDigit(look)) GetNum();
            else GetOp();
        }

        // Get an identifier and scan it for keywords
        public void Scan()
        {
            if (token == 'x')
                token = KWCode[Lookup(KWList, value) + 1];
        }

        // Match a specific input string
        public void MatchString(string s)
        {
            if (value != s) ErrorHandler.Expected("\"" + s + "\"");
            Next();
        }

        public Tokenizer()
        {
            GetChar();
            Next();
        }


    }

}