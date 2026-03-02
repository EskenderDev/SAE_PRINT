using SAE.Sdk.Models;
using SAE.Print.Core.Interfaces;
using System.Text;

namespace SAE.Print.Infrastructure.Generators;

public class TicketGenerator : ITicketGenerator
{
    private const string ESC = "\u001B";
    private const string GS = "\u001D";
    private const string Initialize = ESC + "@";
    private const string AlignLeft = ESC + "a" + "\u0000";
    private const string AlignCenter = ESC + "a" + "\u0001";
    private const string AlignRight = ESC + "a" + "\u0002";
    private const string BoldOn = ESC + "E" + "\u0001";
    private const string BoldOff = ESC + "E" + "\u0000";
    private const string CutPaper = GS + "V" + "\u0041" + "\u0003";

    public Task<byte[]> GenerateAsync(GenerarDocumentoRequest documento)
    {
        var sb = new StringBuilder();

        sb.Append(Initialize);

        // Encabezado
        sb.Append(AlignCenter);
        sb.Append(BoldOn);
        sb.AppendLine(documento.Emisor.Nombre);
        sb.Append(BoldOff);
        sb.AppendLine($"Cedula: {documento.Emisor.Identificacion.Numero}");
        if (documento.Emisor.Telefono != null)
        {
            sb.AppendLine(documento.Emisor.Telefono.Numero.ToString());
        }
        sb.AppendLine("--------------------------------");

        // Datos del Documento
        sb.Append(AlignLeft);
        sb.AppendLine($"Clave: {documento.Clave.Consecutivo.NumeroConsecutivo}");
        sb.AppendLine($"Consecutivo: {documento.Consecutivo.NumeroConsecutivo}");
        sb.AppendLine($"Fecha: {documento.FechaEmision:dd/MM/yyyy HH:mm}");
        sb.AppendLine("--------------------------------");

        // Líneas
        sb.AppendLine("Cant  Desc       Precio     Total");
        if (documento.DetalleServicio != null)
        {
            foreach (var linea in documento.DetalleServicio.LineasDetalle)
            {
                string cantidad = linea.Cantidad.ToString("0.##").PadRight(5);
                string descripcionRaw = linea.Detalle ?? string.Empty;
                string descripcion = descripcionRaw.Length > 10 ? descripcionRaw.Substring(0, 10) : descripcionRaw.PadRight(10);
                string precio = linea.PrecioUnitario.ToString("0").PadLeft(8);
                string total = (linea.MontoTotal ?? 0m).ToString("0").PadLeft(9);

                sb.AppendLine($"{cantidad} {descripcion} {precio} {total}");
            }
        }
        sb.AppendLine("--------------------------------");

        // Totales
        if (documento.ResumenFactura != null)
        {
            sb.Append(AlignRight);
            sb.AppendLine($"SubTotal: {(documento.ResumenFactura.TotalVenta ?? 0m):N2}");
            sb.AppendLine($"IVA:      {(documento.ResumenFactura.TotalImpuesto ?? 0m):N2}");
            sb.Append(BoldOn);
            sb.AppendLine($"TOTAL:    {(documento.ResumenFactura.TotalComprobante ?? 0m):N2}");
            sb.Append(BoldOff);
        }

        // Pie
        sb.Append(AlignCenter);
        sb.AppendLine("\n");
        sb.AppendLine("Gracias por su compra");
        sb.AppendLine("\n\n");

        sb.Append(CutPaper);

        return Task.FromResult(Encoding.UTF8.GetBytes(sb.ToString()));
    }
}
