using System;

namespace LBC
{
    public struct Cradle
    {
        public const char TAB = '\t';
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

        // Recognize an alpha character
        public static bool IsAlpha(char c)
        {
            // return "ABCDEFGHIJKLMNOPQRSTUVWXYZ".IndexOf(Char.ToUpper(c)) > -1;
            return char.IsLetter(c);
        }

        // Recognize a decimal character
        public static bool IsDigit(char c)
        {
            // return "0123456789".IndexOf(c) > -1;
            return char.IsDigit(c);
        }

        // Get an identifier
        public static char GetName()
        {
            if (!IsAlpha(Look)) Expected("Name");
            char token = char.ToUpper(Look);
            GetChar();
            return token;
        }

        // Get a number
        public static char GetNum()
        {
            if (!IsDigit(Look)) Expected("Integer");
            char value = Look;
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