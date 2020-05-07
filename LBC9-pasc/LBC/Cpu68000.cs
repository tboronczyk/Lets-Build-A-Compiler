using System;
using System.Collections.Generic;
using System.Text;

namespace LBC
{
    class Cpu68000: Cradle
    {
        // Post a label to output
        public void PostLabel(char L)
        {
            EmitLn(L + ":");
        }

        // Write the prolog
        public void Prolog(char name)
        {
            EmitLn("WARMST EQU $A01E");
        }

        public void Epilog(char name)
        {
            EmitLn("DC WARMST");
            EmitLn("END " + name);
        }
    }
}
