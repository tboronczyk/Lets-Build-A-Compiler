using System;

namespace LBC
{
    static class ErrorHandler
    {
        private const char BEL = '\a';

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

        // Report an undefined indentifier
        public static void Undefined(string s)
        {
            ErrorHandler.Abort("Undefined Identifier " + s);
        }
    }
}
