using System.ComponentModel.DataAnnotations;

namespace Access.Models.TimeSheet
{
    public class TimeSheetModel
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
        [Range(0, 24)]
        public int WorkingHours { get; set; }
    }
}
