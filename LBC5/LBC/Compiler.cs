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
        // Parse and translate a Boolean condition
        // This is a dummy version
        private void Condition()
        {
            Cradle.EmitLn("<condition>");
        }

        // Parse and translate an expression
        // This is a dummy version
        private void Expression()
        {
            Cradle.EmitLn("<expr>");
        }

        // Recognize and translate an if construct
        private void DoIf(string L)
        {
            Cradle.Match('i');
            Condition();
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
            Condition();
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
            Condition();
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
            Cradle.EmitLn("MOVE DO,-(SP)");
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

        // Recognize and translate a statement block
        private void Block(string L)
        {
            while ("elu".IndexOf(Cradle.Look) == -1)
            {
                switch (Cradle.Look)
                {
                    case 'i': DoIf(L); break;
                    case 'w': DoWhile(); break;
                    case 'p': DoLoop(); break;
                    case 'r': DoRepeat(); break;
                    case 'f': DoFor(); break;
                    case 'd': DoDo(); break;
                    case 'b': DoBreak(L); break;
                    default: Other(); break;
                }
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
