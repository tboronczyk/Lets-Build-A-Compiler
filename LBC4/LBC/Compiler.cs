using System;

namespace LBC
{
    class Compiler
    {
        // Variable storage
        private int[] Table;

        // Parse and translate a math factor
        private int Factor()
        {
            if (Cradle.Look == '(')
            {
                Cradle.Match('(');
                int value = Expression();
                Cradle.Match(')');
                return value;
            }
            else if (Cradle.IsAlpha(Cradle.Look))
            {
                return Table[Cradle.GetName() - 'A'];
            }
            else 
                return Cradle.GetNum();
        }

        // Parse and translate a math term
        private int Term()
        {
            int Value = Factor();
            while ("*/".IndexOf(Cradle.Look) > -1)
            {
                switch (Cradle.Look)
                {
                    case '*':
                        Cradle.Match('*');
                        Value *= Factor();
                        break;
                    case '/': 
                        Cradle.Match('/');
                        Value /= Factor();
                        break;
                }
            }
            return Value;
        }

        // Parse and translate an expression
        private int Expression()
        {
            int Value = (Cradle.IsAddop(Cradle.Look)) ? 0 : Term();
            while (Cradle.IsAddop(Cradle.Look))
            {
                switch (Cradle.Look)
                {
                    case '+':
                        Cradle.Match('+');
                        Value += Term();
                        break;
                    case '-':
                        Cradle.Match('-');
                        Value -= Term();
                        break;
                }
            }
            return Value;
        }

        // Parse and translate an assignment statement
        private void Assignment()
        {
            char Name = Cradle.GetName();
            Cradle.Match('=');
            Table[Name - 'A'] = Expression();
        }

        // Input routine
        private void Input()
        {
            Cradle.Match('?');
            Assignment();
        }

        // Output routine
        private void Output()
        {
            Cradle.Match('!');
            Console.WriteLine(Table[Cradle.GetName() - 'A']);
        }

        public Compiler()
        {
            Table = new int[26];
            for (char c = 'A'; c <= 'Z'; ++c)
                Table[c - 'A'] = 0;
            do
            {
                switch (Cradle.Look)
                {
                    case '?': Input(); break;
                    case '!': Output(); break;
                    default: Assignment(); break;
                }
                Cradle.NewLine();
            } while (Cradle.Look != '.');

        }
    }
}
