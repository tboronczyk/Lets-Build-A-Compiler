using System;
using System.Collections;

namespace LBC
{
    class Compiler : Cradle
    {
        private char[] ST;

        private int[] Params;
        private int NumParams;

        // Initialize parameter table to null
        private void ClearParams()
        {
            for (char c = 'A'; c <= 'Z'; c++)
            {
                Params[c - 'A'] = 0;
            }
            NumParams = 0;
        }

        // Find the parameter number
        private int ParamNumber(char n)
        {
            return Params[n - 'A'];
        }

        // See if an identifier is a parameter
        private bool IsParam(char n)
        {
            return Params[n - 'A'] > 0;
        }

        // Add a new parameter to table
        private void AddParam(char name)
        {
            if (IsParam(name)) Duplicate(name.ToString());
            ++NumParams;
            Params[name - 'A'] = NumParams;
        }

        // Load a parameter to the primary register
        private void LoadParam(int n)
        {
            int offset = 8 + 2 * (NumParams - n);
            EmitLn("MOVE " + offset + "(A6),D0");
        }

        // Store a parameter from the primary register
        private void StoreParam(int n)
        {
            int offset = 8 + 2 * (NumParams - n);
            EmitLn("MOVE D0," + offset + "(A6)");
        }

        // Push the primary register to the stack
        private void Push()
        {
            EmitLn("MOVE D0,-(SP)");
        }

        // Get type of symbol
        private char TypeOf(char n)
        {
            if (IsParam(n))
                return 'f';
            else
                return ST[n - 'A'];
        }

        // Look for symbol in table
        private bool InTable(char n)
        {
            return ST[n - 'A'] != ' ';
        }

        // Add a new symbol to table
        private void AddEntry(char name, char T)
        {
            if (InTable(name)) Duplicate(name.ToString());
            ST[name - 'A'] = T;
        }

        // Check an entry to make sure it's a variable
        private void CheckVar(char name)
        {
            if (!InTable(name)) Undefined(name.ToString());
            if (TypeOf(name) != 'v') Abort(name + " is not a variable");
        }

        // Get an identifier
        private char GetName()
        {
            if (!IsAlpha(Look)) Expected("Name");
            char name = char.ToUpper(Look);
            GetChar();
            SkipWhite();
            return name;
        }

        // Get a number
        private char GetNum() {
            if (!IsDigit(Look)) Expected("Integer");
            char value = Look;
            GetChar();
            SkipWhite();
            return value;
        }

        // Post a label to output
        private void PostLabel(string L)
        {
            EmitLn(L + ":");
        }

        // Load a variable to the primary register
        private void LoadVar(char name)
        {
            CheckVar(name);
            EmitLn("MOVE " + name + "(PC),D0");
        }

        // Store the primary register
        private void StoreVar(char name)
        {
            CheckVar(name);
            EmitLn("LEA " + name + "(PC),A0");
            EmitLn("MOVE D0,(A0)");
        }

        // Parse and translate an expression
        // Vestigial version
        private void Expression()
        {
            char name = GetName();
            if (IsParam(name))
                LoadParam(ParamNumber(name));
            else
                LoadVar(GetName());
        }

        // Parse and translate an assignment statement
        private void Assignment(char name)
        {
            Match('=');
            Expression();
            if (IsParam(name))
                StoreParam(ParamNumber(name));
            else
                StoreVar(name);
        }

        // Call a procedure
        private void Call(char n)
        {
            EmitLn("BSR " + n);
        }

        // Process a formal parameter
        private void FormalParam()
        {
            AddParam(GetName());
        }

        // Process an actual parameter
        private void Param()
        {
            Expression();
            Push();
        }

        // Process the formal parameter list of a procedure
        private void FormalList()
        {
            Match('(');
            if (Look != ')')
            {
                FormalParam();
                while (Look == ',')
                {
                    Match(',');
                    FormalParam();
                }
            }
            Match(')');
        }

        // Process the parameter list for a procedure
        private int ParamList()
        {
            int n = 0;
            Match('(');
            if (Look != ')')
            {
                Param();
                ++n;
                while (Look == ',')
                {
                    Match(',');
                    Param();
                    ++n;
                }
            }
            Match(')');
            return 2 * n;
        }

        // Adjust the stack pointer upwards by n bytes 
        private void CleanStack(int n)
        {
            if (n > 0)
            {
                EmitLn("ADD #" + n + ",SP");
            }
        }

        // Process a procedure call
        private void CallProc(char name)
        {
            int n = ParamList();
            Call(name);
            CleanStack(n);
        }

        // Decide if a statement is an assignment or procedure call
        private void AssignOrProc()
        {
            char name = GetName();
            switch (TypeOf(name))
            {
                case ' ': Undefined(name.ToString()); break;
                case 'v': goto case 'f';
                case 'f': Assignment(name); break;
                case 'p': CallProc(name); break;
                default: Abort("Identifier " + name + " cannot be used here"); break;
            }
        }

        // Parse and translate a block of statements
        private void DoBlock()
        {
            while ("e".IndexOf(Look) == -1)
            {
                AssignOrProc();
                Fin();
            }
        }
        
        // Parse and translate a begin block
        private void BeginBlock()
        {
            Match('b');
            Fin();
            DoBlock();
            Match('e');
            Fin();
        }

        // Allocate storage for a variable
        private void Alloc(char n)
        {
            if (InTable(n)) Duplicate(n.ToString());
            ST[n - 'A'] = 'v';
            EmitLn(n + ":" + TAB + "DC 0");
        }

        // Parse and translate a data declaration
        private void Decl() {
            Match('v');
            Alloc(GetName());
        }

        private void Return()
        {
            EmitLn("RTS");
        }

        private void Prolog()
        {
            PostLabel("MAIN");
        }

        private void Epilog()
        {
            EmitLn("DC WARMST");
            EmitLn("END MAIN");
        }

        // Write the prolog for a procedure
        private void ProcProlog(char n)
        {
            PostLabel(n);
            EmitLn("LINK A6,#0");
        }

        // Write the epilog for a procedure
        private void ProcEpilog()
        {
            EmitLn("UNLK A6");
            EmitLn("RTS");
        }

        // Parse and translate a main program
        private void DoMain()
        {
            Match('P');
            char name = GetName();
            Fin();
            if (InTable(name)) Duplicate(name.ToString());
            Prolog();
            BeginBlock();
        }

        // Parse and translate a procedure declaration
        private void DoProc()
        {
            Match('p');
            char name = GetName();
            FormalList();
            Fin();
            if (InTable(name)) Duplicate(name.ToString());
            ST[name - 'A'] = 'p';
            ProcProlog(n);
            BeginBlock();
            ProcEpilog();
            ClearParams();
        }

        // Parse and translate global declarations
        private void TopDecls()
        {
            while (Look != '.')
            {
                switch (Look)
                {
                    case 'v': Decl(); break;
                    case 'p': DoProc(); break;
                    case 'P': DoMain(); break;
                    default: Abort("Unrecognized Keyword " + Look); break;
                }
                Fin();
            }
        }

        public Compiler()
        {
            GetChar();
            SkipWhite();
            
            ST = new char[26];
            for (char c = 'A'; c <= 'Z'; ++c)
            {
                ST[c - 'A'] = ' ';
            }

            Params = new int[26];
            ClearParams();


            TopDecls();
            Epilog();
        }
    }
}