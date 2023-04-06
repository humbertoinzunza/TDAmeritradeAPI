using System.Security.Cryptography;

namespace TDAmeritradeAPI
{
    /// <summary>
    ///  This class provides a simplified way to encrypt and decrypt files.
    ///  It uses the 128-bit AES encryption.
    /// </summary>
    public static class Cryptography
    {
        /// <summary>
        ///  Writes a byte array into an encrypted file.
        /// </summary>
        /// <param name="encryptedFilename">The filename where the data will be written.</param>
        /// <param name="data">The data.</param>
        /// <param name="encryptionKeyFilename">A file path where the encryption key is found.
        /// If the file doesn't exist a new key will be generated and stored.</param>
        public static void Write(string encryptedFilename, string data, string encryptionKeyFilename)
        {
            try
            {
                using FileStream fileStream = new(encryptedFilename, FileMode.OpenOrCreate);
                using Aes aes = Aes.Create();

                aes.KeySize = 128;
                byte[] key = new byte[16];
                // Check if the key file exists.
                if (File.Exists(encryptionKeyFilename))
                {
                    using FileStream keyFileStream = new(encryptionKeyFilename, FileMode.Open, FileAccess.Read);
                    // Read the 32 bytes from the key file.
                    keyFileStream.Read(key, 0, 16);
                    // Set the key on the AES object.
                    aes.Key = key;
                }
                // The file doesn't exist
                else
                {
                    // Generate a new key
                    aes.GenerateKey();
                    using FileStream keyFileStream = new(encryptionKeyFilename, FileMode.OpenOrCreate, FileAccess.Write);
                    // Write the 32 bytes to the key file.
                    keyFileStream.Write(aes.Key, 0, 16);
                }

                // Get initial value
                byte[] iv = aes.IV;
                fileStream.Write(iv, 0, iv.Length);

                using CryptoStream cryptoStream = new(fileStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
                // Use a StreamWriter to write the string data instead of a buffer of bytes
                using StreamWriter encryptWriter = new(cryptoStream);
                encryptWriter.Write(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"The encryption failed. {ex}");
            }
        }

        /// <summary>
        ///  Reads an encrypted file using a key stored in a file.
        /// </summary>
        /// <param name="encryptedFilename">The filename where the data will be written.</param>
        /// <param name="encryptionKeyFilename">A file path where the encryption key is found.
        /// If the file doesn't exist a new key will be generated and stored.</param>
        /// <returns>A string containing the decrypted file.</returns>
        public static string? Read(string encryptedFilename, string encryptionKeyFilename)
        {
            try
            {
                using FileStream fileStream = new(encryptedFilename, FileMode.Open, FileAccess.Read);
                using Aes aes = Aes.Create();

                // Read the encrypted data
                byte[] iv = new byte[aes.IV.Length];
                fileStream.Read(iv, 0, iv.Length);

                // Get the key stored in the file
                byte[] key = new byte[16];
                using FileStream fs = new(encryptionKeyFilename, FileMode.Open, FileAccess.Read);
                fs.Read(key, 0, 16);

                using CryptoStream cryptoStream = new(fileStream, aes.CreateDecryptor(key, iv), CryptoStreamMode.Read);
                {
                    using StreamReader decryptReader = new(cryptoStream);
                    return decryptReader.ReadToEnd();
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("The decryption failed. File not found.");
                // Return empty array due to unsuccessfull decryption process
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"The decryption failed. {ex}");
                // Return empty array due to unsuccessfull decryption process
                return null;
            }
        }
    }
}
