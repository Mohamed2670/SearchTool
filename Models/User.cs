using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using ServerSide.Model;
namespace SearchTool_ServerSide.Models
{
    public class User : IEntity
    {
        public int Id { get; set; }
        public required string Email { get; set; } // No encryption here

        private string _shortName;
        public required string ShortName
        {
            get => Decrypt(_shortName);
            set => _shortName = Encrypt(value);
        }

        public required string Name { get; set; }
        public required string Password { get; set; }
        public int BranchId { get; set; }
        public ICollection<Log> Logs { get; set; }
        public ICollection<SearchDrugDetailsLogs> SearchDrugDetailsLogs { get; set; } = new List<SearchDrugDetailsLogs>();

        public Branch Branch { get; set; }
        public Role Role { get; set; } = Role.Pharmacist;

        // Store your key/IV securely in production!
        private const string KeyName = "b8Qw7n2Jx4Vt6z1Lr5Yp9s3Dk0Wc8e2H";
        private const string IVName = "A1d2F3g4H5j6K7l8";
        private static readonly byte[] Key = Encoding.UTF8.GetBytes(KeyName);
        private static readonly byte[] IV = Encoding.UTF8.GetBytes(IVName);

        private string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return plainText;
            using var aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;
            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var encrypted = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
            return Convert.ToBase64String(encrypted);
        }

        private string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return cipherText;
            using var aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;
            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            var cipherBytes = Convert.FromBase64String(cipherText);
            var decrypted = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            return Encoding.UTF8.GetString(decrypted);
        }
    }



}