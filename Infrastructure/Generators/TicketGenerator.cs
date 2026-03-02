using SAE.Print.Core.Models;
using SAE.Print.Core.Interfaces;
using SAE.Print.Core.Utils;
using SAE.Print.Core.Enums;

namespace SAE.Print.Infrastructure.Generators;

public class TicketGenerator : ITicketGenerator
{
    public Task<byte[]> GenerateAsync(GenerarDocumentoRequest documento)
    {
        var builder = new TicketBuilder();

        // Header
        builder.Center(documento.Emisor.Nombre, bold: true)
               .Center($"Cedula: {documento.Emisor.Identificacion.Numero}");

        if (documento.Emisor.Telefono != null)
        {
            builder.Center(documento.Emisor.Telefono.Numero.ToString());
        }
        
        builder.Separator()
               .Text($"Clave: {documento.Clave.Consecutivo.NumeroConsecutivo}") // Simplified for example
               .Text($"Consecutivo: {documento.Consecutivo.NumeroConsecutivo}")
               .Text($"Fecha: {documento.FechaEmision:dd/MM/yyyy HH:mm}")
               .Separator();

        // Items Header
        builder.Text("Cant  Desc       Precio     Total", bold: true);

        // Lines
        if (documento.DetalleServicio?.LineasDetalle != null)
        {
            foreach (var linea in documento.DetalleServicio.LineasDetalle)
            {
                builder.Item(linea.Detalle ?? "N/A", linea.Cantidad, linea.PrecioUnitario, linea.MontoTotalLinea);
            }
        }
        
        builder.Separator();

        // Totals
        if (documento.ResumenFactura != null)
        {
            builder.Right($"SubTotal: {documento.ResumenFactura.TotalVenta:N2}")
                   .Right($"IVA:      {documento.ResumenFactura.TotalImpuesto:N2}")
                   .Bold()
                   .Right($"TOTAL:    {documento.ResumenFactura.TotalComprobante:N2}")
                   .Bold(false);
        }

        // Footer
        builder.Feed(1)
               .Center("Gracias por su compra")
               .Feed(2)
               .Cut();

        return Task.FromResult(builder.Build());
    }
}
