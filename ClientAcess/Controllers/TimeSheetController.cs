using Access.Models.TimeSheet;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http;

namespace ClientAcess.Controllers
{
    public class TimeSheetController : Controller
    {
        private readonly HttpClient _httpClient;
        public TimeSheetController(HttpClient httpClient)
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
                else
                {
                    var viewModel = new TimeSheetModel
                    {
                        Year = DateTime.Now.Year,
                        Month = DateTime.Now.Month
                    };
                    return View(viewModel);
                }

            }
            else
            {
                return RedirectToAction("Login", "Account");
            }

            
        }

        [HttpPost]
        public IActionResult ExportToPdf([FromBody] ExportPdfRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Name) || request.Year <= 0 || request.Month <= 0 || request.Month > 12)
            {
                return BadRequest(new { success = false, message = "Invalid input data." });
            }

            var entries = new List<WorkDay>();
            var daysInMonth = DateTime.DaysInMonth(request.Year, request.Month);

            if (request.WorkingHours != null)
            {
                foreach (var kvp in request.WorkingHours)
                {
                    if (int.TryParse(kvp.Key, out int day) && day >= 1 && day <= daysInMonth)
                    {
                        entries.Add(new WorkDay
                        {
                            Day = day,
                            DayOfWeek = kvp.Value.DayOfWeek,
                            StartTime = kvp.Value.StartTime,
                            EndTime = kvp.Value.EndTime,
                            TotalHours = kvp.Value.TotalHours
                        });
                    }
                }
            }

            if (!entries.Any())
            {
                return BadRequest(new { success = false, message = "No valid working hours entered for the selected month." });
            }

            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                    PdfWriter writer = PdfWriter.GetInstance(document, ms);

                    document.Open();


                    BaseColor greenColor = new BaseColor(55, 106, 101); // Cor verde
                    BaseColor yellowColor = new BaseColor(218, 165, 32); // Cor amarela

                    // Adicionar o retângulo verde no topo
                    PdfContentByte canvas = writer.DirectContent;
                    canvas.SetColorFill(greenColor);
                    canvas.Rectangle(0, document.PageSize.Height - 20, document.PageSize.Width, 20);
                    canvas.Fill();

                    // Adicionar o retângulo amarelo cortado à esquerda e alinhado à direita
                    float rectHeight = 20; // Altura do retângulo amarelo
                    float rectWidth = document.PageSize.Width - 180; // Largura do retângulo amarelo
                    float xPosRight = document.PageSize.Width - rectWidth; // Posição à direita

                    canvas.SetColorFill(yellowColor);
                    canvas.MoveTo(xPosRight, document.PageSize.Height - rectHeight); // Canto superior direito do retângulo
                    canvas.LineTo(document.PageSize.Width, document.PageSize.Height - rectHeight); // Canto superior direito da página
                    canvas.LineTo(document.PageSize.Width, document.PageSize.Height - rectHeight - 40); // Descer um pouco para criar corte à esquerda
                    canvas.LineTo(xPosRight + 60, document.PageSize.Height - rectHeight - 40); // Linha diagonal cortando à esquerda
                    canvas.LineTo(xPosRight, document.PageSize.Height - rectHeight); // Fechar o retângulo
                    canvas.Fill();
                    // Adicionar a imagem do logo
                    iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance("./wwwroot/img/logo.png");
                    logo.SetAbsolutePosition(10, document.PageSize.Height - 60); // Ajustar a posição da imagem
                    logo.ScaleAbsolute(30, 30); // Ajustar o tamanho da imagem
                    document.Add(logo);

                    // Adicionar o texto "KEYDEVTEAM"
                    Font font = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, greenColor);
                    Paragraph text = new Paragraph("KEYDEVTEAM", font);
                    //text.Alignment = Element.ALIGN_LEFT;
                    text.IndentationLeft = 20f; // Add right indentation to move the text more to the right
                    text.SpacingBefore = -5f;
                    text.SpacingAfter = 5f;
                    document.Add(text);



                    canvas.SetColorFill(BaseColor.BLACK);

                    // Add title
                    Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                    Paragraph title = new Paragraph($"{request.Name} - {new DateTime(request.Year, request.Month, 1):MMMM yyyy}", titleFont);
                    title.Alignment = Element.ALIGN_CENTER;
                    title.SpacingAfter = 30f;
                    title.SpacingBefore = 40f;
                    document.Add(title);

                    // Create table
                    PdfPTable table = new PdfPTable(5); // 5 columns: Day, Day of Week, Start Time, End Time, Total Hours
                    table.WidthPercentage = 100;
                    table.SetWidths(new float[] { 1f, 2f, 2f, 2f, 2f });

                    // Add table headers
                    string[] headers = { "Day", "Day of Week", "Start Time", "End Time", "Total Hours" };
                    foreach (string header in headers)
                    {
                        PdfPCell cell = new PdfPCell(new Phrase(header, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12)));
                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                        cell.Padding = 5;
                        table.AddCell(cell);
                    }

                    // Add days and working hours
                    float totalMonthHours = 0;
                    foreach (var entry in entries.OrderBy(e => e.Day))
                    {
                        table.AddCell(new PdfPCell(new Phrase(entry.Day.ToString(), FontFactory.GetFont(FontFactory.HELVETICA, 10))) { HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });
                        table.AddCell(new PdfPCell(new Phrase(entry.DayOfWeek, FontFactory.GetFont(FontFactory.HELVETICA, 10))) { HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });
                        table.AddCell(new PdfPCell(new Phrase(entry.StartTime, FontFactory.GetFont(FontFactory.HELVETICA, 10))) { HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });
                        table.AddCell(new PdfPCell(new Phrase(entry.EndTime, FontFactory.GetFont(FontFactory.HELVETICA, 10))) { HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });
                        table.AddCell(new PdfPCell(new Phrase(entry.TotalHours.ToString("F2"), FontFactory.GetFont(FontFactory.HELVETICA, 10))) { HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });

                        totalMonthHours += entry.TotalHours;
                    }

                    document.Add(table);

                    // Add total hours for the month
                    Paragraph totalHours = new Paragraph($"Total Hours for the Month: {totalMonthHours:F2}", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12));
                    totalHours.Alignment = Element.ALIGN_RIGHT;
                    totalHours.SpacingBefore = 20f;
                    totalHours.SpacingAfter = 80f;
                    document.Add(totalHours);

                    // Add signature lines
                    PdfPTable signatureTable = new PdfPTable(3);
                    signatureTable.WidthPercentage = 100;
                    signatureTable.SetWidths(new float[] { 1f, 1f, 1f });

                    // Employee signature
                    PdfPCell employeeSignatureCell = new PdfPCell();
                    employeeSignatureCell.AddElement(new Paragraph("_______________________", FontFactory.GetFont(FontFactory.HELVETICA, 12)));
                    employeeSignatureCell.AddElement(new Paragraph("Employee Signature", FontFactory.GetFont(FontFactory.HELVETICA, 10)));
                    employeeSignatureCell.HorizontalAlignment = Element.ALIGN_CENTER;
                    employeeSignatureCell.Border = Rectangle.NO_BORDER;
                    signatureTable.AddCell(employeeSignatureCell);

                    // Manager signature
                    PdfPCell managerSignatureCell = new PdfPCell();
                    managerSignatureCell.AddElement(new Paragraph("_______________________", FontFactory.GetFont(FontFactory.HELVETICA, 12)));
                    managerSignatureCell.AddElement(new Paragraph("Manager Signature", FontFactory.GetFont(FontFactory.HELVETICA, 10)));
                    managerSignatureCell.HorizontalAlignment = Element.ALIGN_CENTER;
                    managerSignatureCell.Border = Rectangle.NO_BORDER;
                    signatureTable.AddCell(managerSignatureCell);

                    PdfPCell clientSignatureCell = new PdfPCell();
                    clientSignatureCell.AddElement(new Paragraph("_______________________", FontFactory.GetFont(FontFactory.HELVETICA, 12)));
                    clientSignatureCell.AddElement(new Paragraph("Client Signature", FontFactory.GetFont(FontFactory.HELVETICA, 10)));
                    clientSignatureCell.HorizontalAlignment = Element.ALIGN_CENTER;
                    clientSignatureCell.Border = Rectangle.NO_BORDER;
                    signatureTable.AddCell(clientSignatureCell);


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

                    document.Close();

                    return File(ms.ToArray(), "application/pdf", $"{request.Name}_{request.Year}_{request.Month}.pdf");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error generating PDF: {ex.Message}" });
            }
        }
    }

}
