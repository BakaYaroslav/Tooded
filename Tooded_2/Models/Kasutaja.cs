using System.ComponentModel.DataAnnotations;

namespace Tooded_2;
public class Kasutaja
{
    [Key]
    public int Id { get; set; }
    public string Kasutajanimi { get; set; } = "";
    public string ParoolHash { get; set; } = "";
    public string Roll { get; set; } = "Klient"; // "Admin" või "Klient"
}
