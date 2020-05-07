using System;

namespace LBC
{
    public struct Cradle
    {
        public const char TAB = '\t';
        public const char CR = '\r';
        public const char LF = '\n';
        public const char BEL = '\a';

        public static char Look;    // Look ahead character

        // Read new character from input stream 
        public static void GetChar()
        {
            Look = Console.ReadKey().KeyChar;
        }

        // Report an error
        public static void Error(string s)
        {
            Console.WriteLine(BEL + "Error: " + s);
        }

        // Report error and halt
        public static void Abort(string s)
        {
            Error(s);
            Environment.Exit(-1);
        }

        // Report what was expected
        public static void Expected(string s)
        {
            Abort(s + " Expected");
        }

        // Match a specific input character
        public static void Match(char c)
        {
            if (Look == c)
            {
                GetChar();
            }
            else
            {
                Expected("'" + c + "'");
            }
        }

        // Recognize and skip over a new line
        public static void NewLine()
        {
            if (Look == CR)
            {
                GetChar();
                if (Look == LF)
                    GetChar();
            }
        }

        // Skip a CRLF
        public static void Fin()
        {
            if (Look == CR) GetChar();
            if (Look == LF) GetChar();
        }

        // Recognize an alpha character
        public static bool IsAlpha(char c)
        {
            // return "ABCDEFGHIJKLMNOPQRSTUVWXYZ".IndexOf(Char.ToUpper(c)) > -1;
            return char.IsLetter(c);
        }

        // Recognize an alphanumeric
        public static bool IsAlNum(char c)
        {
            // return IsAlpha(c) || IsDigit(c);
            return char.IsLetterOrDigit(c);
        }

        // Recognize white space
        public static bool IsWhite(char c)
        {
            return (" " + TAB).IndexOf(c) > -1;
        }

        // Skip over leading white space
        public static void SkipWhite()
        {
            while (IsWhite(Look))
                GetChar();
        }

        // Recognize an addop
        public static bool IsDigit(char c)
        {
            // return "0123456789".IndexOf(c) > -1;
            return char.IsDigit(c);
        }

        // Recognize an addop
        public static bool IsAddop(char c)
        {
            return "+-".IndexOf(c) > -1;
        }

        // Recognize a Boolean literal      
        public static bool IsBoolean(char c)
        {
            return "TF".IndexOf(char.ToUpper(c)) > -1;
        }

        // Get a Boolean literal
        public static bool GetBoolean()
        {
            if (!IsBoolean(Look)) Expected("Boolean Literal");
            bool value = char.ToUpper(Look) == 'T';
            GetChar();
            return value;
        }

        // Output a string with tab
        public static void Emit(string s)
        {
            Console.Write(TAB + s);
        }

        // Output a string with tab and newline
        public static void EmitLn(string s)
        {
            Console.WriteLine(TAB + s);
        }

        // Initialize cradle
        static Cradle()
        {
            GetChar();
        }
    }
}