namespace EFaturaAktarim
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    public static class MD5Hash
    {
        private static string ByteToHex(byte[] data)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                builder.Append(data[i].ToString("x2"));
            }
            return builder.ToString();
        }

        public static string GetMD5Hash(byte[] buffer)
        {
            MD5 md = new MD5CryptoServiceProvider();
            return ByteToHex(md.ComputeHash(buffer));
        }

        public static string GetMD5Hash(FileStream file)
        {
            MD5 md = new MD5CryptoServiceProvider();
            return ByteToHex(md.ComputeHash(file));
        }

        public static string GetMD5Hash(string filePath)
        {
            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                return GetMD5Hash(stream);
            }
        }

        public static bool VerifyMd5Hash(string filePath, string hash)
        {
            string x = GetMD5Hash(filePath);
            return (0 == StringComparer.OrdinalIgnoreCase.Compare(x, hash));
        }

        public static bool VerifyMD5Hash(byte[] buffer, string hash)
        {
            string x = GetMD5Hash(buffer);
            return (0 == StringComparer.OrdinalIgnoreCase.Compare(x, hash));
        }

        public static bool VerifyMD5Hash(FileStream file, string hash)
        {
            string x = GetMD5Hash(file);
            return (0 == StringComparer.OrdinalIgnoreCase.Compare(x, hash));
        }
    }
}

