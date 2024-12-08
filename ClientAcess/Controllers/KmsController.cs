using Access.Models.TimeSheet;
using ClientAcess.Models.Kms;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace ClientAcess.Controllers
{
    public class KmsController : Controller
    {
        private readonly HttpClient _httpClient;
        public KmsController(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://accessapi.keydevteam.com/api/authentication/");
        }
        public async Task<IActionResult> Index()
        {
            Request.Cookies.TryGetValue("jwtToken", out var token);
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.GetAsync("ValidateToken");
                if (!response.IsSuccessStatusCode)
                {
                    Response.Cookies.Append("jwtToken", "", new CookieOptions
                    {
                        Expires = DateTime.UtcNow.AddDays(-1) // Expire the cookie
                    });
                    return RedirectToAction("Login", "Account");
                }

            }

            var viewModel = new KmsModel
            {
                Year = DateTime.Now.Year,
                Month = DateTime.Now.Month
            };
            return View(viewModel);
        }

        public IActionResult ExportToPdf([FromBody] KmsExportPdfRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Name) || string.IsNullOrEmpty(request.LicensePlate) || request.Year <= 0 || request.Month <= 0 || request.Month > 12)
            {
                return BadRequest(new { success = false, message = "Invalid input data." });
            }

            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                    PdfWriter writer = PdfWriter.GetInstance(document, ms);

                    document.Open();

                    BaseColor greenColor = new BaseColor(55, 106, 101);
                    BaseColor yellowColor = new BaseColor(218, 165, 32);

                    // Add green rectangle at the top
                    PdfContentByte canvas = writer.DirectContent;
                    canvas.SetColorFill(greenColor);
                    canvas.Rectangle(0, document.PageSize.Height - 20, document.PageSize.Width, 20);
                    canvas.Fill();

                    // Add yellow rectangle cut on the left and aligned to the right
                    float rectHeight = 20;
                    float rectWidth = document.PageSize.Width - 180;
                    float xPosRight = document.PageSize.Width - rectWidth;

                    canvas.SetColorFill(yellowColor);
                    canvas.MoveTo(xPosRight, document.PageSize.Height - rectHeight);
                    canvas.LineTo(document.PageSize.Width, document.PageSize.Height - rectHeight);
                    canvas.LineTo(document.PageSize.Width, document.PageSize.Height - rectHeight - 40);
                    canvas.LineTo(xPosRight + 60, document.PageSize.Height - rectHeight - 40);
                    canvas.LineTo(xPosRight, document.PageSize.Height - rectHeight);
                    canvas.Fill();

                    // Add logo
                    iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance("./wwwroot/img/logo.png");
                    logo.SetAbsolutePosition(10, document.PageSize.Height - 60);
                    logo.ScaleAbsolute(30, 30);
                    document.Add(logo);

                    Font font = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20, greenColor);
                    Paragraph text = new Paragraph("KEYDEVTEAM", font);
                    text.Alignment = Element.ALIGN_LEFT;
                    text.IndentationLeft = 20f;
                    text.SpacingBefore = -8f;
                    document.Add(text);

                    canvas.SetColorFill(BaseColor.BLACK);

                    // Add header information
                    PdfPTable headerTable = new PdfPTable(2);
                    headerTable.WidthPercentage = 100;
                    headerTable.SetWidths(new float[] { 2f, 1f });

                    // Left side: Company name and VAT
                    PdfPCell leftCell = new PdfPCell(new Phrase("KeyDevTeam Unipessoal Lda 517824370", FontFactory.GetFont(FontFactory.HELVETICA, 10)));
                    leftCell.Border = Rectangle.NO_BORDER;
                    leftCell.HorizontalAlignment = Element.ALIGN_LEFT;
                    headerTable.AddCell(leftCell);

                    // Right side: Year and Month
                    PdfPCell rightCell = new PdfPCell(new Phrase($"{request.Year} - {new DateTime(request.Year, request.Month, 1):MMMM}", FontFactory.GetFont(FontFactory.HELVETICA, 10)));
                    rightCell.Border = Rectangle.NO_BORDER;
                    rightCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    headerTable.AddCell(rightCell);

                    //document.Add(headerTable);

                    // Add employee name
                    Font nameFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
                    Paragraph nameParagraph = new Paragraph(request.Name, nameFont);
                    nameParagraph.Alignment = Element.ALIGN_CENTER;
                    nameParagraph.SpacingBefore = 10f;
                    nameParagraph.SpacingAfter = 10f;
                    document.Add(nameParagraph);

                    // Add license plate
                    Font licensePlateFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);
                    Paragraph licensePlateParagraph = new Paragraph($"License Plate: {request.LicensePlate}", licensePlateFont);
                    licensePlateParagraph.Alignment = Element.ALIGN_CENTER;
                    licensePlateParagraph.SpacingAfter = 20f;
                    document.Add(licensePlateParagraph);

                    // Create table
                    PdfPTable table = new PdfPTable(6);
                    table.WidthPercentage = 100;
                    table.SetWidths(new float[] { 1f, 2f, 2f, 2f, 2f, 1f });

                    // Add table headers
                    string[] headers = { "Day", "Day of Week", "Departure", "Arrive", "Justification", "Kms" };
                    foreach (string header in headers)
                    {
                        PdfPCell cell = new PdfPCell(new Phrase(header, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12)));
                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                        cell.Padding = 5;
                        table.AddCell(cell);
                    }

                    // Add days and consultancy data
                    int totalKms = 0;
                    foreach (var entry in request.KmsData)
                    {
                        table.AddCell(new PdfPCell(new Phrase(entry.Day.ToString(), FontFactory.GetFont(FontFactory.HELVETICA, 10))) { HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });
                        table.AddCell(new PdfPCell(new Phrase(entry.DayOfWeek, FontFactory.GetFont(FontFactory.HELVETICA, 10))) { HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });
                        table.AddCell(new PdfPCell(new Phrase(entry.Departure, FontFactory.GetFont(FontFactory.HELVETICA, 10))) { HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });
                        table.AddCell(new PdfPCell(new Phrase(entry.Arrive, FontFactory.GetFont(FontFactory.HELVETICA, 10))) { HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });
                        table.AddCell(new PdfPCell(new Phrase(entry.Justification, FontFactory.GetFont(FontFactory.HELVETICA, 10))) { HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });
                        table.AddCell(new PdfPCell(new Phrase(entry.Kms.ToString(), FontFactory.GetFont(FontFactory.HELVETICA, 10))) { HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });

                        totalKms += entry.Kms;
                    }

                    document.Add(table);

                    // Add total kms for the month
                    Paragraph totalKmsText = new Paragraph($"Total Kms for the Month: {totalKms}", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12));
                    totalKmsText.Alignment = Element.ALIGN_RIGHT;
                    totalKmsText.SpacingBefore = 20f;
                    totalKmsText.SpacingAfter = 20f;
                    document.Add(totalKmsText);

                    // Add signature lines
                    PdfPTable signatureTable = new PdfPTable(2);
                    signatureTable.WidthPercentage = 100;
                    signatureTable.SetWidths(new float[] { 1f, 1f });

                    // Employee signature
                    PdfPCell employeeSignatureCell = new PdfPCell();
                    employeeSignatureCell.AddElement(new Paragraph("_______________________", FontFactory.GetFont(FontFactory.HELVETICA, 12)));
                    employeeSignatureCell.AddElement(new Paragraph("Employee Signature", FontFactory.GetFont(FontFactory.HELVETICA, 10)));
                    employeeSignatureCell.HorizontalAlignment = Element.ALIGN_LEFT;
                    employeeSignatureCell.Border = Rectangle.NO_BORDER;
                    signatureTable.AddCell(employeeSignatureCell);

                    // Manager signature
                    PdfPCell managerSignatureCell = new PdfPCell();
                    managerSignatureCell.AddElement(new Paragraph("_______________________", FontFactory.GetFont(FontFactory.HELVETICA, 12)));
                    managerSignatureCell.AddElement(new Paragraph("Manager Signature", FontFactory.GetFont(FontFactory.HELVETICA, 10)));
                    managerSignatureCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    managerSignatureCell.Border = Rectangle.NO_BORDER;
                    signatureTable.AddCell(managerSignatureCell);

                    document.Add(signatureTable);



                    PdfContentByte canvasfooter = writer.DirectContent;


                    canvasfooter.SetColorFill(greenColor);
                    canvasfooter.Rectangle(0, 0, document.PageSize.Width, 20);
                    canvasfooter.Fill();



                    float rectHeightfooter = 40; // Altura do retângulo amarelo
                    float rectWidthfooter = 100; // Definir a largura do retângulo amarelo                    
                    float xPosLeft = 0;
                    float yPosBottom = 0;
                    // Configurar a cor de preenchimento para o amarelo
                    canvasfooter.SetColorFill(yellowColor);

                    // Definir as coordenadas para desenhar o retângulo com o corte à direita
                    canvasfooter.MoveTo(xPosLeft, rectHeightfooter + yPosBottom); // Ponto inicial na parte inferior
                    canvasfooter.LineTo(xPosLeft + rectWidthfooter - 60, rectHeightfooter + yPosBottom); // Criar linha horizontal até perto da borda direita (60px de corte)
                    canvasfooter.LineTo(xPosLeft + rectWidthfooter, yPosBottom); // Criar o corte para a direita, subindo um pouco
                    canvasfooter.LineTo(xPosLeft, yPosBottom); // Desenhar linha reta até o rodapé
                    canvasfooter.LineTo(xPosLeft, rectHeightfooter + yPosBottom); // Fechar o retângulo na parte inferior esquerda
                    canvasfooter.Fill();

                    // Add green rectangle at the top

                    document.Close();

                    return File(ms.ToArray(), "application/pdf", $"{request.Name}_{request.LicensePlate}_{request.Year}_{request.Month}_Consultancy.pdf");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error generating PDF: {ex.Message}" });
            }
        }
    }
}
