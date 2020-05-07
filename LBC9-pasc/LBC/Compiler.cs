using System;
using System.Collections;

namespace LBC
{
    class Compiler : Cradle
    {
        private Cpu68000 Platform;

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

        // Process label statement
        private void Labels()
        {
            Match('l');
        }

        // Process const statement
        private void Constants()
        {
            Match('c');
        }

        // Process type statement
        private void Types()
        {
            Match('t');
        }

        // Process var statement
        private void Variables()
        {
            Match('v');
        }

        // Process Procedure definition
        private void DoProcedure()
        {
            Match('p');
        }

        // Process function definition
        private void DoFunction()
        {
            Match('f');
        }

        // Parse and translate the declarations part
        private void Declarations()
        {
            while ("lctvpf".IndexOf(Look) > -1)
            {
                switch (Look)
                {
                    case 'l': Labels(); break;
                    case 'c': Constants(); break;
                    case 't': Types(); break;
                    case 'v': Variables(); break;
                    case 'p': DoProcedure(); break;
                    case 'f': DoFunction(); break;
                }
            }
        }

        // Parse and translate the statment part
        private void Statements() {
            Match('b');
            while (Look != 'e')
                GetChar();
            Match('e');
        }

        // Parse and translate a Pascal block
        private void DoBlock(char name) {
            Declarations();
            Platform.PostLabel(name);
            Statements();
        }

        // Parse and translate a program
        private void Prog()
        {
            Match('p');
            char name = GetName();
            Platform.Prolog(name);
            DoBlock(name);
            Match('.');
            Platform.Epilog(name);
        }
        
        public Compiler()
        {
            Platform = new Cpu68000();
            GetChar();
            Prog();
        }
    }
}