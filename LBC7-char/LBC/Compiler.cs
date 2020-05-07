using System;
using System.Collections;

namespace LBC
{
    class Compiler
    {
        // Definition of keyword tokens
        private readonly string[] KWList = { "IF", "ELSE", "ENDIF", "END" };
        private const string KWCode = "xilee";

        private char Token;        // Current token
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
            if (Value.Length == 1)
                Token = Value[0];
            else
                Token = '?';
        }

        // Get an identifier
        private void GetName()
        {
            Value = "";
            if (!Cradle.IsAlpha(Cradle.Look)) Cradle.Expected("Name");
            while (Cradle.IsAlNum(Cradle.Look))
            {
                Value += char.ToUpper(Cradle.Look);
                Cradle.GetChar();
            }
            Token = KWCode[Lookup(Value) + 1];
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
            Token = '#';
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
                Token = '?';
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
                    case 'x': Cradle.Emit("Ident "); break;
                    case '#': Cradle.Emit("Number "); break;
                    case 'i': Cradle.Emit("Keyword "); break;
                    case 'e': Cradle.Emit("Keyword "); break;
                    case 'l': Cradle.Emit("Keyword "); break;
                    default: Cradle.Emit("Operator "); break;
                }
                Cradle.EmitLn(Value);
            }
            while (Value != "END");
        }
    }
}