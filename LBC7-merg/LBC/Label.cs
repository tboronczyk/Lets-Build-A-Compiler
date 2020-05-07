namespace LBC
{
    struct Label
    {
        private static int Value;    // Label Counter

        // Generate a unique label
        public static string NewLabel()
        {
            return "L" + Value++;
        }

        // Initialize Label
        static Label()
        {
            Value = 0;
        }
    }
}
