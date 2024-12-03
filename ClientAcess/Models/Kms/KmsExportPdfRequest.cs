namespace ClientAcess.Models.Kms
{
    public class KmsExportPdfRequest
    {
        public string Name { get; set; }
        public string LicensePlate { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public List<KmsDataModel> KmsData { get; set; }
    }
}
