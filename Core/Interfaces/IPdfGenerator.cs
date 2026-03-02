using SAE.Print.Core.Models;
using SAE.Print.Core.Enums;

namespace SAE.Print.Core.Interfaces;

public interface IPdfGenerator
{
    Task<byte[]> GenerateAsync(GenerarDocumentoRequest documento, PaperSize paperSize = PaperSize.Letter);
}
