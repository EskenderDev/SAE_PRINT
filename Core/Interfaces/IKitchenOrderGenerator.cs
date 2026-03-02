using SAE.Print.Core.Models;

namespace SAE.Print.Core.Interfaces;

public interface IKitchenOrderGenerator
{
    Task<byte[]> GenerateAsync(KitchenOrderRequest request);
}
