using System.Numerics;

namespace crypto
{
    public class PrimalityTests
    {
        /// <summary>
        /// Returns a modpow for a biginteger type (same as the int.ModPow but for a bigInteger)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="modulus"></param>
        /// <returns></returns>
        private static BigInteger power(BigInteger x, BigInteger y, BigInteger modulus)
        {

            BigInteger res = 1; // Initialize result

            // Update x if it is more than
            // or equal to p
            x = x % modulus;

            while (y > 0)
            {

                // If y is odd, multiply x with result
                if ((y & 1) == 1)
                    res = (res * x) % modulus;

                // y must be even now
                y = y >> 1; // y = y/2
                x = (x * x) % modulus;
            }

            return res;
        }

        // This function is called for all k trials.
        // It returns false if n is composite and
        // returns false if n is probably prime.
        // d is an odd number such that d*2
        // = n-1 for some r >= 1
        public static bool millerTest(BigInteger d, BigInteger n)
        {

            // Pick a random number in [2..n-2]
            // Corner cases make sure that n > 4
            Random r = new Random();
            BigInteger a = 2 + (BigInteger)(r.Next() % (n - 4));

            // Compute a^d % n
            //BigInteger x = BigInteger.ModPow(a,d,n);
            BigInteger x = power(a, d, n);

            if (x == 1 || x == n - 1)
                return true;

            // Keep squaring x while:
            // (i) d does not reach n-1
            // (ii) (x^2) % n is not 1
            // (iii) (x^2) % n is not n-1
            while (d != n - 1)
            {
                x = (x * x) % n;
                d *= 2;

                if (x == 1)
                    return false;
                if (x == n - 1)
                    return true;
            }

            // Return composite
            return false;
        }

        // false: n is composite
        // true: n is probably prime
        // k determines accuracy level. Higher
        // k indicates more accuracy due to more iterations.

        /// <summary>
        /// Determine whether or not a number is prime using primality tests to test whether a number is composite
        /// </summary>
        /// <param name="n"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        public static bool IsPrime(BigInteger n, BigInteger k)
        {

            // Corner cases
            if (n <= 1 || n == 4)
                return false;
            if (n <= 3)
                return true;

            // Find r such that n = 2^d * r + 1
            // when r >= 1
            BigInteger d = n - 1;

            while (d % 2 == 0) //check if n is odd because n - 1 should be an even number
            {
                d /= 2;
            }



            //iterate Miller-Rabin k number of times
            for (BigInteger i = 0; i < k; i++)
            {
                if (millerTest(d, n) == false)
                {
                    return false;
                }

            }

            return true;
        }
    }
}