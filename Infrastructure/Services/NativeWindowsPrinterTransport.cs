using SAE.Print.Core.Interfaces;
using System.Runtime.InteropServices;

namespace SAE.Print.Infrastructure.Services;

public class NativeWindowsPrinterTransport : IPrinterTransport
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public class DOCINFOA
    {
        [MarshalAs(UnmanagedType.LPStr)] public string pDocName = null!;
        [MarshalAs(UnmanagedType.LPStr)] public string pOutputFile = null!;
        [MarshalAs(UnmanagedType.LPStr)] public string pDataType = null!;
    }

    [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    public static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinter, out IntPtr hPrinter, IntPtr pd);

    [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    public static extern bool ClosePrinter(IntPtr hPrinter);

    [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    public static extern int StartDocPrinter(IntPtr hPrinter, int level, [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFOA di);

    [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    public static extern bool EndDocPrinter(IntPtr hPrinter);

    [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    public static extern bool StartPagePrinter(IntPtr hPrinter);

    [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    public static extern bool EndPagePrinter(IntPtr hPrinter);

    [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    public static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, int dwCount, out int dwWritten);

    private IntPtr _hPrinter = IntPtr.Zero;

    public bool Open(string name)
    {
        if (_hPrinter != IntPtr.Zero) return false;
        return OpenPrinter(name, out _hPrinter, IntPtr.Zero);
    }

    public bool Close()
    {
        if (_hPrinter == IntPtr.Zero) return false;
        if (ClosePrinter(_hPrinter))
        {
            _hPrinter = IntPtr.Zero;
            return true;
        }
        return false;
    }

    public bool StartDocument(string docName, string dataType = "RAW")
    {
        if (_hPrinter == IntPtr.Zero) return false;
        var di = new DOCINFOA { pDocName = docName, pDataType = dataType };
        return StartDocPrinter(_hPrinter, 1, di) > 0;
    }

    public bool EndDocument()
    {
        if (_hPrinter == IntPtr.Zero) return false;
        return EndDocPrinter(_hPrinter);
    }

    public bool StartPage()
    {
        if (_hPrinter == IntPtr.Zero) return false;
        return StartPagePrinter(_hPrinter);
    }

    public bool EndPage()
    {
        if (_hPrinter == IntPtr.Zero) return false;
        return EndPagePrinter(_hPrinter);
    }

    public bool Write(byte[] buffer, int offset, int count)
    {
        if (_hPrinter == IntPtr.Zero) return false;
        var pBytes = Marshal.AllocCoTaskMem(count);
        Marshal.Copy(buffer, offset, pBytes, count);
        bool success = WritePrinter(_hPrinter, pBytes, count, out _);
        Marshal.FreeCoTaskMem(pBytes);
        return success;
    }

    public void Dispose()
    {
        Close();
    }
}
