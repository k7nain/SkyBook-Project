using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using FlyzenApi.Application.Interfaces;

namespace FlyzenApi.Infrastructure.Services
{
    public class TicketPdfService : ITicketPdfService
    {
        static TicketPdfService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] GenerateTicketPdf(string ticketNumber, string passengerName, string flightNumber, 
            string departureCity, string arrivalCity, DateTime departureTime, DateTime arrivalTime, 
            string seatNumber, string? qrCodeBase64 = null)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);

                    page.Content().Column(column =>
                    {
                        // Header
                        column.Item().Row(row =>
                        {
                            row.RelativeColumn().Text("AIRLINE TICKET").FontSize(24).Bold();
                            row.RelativeColumn().AlignRight().Text(ticketNumber).FontSize(12);
                        });

                        column.Item().PaddingVertical(10).Divider();

                        // Passenger Info
                        column.Item().Text($"Passenger: {passengerName}").FontSize(14).Bold();
                        column.Item().PaddingVertical(5);

                        // Flight Details
                        column.Item().Row(row =>
                        {
                            row.RelativeColumn().Column(c =>
                            {
                                c.Item().Text("Departure").FontSize(12).Bold();
                                c.Item().Text(departureCity).FontSize(11);
                                c.Item().Text(departureTime.ToString("yyyy-MM-dd HH:mm")).FontSize(11);
                            });

                            row.RelativeColumn().Column(c =>
                            {
                                c.Item().Text("Arrival").FontSize(12).Bold();
                                c.Item().Text(arrivalCity).FontSize(11);
                                c.Item().Text(arrivalTime.ToString("yyyy-MM-dd HH:mm")).FontSize(11);
                            });

                            row.RelativeColumn().Column(c =>
                            {
                                c.Item().Text("Flight Number").FontSize(12).Bold();
                                c.Item().Text(flightNumber).FontSize(11);
                                c.Item().PaddingTop(5);
                                c.Item().Text("Seat").FontSize(12).Bold();
                                c.Item().Text(seatNumber).FontSize(11);
                            });
                        });

                        column.Item().PaddingVertical(10).Divider();

                        // QR Code
                        if (!string.IsNullOrEmpty(qrCodeBase64))
                        {
                            column.Item().AlignCenter().Height(150).Image(Convert.FromBase64String(qrCodeBase64));
                        }

                        column.Item().PaddingTop(20).Text("Thank you for booking with us!").FontSize(10).AlignCenter();
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}
