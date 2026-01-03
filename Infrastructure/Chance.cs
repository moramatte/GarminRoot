using System;
using System.Collections.Generic;

namespace Infrastructure
{
    public static class Chance
    {
        private static Random random = new();
        private static Dictionary<int, Random> seededRandoms = new();

        public static int Within(int min, int max)
        {
            return random.Next(min, max);
        }

        public static int Within(int seed, int min, int max)
        {
            if (!seededRandoms.ContainsKey(seed))
            {
                seededRandoms[seed] = new Random(seed);
            }
            var seededRandom = seededRandoms[seed];
            return seededRandom.Next(min, max);
        }
    }
}