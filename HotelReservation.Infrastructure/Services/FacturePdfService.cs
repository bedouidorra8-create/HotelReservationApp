using HotelReservation.Application.DTOs;
using HotelReservation.Application.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HotelReservation.Infrastructure.Services;

public class FacturePdfService : IFacturePdfService
{
    static FacturePdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public Task<byte[]> GenererPdfAsync(FactureDto facture, CancellationToken cancellationToken = default)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(35);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Lato").FontColor(Colors.Grey.Darken3));

                page.Header().Element(header =>
                {
                    header.Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text(facture.Hotel?.Nom ?? "Grand Hôtel").FontSize(20).SemiBold().FontColor(Colors.Blue.Darken2);
                            if (!string.IsNullOrWhiteSpace(facture.Hotel?.Adresse))
                                col.Item().Text(facture.Hotel.Adresse).FontSize(9).FontColor(Colors.Grey.Medium);
                        });

                        row.RelativeItem().Column(col =>
                        {
                            col.Item().AlignRight().Text("FACTURE OFFICIELLE").FontSize(16).Bold().FontColor(Colors.Grey.Darken4);
                            col.Item().AlignRight().Text($"Réf : {facture.ReservationId.ToString().Substring(0, 8).ToUpper()}").FontSize(10);
                            col.Item().AlignRight().Text($"Date : {facture.DateEmission:dd/MM/yyyy HH:mm} UTC").FontSize(9).FontColor(Colors.Grey.Medium);
                        });
                    });
                });

                page.Content().PaddingVertical(20).Column(col =>
                {
                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    col.Item().PaddingVertical(15).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("INFORMATIONS CLIENT").FontSize(11).Bold().FontColor(Colors.Blue.Darken2);
                            if (facture.Client != null)
                            {
                                c.Item().Text($"{facture.Client.Nom} {facture.Client.Prenom}").SemiBold();
                                c.Item().Text(facture.Client.Email);
                            }
                            else
                            {
                                c.Item().Text("Client non renseigné");
                            }
                        });

                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("DÉTAILS DU SÉJOUR").FontSize(11).Bold().FontColor(Colors.Blue.Darken2);
                            c.Item().Text($"Chambre N° : {facture.Chambre?.Numero ?? "N/A"}");
                            c.Item().Text($"Réservation ID : {facture.ReservationId}");
                        });
                    });

                    col.Item().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(6).Text("Description").SemiBold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(6).AlignRight().Text("Référence").SemiBold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(6).AlignRight().Text("Montant").SemiBold();
                        });

                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text("Hébergement & Prestations associées");
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).AlignRight().Text($"Chambre {facture.Chambre?.Numero ?? ""}");
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).AlignRight().Text($"{facture.Total:N2} {facture.Devise}");

                        foreach (var paiement in facture.Paiements)
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten3).Padding(6)
                                .Text($"Règlement ({paiement.ModePaiement}) - {paiement.Date:dd/MM/yyyy}");
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten3).Padding(6).AlignRight()
                                .Text(paiement.Reference ?? "N/A");
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten3).Padding(6).AlignRight()
                                .Text($"- {paiement.Montant:N2} {paiement.Devise}");
                        }
                    });

                    var montantPaye = facture.Paiements
                        .Where(p => p.Statut == Domain.Enums.StatutPaiement.Paye)
                        .Sum(p => p.Montant);
                    var resteAPayer = Math.Max(0, facture.Total - montantPaye);

                    col.Item().PaddingTop(15).AlignRight().Column(totals =>
                    {
                        totals.Item().Text($"Total Facturé : {facture.Total:N2} {facture.Devise}").FontSize(11).Bold();
                        totals.Item().Text($"Total Réglé : {montantPaye:N2} {facture.Devise}").FontSize(10).FontColor(Colors.Green.Darken2);
                        totals.Item().Text($"Reste à Payer : {resteAPayer:N2} {facture.Devise}").FontSize(11).Bold().FontColor(resteAPayer > 0 ? Colors.Red.Darken2 : Colors.Grey.Darken2);
                    });
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Document généré automatiquement par la plateforme HOTELI (Tunisie) - Page ");
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
        });

        var bytes = document.GeneratePdf();
        return Task.FromResult(bytes);
    }
}
