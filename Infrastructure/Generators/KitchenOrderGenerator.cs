using SAE.Print.Core.Interfaces;
using SAE.Print.Core.Models;
using SAE.Print.Core.Utils;
using SAE.Print.Core.Enums;

namespace SAE.Print.Infrastructure.Generators;

public class KitchenOrderGenerator : IKitchenOrderGenerator
{
    public Task<byte[]> GenerateAsync(KitchenOrderRequest request)
    {
        var builder = new TicketBuilder();

        // Beep for new orders
        if (!request.EsReimpresion) builder.Sound();

        // Header
        builder.SetFontSize(PrinterFontSize.KitchenProducts)
               .Bold()
               .Center("COMANDA DE COCINA")
               .Bold(false)
               .SetFontSize(PrinterFontSize.Normal);
        
        builder.Text($"Orden #: {request.NumeroOrden}")
               .If(!string.IsNullOrEmpty(request.Ubicacion), b => b.Text($"Zona: {request.Ubicacion}"))
               .If(!string.IsNullOrEmpty(request.NumeroMesa), b => b.Text($"Mesa: {request.NumeroMesa}"))
               .Text($"Fecha: {request.FechaHora:dd/MM/yyyy HH:mm}")
               .If(!string.IsNullOrEmpty(request.Encargado), b => b.Text($"Atiende: {request.Encargado}"))
               .If(!string.IsNullOrEmpty(request.NombreCliente), b => b.Text($"Cliente: {request.NombreCliente}"));

        if (request.EsProductosNuevos)
        {
            builder.Separator()
                   .Bold()
                   .Center("*** PRODUCTOS NUEVOS ***")
                   .Bold(false);
        }

        builder.Separator()
               .Text("Cant.   Producto / Detalle", bold: true)
               .Separator();

        // Detail
        foreach (var item in request.Items)
        {
            builder.Bold()
                   .Text($"{item.Cantidad.ToString("0.##").PadRight(7)}{item.Producto}{(item.PrecioEspecial > 0 ? " (Esp)" : "")}")
                   .Bold(false)
                   .If(!string.IsNullOrEmpty(item.NombreSubsubcategoria), b => b.Text($"  [{item.NombreSubsubcategoria}]"))
                   .If(!string.IsNullOrEmpty(item.Observaciones), b => b.Text($"  * {item.Observaciones}"));
        }

        builder.Separator();

        // General Notes
        if (!string.IsNullOrEmpty(request.Descripcion))
        {
            builder.Bold().Text("OBSERVACIONES:").Bold(false)
                   .Text(request.Descripcion)
                   .Separator();
        }

        // Footer
        if (request.EsReimpresion)
        {
            builder.Center("*** REIMPRESION ***", bold: true);
        }
        
        if (!string.IsNullOrEmpty(request.QuienImprime))
            builder.Center($"Usuario: {request.QuienImprime}");
        
        builder.Feed(2).Cut();

        return Task.FromResult(builder.Build());
    }
}
