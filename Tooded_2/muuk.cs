using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tooded_2
{
    public class Muuk
    {
        [Key]
        public int Id { get; set; }

        public DateTime Aeg { get; set; }

        public int ToodeId { get; set; }

        public string Toodenimetus { get; set; } = "";

        public int Kogus { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Hind { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Summa { get; set; }
    }
}