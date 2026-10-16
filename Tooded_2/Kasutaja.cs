using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace Tooded_2
{
    public class Kasutaja
    {
        [Key]
        public int Id { get; set; }
        public string Kasutajanimi { get; set; } = "";
        public string ParoolHash { get; set; } = "";
        public string Roll { get; set; } = "Klient";   // "Admin" või "Klient"
    }

    // Parooli hashimine ja kontrollimine (parooli ei hoita selgelt andmebaasis)
    public static class Parool
    {
        // Loob hashi kujul "sool:hash" (mõlemad Base64)
        public static string Hashi(string parool)
        {
            byte[] sool = RandomNumberGenerator.GetBytes(16);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                parool, sool, 100_000, HashAlgorithmName.SHA256, 32);
            return Convert.ToBase64String(sool) + ":" + Convert.ToBase64String(hash);
        }

        // Kontrollib sisestatud parooli salvestatud hashi vastu
        public static bool Kontrolli(string parool, string salvestatud)
        {
            var osad = salvestatud.Split(':');
            if (osad.Length != 2) return false;

            byte[] sool = Convert.FromBase64String(osad[0]);
            byte[] oodatud = Convert.FromBase64String(osad[1]);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                parool, sool, 100_000, HashAlgorithmName.SHA256, 32);

            return CryptographicOperations.FixedTimeEquals(hash, oodatud);
        }
    }
}