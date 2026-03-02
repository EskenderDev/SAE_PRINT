using PdfSharp.Drawing;
using PdfSharp.Pdf;
using SAE.Print.Core.Models;
using SAE.Print.Core.Interfaces;

using SAE.Print.Core.Enums;
using PdfSharp;

namespace SAE.Print.Infrastructure.Generators;

public class PdfGenerator : IPdfGenerator
{
    private const double Margin = 40;
    private const double LineHeight = 15;

    public Task<byte[]> GenerateAsync(GenerarDocumentoRequest documento, PaperSize paperSize = PaperSize.Letter)
    {
        using var stream = new MemoryStream();
        var document = new PdfDocument();
        document.Info.Title = $"Documento {documento.Consecutivo.NumeroConsecutivo}";

        var page = document.AddPage();
        
        // Set Page Size
        if (paperSize == PaperSize.HalfLetter)
        {
            page.Width = XUnit.FromInch(5.5);
            page.Height = XUnit.FromInch(8.5);
        }
        else // Default or Letter
        {
            page.Size = PageSize.Letter;
        }

        var gfx = XGraphics.FromPdfPage(page);

        // Fuentes
        var titleFont = new XFont("Arial", 14, XFontStyleEx.Bold);
        var headerFont = new XFont("Arial", 10, XFontStyleEx.Bold);
        var bodyFont = new XFont("Arial", 9, XFontStyleEx.Regular);
        var smallFont = new XFont("Arial", 8, XFontStyleEx.Regular);

        double y = Margin;

        // 1. Encabezado
        DrawHeader(gfx, documento, titleFont, bodyFont, ref y, page.Width.Point);

        // 2. Detalles del Documento
        y += 10;
        DrawDocumentDetails(gfx, documento, headerFont, bodyFont, ref y);

        // 3. Líneas de Detalle
        y += 20;
        if (documento.DetalleServicio != null)
        {
            DrawLines(gfx, documento.DetalleServicio.LineasDetalle, headerFont, bodyFont, ref y, page.Width.Point);
        }

        // 4. Totales
        y += 20;
        if (documento.ResumenFactura != null)
        {
            DrawTotals(gfx, documento.ResumenFactura, headerFont, bodyFont, ref y, page.Width.Point);
        }

        // 5. Pie de página
        DrawFooter(gfx, smallFont, page.Height.Point, page.Width.Point);

        document.Save(stream, false);
        return Task.FromResult(stream.ToArray());
    }

    private void DrawHeader(XGraphics gfx, GenerarDocumentoRequest doc, XFont titleFont, XFont bodyFont, ref double y, double pageWidth)
    {
        gfx.DrawString(doc.Emisor.Nombre, titleFont, XBrushes.Black, new XPoint(Margin, y), XStringFormats.Default);
        y += 20;
        gfx.DrawString($"Cédula: {doc.Emisor.Identificacion.Numero}", bodyFont, XBrushes.Black, new XPoint(Margin, y), XStringFormats.Default);
        y += LineHeight;
        if (doc.Emisor.Telefono != null)
        {
            gfx.DrawString($"Tel: {doc.Emisor.Telefono.Numero}", bodyFont, XBrushes.Black, new XPoint(Margin, y), XStringFormats.Default);
        }
        y += LineHeight;
        if (doc.Emisor.CorreoElectronico != null && doc.Emisor.CorreoElectronico.Count > 0)
        {
            gfx.DrawString($"Correo: {doc.Emisor.CorreoElectronico[0]}", bodyFont, XBrushes.Black, new XPoint(Margin, y), XStringFormats.Default);
        }

        if (doc.Receptor != null)
        {
            y += 20;
            gfx.DrawString("Receptor:", titleFont, XBrushes.Black, new XPoint(Margin, y), XStringFormats.Default);
            y += 20;
            gfx.DrawString(doc.Receptor.Nombre, bodyFont, XBrushes.Black, new XPoint(Margin, y), XStringFormats.Default);
            y += LineHeight;
            gfx.DrawString($"Cédula: {doc.Receptor.Identificacion.Numero}", bodyFont, XBrushes.Black, new XPoint(Margin, y), XStringFormats.Default);
        }

        y += 20;
    }

    private void DrawDocumentDetails(XGraphics gfx, GenerarDocumentoRequest doc, XFont headerFont, XFont bodyFont, ref double y)
    {
        gfx.DrawString($"Clave: {doc.Clave.Consecutivo.NumeroConsecutivo}", bodyFont, XBrushes.Black, new XPoint(Margin, y), XStringFormats.Default);
        y += LineHeight;
        gfx.DrawString($"Consecutivo: {doc.Consecutivo.NumeroConsecutivo}", headerFont, XBrushes.Black, new XPoint(Margin, y), XStringFormats.Default);
        y += LineHeight;
        gfx.DrawString($"Fecha: {doc.FechaEmision:dd/MM/yyyy HH:mm:ss}", bodyFont, XBrushes.Black, new XPoint(Margin, y), XStringFormats.Default);
    }

    private void DrawLines(XGraphics gfx, List<LineaDetalleRequest> lineas, XFont headerFont, XFont bodyFont, ref double y, double pageWidth)
    {
        double availableWidth = pageWidth - (Margin * 2);
        
        // Calculate proportional columns
        double colQty = Margin;
        double colDesc = Margin + (availableWidth * 0.15);
        double colPrice = Margin + (availableWidth * 0.65);
        double colTotal = Margin + (availableWidth * 0.85);

        gfx.DrawString("Cant.", headerFont, XBrushes.Black, new XPoint(colQty, y), XStringFormats.Default);
        gfx.DrawString("Descripción", headerFont, XBrushes.Black, new XPoint(colDesc, y), XStringFormats.Default);
        gfx.DrawString("Precio U.", headerFont, XBrushes.Black, new XPoint(colPrice, y), XStringFormats.Default);
        gfx.DrawString("Total", headerFont, XBrushes.Black, new XPoint(colTotal, y), XStringFormats.Default);

        y += LineHeight + 5;
        gfx.DrawLine(XPens.Black, Margin, y - 5, pageWidth - Margin, y - 5);

        foreach (var linea in lineas)
        {
            gfx.DrawString(linea.Cantidad.ToString("N2"), bodyFont, XBrushes.Black, new XPoint(colQty, y), XStringFormats.Default);
            gfx.DrawString(linea.Detalle ?? string.Empty, bodyFont, XBrushes.Black, new XPoint(colDesc, y), XStringFormats.Default);
            gfx.DrawString(linea.PrecioUnitario.ToString("N2"), bodyFont, XBrushes.Black, new XPoint(colPrice, y), XStringFormats.Default);
            gfx.DrawString((linea.MontoTotal ?? 0m).ToString("N2"), bodyFont, XBrushes.Black, new XPoint(colTotal, y), XStringFormats.Default);
            y += LineHeight;
        }

        gfx.DrawLine(XPens.Black, Margin, y, pageWidth - Margin, y);
    }

    private void DrawTotals(XGraphics gfx, ResumenFacturaRequest resumen, XFont headerFont, XFont bodyFont, ref double y, double pageWidth)
    {
        double xLabel = pageWidth - Margin - 150;
        double xValue = pageWidth - Margin - 50;

        DrawTotalLine(gfx, "SubTotal:", resumen.TotalVenta ?? 0m, bodyFont, xLabel, xValue, ref y);
        DrawTotalLine(gfx, "Descuentos:", resumen.TotalDescuentos ?? 0m, bodyFont, xLabel, xValue, ref y);
        DrawTotalLine(gfx, "Impuestos:", resumen.TotalImpuesto ?? 0m, bodyFont, xLabel, xValue, ref y);

        y += 5;
        DrawTotalLine(gfx, "TOTAL:", resumen.TotalComprobante ?? 0m, headerFont, xLabel, xValue, ref y);
    }

    private void DrawTotalLine(XGraphics gfx, string label, decimal value, XFont font, double xLabel, double xValue, ref double y)
    {
        gfx.DrawString(label, font, XBrushes.Black, new XPoint(xLabel, y), XStringFormats.Default);
        gfx.DrawString(value.ToString("N2"), font, XBrushes.Black, new XPoint(xValue, y), XStringFormats.Default);
        y += LineHeight;
    }

    private void DrawFooter(XGraphics gfx, XFont smallFont, double pageHeight, double pageWidth)
    {
        double y = pageHeight - Margin - 20;
        gfx.DrawString("Autorizado mediante resolución DGT-R-033-2019", smallFont, XBrushes.Gray, new XPoint(Margin, y), XStringFormats.Default);
        gfx.DrawString("Generado por SAE SYSTEM", smallFont, XBrushes.Gray, new XPoint(pageWidth - Margin - 150, y), XStringFormats.Default);
    }
}
