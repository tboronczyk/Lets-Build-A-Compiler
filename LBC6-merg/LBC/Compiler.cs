using System;

namespace LBC
{
    public struct Label
    {
        private static int Value;    // Label Counter

        // Generate a unique label
        public static string NewLabel()
        {
            return "L" + Value++;
        }

        // Post a label to output
        public static void PostLabel(string L)
        {
            Cradle.EmitLn(L + ":");
        }

        // Initialize Label
        static Label()
        {
            Value = 0;
        }
    }

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

        // Recognize and translate an if construct
        private void DoIf(string L)
        {
            Cradle.Match('i');
            BoolExpression();
            string L1 = Label.NewLabel();
            string L2 = L1;
            Cradle.EmitLn("BEQ  " + L1);
            Block(L);
            if (Cradle.Look == 'l')
            {
                Cradle.Match('l');
                L2 = Label.NewLabel();
                Cradle.EmitLn("BRA " + L2);
                Label.PostLabel(L1);
                Block(L);
            }
            Cradle.Match('e');
            Label.PostLabel(L2);
        }

        // Parse and translate a while statement
        private void DoWhile()
        {
            Cradle.Match('w');
            string L1 = Label.NewLabel();
            string L2 = Label.NewLabel();
            Label.PostLabel(L1);
            BoolExpression();
            Cradle.EmitLn("BEQ " + L2);
            Block(L2);
            Cradle.Match('e');
            Cradle.EmitLn("BRA " + L1);
            Label.PostLabel("L2");
        }

        // Parse and translate a loop statement
        private void DoLoop()
        {
            Cradle.Match('p');
            string L1 = Label.NewLabel();
            string L2 = Label.NewLabel();
            Label.PostLabel(L1);
            Block(L2);
            Cradle.Match('e');
            Cradle.EmitLn("BRA " + L1);
            Label.PostLabel(L2);
        }

        // Parse and translate a repeat block
        private void DoRepeat()
        {
            Cradle.Match('r');
            string L1 = Label.NewLabel();
            string L2 = Label.NewLabel();
            Label.PostLabel(L1);
            Block(L2);
            Cradle.Match('u');
            BoolExpression();
            Cradle.EmitLn("BEQ " + L1);
            Label.PostLabel(L2);
        }

        // Parse and translate a for statement
        private void DoFor()
        {
            Cradle.Match('f');
            string L1 = Label.NewLabel();
            string L2 = Label.NewLabel();
            char Name = Cradle.GetName();
            Cradle.Match('=');
            Expression();
            Cradle.EmitLn("SUBQ #1,D0");
            Cradle.EmitLn("LEA " + Name + "(PC),A0");
            Cradle.EmitLn("MOVE D0,(A0)");
            Expression();
            Cradle.EmitLn("MOVE D0,-(SP)");
            Label.PostLabel(L1);
            Cradle.EmitLn("LEA " + Name + "(PC),A0");
            Cradle.EmitLn("ADDQ #1,D0");
            Cradle.EmitLn("MOVE D0,(A0)");
            Cradle.EmitLn("CMP (SP),D0)");
            Cradle.EmitLn("BGT " + L2);
            Block(L2);
            Cradle.Match('e');
            Cradle.EmitLn("BRA " + L1);
            Label.PostLabel(L2);
            Cradle.EmitLn("ADDQ #2,SP");
        }

        // Parse and translate a do statement
        private void DoDo()
        {
            Cradle.Match('d');
            string L1 = Label.NewLabel();
            string L2 = Label.NewLabel();
            Expression();
            Cradle.EmitLn("SUBQ #1,D0");
            Label.PostLabel(L1);
            Cradle.EmitLn("MOVE D0,-(SP)");
            Block(L2);
            Cradle.Match('e');
            Cradle.EmitLn("MOVE (SP)+,D0");
            Cradle.EmitLn("DBRA D0," + L1);
            Cradle.EmitLn("SUBQ #2,SP");
            Label.PostLabel(L2);
            Cradle.EmitLn("ADDQ #2,SP");
        }

        // Recognize and translate a break
        private void DoBreak(string L)
        {
            Cradle.Match('b');
            if (L.Length > 0)
                Cradle.EmitLn("BRA " + L);
            else
                Cradle.Abort("No loop to break");
        }

        // Recognize and translate an other
        private void Other()
        {
            Cradle.EmitLn(Cradle.GetName().ToString());
        }

        // Parse and translate an assignment statement
        private void Assignment()
        {
            char name = Cradle.GetName();
            Cradle.Match('=');
            BoolExpression();
            Cradle.EmitLn("LEA " + name + "(PC),A0");
            Cradle.EmitLn("MOVE D0,(A0)");
        }

        // Recognize and translate a statement block
        private void Block(string L)
        {
            while ("elu".IndexOf(Cradle.Look) == -1)
            {
                Cradle.Fin();
                switch (Cradle.Look)
                {
                    case 'i': DoIf(L); break;
                    case 'w': DoWhile(); break;
                    case 'p': DoLoop(); break;
                    case 'r': DoRepeat(); break;
                    case 'f': DoFor(); break;
                    case 'd': DoDo(); break;
                    case 'b': DoBreak(L); break;
                    default: Assignment(); break;
                }
                Cradle.Fin();
            }
        }

        // Parse and translate a program
        private void DoProgram()
        {
            Block("");
            if (Cradle.Look != 'e') Cradle.Expected("End");
            Cradle.EmitLn("END");
        }

        public Compiler()
        {
            DoProgram();
        }
    }
}
