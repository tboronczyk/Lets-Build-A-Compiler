namespace LBC
{
    class Compiler
    {
        // Parse and translate a math factor
        private void Factor()
        {
            if (Cradle.Look == '(')
            {
                Cradle.Match('(');
                Expression();
                Cradle.Match(')');
            }
            else
                Cradle.EmitLn("MOVE #" + Cradle.GetNum() + ",D0");
        }

        // Recognize and translate a multiply
        private void Multiply()
        {
            Cradle.Match('*');
            Factor();
            Cradle.EmitLn("MULS (SP)+,D0");
        }

        // Recognize and translate a divide
        private void Divide()
        {
            Cradle.Match('/');
            Factor();
            Cradle.EmitLn("MOVE(SP)+,D1");
            Cradle.EmitLn("DIVS D1,D0");
        }

        // Parse and translate a math term
        private void Term()
        {
            Factor();
            while ("*/".IndexOf(Cradle.Look) > -1)
            {
                Cradle.EmitLn("MOVE D0,-(SP)");
                switch (Cradle.Look)
                {
                    case '*': Multiply(); break;
                    case '/': Divide(); break;
                    default: Cradle.Expected("Mulop"); break;
                }
            }
        }

        // Recognize and translate an add
        private void Add()
        {
            Cradle.Match('+');
            Term();
            Cradle.EmitLn("ADD (SP)+,D0");
        }

        // Recognize and translate a subtract
        private void Subtract()
        {
            Cradle.Match('-');
            Term();
            Cradle.EmitLn("SUB (SP)+,D0");
            Cradle.EmitLn("NEG D0");
        }

        // Parse and translate an expression
        private void Expression()
        {
            if (Cradle.IsAddop(Cradle.Look))
                Cradle.EmitLn("CLR D0");
            else
                Term();

            while (Cradle.IsAddop(Cradle.Look))
            {
                Cradle.EmitLn("MOVE D0,-(SP)");
                switch (Cradle.Look)
                {
                    case '+': Add(); break;
                    case '-': Subtract(); break;
                    default: Cradle.Expected("Addop"); break;
                }
            }
        }

        public Compiler()
        {
            Expression();
        }
    }
}
