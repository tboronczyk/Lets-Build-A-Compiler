using System;
using System.Collections;

namespace LBC
{
    class Compiler : Cradle
    {
        private Cpu68000 Platform;

        private char Class;
        private char Typ;
        private char Sign;

        // Get an identifier
        private char GetName()
        {
            if (!IsAlpha(Look)) Expected("Name");
            char token = char.ToUpper(Look);
            GetChar();
            return token;
        }

        // Get a number
        private int GetNum()
        {
            int value = 0;
            if (!IsDigit(Look)) Expected("Integer");
            while (IsDigit(Look))
            {
                value = value * 10 + Look - '0';
                GetChar();
            }
            return value;
        }

        // Get a storage class specifier
        private void GetClass()
        {
            if ("axs".IndexOf(Look) > -1)
            {
                Class = Look;
                GetChar();
            }
            else
                Class = 'a';
        }

        // Get a type specifier
        private void GetTyp()
        {
            Typ = ' ';
            if (Look == 'u')
            {
                Sign = 'u';
                Typ = 'i';
                GetChar();
            }
            else
            {
                Sign = 's';
                if ("ilc".IndexOf(Look) > -1)
                {
                    Typ = Look;
                    GetChar();
                }
            }
        }

        // Process a function definition
        private void DoFunc(char n)
        {
            Match('(');
            Match(')');
            Match('{');
            Match('}');
            if (Typ == ' ') Typ = 'i';
            EmitLn(Class + " " + Sign + " " + Typ + " function " + n);
        }

        // Process a data declaration
        private void DoData(char n)
        {
            if (Typ == ' ') Expected("Type declaration");
            EmitLn(Class + " " + Sign + " " + Typ + " data " + n);
            while (Look == ',')
            {
                Match(',');
                n = GetName();
                EmitLn(Class + " " + Sign + " " + Typ + " data " + n);
            }
            Match(';');
        }

        // Process a top-level declaration
        private void TopDecl()
        {
            char name = GetName();
            if (Look == '(')
                DoFunc(name);
            else
                DoData(name);
        }

        // Parse and translate a program
        private void Prog()
        {
            while (Look != EOF)
            {
                GetClass();
                GetTyp();
                TopDecl();
            }
        }

        public Compiler()
        {
            Platform = new Cpu68000();
            GetChar();
            Prog();
        }
    }
}