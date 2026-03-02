namespace SAE.Print.Core.Interfaces;

public interface IPrinterService
{
    bool PrintText(string printerName, string content, string docName = "");
    bool PrintLines(string printerName, IEnumerable<string> lines);
    bool PrintFormatted(string printerName, IEnumerable<(int yoffset, int xoffset, string cadena)> content);
}
