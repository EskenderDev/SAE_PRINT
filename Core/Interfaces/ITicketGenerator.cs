using SAE.Print.Core.Models;

namespace SAE.Print.Core.Interfaces;

public interface ITicketGenerator
{
    Task<byte[]> GenerateAsync(GenerarDocumentoRequest documento);
}
