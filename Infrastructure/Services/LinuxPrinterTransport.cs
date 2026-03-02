using SAE.Print.Core.Interfaces;
using System.Diagnostics;
using System.Text;

namespace SAE.Print.Infrastructure.Services;

public class LinuxPrinterTransport : IPrinterTransport
{
    private string? _printerName;
    private MemoryStream? _dataStream;
    private bool _isDisposed;

    public bool Open(string name)
    {
        _printerName = name;
        _dataStream = new MemoryStream();
        return true;
    }

    public bool Close()
    {
        _dataStream?.Dispose();
        _dataStream = null;
        return true;
    }

    public bool StartDocument(string docName, string dataType = "RAW")
    {
        // En Linux 'lp' no requiere StartDocument explícito, pero reiniciamos el stream
        _dataStream?.SetLength(0);
        return true;
    }

    public bool EndDocument()
    {
        if (_dataStream == null || string.IsNullOrEmpty(_printerName))
            return false;

        try
        {
            // Ejecutar comando: lp -d printername
            var psi = new ProcessStartInfo
            {
                FileName = "lp",
                Arguments = $"-d \"{_printerName}\" -o raw",
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null) return false;

            _dataStream.Position = 0;
            _dataStream.CopyTo(process.StandardInput.BaseStream);
            process.StandardInput.Close();

            process.WaitForExit();
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    public bool StartPage() => true;

    public bool EndPage() => true;

    public bool Write(byte[] buffer, int offset, int count)
    {
        if (_dataStream == null) return false;
        _dataStream.Write(buffer, offset, count);
        return true;
    }

    public void Dispose()
    {
        if (!_isDisposed)
        {
            _dataStream?.Dispose();
            _isDisposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
