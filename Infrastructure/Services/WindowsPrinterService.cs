using SAE.Print.Core.Interfaces;
using System.Text;

namespace SAE.Print.Infrastructure.Services;

public class WindowsPrinterService : IPrinterService
{
    private readonly IPrinterTransport _transport;

    public WindowsPrinterService(IPrinterTransport transport)
    {
        _transport = transport;
    }

    public bool PrintText(string printerName, string content, string docName = "")
    {
        var data = Encoding.Default.GetBytes(content);
        return PrintBytes(printerName, data, docName);
    }

    public bool PrintLines(string printerName, IEnumerable<string> lines)
    {
        var sb = new StringBuilder();
        foreach (var line in lines)
        {
            sb.AppendLine(line);
        }
        var data = Encoding.Default.GetBytes(sb.ToString());
        return PrintBytes(printerName, data, "");
    }

    public bool PrintFormatted(string printerName, IEnumerable<(int yoffset, int xoffset, string cadena)> content)
    {
        var sb = new StringBuilder();
        int currentYOffset = 0;
        foreach (var (yoffset, xoffset, cadena) in content)
        {
            while (currentYOffset < yoffset)
            {
                sb.AppendLine();
                currentYOffset++;
            }
            sb.Append(cadena.PadLeft(xoffset + cadena.Length));
            sb.AppendLine();
        }
        var data = Encoding.Default.GetBytes(sb.ToString());
        return PrintBytes(printerName, data, "");
    }

    private bool PrintBytes(string printerName, byte[] data, string docName)
    {
        if (!_transport.Open(printerName))
            return false;

        try
        {
            if (!_transport.StartDocument(docName))
                return false;

            if (!_transport.StartPage())
                return false;

            if (!_transport.Write(data, 0, data.Length))
                return false;

            _transport.EndPage();
            _transport.EndDocument();
            return true;
        }
        finally
        {
            _transport.Close();
        }
    }
}
