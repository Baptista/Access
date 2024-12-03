namespace Access.Models.TimeSheet
{
    public class WorkDay
    {
        public int Day { get; set; }
        public string DayOfWeek { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public float TotalHours { get; set; }
    }
}
