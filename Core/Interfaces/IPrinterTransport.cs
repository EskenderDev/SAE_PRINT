namespace SAE.Print.Core.Interfaces;

public interface IPrinterTransport : IDisposable
{
    bool Open(string name);
    bool Close();
    bool StartDocument(string docName, string dataType = "RAW");
    bool EndDocument();
    bool StartPage();
    bool EndPage();
    bool Write(byte[] buffer, int offset, int count);
}

