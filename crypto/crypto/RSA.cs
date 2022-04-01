using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace crypto
{
    public static class RSA
    {
        /// <summary>
        /// Checks whether a number is coprime and returns a boolean result
        /// </summary>
        /// <param name="p"></param>
        /// <param name="q"></param>
        /// <returns></returns>
        private static bool IsCoPrime(BigInteger p, BigInteger q)
        {

            //returns true if GCD of p and q is 1 (relatively prime)
            if (GetGCD(p, q) == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Obtains the greatest common denominator of two integers
        /// </summary>
        /// <param name="p"></param>
        /// <param name="q"></param>
        /// <returns></returns>
        private static BigInteger GetGCD(BigInteger p, BigInteger q)
        {
            //euclidian algorithm
            if (q == 0)
                return p;
            else
                return GetGCD(q, p % q);
        }

        /// <summary>
        /// Converts a string of ASCII text to a large integer value
        /// </summary>
        /// <param name="plaintext"></param>
        /// <returns></returns>
        private static BigInteger StringToBigInt(string plaintext)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(plaintext);
            BigInteger bigInteger = new BigInteger(bytes.Concat(new byte[] { 0 }).ToArray());
            return bigInteger;
        }

        /// <summary>
        /// Converts a large integer to a string of ASCII text
        /// </summary>
        /// <param name="bigInteger"></param>
        /// <returns></returns>
        private static string BigIntToString(BigInteger bigInteger)
        {
            byte[] byteArray = bigInteger.ToByteArray();
            string str = Encoding.ASCII.GetString(byteArray);
            Console.WriteLine(str);
            return str;
        }

        /// <summary>
        /// Generate a large and random prime number using MillerRabin primality tests
        /// </summary>
        /// <param name="keySize"></param>
        /// <returns></returns>
        private static BigInteger GenerateLargePrime(int keySize)
        {
            //return random large prime of keysize bits in size
            while (true)
            {
                byte[] bytes = RandomNumberGenerator.GetBytes(keySize / 8);
                //var rng = new RNGCryptoServiceProvider();
                //byte[] bytes = new byte[keySize / 8];
                //rng.GetBytes(bytes);
                BigInteger largeRandom = new BigInteger(bytes);
                if (PrimalityTests.IsPrime(largeRandom, 40))
                {
                    return largeRandom;
                }
            }
        }

        /// <summary>
        /// Generate public and private keys and return a keypair
        /// </summary>
        /// <param name="keySize"></param>
        /// <returns></returns>
        public static Dictionary<char, byte[]> GenerateKeys(int keySize)
        {
            BigInteger e, d, N;

            e = d = N = 0;

            //generate large primes p & q
            BigInteger p = GenerateLargePrime(keySize);
            BigInteger q = GenerateLargePrime(keySize);


            N = p * q; //RSA modulus
            BigInteger phiN = (p - 1) * (q - 1); //totient rule

            //choose e, e is coprime with phiN and 1 < e <= phiN
            while (true)
            {

                BigInteger largeRandom = GenerateLargePrime(keySize);
                if (IsCoPrime(largeRandom, phiN) && largeRandom > 0)
                {
                    e = largeRandom;
                    break;
                }
            }
            //choose d, d is mod inverse of e with respect to phiN, e*d (mod phiN) = 1
            d = ModInverse(e, phiN);
            Console.WriteLine($"Keys have been generated\n\np: {p}\n\nq: {q}\n\ne (public): {e}\n\nd (private): {d}\n\nN: {N}");
            Dictionary<char, byte[]> keyDict = new Dictionary<char, byte[]>()
            {
                {'e',e.ToByteArray()},
                {'d',d.ToByteArray()},
                {'N',N.ToByteArray()}
            };
            return (keyDict);
        }

        /// <summary>
        /// find the multiplicative modular inverse of an integer given a second integer
        /// </summary>
        /// <param name="e"></param>
        /// <param name="phiN"></param>
        /// <returns></returns>
        private static BigInteger ModInverse(BigInteger e, BigInteger phiN)
        {
            BigInteger m0 = phiN;
            BigInteger y = 0, x = 1;

            if (phiN == 1)
                return 0;

            while (e > 1)
            {
                // q is quotient
                BigInteger q = e / phiN;

                BigInteger t = phiN;

                // m is remainder now, process
                // same as Euclid's algo
                phiN = e % phiN;
                e = t;
                t = y;

                // Update x and y
                y = x - q * y;
                x = t;
            }

            // Make x positive
            if (x < 0)
                x += m0;

            return x;
        }


        /// <summary>
        /// Encrypt plain-data given a public key and return a cipher as an array of bytes
        /// </summary>
        /// <param name="e"></param>
        /// <param name="N"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static byte[] Encrypt(byte[] e, byte[] N, string msg)
        {
            BigInteger eInt = new BigInteger(e.Concat(new byte[] { 0 }).ToArray());
            BigInteger NInt = new BigInteger(N.Concat(new byte[] { 0 }).ToArray());
            BigInteger msgInt = StringToBigInt(msg);
            BigInteger tempInt = BigInteger.ModPow(msgInt, eInt, NInt);
            byte[] bytes = tempInt.ToByteArray();
            return bytes;

        }

        /// <summary>
        /// Decrypt an array of bytes given a private key and return a string of plain-data     
        /// </summary>
        /// <param name="d"></param>
        /// <param name="N"></param>
        /// <param name="cipher"></param>
        /// <returns></returns>
        public static string Decrypt(byte[] d, byte[] N, byte[] cipher)
        {

            BigInteger dInt = new BigInteger(d.Concat(new byte[] { 0 }).ToArray());
            BigInteger NInt = new BigInteger(N.Concat(new byte[] { 0 }).ToArray());
            BigInteger bigInteger = new BigInteger(cipher);
            string msg = BigIntToString((BigInteger.ModPow(bigInteger, dInt, NInt)));

            return msg;
        }
    }
}