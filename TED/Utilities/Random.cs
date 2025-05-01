
using System.Collections.Generic;
using System.Linq;

namespace TED.Utilities
{
    /// <summary>
    /// Shared RNG used by al all the randomization primitives
    /// </summary>
    public static class Random
    {
        /// <summary>
        /// Shared RNG used by TED.
        /// Set the seed on this if you want deterministic behavior.
        /// </summary>
        private static System.Random Rng = new System.Random();

        /// <summary>
        /// Set the seed of the global random number generator used by TED.
        /// If this is not called, then randomization has the same behavior as System.Random.
        /// </summary>
        public static void SetGlobalSeed(int seed) => Rng = new System.Random(seed);

        /// <summary>
        /// Generate a new random number generator whose initial seed is determined by the global RNG.
        /// </summary>
        /// <returns></returns>
        public static System.Random MakeRng() => new System.Random(Rng.Next());

        /// <summary>
        /// Generate a random float
        /// </summary>
        public static float Float(this System.Random rng) => (float)rng.NextDouble();

        /// <summary>
        /// Generate a random Boolean with a specified probability of being true
        /// </summary>
        /// <param name="rng">Random number generator</param>
        /// <param name="probability">Probability of returning true.</param>
        public static bool Roll(this System.Random rng, float probability) => rng.Float() <= probability;

        /// <summary>
        /// Return a random integer from start to end-1.
        /// </summary>
        public static int InRangeExclusive(this System.Random rng, int start, int end) => start + rng.Next() % (end - start);
        
        /// <summary>
        /// Return the input sequence shuffled.
        /// </summary>
        public static T[] Shuffle<T>(this IEnumerable<T> sequence, System.Random rng) {
            var result = sequence.ToArray();
            for (var i = result.Length - 1; i > 0; i--) {
                var index = rng.Next(i + 1);
                (result[index], result[i]) = (result[i], result[index]);
            }
            return result;
        }
    }
}
