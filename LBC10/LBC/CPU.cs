namespace LBC
{
    abstract class CPU
    {
        // Write header info
        public abstract void Header();

        // Write the prolog
        public abstract void Prolog();

        // Write the epilog
        public abstract void Epilog();

        // Allocate storage for a variable
        public abstract void Alloc(char name, int value);

        // store primary to variable
        public abstract void Store(char name);

        // Post a label to output
        public abstract void PostLabel(string label);

        // Load a constant value to primary register
        public abstract void LoadConst(int value);

        // Load a variable to primary register
        public abstract void LoadVar(char name);

        // Clear the primary register
        public abstract void Clear();

        // Negate the primary register
        public abstract void Negate();

        // Complement the primary register
        public abstract void NotIt();

        // Push primary onto stack
        public abstract void Push();

        // Add top of stack to primary
        public abstract void PopAdd();

        // Subtract primary from top of stack
        public abstract void PopSub();

        // Multiply top of stack by primary
        public abstract void PopMul();

        // Divide top of stack by primary
        public abstract void PopDiv();

        // And top of stack with primary
        public abstract void PopAnd();

        // Or top of stack wih primary
        public abstract void PopOr();

        // XOR top of stack with primary
        public abstract void PopXor();

        // Compare top of stack with primary
        public abstract void PopCompare();

        // Set primary if compare was =
        public abstract void SetEqual();

        // Set primary if compare was !=
        public abstract void SetNEqual();

        // Set primary if compare was >
        public abstract void SetGreater();

        // Set primary if compare was <
        public abstract void SetLess();

        // Branch unconditional
        public abstract void Branch(string label);

        // Branch false
        public abstract void BranchFalse(string label);
    }
}
