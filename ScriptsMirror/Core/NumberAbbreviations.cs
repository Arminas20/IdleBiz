namespace IdleBiz.Core
{
    public static class NumberAbbreviations
    {
        private static readonly (double threshold, string suffix)[] steps = new[]
        {
            (1_000_000_000d, "B"),
            (1_000_000d, "M"),
            (1_000d, "K"),
        };

        public static string Format(double value)
        {
            double abs = System.Math.Abs(value);
            foreach (var (t, s) in steps)
                if (abs >= t) return (value / t).ToString("0.##") + s;

            return value.ToString("0.##");
        }
    }
}
