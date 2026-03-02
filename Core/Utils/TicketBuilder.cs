using System.Text;
using SAE.Print.Core.Enums;

namespace SAE.Print.Core.Utils;

public class TicketBuilder
{
    private readonly StringBuilder _sb = new();
    private readonly int _width;
    private readonly Encoding _encoding;

    // ESC/POS Commands
    private const string ESC = "\u001B";
    private const string GS = "\u001D";
    private const string Initialize = ESC + "@";
    private const string BoldOn = ESC + "E" + "\u0001";
    private const string BoldOff = ESC + "E" + "\u0000";
    private const string DoubleHeightOn = ESC + "!" + "\u0010";
    private const string DoubleHeightOff = ESC + "!" + "\u0000";
    private const string CutPaper = GS + "V" + "\u0041" + "\u0003";
    private const string OpenDrawer = ESC + "p" + "\u0000" + "\u000F" + "\u0096";
    private const string Beep = ESC + "B" + "\u0002" + "\u0001";

    public TicketBuilder(int width = 40, Encoding? encoding = null)
    {
        _width = width;
        _encoding = encoding ?? Encoding.UTF8;
        _sb.Append(Initialize);
    }

    public TicketBuilder AppendLine(char c)
    {
        _sb.AppendLine(new string(c, _width));
        return this;
    }

    public TicketBuilder Separator() => AppendLine('-');
    public TicketBuilder DoubleSeparator() => AppendLine('=');
    public TicketBuilder AsteriskSeparator() => AppendLine('*');

    public TicketBuilder Text(string text, TicketAlignment alignment = TicketAlignment.Left, bool bold = false)
    {
        if (bold) _sb.Append(BoldOn);
        
        var lines = WrapText(text, _width);
        foreach (var line in lines)
        {
            _sb.AppendLine(FormatLine(line, alignment));
        }

        if (bold) _sb.Append(BoldOff);
        return this;
    }

    public TicketBuilder Center(string text, bool bold = false) => Text(text, TicketAlignment.Center, bold);
    public TicketBuilder Right(string text, bool bold = false) => Text(text, TicketAlignment.Right, bold);
    
    public TicketBuilder DualText(string left, string right)
    {
        int spaces = _width - (left.Length + right.Length);
        if (spaces < 1)
        {
            _sb.AppendLine(left);
            _sb.AppendLine(right.PadLeft(_width));
        }
        else
        {
            _sb.AppendLine(left + new string(' ', spaces) + right);
        }
        return this;
    }

    public TicketBuilder Item(string description, decimal quantity, decimal? price = null, decimal? total = null)
    {
        string qty = quantity.ToString("0.##").PadRight(6);
        string totalTxt = total.HasValue ? total.Value.ToString("N2").PadLeft(10) : "";
        
        var availableWidth = _width - (price.HasValue ? 18 : 6); // 6 for qty, 10 for total + space
        
        var lines = WrapText(description, availableWidth);
        
        for (int i = 0; i < lines.Count; i++)
        {
            string line = lines[i];
            if (i == 0)
            {
                _sb.Append(qty);
                _sb.Append(line.PadRight(availableWidth));
                if (total.HasValue) _sb.Append(totalTxt);
                _sb.AppendLine();
            }
            else
            {
                _sb.Append(new string(' ', 6));
                _sb.AppendLine(line);
            }
        }

        if (price.HasValue && !total.HasValue)
        {
            _sb.AppendLine($"{price.Value:N2}".PadLeft(_width));
        }
        
        return this;
    }

    public TicketBuilder MultiLineItem(string description, decimal quantity)
    {
        return Item(description, quantity);
    }

    public TicketBuilder SetFontSize(PrinterFontSize size)
    {
        string cmd = size switch
        {
            PrinterFontSize.Normal => ESC + "!" + "\u0000",
            PrinterFontSize.KitchenProducts => ESC + "!" + "\u0010", // Double height
            PrinterFontSize.KitchenProductsLarge => ESC + "!" + "\u0020", // Double width
            PrinterFontSize.KitchenProductsExtraLarge => ESC + "!" + "\u0030", // Both
            PrinterFontSize.Normal58mm => ESC + "!" + "\u0001",
            _ => ESC + "!" + "\u0000"
        };
        _sb.Append(cmd);
        return this;
    }

    public TicketBuilder Bold(bool state = true)
    {
        _sb.Append(state ? BoldOn : BoldOff);
        return this;
    }

    public TicketBuilder Feed(int lines = 1)
    {
        for (int i = 0; i < lines; i++) _sb.AppendLine();
        return this;
    }

    public TicketBuilder AdvancedFeed(int units)
    {
        _sb.Append(ESC + "d" + (char)units);
        return this;
    }

    public TicketBuilder Sound()
    {
        _sb.Append(Beep);
        return this;
    }

    public TicketBuilder OpenCashDrawer()
    {
        _sb.Append(OpenDrawer);
        return this;
    }

    public TicketBuilder Cut()
    {
        _sb.Append(CutPaper);
        return this;
    }

    public byte[] Build()
    {
        return _encoding.GetBytes(_sb.ToString());
    }

    public override string ToString() => _sb.ToString();

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
    
    // Conditionals
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
}
