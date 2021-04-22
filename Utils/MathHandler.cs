using System;

namespace G4Studio.Utils
{
    public static class MathHandler
    {
        public static double GetRandomNumber(double minimum, double maximum)
        {
            Random random = new Random();
            return random.NextDouble() * (maximum - minimum) + minimum;
        }

        public static double GetRandomNumber(int minimum, int maximum)
        {
            Random random = new Random();
            return Math.Round(random.NextDouble() * (maximum - minimum) + minimum, 0);
        }
    }
}
