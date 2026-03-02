using SAE.Print.Core.Enums;
using SAE.Print.Infrastructure.Generators;
using SAE.Sdk.Models;
using PdfSharp.Pdf.IO;

namespace SAE.Print.Tests;

public class PdfGeneratorTests
{
    private GenerarDocumentoRequest GetMockRequest()
    {
        return new GenerarDocumentoRequest
        {
            Emisor = new EmisorRequest
            {
                Nombre = "Empresa de Prueba S.A.",
                Identificacion = new IdentificacionRequest { Tipo = "01", Numero = "123456789" },
                Ubicacion = new UbicacionRequest { Provincia = "1", Canton = "01", Distrito = "01", Barrio = "01", OtrasSenas = "San José" },
                Telefono = new TelefonoRequest { CodigoPais = "506", Numero = 88888888 },
                CorreoElectronico = new List<string> { "prueba@empresa.com" }
            },
            Receptor = new ReceptorRequest
            {
                Nombre = "Cliente Final",
                Identificacion = new IdentificacionRequest { Tipo = "01", Numero = "987654321" },
                CorreoElectronico = new List<string> { "cliente@gmail.com" }
            },
            Clave = new ClaveRequest { Sucursal = 1, Terminal = 1, TipoComprobante = "01", Consecutivo = new ConsecutivoRequest { Sucursal = 1, Terminal = 1, TipoComprobante = "01", NumeroConsecutivo = "1" }, SituacionComprobante = "1", CodigoSeguridad = "12345678" },
            Consecutivo = new ConsecutivoRequest { Sucursal = 1, Terminal = 1, TipoComprobante = "01", NumeroConsecutivo = "1" },
            FechaEmision = DateTime.UtcNow,
            DetalleServicio = new DetalleServicioRequest
            {
                LineasDetalle = new List<LineaDetalleRequest>
                {
                    new LineaDetalleRequest { NumeroLinea = 1, Cantidad = 2m, UnidadMedida = "Unid", Detalle = "Producto A", PrecioUnitario = 5000m, MontoTotal = 10000m, MontoTotalLinea = 10000m, NaturalezaDescuento = "No", SubTotal = 10000m },
                    new LineaDetalleRequest { NumeroLinea = 2, Cantidad = 1m, UnidadMedida = "Unid", Detalle = "Servicio B", PrecioUnitario = 15000m, MontoTotal = 15000m, MontoTotalLinea = 15000m, NaturalezaDescuento = "No", SubTotal = 15000m }
                }
            },
            ResumenFactura = new ResumenFacturaRequest
            {
                TotalVenta = 25000m, TotalDescuentos = 0m, TotalVentaNeta = 25000m, TotalImpuesto = 3250m, TotalComprobante = 28250m
            }
        };
    }

    [Fact]
    public async Task GenerateAsync_WithPaperSizeLetter_ShouldReturnValidPdf()
    {
        // Arrange
        var generator = new PdfGenerator();
        var request = GetMockRequest();

        // Act
        byte[] pdfBytes = await generator.GenerateAsync(request, PaperSize.Letter);

        // Assert
        Assert.NotNull(pdfBytes);
        Assert.True(pdfBytes.Length > 0);

        // Verify it's a valid PDF by attempting to open it
        using var stream = new MemoryStream(pdfBytes);
        var document = PdfReader.Open(stream, PdfDocumentOpenMode.ReadOnly);
        
        Assert.Equal(1, document.PageCount);
        
        // Assert Letter Size (approx 612 x 792 points)
        var page = document.Pages[0];
        Assert.True(Math.Abs(page.Width.Point - 612) < 1);
        Assert.True(Math.Abs(page.Height.Point - 792) < 1);
    }

    [Fact]
    public async Task GenerateAsync_WithPaperSizeHalfLetter_ShouldSetCorrectDimensions()
    {
        // Arrange
        var generator = new PdfGenerator();
        var request = GetMockRequest();

        // Act
        byte[] pdfBytes = await generator.GenerateAsync(request, PaperSize.HalfLetter);

        // Assert
        Assert.NotNull(pdfBytes);
        
        using var stream = new MemoryStream(pdfBytes);
        var document = PdfReader.Open(stream, PdfDocumentOpenMode.ReadOnly);
        var page = document.Pages[0];
        
        // Assert Half-Letter Size (5.5 x 8.5 inches -> 396 x 612 points)
        Assert.True(Math.Abs(page.Width.Point - 396) < 1);
        Assert.True(Math.Abs(page.Height.Point - 612) < 1);
    }
}
