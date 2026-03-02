using SAE.Print.Core.Interfaces;
using SAE.Print.Infrastructure.Generators;

namespace SAE.Print.Infrastructure.Factories;

public class DocumentoFormateadorFactory : IDocumentoFormateadorFactory
{
    private readonly IServiceProvider _serviceProvider;

    public DocumentoFormateadorFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public ITicketGenerator CreateTicketGenerator() => new TicketGenerator();
    public IKitchenOrderGenerator CreateKitchenOrderGenerator() => new KitchenOrderGenerator();
    public IPdfGenerator CreatePdfGenerator() => new PdfGenerator();
}
