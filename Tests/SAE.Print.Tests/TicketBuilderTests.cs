using System.Text;
using SAE.Print.Core.Enums;
using SAE.Print.Core.Utils;

namespace SAE.Print.Tests;

public class TicketBuilderTests
{
    private static string GetString(byte[] bytes) => Encoding.UTF8.GetString(bytes);

    [Fact]
    public void Center_ShouldPadCorrectly()
    {
        // Arrange
        var builder = new TicketBuilder(40);
        
        // Act
        builder.Center("SAE");
        var output = GetString(builder.Build());

        // Assert
        // Length of SAE is 3. Total width 40. Left pad = (40+3)/2 = 21. Total width 40.
        // Expected padding = 21 left padded, padded to 40.
        string expected = "SAE".PadLeft(21).PadRight(40);
        Assert.Contains(expected, output);
    }

    [Fact]
    public void Item_ShouldFormatQuantitiesAndPrices()
    {
        // Force invariant culture to ensure numbers use standard formatting
        var originalCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
        try
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

            // Arrange
            var builder = new TicketBuilder(40);

            // Act
            builder.Item("Pizza Margarita", 1.5m, 15000m, 22500m);
            var output = GetString(builder.Build());

            // Assert
            Assert.Contains("1.5", output);
            Assert.Contains("Pizza Margarita", output);
            Assert.Contains("22,500.00", output); 
        }
        finally
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = originalCulture;
        }
    }


    [Fact]
    public void MultiLineItem_ShouldWrapTextCorrectly()
    {
        // Arrange
        var builder = new TicketBuilder(40);

        // Act
        // Without price: available width = 40 - 6 = 34
        string longText = "This is a very long description that should wrap over multiple lines.";
        builder.MultiLineItem(longText, 1);
        var output = GetString(builder.Build());

        // Assert
        Assert.Contains("1     This is a very long description th", output);
        Assert.Contains("      at should wrap over multiple lines", output);
    }

    [Fact]
    public void Functional_Each_ShouldIterateCollection()
    {
        // Arrange
        var builder = new TicketBuilder(40);
        var items = new[] { "A", "B" };

        // Act
        builder.Each(items, (b, item) => b.Text(item));
        var output = GetString(builder.Build());

        // Assert
        Assert.Contains("A                                       ", output);
        Assert.Contains("B                                       ", output);
    }

    [Fact]
    public void QrCode_ShouldGenerateValidEscPosCommand()
    {
        // Arrange
        var builder = new TicketBuilder(40);
        string data = "test";
        
        // Act
        builder.QrCode(data);
        var bytes = builder.Build();

        // Assert
        // We look for GS ( k sequence (29, 40, 107) for QRCode model 2
        bool foundGs = false;
        for (int i = 0; i < bytes.Length - 2; i++)
        {
            if (bytes[i] == 29 && bytes[i+1] == 40 && bytes[i+2] == 107)
            {
                foundGs = true;
                break;
            }
        }
        Assert.True(foundGs, "GS ( k command not found in output bytes");
    }
}
