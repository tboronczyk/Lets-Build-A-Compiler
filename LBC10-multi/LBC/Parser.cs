using System;

namespace LBC
{
    class Parser
    {
        private CPU cpu;
        private Tokenizer tokenizer;

        private int NEntry;
        private const int NMAXENTRY = 100;
        private string[] ST;
        private char[] SType;

        // Look for symbol in table
        private bool InTable(string n)
        {
            return tokenizer.Lookup(ST, n) > -1;
        }

        // Add a new entry to symbol table
        private void AddEntry(string n, char t)
        {
            if (InTable(n)) ErrorHandler.Abort("Duplicate identifier " + n);
            if (NEntry == NMAXENTRY) ErrorHandler.Abort("Symbol table full");
            ST[NEntry] = n;
            SType[NEntry] = t;
            NEntry++;
        }

        // Load a variable 
        private void LoadVar(string name)
        {
            if (!InTable(name)) ErrorHandler.Undefined(name);
            cpu.LoadVar(name);
        }

        // Store primary to variable
        private void Store(string name)
        {
            if (!InTable(name)) ErrorHandler.Undefined(name);
            cpu.Store(name);
        }

        // Read variable to primary
        private void ReadVar()
        {
            cpu.ReadVar();
            Store(tokenizer.Value);
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
                tokenizer.GetName();
                LoadVar(tokenizer.Value);
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

        // Recognize and translate a relational "less than or equal"
        private void LessOrEqual()
        {
            tokenizer.Match('=');
            Expression();
            cpu.PopCompare();
            cpu.SetLessOrEqual();
        }

        // Recognize and translate a relational "not equals"
        private void NotEquals()
        {
            tokenizer.Match('>');
            Expression();
            cpu.PopCompare();
            cpu.SetNEqual();
        }

        // Recognize and translate a relational "less than"
        private void Less()
        {
            tokenizer.Match('<');
            switch (tokenizer.Look)
            {
                case '=': LessOrEqual(); break;
                case '>': NotEquals(); break;
                default:
                    Expression();
                    cpu.PopCompare();
                    cpu.SetLess();
                    break;
            }
        }

        // Recognize ad translate a relational "greater than"
        private void Greater()
        {
            tokenizer.Match('>');
            if (tokenizer.Look == '=')
            {
                tokenizer.Match('=');
                Expression();
                cpu.PopCompare();
                cpu.SetGreaterOrEqual();
            }
            else
            {
                Expression();
                cpu.PopCompare();
                cpu.SetGreater();
            }
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

        // Parse and translate an assignment statement
        private void Assignment()
        {
            string name = tokenizer.Value;
            tokenizer.Match('=');
            BoolExpression();
            Store(name);
        }

        // Recognize and translate an if construct
        private void DoIf()
        {
        BoolExpression();
        string L1 = Label.NewLabel();
        string L2 = L1;
        cpu.BranchFalse(L1);
        Block();
        if (tokenizer.Token == 'l')
        {
        L2 = Label.NewLabel();
        cpu.Branch(L2);
        cpu.PostLabel(L1);
        Block();
        }
        cpu.PostLabel(L2);
        tokenizer.MatchString("ENDIF");
        }

        // Parse and translate a while statement
        private void DoWhile()
        {
        string L1 = Label.NewLabel();
        string L2 = Label.NewLabel();
        cpu.PostLabel(L1);
        BoolExpression();
        cpu.BranchFalse(L2);
        Block();
        tokenizer.MatchString("ENDWHILE");
        cpu.Branch(L1);
        cpu.PostLabel(L2);
        }

        // Process a read statement
        private void DoRead()
        {
            tokenizer.Match('(');
            tokenizer.GetName();
            ReadVar();
            while (tokenizer.Look == ',')
            {
                tokenizer.Match(',');
                tokenizer.GetName();
                ReadVar();
            }
            tokenizer.Match(')');
        }

        // Process a write statement
        private void DoWrite()
        {
            tokenizer.Match('(');
            Expression();
            cpu.WriteVar();
            while (tokenizer.Look == ',')
            {
                tokenizer.Match(',');
                Expression();
                cpu.WriteVar();
            }
            tokenizer.Match(')');
        }

        // Parse and translate a block of statements
        private void Block()
        {
            tokenizer.Scan();
            while ("el".IndexOf(tokenizer.Token) == -1)
            {
                switch (tokenizer.Token)
                {
                    case 'i': DoIf(); break;
                    case 'w': DoWhile(); break;
                    case 'R': DoRead(); break;
                    case 'W': DoWrite(); break;
                    default: Assignment(); break;
                }
                tokenizer.Scan();
            }
        }

        // Allocate storage for a variable
        private void Alloc(string n)
        {
            if (InTable(n)) ErrorHandler.Abort("Duplicate variable name " + n);
            AddEntry(n, 'v');

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
            //tokenizer.Match('v');
            tokenizer.GetName();
            Alloc(tokenizer.Value);
            while (tokenizer.Look == ',')
            {
                tokenizer.Match(',');
                tokenizer.GetName();
                Alloc(tokenizer.Value);
            }
        }

        // Parse and translate global declarations
        private void TopDecls()
        {
            tokenizer.Scan();
            while (tokenizer.Token != 'b')
            {
                switch (tokenizer.Token)
                {
                    case 'v': Decl(); break;
                    default: ErrorHandler.Abort("Unrecognized Keyword \"" + tokenizer.Value + "\""); break;
                }
                tokenizer.Scan();
            }
        }

        // Parse and translate the main program
        private void DoMain()
        {
            tokenizer.MatchString("BEGIN");
            cpu.Prolog();
            Block();
            tokenizer.MatchString("END");
            cpu.Epilog();
        }

        // Parse and translate a program
        private void Prog()
        {
            tokenizer.MatchString("PROGRAM");
            cpu.Header();
            TopDecls();
            DoMain();
            tokenizer.Match('.');
        }

        public Parser()
        {
            NEntry = 0;
            ST = new string[NMAXENTRY];
            SType = new char[NMAXENTRY];
            for (int i = 0; i < NMAXENTRY; i++)
            {
                SType[i] = ' ';
                ST[i] = " ";
            }

            tokenizer = new Tokenizer();
            cpu = new Cpu68000();

            Prog();
            if (tokenizer.Look != Tokenizer.CR) ErrorHandler.Abort("Unexpected data after \".\"");
        }
    }
}