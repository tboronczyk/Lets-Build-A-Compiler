using System;
using System.Collections;

namespace LBC
{
    class Compiler : Cradle
    {
        // Definition of keyword tokens
        private readonly string[] KWList = { "IF", "ELSE", "ENDIF", "END" };
        private const string KWCode = "xilee";

        private char Token;        // Current token
        private string Value;      // Value of current token

        // Table lookup
        // Return entry index of given string, else -1 if not matched
        private int Lookup(string s) {
            int index = 0;
            foreach (string entry in KWList)
            {
                if (entry == s) return index;
                ++index;
            }
            return -1;
        }

        // Get an identifier
        private void GetName()
        {
            while (Look == CR)
                Fin();
            if (!IsAlpha(Look)) Expected("Name");
            Value = "";
            while (IsAlNum(Look))
            {
                Value += char.ToUpper(Look);
                GetChar();
            }
            SkipWhite();
        }

        // Get a number
        private void GetNum()
        {
            if (!IsDigit(Look)) Expected("Integer");
            Value = "";
            while (IsDigit(Look))
            {
                Value += Look;
                GetChar();
            }
            Token = '#';
            SkipWhite();
        }


        // Get an identifier and scan it for keywords
        private void Scan()
        {
            GetName();
            Token = KWCode[Lookup(Value) + 1];
        }

        // Match a specific input string
        private void MatchString(string s)
        {
            if (Value != s) Expected("\"" + s + "\"");
        }

        // Post a label to output
        private void PostLabel(string L)
        {
            EmitLn(L + ":");
        }

        // Parse and translate an identifier
        private void Ident()
        {
            GetName();
            if (Look == '(')
            {
                Match('(');
                Match(')');
                EmitLn("BSR " + Value);
            }
            else
                EmitLn("MOVE " + Value + "(PC),D0");
        }

        // Parse and translate a math factor
        private void Factor()
        {
            if (Look == '(')
            {
                Match('(');
                Expression();
                Match(')');
            }
            else if (IsAlpha(Look))
                Ident();
            else
            {
                GetNum();
                EmitLn("MOVE #" + Value + ",D0");
            }
        }

        // Parse and translate the first math factor
        private void SignedFactor()
        {
            bool s = Look == '-';
            if (IsAddop(Look))
            {
                GetChar();
                SkipWhite();
            }
            Factor();
            if (s)
                EmitLn("NEG D0");
        }

        // Recognize and translate a multiply
        private void Multiply()
        {
            Match('*');
            Factor();
            EmitLn("MULS (SP)+,D0");
        }

        // Recognize and translate a divide
        private void Divide()
        {
            Match('/');
            Factor();
            EmitLn("MOVE(SP)+,D1");
            EmitLn("DIVS D1,D0");
        }

        // Completion of term processing (called by Term and FirstTerm)
        private void Term1()
        {
            while (IsMulop(Look))
            {
                EmitLn("MOVE D0,-(SP)");
                switch (Look)
                {
                    case '*': Multiply(); break;
                    case '/': Divide(); break;
                }
            }
        }

        // Parse and translate a math term
        private void Term()
        {
            Factor();
            Term1();
        }

        // Parse and translate a math term with a possible leading sign
        private void FirstTerm()
        {
            SignedFactor();
            Term1();
        }

        // Recognize and translate an add
        private void Add()
        {
            Match('+');
            Term();
            EmitLn("ADD (SP)+,D0");
        }

        // Recognize and translate a subtract
        private void Subtract()
        {
            Match('-');
            Term();
            EmitLn("SUB (SP)+,D0");
            EmitLn("NEG D0");
        }

        // Parse and translate an expression
        private void Expression()
        {
            FirstTerm();
            while (IsAddop(Look))
            {
                EmitLn("MOVE D0,-(SP)");
                switch (Look)
                {
                    case '+': Add(); break;
                    case '-': Subtract(); break;
                }
            }
        }

        // Parse and translate a Boolean conditon
        // this version is a dummy
        private void Condition()
        {
            EmitLn("Condition");
        }

        // Recognize and translate an if construct
        private void DoIf()
        {
            Condition();
            string L1 = Label.NewLabel();
            string L2 = L1;
            EmitLn("BEQ " + L1);
            Block();
            if (Token == 'l')
            {
                L2 = Label.NewLabel();
                EmitLn("BRA " + L2);
                PostLabel(L1);
                Block();
            }
            PostLabel(L2);
            MatchString("ENDIF");
        }

        // Parse and translate an assignment statement
        private void Assignment()
        {
            string name = Value;
            Match('=');
            Expression();
            EmitLn("LEA " + name + "(PC),A0");
            EmitLn("MOVE D0,(A0)");
        }

        // Parse and translate a statement block
        private void Block()
        {
            Scan();
            while ("el".IndexOf(Token) == -1)
            {
                switch (Token)
                {
                    case 'i': DoIf(); break;
                    default: Assignment(); break;
                }
                Scan();
            }
        }

        // Parse and translate a program
        private void DoProgram()
        {
            Block();
            MatchString("END");
            EmitLn("END");
        }
        
        public Compiler()
        {
            GetChar();
            DoProgram();
        }
    }
}