using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.AspNetCore.Mvc;

namespace ClientAcess.Controllers
{
    public class VacationController : Controller
    {
        public IActionResult Index()
        {
            int currentYear = DateTime.Now.Year;
            return View(currentYear);
        }

        [HttpPost]
        public IActionResult SaveSelection(string userName, List<string> selectedDays)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                TempData["Error"] = "Name is required.";
                return RedirectToAction("Index");
            }

            if (selectedDays == null || selectedDays.Count == 0)
            {
                TempData["Error"] = "No days were selected.";
                return RedirectToAction("Index");
            }

            try
            {
                using (var stream = new MemoryStream())
                {
                    iTextSharp.text.Document pdfDoc = new iTextSharp.text.Document(PageSize.A4, 25, 25, 30, 30);
                    PdfWriter writer = PdfWriter.GetInstance(pdfDoc, stream);
                    pdfDoc.Open();

                    BaseColor greenColor = new BaseColor(55, 106, 101); // Cor verde
                    BaseColor yellowColor = new BaseColor(218, 165, 32); // Cor amarela

                    // Adicionar o retângulo verde no topo
                    PdfContentByte canvas = writer.DirectContent;
                    canvas.SetColorFill(greenColor);
                    canvas.Rectangle(0, pdfDoc.PageSize.Height - 20, pdfDoc.PageSize.Width, 20);
                    canvas.Fill();

                    // Adicionar o retângulo amarelo cortado à esquerda e alinhado à direita
                    float rectHeight = 20; // Altura do retângulo amarelo
                    float rectWidth = pdfDoc.PageSize.Width - 180; // Largura do retângulo amarelo
                    float xPosRight = pdfDoc.PageSize.Width - rectWidth; // Posição à direita

                    canvas.SetColorFill(yellowColor);
                    canvas.MoveTo(xPosRight, pdfDoc.PageSize.Height - rectHeight); // Canto superior direito do retângulo
                    canvas.LineTo(pdfDoc.PageSize.Width, pdfDoc.PageSize.Height - rectHeight); // Canto superior direito da página
                    canvas.LineTo(pdfDoc.PageSize.Width, pdfDoc.PageSize.Height - rectHeight - 40); // Descer um pouco para criar corte à esquerda
                    canvas.LineTo(xPosRight + 60, pdfDoc.PageSize.Height - rectHeight - 40); // Linha diagonal cortando à esquerda
                    canvas.LineTo(xPosRight, pdfDoc.PageSize.Height - rectHeight); // Fechar o retângulo
                    canvas.Fill();
                    // Adicionar a imagem do logo
                    iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance("./wwwroot/img/logo.png");
                    logo.SetAbsolutePosition(10, pdfDoc.PageSize.Height - 60); // Ajustar a posição da imagem
                    logo.ScaleAbsolute(30, 30); // Ajustar o tamanho da imagem
                    pdfDoc.Add(logo);

                    // Adicionar o texto "KEYDEVTEAM"
                    Font font = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, greenColor);
                    Paragraph text = new Paragraph("KEYDEVTEAM", font);
                    //text.Alignment = Element.ALIGN_LEFT;
                    text.IndentationLeft = 20f; // Add right indentation to move the text more to the right
                    text.SpacingBefore = -5f;
                    text.SpacingAfter = 5f;
                    pdfDoc.Add(text);



                    canvas.SetColorFill(BaseColor.BLACK);


                    pdfDoc.Add(new Paragraph("\n"));
                    pdfDoc.Add(new Paragraph("\n"));
                    pdfDoc.Add(new Paragraph("\n"));

                    // Add name
                    var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                    pdfDoc.Add(new Paragraph($"Vacation: {userName}", titleFont)
                    { Alignment = Element.ALIGN_CENTER });

                    // Add a line break
                    pdfDoc.Add(new Paragraph("\n"));
                    pdfDoc.Add(new Paragraph("\n"));
                    pdfDoc.Add(new Paragraph("\n"));
                    // Add the selected days
                    var textFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);
                    foreach (var day in selectedDays)
                    {
                        DateTime formattedDate = DateTime.Parse(day);
                        string formattedDay = formattedDate.ToString("dd MMMM yyyy");
                        pdfDoc.Add(new Paragraph(formattedDay, textFont));
                    }

                    //pdfDoc.Close();


                    pdfDoc.Add(new Paragraph("\n"));
                    pdfDoc.Add(new Paragraph("\n"));
                    pdfDoc.Add(new Paragraph("\n"));
                    pdfDoc.Add(new Paragraph("\n"));
                    pdfDoc.Add(new Paragraph("\n"));

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


                    pdfDoc.Add(signatureTable);


                    PdfContentByte canvasfooter = writer.DirectContent;


                    canvasfooter.SetColorFill(greenColor);
                    canvasfooter.Rectangle(0, 0, pdfDoc.PageSize.Width, 20);
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


                    pdfDoc.Close();

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
