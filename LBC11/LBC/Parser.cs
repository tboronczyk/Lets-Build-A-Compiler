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

        // Check to make sure current token is an identifier
        private void CheckIdent()
        {
            if (tokenizer.Token != 'x') ErrorHandler.Expected("Identifier");
        }

        // Locate a symbol in table
        // Returns the index of the entry, -1 if not matched
        private int Locate(string s)
        {
            return tokenizer.Lookup(ST, s);
        }

        // Look for symbol in table
        private bool InTable(string s)
        {
            return tokenizer.Lookup(ST, s) > -1;
        }

        // Check to see if an identifier is in the symbol table
        // Report an error if it's not
        private void CheckTable(string s)
        {
            if (!InTable(s)) ErrorHandler.Undefined(s);
        }

        // Check the symbol table for a duplicate identifier
        // Report an error if identifier is already in table
        private void CheckDup(string s)
        {
            if (InTable(s)) ErrorHandler.Duplicate(s);
        }

        // Add a new entry to symbol table
        private void AddEntry(string n, char t)
        {
            CheckDup(n);
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
            // if (!InTable(name)) ErrorHandler.Undefined(name);
            cpu.Store(name);
        }

        // Read variable to primary
        private void ReadIt(string name)
        {
            cpu.ReadVar();
            Store(name);
        }

        // Parse and translate a math factor
        private void Factor()
        {
            if (tokenizer.Token == '(')
            {
                tokenizer.Next();
                BoolExpression();
                tokenizer.MatchString(")");
            }
            else
            {
                if (tokenizer.Token == 'x')
                    LoadVar(tokenizer.Value);
                else if (tokenizer.Token == '#')
                    cpu.LoadConst(tokenizer.Value);
                else
                    ErrorHandler.Expected("Math Factor");
                tokenizer.Next();
            }
        }

        // Recognize and translate a multiply
        private void Multiply()
        {
            tokenizer.Next();
            Factor();
            cpu.PopMul();
        }

        // Recognize and translate a divide
        private void Divide()
        {
            tokenizer.Next();
            Factor();
            cpu.PopDiv();
        }

        // Parse and translate a math term
        private void Term()
        {
            Factor();
            while (tokenizer.IsMulop(tokenizer.Token))
            {
                cpu.Push();
                switch (tokenizer.Token)
                {
                    case '*': Multiply(); break;
                    case '/': Divide(); break;
                }
            }
        }

        // Recognize and translate an add
        private void Add()
        {
            tokenizer.Next();
            Term();
            cpu.PopAdd();
        }

        // Recognize and translate a subtract
        private void Subtract()
        {
            tokenizer.Next();
            Term();
            cpu.PopSub();
        }

        // Parse and translate an expression
        private void Expression()
        {
            if (tokenizer.IsAddop(tokenizer.Token))
                cpu.Clear();
            else
                Term();

            while (tokenizer.IsAddop(tokenizer.Token))
            {
                cpu.Push();
                switch (tokenizer.Token)
                {
                    case '+': Add(); break;
                    case '-': Subtract(); break;
                }
            }
        }

        // Get another expression and compare
        private void CompareExpression()
        {
            Expression();
            cpu.PopCompare();
        }

        // Get the next expression and compare
        private void NextExpression()
        {
            tokenizer.Next();
            CompareExpression();
        }

        // Recogize and translate a relational "equals"
        private void RelEquals()
        {
            NextExpression();
            cpu.SetEqual();
        }

        // Recognize and translate a relational "less than or equal"
        private void LessOrEqual()
        {
            NextExpression();
            cpu.SetLessOrEqual();
        }

        // Recognize and translate a relational "not equals"
        private void NotEquals()
        {
            NextExpression();
            cpu.SetNEqual();
        }

        // Recognize and translate a relational "less than"
        private void Less()
        {
            tokenizer.Next();
            switch (tokenizer.Token)
            {
                case '=': LessOrEqual(); break;
                case '>': NotEquals(); break;
                default:
                    CompareExpression();
                    cpu.SetLess();
                    break;
            }
        }

        // Recognize ad translate a relational "greater than"
        private void Greater()
        {
            tokenizer.Next();
            if (tokenizer.Look == '=')
            {
                NextExpression();
                cpu.SetGreaterOrEqual();
            }
            else
            {
                CompareExpression();
                cpu.SetGreater();
            }
        }

        // Recognize and translate a relationship
        private void Relation()
        {
            Expression();
            if (tokenizer.IsRelop(tokenizer.Token))
            {
                cpu.Push();
                switch (tokenizer.Token)
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
                tokenizer.Next();
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
            while (tokenizer.Token == '&')
            {
                cpu.Push();
                tokenizer.Next();
                NotFactor();
                cpu.PopAnd();
            }
        }

        // Recognize and translate a Boolean or
        private void BoolOr()
        {
            tokenizer.Next();
            BoolTerm();
            cpu.PopOr();
        }

        // Recognize and translate an exclusive or
        private void BoolXor()
        {
            tokenizer.Next();
            BoolTerm();
            cpu.PopXor();
        }

        // Parse and translate a Boolean expression
        private void BoolExpression()
        {
            BoolTerm();
            while (tokenizer.IsOrop(tokenizer.Token))
            {
                cpu.Push();
                switch (tokenizer.Token)
                {
                    case '|': BoolOr(); break;
                    case '~': BoolXor(); break;
                }
            }
        }

        // Parse and translate an assignment statement
        private void Assignment()
        {
            CheckTable(tokenizer.Value);
            string name = tokenizer.Value;
            tokenizer.Next();
            tokenizer.MatchString("=");
            BoolExpression();
            Store(name);
        }

        // Recognize and translate an if construct
        private void DoIf()
        {
            tokenizer.Next();
            BoolExpression();
            string L1 = Label.NewLabel();
            string L2 = L1;
            cpu.BranchFalse(L1);
            Block();
            if (tokenizer.Token == 'l')
            {
                tokenizer.Next();
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
            tokenizer.Next();
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

        // Read a single variable
        private void ReadVar()
        {
            CheckIdent();
            CheckTable(tokenizer.Value);
            ReadIt(tokenizer.Value);
            tokenizer.Next();
        }

        // Process a read statement
        private void DoRead()
        {
            tokenizer.Next();
            tokenizer.MatchString("(");
            ReadVar();
            while (tokenizer.Token == ',')
            {
                tokenizer.Next();
                ReadVar();
            }
            tokenizer.MatchString(")");
        }

        // Process a write statement
        private void DoWrite()
        {
            tokenizer.Next();
            tokenizer.MatchString("(");
            Expression();
            cpu.WriteIt();
            while (tokenizer.Token == ',')
            {
                tokenizer.Next();
                Expression();
                cpu.WriteIt();
            }
            tokenizer.MatchString(")");
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
        private void Alloc()
        {
            tokenizer.Next();
            if (tokenizer.Token != 'x') ErrorHandler.Expected("Variable name");
            CheckDup(tokenizer.Value);
            AddEntry(tokenizer.Value, 'v');
            cpu.Alloc(tokenizer.Value, 0);
            tokenizer.Next();
        }

        // Parse and translate global declarations
        private void TopDecls()
        {
            tokenizer.Scan();
            while (tokenizer.Token == 'v')
            {
                Alloc();
            }
            while (tokenizer.Token == ',')
            {
                Alloc();
            }

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

            tokenizer.MatchString("PROGRAM");
            cpu.Header();
            TopDecls();
            tokenizer.MatchString("BEGIN");
            cpu.Prolog();
            Block();
            tokenizer.MatchString("END");
            cpu.Epilog();
        }
    }
}