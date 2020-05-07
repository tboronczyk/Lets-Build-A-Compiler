using System;

namespace LBC
{
    class Compiler
    {
        // Parse and translate a Boolean factor
        private void BoolFactor()
        {
            if (Cradle.IsBoolean(Cradle.Look))
            {
                if (Cradle.GetBoolean())
                    Cradle.EmitLn("MOVE #-1,D0");
                else
                    Cradle.EmitLn("CLR D0");
            }
            Relation();
        }

        // Recognize and translate a relational equals
        private void RelEquals()
        {
            Cradle.Match('=');
            Expression();
            Cradle.EmitLn("CMP (SP)+,D0");
            Cradle.EmitLn("SEQ D0");
        }
        // Recognize and translate a relational not equals
        private void NotEquals()
        {
            Cradle.Match('#');
            Expression();
            Cradle.EmitLn("CMP (SP)+,D0");
            Cradle.EmitLn("SNE D0");
        }

        // Recognize and translate a relational less than
        private void Less()
        {
            Cradle.Match('<');
            Expression();
            Cradle.EmitLn("CMP (SP)+,D0");
            Cradle.EmitLn("SGE D0");
        }

        // Recognize and translate a relational greater than
        private void Greater()
        {
            Cradle.Match('>');
            Expression();
            Cradle.EmitLn("CMP (SP)+,D0");
            Cradle.EmitLn("SLE D0");
        }

        // Parse and translate a relation
        private void Relation()
        {
            Expression();
            if (IsRelop(Cradle.Look))
            {
                Cradle.EmitLn("MOVE D0,-(SP)");
                switch (Cradle.Look)
                {
                    case '=': RelEquals(); break;
                    case '#': NotEquals(); break;
                    case '<': Less(); break;
                    case '>': Greater(); break;
                }
                Cradle.EmitLn("TST D0");
            }
        }
        
        // Recognize a relop
        private bool IsRelop(char c)
        {
            return "=#<>".IndexOf(c) > -1;
        }

        // Recognize a Boolean Orop
        private bool IsOrop(char c)
        {
            return "|~".IndexOf(c) > -1;
        }

        // Recognize and translate a Boolean or
        private void BoolOr()
        {
            Cradle.Match('|');
            BoolTerm();
            Cradle.EmitLn("OR (SP)+,D0");
        }

        // Recognize and translate an exclusive or 
        private void BoolXor()
        {
            Cradle.Match('~');
            BoolTerm();
            Cradle.EmitLn("EOR (SP)+,D0");
        }

        //  Parse and translate a Boolean factor with not
        private void NotFactor()
        {
            if (Cradle.Look == '!')
            {
                Cradle.Match('!');
                BoolFactor();
                Cradle.EmitLn("EOR #-1,D0");
            }
            else
                BoolFactor();
        }

        // Parse and recognize a Boolean term
        private void BoolTerm()
        {
            NotFactor();
            while (Cradle.Look == '&')
            {
                Cradle.EmitLn("MOVE D0,-(SP)");
                Cradle.Match('&');
                NotFactor();
                Cradle.EmitLn("AND (SP)+,D0");
            }
        }

        // Parse and translate a Boolean expression
        private void BoolExpression()
        {
            BoolTerm();
            while (IsOrop(Cradle.Look))
            {
                Cradle.EmitLn("MOVE D0,-(SP)");
                switch (Cradle.Look)
                {
                    case '|': BoolOr(); break;
                    case '~': BoolXor(); break;
                }
            }
        }

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

        // Parse and translate the first math factor
        private void SignedFactor()
        {
            if (Cradle.Look == '+') Cradle.GetChar();
            if (Cradle.Look == '-')
            {
                Cradle.GetChar();
                if (Cradle.IsDigit(Cradle.Look))
                    Cradle.EmitLn("MOVE #-" + Cradle.GetNum() + ",D0");
                else
                {
                    Factor();
                    Cradle.EmitLn("NEG D0");
                }
            }
            else
                Factor();
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
            SignedFactor();
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


        public Compiler()
        {
            BoolExpression();
        }
    }
}
