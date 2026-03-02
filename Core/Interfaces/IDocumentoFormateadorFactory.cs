using SAE.Print.Core.Interfaces;

namespace SAE.Print.Core.Interfaces;

public interface IDocumentoFormateadorFactory
{
    ITicketGenerator CreateTicketGenerator();
    IKitchenOrderGenerator CreateKitchenOrderGenerator();
    IPdfGenerator CreatePdfGenerator();
}
