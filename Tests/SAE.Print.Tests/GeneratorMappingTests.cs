using SAE.Print.Infrastructure.Generators;
using SAE.Print.Core.Models;
using Xunit;
using Microsoft.Extensions.Logging.Abstractions;
using PdfSharp;
using PdfSharp.Fonts;

namespace SAE.Print.Tests
{
    public class GeneratorMappingTests
    {
        static GeneratorMappingTests()
        {
            try { GlobalFontSettings.FontResolver = new TestFontResolver(); } catch { }
        }

        private class TestFontResolver : IFontResolver
        {
            public byte[] GetFont(string faceName) => null;
            public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
                => new FontResolverInfo("Arial");
        }

        [Fact(Skip = "Requires system fonts or full font resolver setup")]
        public async Task PdfGenerator_ShouldHandleRequest()
        {
            var generator = new PdfGenerator();
            var request = new GenerarDocumentoRequest 
            { 
                Consecutivo = new ConsecutivoRequest(1, 1, 100, "01"),
                Clave = new ClaveRequest(506, 1, 1, 24, new ConsecutivoRequest(1, 1, 100, "01"), "123456789", "Normal"),
                Emisor = new EmisorRequest { Nombre = "Test Emisor", Identificacion = new IdentificacionRequest("01", "123"), Telefono = new TelefonoRequest(506, 12345678) },
                Receptor = new ReceptorRequest { Nombre = "Test Receptor", Identificacion = new IdentificacionRequest("01", "456") },
                DetalleServicio = new DetalleServicioRequest { LineasDetalle = new List<LineaDetalleRequest>() },
                FechaEmision = DateTime.Now
            };

            var result = await generator.GenerateAsync(request);
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task TicketGenerator_ShouldHandleRequest()
        {
            var generator = new TicketGenerator();
            var request = new GenerarDocumentoRequest 
            { 
                Consecutivo = new ConsecutivoRequest(1, 1, 100, "01"),
                Clave = new ClaveRequest(506, 1, 1, 24, new ConsecutivoRequest(1, 1, 100, "01"), "123456789", "Normal"),
                Emisor = new EmisorRequest { Nombre = "Test Emisor", Identificacion = new IdentificacionRequest("01", "123"), Telefono = new TelefonoRequest(506, 12345678) },
                Receptor = new ReceptorRequest { Nombre = "Test Receptor", Identificacion = new IdentificacionRequest("01", "456") },
                DetalleServicio = new DetalleServicioRequest { LineasDetalle = new List<LineaDetalleRequest>() },
                FechaEmision = DateTime.Now
            };

            var result = await generator.GenerateAsync(request);
            Assert.NotNull(result);
        }
    }
}
