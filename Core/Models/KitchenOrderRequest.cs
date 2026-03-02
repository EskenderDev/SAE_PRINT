namespace SAE.Print.Core.Models;

public class KitchenOrderRequest
{
    public int NumeroOrden { get; set; }
    public string? Ubicacion { get; set; } // e.g., "Mesa 5", "Express"
    public string? NumeroMesa { get; set; }
    public string? Encargado { get; set; }
    public string? NombreCliente { get; set; }
    public string? Descripcion { get; set; } // Observaciones generales
    public DateTime FechaHora { get; set; }
    public string? QuienImprime { get; set; }
    public bool EsReimpresion { get; set; }
    public bool EsProductosNuevos { get; set; }
    public List<KitchenOrderItemRequest> Items { get; set; } = new();
}

public class KitchenOrderItemRequest
{
    public decimal Cantidad { get; set; }
    public string Producto { get; set; } = string.Empty;
    public string? NombreSubcategoria { get; set; }
    public string? NombreSubsubcategoria { get; set; }
    public string? Observaciones { get; set; }
    public decimal PrecioEspecial { get; set; } // >0 indica (Esp)
}
