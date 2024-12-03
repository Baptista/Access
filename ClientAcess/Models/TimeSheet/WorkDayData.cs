namespace Access.Models.TimeSheet
{
    public class WorkDayData
    {
        public string DayOfWeek { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public float TotalHours { get; set; }
    }
}
