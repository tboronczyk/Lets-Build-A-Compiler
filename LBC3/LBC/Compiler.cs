namespace LBC
{
    class Compiler
    {
        // Parse and translate an identity
        private void Ident()
        {
            char name = Cradle.GetName();
            if (Cradle.Look == '(')
            {
                Cradle.Match('(');
                Expression();
                Cradle.Match(')');
                Cradle.EmitLn("BSR " + name);
            }
            else
                Cradle.EmitLn("MOVE " + name + "(PC),D0");
        }

        // Parse and translate a math factor
        private void Factor()
        {
            if (Cradle.Look == '(')
            {
                Cradle.Match('(');
                Expression();
                Cradle.Match(')');
            }
            else if (Cradle.IsAlpha(Cradle.Look))
                Ident();
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
                }
            }
        }

        // Parse and translate an assignment statement
        private void Assignment()
        {
            char name = Cradle.GetName();
            Cradle.Match('=');
            Expression();
            Cradle.EmitLn("LEA " + name + "(PC),A0");
            Cradle.EmitLn("MOVE D0,(A0)");
        }

        public Compiler()
        {
            Assignment();
            if (Cradle.Look != Cradle.CR) Cradle.Expected("Newline");
        }
    }
}
