using SAE.Sdk.Models;

namespace SAE.Print.Core.Interfaces;

public interface IPdfGenerator
{
    Task<byte[]> GenerateAsync(GenerarDocumentoRequest documento);
}
