using System.ComponentModel.DataAnnotations;

namespace ClientAcess.Models.Kms
{
    public class KmsModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [Range(1, 31)]
        public int Day { get; set; }

        [Required]
        [Range(1, 12)]
        public int Month { get; set; }

        [Required]
        public int Year { get; set; }

        [Required]
        public string Departure { get; set; }

        [Required]
        public string Arrive { get; set; }

        [Required]
        public string Justification { get; set; }

        [Required]
        [Range(0, 1000)]
        public int Kms { get; set; }
    }
}
