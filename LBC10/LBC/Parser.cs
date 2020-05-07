using System;

namespace LBC
{
    class Parser
    {
        private CPU cpu;
        private Tokenizer tokenizer;

        private char[] ST;

        // Look for Symbol in table
        private bool InTable(char n)
        {
            // if (n < 'A' || n > 'Z') ErrorHandler.Expected("Expected variable name");
            return ST[n - 'A'] != ' ';
        }

        // Parse and translate a math factor
        private void Factor()
        {
            if (tokenizer.Look == '(')
            {
                tokenizer.Match('(');
                BoolExpression();
                tokenizer.Match(')');
            }
            else if (tokenizer.IsAlpha(tokenizer.Look))
            {
                char name = tokenizer.GetName();
                LoadVar(name);
            }
            else
                cpu.LoadConst(tokenizer.GetNum());
        }

        // Parse and translate a negative factor
        private void NegFactor()
        {
            tokenizer.Match('-');
            if (tokenizer.IsDigit(tokenizer.Look))
                cpu.LoadConst(-tokenizer.GetNum());
            else
            {
                Factor();
                cpu.Negate();
            }
        }

        // Parse and translate a leading factor
        private void FirstFactor()
        {
            switch (tokenizer.Look)
            {
                case '+':
                    tokenizer.Match('+');
                    Factor();
                    break;
                case '-': NegFactor(); break;
                default: Factor(); break;
            }
        }

        // Recognize and translate a multiply
        private void Multiply()
        {
            tokenizer.Match('*');
            Factor();
            cpu.PopMul();
        }

        // Recognize and translate a divide
        private void Divide()
        {
            tokenizer.Match('/');
            Factor();
            cpu.PopDiv();
        }

        // Common code used by Term and FirstTerm
        private void Term1()
        {
            while (tokenizer.IsMulop(tokenizer.Look))
            {
                cpu.Push();
                switch (tokenizer.Look)
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

        // Parse and translate a leading term
        private void FirstTerm()
        {
            FirstFactor();
            Term1();
        }

        // Recognize and translate an add
        private void Add()
        {
            tokenizer.Match('+');
            Term();
            cpu.PopAdd();
        }

        // Recognize and translate a subtract
        private void Subtract()
        {
            tokenizer.Match('-');
            Term();
            cpu.PopSub();
        }

        // Parse and translate an expression
        private void Expression()
        {
            FirstTerm();
            while (tokenizer.IsAddop(tokenizer.Look))
            {
                cpu.Push();
                switch (tokenizer.Look)
                {
                    case '+': Add(); break;
                    case '-': Subtract(); break;
                }
            }
        }


        // Recogize and translate a relational "equals"
        private void RelEquals()
        {
            tokenizer.Match('=');
            Expression();
            cpu.PopCompare();
            cpu.SetEqual();
        }

        // Recognize and translate a relational "not equals"
        private void NotEquals()
        {
            tokenizer.Match('#');
            Expression();
            cpu.PopCompare();
            cpu.SetNEqual();
        }

        // Recognize and translate a relational "less than"
        private void Less()
        {
            tokenizer.Match('<');
            Expression();
            cpu.PopCompare();
            cpu.SetLess();
        }

        // Recognize ad translate a relational "greater than"
        private void Greater()
        {
            tokenizer.Match('>');
            Expression();
            cpu.PopCompare();
            cpu.SetGreater();
        }

        // Recognize and translate a relationship
        private void Relation()
        {
            Expression();
            if (tokenizer.IsRelop(tokenizer.Look))
            {
                cpu.Push();
                switch (tokenizer.Look)
                {
                    case '=': RelEquals(); break;
                    case '#': NotEquals(); break;
                    case '<': Less(); break;
                    case '>': Greater(); break;
                }
            }
        }

        // Parse and translate a Boolean factor with leading not
        private void NotFactor()
        {
            if (tokenizer.Look == '!')
            {
                tokenizer.Match('!');
                Relation();
                cpu.NotIt();
            }
            else
                Relation();
        }

        // Parse and translate a Boolean term
        private void BoolTerm()
        {
            NotFactor();
            while (tokenizer.Look == '&')
            {
                cpu.Push();
                tokenizer.Match('&');
                NotFactor();
                cpu.PopAnd();
            }
        }

        // Recognize and translate a Boolean or
        private void BoolOr()
        {
            tokenizer.Match('|');
            BoolTerm();
            cpu.PopOr();
        }

        // Recognize and translate an exclusive or
        private void BoolXor()
        {
            tokenizer.Match('~');
            BoolTerm();
            cpu.PopXor();
        }

        // Parse and translate a Boolean expression
        private void BoolExpression()
        {
            BoolTerm();
            while (tokenizer.IsOrop(tokenizer.Look))
            {
                cpu.Push();
                switch (tokenizer.Look)
                {
                    case '|': BoolOr(); break;
                    case '~': BoolXor(); break;
                }
            }
        }

        // Recognize and translate an if construct
        private void DoIf()
        {
        tokenizer.Match('i');
        BoolExpression();
        string L1 = Label.NewLabel();
        string L2 = L1;
        cpu.BranchFalse(L1);
        Block();
        if (tokenizer.Look == 'l')
        {
        tokenizer.Match('l');
        L2 = Label.NewLabel();
        cpu.Branch(L2);
        cpu.PostLabel(L1);
        Block();
        }
        cpu.PostLabel(L2);
        tokenizer.Match('e');
        }

        // Parse and translate a while statement
        private void DoWhile()
        {
        tokenizer.Match('w');
        string L1 = Label.NewLabel();
        string L2 = Label.NewLabel();
        cpu.PostLabel(L1);
        BoolExpression();
        cpu.BranchFalse(L2);
        Block();
        tokenizer.Match('e');
        cpu.Branch(L1);
        cpu.PostLabel(L2);
        }

        // Load a variable 
        private void LoadVar(char name)
        {
            if (!InTable(name)) ErrorHandler.Undefined(name);
            cpu.LoadVar(name);
        }

        // Store primary to variable
        private void Store(char name)
        {
            if (!InTable(name)) ErrorHandler.Undefined(name);
            cpu.Store(name);
        }

        // Parse and translate an assignment statement
        private void Assignment()
        {
            char name = tokenizer.GetName();
            tokenizer.Match('=');
            BoolExpression();
            Store(name);
        }

        // Parse and translate a block of statements
        private void Block()
        {
            tokenizer.NewLine();
            while ("el".IndexOf(tokenizer.Look) == -1)
            {
                switch (tokenizer.Look)
                {
                    case 'i': DoIf(); break;
                    case 'w': DoWhile(); break;
                    default: Assignment(); break;
                }
                tokenizer.NewLine();
            }
        }

        // Parse and translate the main program
        private void DoMain()
        {
            tokenizer.Match('b');
            cpu.Prolog();
            Block();
            tokenizer.Match('e');
            cpu.Epilog();
        }

        // Allocate storage for a variable
        private void Alloc(char n)
        {
            if (InTable(n)) ErrorHandler.Abort("Duplicate variable name " + n);
            ST[n - 'A'] = 'v';

            if (tokenizer.Look == '=')
            {
                tokenizer.Match('=');
                if (tokenizer.Look == '-')
                {
                    tokenizer.Match('-');
                    cpu.Alloc(n, -tokenizer.GetNum());
                }
                cpu.Alloc(n, tokenizer.GetNum());
            }
            else
                cpu.Alloc(n, 0);
        }

        // Process a data declaration
        private void Decl()
        {
            tokenizer.Match('v');
            Alloc(tokenizer.GetName());
            while (tokenizer.Look == ',')
            {
                tokenizer.GetChar();
                Alloc(tokenizer.GetName());
            }
        }

        // Parse and translate global declarations
        private void TopDecls()
        {
            tokenizer.NewLine();
            while (tokenizer.Look != 'b')
            {
                switch (tokenizer.Look)
                {
                    case 'v': Decl(); break;
                    default: ErrorHandler.Abort("Unrecognized Keyword \"" + tokenizer.Look + "\""); break;
                }
                tokenizer.NewLine();
            }
        }

        // Parse and translate a program
        private void Prog()
        {
            tokenizer.Match('p');
            cpu.Header();
            TopDecls();
            DoMain();
            tokenizer.Match('.');
        }

        public Parser()
        {
            cpu = new Cpu68000();
            tokenizer = new Tokenizer();

            ST = new char[26];
            for (char i = 'A'; i <= 'Z'; i++)
            {
                ST[i - 'A'] = ' ';
            }



            Prog();
            if (tokenizer.Look != Tokenizer.CR) ErrorHandler.Abort("Unexpected data after \".\"");
        }
    }
}