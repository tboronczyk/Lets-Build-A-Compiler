using System;

namespace LBC
{
    class Cpu68000 : CPU
    {
        private const char TAB = '\t';

        // Output a string with tab
        private void Emit(string s)
        {
            Console.Write(TAB + s);
        }

        // Output a string with tab and newline
        private void EmitLn(string s)
        {
            Console.WriteLine(TAB + s);
        }

        public override void Header()
        {
            EmitLn("WARMST" + TAB + "EQU $A01E");
        }

        public override void Prolog()
        {
            PostLabel("MAIN");
        }

        public override void Epilog()
        {
            EmitLn("DC WARMST");
            EmitLn("END MAIN");
        }

        public override void Alloc(char name, int value)
        {
            EmitLn(name + ":" + TAB + "DC " + value);
        }

        public override void Store(char name)
        {
            EmitLn("LEA " + name + "(PC),A0");
            EmitLn("MOVE D0,(A0)");
        }

        public override void PostLabel(string label)
        {
            EmitLn(label + ":");
        }

        public override void LoadConst(int value)
        {
            EmitLn("MOVE #" + value + ",D0");
        }

        public override void LoadVar(char name)
        {
            EmitLn("MOVE " + name + "(PC),D0");
        }

        public override void Clear()
        {
            EmitLn("CLR D0");
        }

        public override void Negate()
        {
            EmitLn("NEG D0");
        }

        public override void NotIt()
        {
            EmitLn("NOT D0");
        }

        public override void Push()
        {
            EmitLn("MOVE D0,-(SP)");
        }


        public override void PopAdd()
        {
            EmitLn("ADD (SP)+,D0");
        }

        public override void PopSub()
        {
            EmitLn("SUB (SP)+,D0");
        }

        public override void PopMul()
        {
            EmitLn("MULS (SP)+,D0");
        }

        public override void PopDiv()
        {
            EmitLn("MOVE (SP)+,D7");
            EmitLn("EXT.L D7");
            EmitLn("DIVS D0,D7");
            EmitLn("MOVE D7,D0");
        }

        public override void PopAnd()
        {
            EmitLn("AND (SP)+,D0");
        }

        public override void PopOr()
        {
            EmitLn("OR (SP)+,D0");
        }

        public override void PopXor()
        {
            EmitLn("EOR (SP)+,D0");
        }

        public override void PopCompare()
        {
            EmitLn("CMP (SP)+,D0");
        }

        public override void SetEqual()
        {
            EmitLn("SEQ D0");
            EmitLn("EXT D0");
        }

        public override void SetNEqual()
        {
            EmitLn("SNE D0");
            EmitLn("EXT D0");
        }

        public override void SetGreater()
        {
            EmitLn("SLT D0");
            EmitLn("EXT D0");
        }

        public override void SetLess()
        {
            EmitLn("SGT D0");
            EmitLn("EXT D0");
        }

        public override void Branch(string label)
        {
            EmitLn("BRA " + label);
        }

        public override void BranchFalse(string label)
        {
            EmitLn("TST D0");
            EmitLn("BEQ " + label);
        }
    }
}
