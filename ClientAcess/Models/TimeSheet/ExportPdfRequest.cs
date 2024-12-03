namespace Access.Models.TimeSheet
{
    public class ExportPdfRequest
    {
        public string Name { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public Dictionary<string, WorkDayData> WorkingHours { get; set; }
    }
}
