using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Globalization;
using System.Reflection.Metadata;


namespace vacations.Controllers
{
	public class CalendarController : Controller
	{
		public IActionResult Index()
		{
			int currentYear = DateTime.Now.Year;
			return View(currentYear);
		}

        [HttpPost]
        public IActionResult SaveSelection(List<string> selectedDays)
        {
            if (selectedDays == null || selectedDays.Count == 0)
            {
                TempData["Error"] = "No days were selected.";
                return RedirectToAction("Index");
            }

            try
            {
                // Create the PDF document in a memory stream
                using (var stream = new MemoryStream())
                {
                    // Create a new PDF document
                    iTextSharp.text.Document pdfDoc = new iTextSharp.text.Document(PageSize.A4, 25, 25, 30, 30);
                    PdfWriter writer = PdfWriter.GetInstance(pdfDoc, stream);
                    pdfDoc.Open();

                    // Add title
                    var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                    pdfDoc.Add(new Paragraph("Vacations", titleFont) { Alignment = Element.ALIGN_CENTER });

                    // Add a line break
                    pdfDoc.Add(new Paragraph("\n"));

                    // Add the selected days
                    var textFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);
                    foreach (var day in selectedDays)
                    {
                        DateTime formattedDate = DateTime.Parse(day); // Parse the day string into a DateTime
                        string formattedDay = formattedDate.ToString("dd MMMM yyyy"); // Format the date
                        pdfDoc.Add(new Paragraph(formattedDay, textFont));
                    }

                    pdfDoc.Close();

                    // Return the PDF file as a downloadable response
                    //stream.Position = 0;
                    return File(stream.ToArray(), "application/pdf", "SelectedDays.pdf");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred while generating the PDF: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
    }
}
