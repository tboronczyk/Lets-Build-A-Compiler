namespace LBC
{
    struct Label
    {
        private static int labelValue;    // Label Counter

        // Generate a unique label
        public static string NewLabel()
        {
            return "L" + labelValue++;
        }

        // Initialize Label
        static Label()
        {
            labelValue = 0;
        }
    }
}
