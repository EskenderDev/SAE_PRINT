using System.ComponentModel.DataAnnotations;

namespace SAE.Print.Core.Models;

public record CodigoComercialRequest(
    string Tipo,
    string Codigo);

public class GenerarDocumentoRequest
{
    [Required] public string TipoDocumento { get; set; } = string.Empty;
    public ClaveRequest Clave { get; set; } = null!;
    public ConsecutivoRequest Consecutivo { get; set; } = null!;
    public string CodigoActividadEmisor { get; set; } = string.Empty;
    [Required] public string CodigoActividadReceptor { get; set; } = string.Empty;
    [Required] public DateTime FechaEmision { get; set; }
    public string ProveedorSistemas { get; set; } = string.Empty;
    public EmisorRequest Emisor { get; set; } = null!;
    [Required] public ReceptorRequest Receptor { get; set; } = null!;
    [Required] public string CondicionVenta { get; set; } = string.Empty;
    public int? PlazoCredito { get; set; }
    [Required] public DetalleServicioRequest DetalleServicio { get; set; } = null!;
    public ResumenFacturaRequest? ResumenFactura { get; set; }
}

public record ClaveRequest(
    [Range(1, 999)] int CodigoPais,
    [Range(1, 31)] int Dia,
    [Range(1, 12)] int Mes,
    [Range(0, 99)] int Ano,
    ConsecutivoRequest Consecutivo,
    [StringLength(12, MinimumLength = 9)] string CedulaEmisor,
    string Situacion);

public record ConsecutivoRequest(
    [Range(1, 999)] int Establecimiento,
    [Range(1, 99999)] int Terminal,
    [Range(1, 9999999999)] long NumeroConsecutivo,
    string TipoComprobante);

public record IdentificacionRequest(
    string Tipo,
    string Numero);

public class EmisorRequest
{
    [Required] public IdentificacionRequest Identificacion { get; set; } = null!;
    [Required] public string Nombre { get; set; } = string.Empty;
    public string? NombreComercial { get; set; }
    [Required] public UbicacionRequest Ubicacion { get; set; } = null!;
    [Required] public TelefonoRequest Telefono { get; set; } = null!;
    [Required] public List<string> CorreoElectronico { get; set; } = new();
    public string? RegistroFiscal8707 { get; set; }
    public bool SeFacturanBebidasAlc { get; set; } = false;
}

public record TelefonoRequest(
    int CodigoPais,
    long Numero);

public class ReceptorRequest
{
    [Required] public IdentificacionRequest Identificacion { get; set; } = null!;
    [Required] public string Nombre { get; set; } = string.Empty;
    public string? IdentificacionExtranjero { get; set; }
    public string? NombreComercial { get; set; }
    public UbicacionRequest? Ubicacion { get; set; }
    public string? OtrasSenasExtranjero { get; set; }
    public TelefonoRequest? Telefono { get; set; }
    [Required] public string CorreoElectronico { get; set; } = string.Empty;
}

public class UbicacionRequest
{
    [Required] public string Provincia { get; set; } = string.Empty;
    [Required] public string Canton { get; set; } = string.Empty;
    [Required] public string Distrito { get; set; } = string.Empty;
    [Required] public string Barrio { get; set; } = string.Empty;
    [Required] public string OtrasSenas { get; set; } = string.Empty;
}

public record DetalleServicioRequest
{
    public List<LineaDetalleRequest> LineasDetalle { get; set; } = new();
}

public record LineaDetalleRequest
{
    public int? NumeroLinea { get; set; }
    public string? PartidaArancelaria { get; set; }
    public string? CodigoCABYS { get; set; }
    public List<CodigoComercialRequest> CodigosComerciales { get; set; } = new();
    public decimal Cantidad { get; set; }
    public string UnidadMedida { get; set; } = string.Empty;
    public string TipoTransaccion { get; set; } = string.Empty;
    public string? UnidadMedidaComercial { get; set; }
    public string? Detalle { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal? MontoTotal { get; set; }
    public List<DescuentoRequest> Descuentos { get; set; } = new();
    public decimal? SubTotal { get; set; }
    public string? IVACobradoFabrica { get; set; }
    public decimal? BaseImponible { get; set; }
    public List<ImpuestoRequest> Impuestos { get; set; } = new();
    public decimal? ImpuestoAsumidoEmisorFabrica { get; set; }
    public decimal? ImpuestoNeto { get; set; }
    public decimal? MontoTotalLinea { get; set; }
}

public record DescuentoRequest(
    decimal? MontoDescuento,
    string? CodigoDescuento,
    string? CodigoDescuentoOtro,
    string? NaturalezaDescuento);

public record ImpuestoRequest(
    string Codigo,
    string? CodigoImpuestoOTRO,
    string CodigoTarifaIVA,
    decimal? Tarifa,
    decimal? FactorCalculoIVA,
    decimal? Monto,
    ExoneracionRequest? Exoneracion);

public record ExoneracionRequest(
    string TipoDocumentoEX1,
    string? TipoDocumentoOTRO,
    string NumeroDocumento,
    int Articulo,
    int Inciso,
    string NombreInstitucion,
    string? NombreInstitucionOtros,
    DateTimeOffset FechaEmisionEX,
    decimal TarifaExonerada,
    decimal MontoExoneracion);

public record ResumenFacturaRequest(
    CodigoTipoMonedaRequest? CodigoTipoMoneda,
    decimal? TotalServGravados,
    decimal? TotalServExentos,
    decimal? TotalServExonerado,
    decimal? TotalServNoSujeto,
    decimal? TotalMercanciasGravadas,
    decimal? TotalMercanciasExentas,
    decimal? TotalMercExonerada,
    decimal? TotalMercNoSujeta,
    decimal? TotalGravado,
    decimal? TotalExento,
    decimal? TotalExonerado,
    decimal? TotalNoSujeto,
    decimal? TotalVenta,
    decimal? TotalDescuentos,
    decimal? TotalVentaNeta,
    TotalDesgloseImpuestoRequest? TotalDesgloseImpuesto,
    decimal? TotalImpuesto,
    decimal? TotalImpAsumEmisorFabrica,
    decimal? TotalIvaDevuelto,
    decimal? TotalOtrosCargos,
    List<MedioPagoRequest>? MediosPago,
    decimal? TotalComprobante);

public record CodigoTipoMonedaRequest(
    string CodigoMoneda,
    decimal TipoCambio);

public record TotalDesgloseImpuestoRequest(
    string Codigo,
    string CodigoTarifaIVA,
    decimal TotalMontoImpuesto);

public record MedioPagoRequest(
    string? TipoMedioPago,
    string? MedioPagoOtros,
    decimal? TotalMedioPago);
