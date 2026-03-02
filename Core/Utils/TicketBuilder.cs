using System.Text;
using SAE.Print.Core.Enums;

namespace SAE.Print.Core.Utils;

public class TicketBuilder
{
    private readonly List<byte> _buffer = new();
    private readonly int _width;
    private readonly Encoding _encoding;

    // ESC/POS Commands as byte arrays
    private static readonly byte[] Initialize = { 27, 64 };
    private static readonly byte[] BoldOn = { 27, 69, 1 };
    private static readonly byte[] BoldOff = { 27, 69, 0 };
    private static readonly byte[] CutPaper = { 29, 86, 65, 3 };
    private static readonly byte[] OpenDrawer = { 27, 112, 0, 15, 150 };
    private static readonly byte[] Beep = { 27, 66, 2, 1 };
    private static readonly byte[] AlignLeft = { 27, 97, 0 };
    private static readonly byte[] AlignCenter = { 27, 97, 1 };
    private static readonly byte[] AlignRight = { 27, 97, 2 };

    public TicketBuilder(int width = 40, Encoding? encoding = null)
    {
        _width = width;
        _encoding = encoding ?? Encoding.UTF8;
        _buffer.AddRange(Initialize);
    }

    private void Append(string text) => _buffer.AddRange(_encoding.GetBytes(text));
    private void AppendLine(string text = "")
    {
        if (!string.IsNullOrEmpty(text)) Append(text);
        _buffer.Add(10); // LF
    }

    public TicketBuilder AppendLineChar(char c)
    {
        AppendLine(new string(c, _width));
        return this;
    }

    public TicketBuilder Separator() => AppendLineChar('-');
    public TicketBuilder DoubleSeparator() => AppendLineChar('=');
    public TicketBuilder AsteriskSeparator() => AppendLineChar('*');

    public TicketBuilder Text(string text, TicketAlignment alignment = TicketAlignment.Left, bool bold = false)
    {
        if (bold) _buffer.AddRange(BoldOn);
        
        var lines = WrapText(text, _width);
        foreach (var line in lines)
        {
            AppendLine(FormatLine(line, alignment));
        }

        if (bold) _buffer.AddRange(BoldOff);
        return this;
    }

    public TicketBuilder Center(string text, bool bold = false) => Text(text, TicketAlignment.Center, bold);
    public TicketBuilder Right(string text, bool bold = false) => Text(text, TicketAlignment.Right, bold);
    
    public TicketBuilder DualText(string left, string right)
    {
        int spaces = _width - (left.Length + right.Length);
        if (spaces < 1)
        {
            AppendLine(left);
            AppendLine(right.PadLeft(_width));
        }
        else
        {
            AppendLine(left + new string(' ', spaces) + right);
        }
        return this;
    }

    public TicketBuilder Item(string description, decimal quantity, decimal? price = null, decimal? total = null)
    {
        string qty = quantity.ToString("0.##").PadRight(6);
        string totalTxt = total.HasValue ? total.Value.ToString("N2").PadLeft(10) : "";
        
        var availableWidth = _width - (price.HasValue ? 18 : 6);
        var lines = WrapText(description, availableWidth);
        
        for (int i = 0; i < lines.Count; i++)
        {
            string line = lines[i];
            if (i == 0)
            {
                Append(qty);
                Append(line.PadRight(availableWidth));
                if (total.HasValue) Append(totalTxt);
                AppendLine();
            }
            else
            {
                Append(new string(' ', 6));
                AppendLine(line);
            }
        }

        if (price.HasValue && !total.HasValue)
        {
            AppendLine($"{price.Value:N2}".PadLeft(_width));
        }
        
        return this;
    }

    public TicketBuilder MultiLineItem(string description, decimal quantity)
    {
        return Item(description, quantity);
    }

    public TicketBuilder SetFontSize(PrinterFontSize size)
    {
        byte sizeByte = size switch
        {
            PrinterFontSize.Normal => 0,
            PrinterFontSize.KitchenProducts => 16,
            PrinterFontSize.KitchenProductsLarge => 32,
            PrinterFontSize.KitchenProductsExtraLarge => 48,
            PrinterFontSize.Normal58mm => 1,
            _ => 0
        };
        _buffer.AddRange(new byte[] { 27, 33, sizeByte });
        return this;
    }

    public TicketBuilder Bold(bool state = true)
    {
        _buffer.AddRange(state ? BoldOn : BoldOff);
        return this;
    }

    public TicketBuilder Feed(int lines = 1)
    {
        for (int i = 0; i < lines; i++) _buffer.Add(10);
        return this;
    }

    public TicketBuilder AdvancedFeed(int units)
    {
        _buffer.AddRange(new byte[] { 27, 100, (byte)units });
        return this;
    }

    public TicketBuilder Sound()
    {
        _buffer.AddRange(Beep);
        return this;
    }

    public TicketBuilder OpenCashDrawer()
    {
        _buffer.AddRange(OpenDrawer);
        return this;
    }

    public TicketBuilder Cut()
    {
        _buffer.AddRange(CutPaper);
        return this;
    }

    public TicketBuilder QrCode(string data, TicketAlignment alignment = TicketAlignment.Center)
    {
        byte[] dataBytes = _encoding.GetBytes(data);
        int storeLen = dataBytes.Length + 3;
        byte pL = (byte)(storeLen % 256);
        byte pH = (byte)(storeLen / 256);

        // Alignment
        if (alignment == TicketAlignment.Center) _buffer.AddRange(AlignCenter);
        else if (alignment == TicketAlignment.Right) _buffer.AddRange(AlignRight);

        // Model 2
        _buffer.AddRange(new byte[] { 29, 40, 107, 4, 0, 49, 65, 50, 0 });
        // Module size
        _buffer.AddRange(new byte[] { 29, 40, 107, 3, 0, 49, 67, 6 });
        // Error correction M
        _buffer.AddRange(new byte[] { 29, 40, 107, 3, 0, 49, 69, 49 });
        
        // Data
        _buffer.AddRange(new byte[] { 29, 40, 107, pL, pH, 49, 80, 48 });
        _buffer.AddRange(dataBytes);

        // Print
        _buffer.AddRange(new byte[] { 29, 40, 107, 3, 0, 49, 81, 48 });

        // Reset alignment
        if (alignment != TicketAlignment.Left) _buffer.AddRange(AlignLeft);
        
        Feed(1);
        return this;
    }

    public byte[] Build()
    {
        return _buffer.ToArray();
    }

    public override string ToString() => _encoding.GetString(_buffer.ToArray());

    // Helper Methods
    private string FormatLine(string text, TicketAlignment alignment)
    {
        if (text.Length > _width) text = text[.._width];

        return alignment switch
        {
            TicketAlignment.Center => text.PadLeft((_width + text.Length) / 2).PadRight(_width),
            TicketAlignment.Right => text.PadLeft(_width),
            _ => text.PadRight(_width)
        };
    }

    private static List<string> WrapText(string text, int width)
    {
        if (string.IsNullOrEmpty(text)) return new List<string> { "" };
        
        var lines = new List<string>();
        for (int i = 0; i < text.Length; i += width)
        {
            lines.Add(text.Substring(i, Math.Min(width, text.Length - i)));
        }
        return lines;
    }
    
    // Conditionals & Loops
    public TicketBuilder If(bool condition, Action<TicketBuilder> action)
    {
        if (condition) action(this);
        return this;
    }

    public TicketBuilder IfElse(bool condition, Action<TicketBuilder> ifAction, Action<TicketBuilder> elseAction)
    {
        if (condition) ifAction(this);
        else elseAction(this);
        return this;
    }

    public TicketBuilder Each<T>(IEnumerable<T> items, Action<TicketBuilder, T> action)
    {
        if (items != null)
        {
            foreach (var item in items)
            {
                action(this, item);
            }
        }
        return this;
    }

    public TicketBuilder Map<T>(IEnumerable<T> items, Func<T, string> func)
    {
        if (items != null)
        {
            foreach (var item in items)
            {
                AppendLine(func(item));
            }
        }
        return this;
    }
}
