//namespace crypto
//{
//    /// <summary>
//    /// mainly used for debugging purposes as the keys are now stored within a dictionary
//    /// </summary>
//    public static class InputOutput
//    {
//        public static async Task StoreKey(Dictionary<char, byte[]> keyDict)
//        {

//            using StreamWriter file = new("KeyPair.txt");

//            foreach (var key in keyDict)
//            {
//                await file.WriteAsync($"{key.Key.ToString()}: {Convert.ToBase64String(key.Value)}\n");

//            }
//        }

//        public static async Task StoreCipher(string cipher)
//        {

//            using StreamWriter file = new("KeyPair.txt");

//            await file.WriteAsync($"{cipher}");
//        }


//        public static Dictionary<char, byte[]> ReadKey()
//        {
//            Dictionary<char, byte[]> keyDict = new Dictionary<char, byte[]>();
//            foreach (string line in System.IO.File.ReadLines("KeyPair.txt"))
//            {
//                if (line.StartsWith('e'))
//                    keyDict.Add('e', Convert.FromBase64String(line.Substring(3)));
//                else if (line.StartsWith('d'))
//                    keyDict.Add('d', Convert.FromBase64String(line.Substring(3)));
//                else if (line.StartsWith('N'))
//                    keyDict.Add('N', Convert.FromBase64String(line.Substring(3)));

//            }
//            return keyDict;
//        }
//    }
//}