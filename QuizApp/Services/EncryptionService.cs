using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace QuizApp.Services
{
    public static class EncryptionService
    {
        private const string Salt = "SaltValue123"; // Stała wartość soli (możesz zmienić na bardziej unikalną)

        public static void EncryptToFile(string filePath, string data, string password)
        {
            using (var aes = Aes.Create())
            {
                var key = GenerateKey(password);
                aes.Key = key.Key;
                aes.IV = key.IV;

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                using (var cryptoStream = new CryptoStream(fileStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                using (var writer = new StreamWriter(cryptoStream))
                {
                    writer.Write(data);
                }
            }
        }

        public static string DecryptFromFile(string filePath, string password)
        {
            using (var aes = Aes.Create())
            {
                var key = GenerateKey(password);
                aes.Key = key.Key;
                aes.IV = key.IV;

                using (var fileStream = new FileStream(filePath, FileMode.Open))
                using (var cryptoStream = new CryptoStream(fileStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
                using (var reader = new StreamReader(cryptoStream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private static (byte[] Key, byte[] IV) GenerateKey(string password)
        {
            using (var keyGenerator = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes(Salt), 10000))
            {
                return (keyGenerator.GetBytes(32), keyGenerator.GetBytes(16)); // 256-bitowy klucz i 128-bitowy IV
            }
        }
    }
}
