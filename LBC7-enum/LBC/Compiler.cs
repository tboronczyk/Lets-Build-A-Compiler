using System;
using System.Collections;

namespace LBC
{
    class Compiler
    {
        // Definition of keyword tokens
        private readonly string[] KWList = { "IF", "ELSE", "ENDIF", "END" };
        private enum SymType
        {
            IfSym,        ElseIfSym,    EndIfSym,     EndSym,
            Ident,        Number,       Operator
        }

        private SymType Token;     // Current token
        private string Value;      // Value of current token

        // Recognize any operator
        private bool IsOp(char c)
        {
            return "+-/*<>:=".IndexOf(c) > -1;
        }

        // Get an operator 
        private void GetOp()
        {
            Value = "";
            if (!IsOp(Cradle.Look)) Cradle.Expected("Operator");
            while (IsOp(Cradle.Look))
            {
                Value += Cradle.Look;
                Cradle.GetChar();
            }
            Token = SymType.Operator;
        }

        // Get an identifier
        private void GetName()
        {
            int k;
            Value = "";
            if (!Cradle.IsAlpha(Cradle.Look)) Cradle.Expected("Name");
            while (Cradle.IsAlNum(Cradle.Look))
            {
                Value += char.ToUpper(Cradle.Look);
                Cradle.GetChar();
            }
            k = Lookup(Value);
            if (k == -1)
                Token = SymType.Ident;
            else
                Token = (SymType)k;
        }

        // Get a number
        private void GetNum()
        {
            Value = "";
            if (!Cradle.IsDigit(Cradle.Look)) Cradle.Expected("Integer");
            while (Cradle.IsDigit(Cradle.Look))
            {
                Value += Cradle.Look;
                Cradle.GetChar();
            }
            Token = SymType.Number;
        }

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

        // Skip over a comma
        private void SkipComma()
        {
            Cradle.SkipWhite();
            if (Cradle.Look == ',')
            {
                Cradle.GetChar();
                Cradle.SkipWhite();
            }
        }

        // Lexical scanner
        private void Scan()
        {
            int k;
            while (Cradle.Look == Cradle.CR) Cradle.Fin();
            if (Cradle.IsAlpha(Cradle.Look))
            {
                GetName();
            }
            else if (Cradle.IsDigit(Cradle.Look))
            {
                GetNum();
            }
            else if (IsOp(Cradle.Look))
            {
                GetOp();
            }
            else
            {
                Value = Cradle.Look.ToString();
                Token = SymType.Operator;
                Cradle.GetChar();
            }
            Cradle.SkipWhite();
        }

        public Compiler()
        {
            do
            {
                Scan();
                switch (Token)
                {
                    case SymType.Ident: Cradle.Emit("Ident "); break;
                    case SymType.Number: Cradle.Emit("Number "); break;
                    case SymType.Operator: Cradle.Emit("Operator "); break;
                    default: Cradle.Emit("Keyword "); break;
                }
                Cradle.EmitLn(Value);
            }
            while (Token != SymType.EndSym);
        }
    }
}