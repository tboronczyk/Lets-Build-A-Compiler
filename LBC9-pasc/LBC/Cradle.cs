using System;

namespace LBC
{
    class Cradle
    {
        protected const char TAB = '\t';
        protected const char CR = '\r';
        protected const char LF = '\n';
        protected const char BEL = '\a';

        protected char Look;    // Look ahead character

        // Read new character from input stream 
        protected void GetChar()
        {
            Look = Console.ReadKey().KeyChar;
        }

        // Report an error
        protected void Error(string s)
        {
            Console.WriteLine(BEL + "Error: " + s);
        }

        // Report error and halt
        protected void Abort(string s)
        {
            Error(s);
            Environment.Exit(-1);
        }

        // Report what was expected
        protected void Expected(string s)
        {
            Abort(s + " Expected");
        }

        // Recognize an alpha character
        protected bool IsAlpha(char c)
        {
            // return "ABCDEFGHIJKLMNOPQRSTUVWXYZ".IndexOf(Char.ToUpper(c)) > -1;
            return char.IsLetter(c);
        }

        // Recognize an addop
        protected bool IsDigit(char c)
        {
            // return "0123456789".IndexOf(c) > -1;
            return char.IsDigit(c);
        }

        // Recognize an alphanumeric
        protected bool IsAlNum(char c)
        {
            // return IsAlpha(c) || IsDigit(c);
            return char.IsLetterOrDigit(c);
        }

        // Recognize an addop
        protected bool IsAddop(char c)
        {
            return "+-".IndexOf(c) > -1;
        }

        // Recognize a mulop
        protected bool IsMulop(char c)
        {
            return "*/".IndexOf(c) > -1;
        }

        // Recognize white space
        protected bool IsWhite(char c)
        {
            return (" " + TAB).IndexOf(c) > -1;
        }

        // Skip over leading white space
        protected void SkipWhite()
        {
            while (IsWhite(Look))
                GetChar();
        }

        // Match a specific input character
        protected void Match(char c)
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
        protected void NewLine()
        {
            if (Look == CR)
            {
                GetChar();
                if (Look == LF)
                    GetChar();
            }
        }

        // Skip a CRLF
        protected void Fin()
        {
            if (Look == CR) GetChar();
            if (Look == LF) GetChar();
        }

        // Output a string with tab
        protected void Emit(string s)
        {
            Console.Write(TAB + s);
        }

        // Output a string with tab and newline
        protected void EmitLn(string s)
        {
            Console.WriteLine(TAB + s);
        }
    }
}