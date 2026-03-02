using SAE.Print.Core.Interfaces;
using System.Collections;
using System.Runtime.InteropServices;


namespace SAE.Print.Transport
{
    public class Win32PrinterTransport : IPrinterTransport
    {

        private nint _hPrinter = nint.Zero;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        private class DOCINFOA
        {
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDocName = string.Empty;
            [MarshalAs(UnmanagedType.LPStr)]
            public string? pOutputFile = null;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDataType = "RAW";
        }

        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool OpenPrinter(string szPrinter, out nint hPrinter, nint pd);

        [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool ClosePrinter(nint hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool StartDocPrinter(nint hPrinter, int level, [In] DOCINFOA di);

        [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool EndDocPrinter(nint hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool StartPagePrinter(nint hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool EndPagePrinter(nint hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool WritePrinter(nint hPrinter, nint pBytes, int dwCount, out int dwWritten);

        public static bool SendBytesToPrinter(string printerName, nint pBytes, int count, string docName = "")
        {
            if (OpenPrinter(printerName, out nint hPrinter, nint.Zero))
            {
                try
                {
                    var docInfo = new DOCINFOA { pDocName = docName, pOutputFile = null, pDataType = "RAW" };
                    if (StartDocPrinter(hPrinter, 1, docInfo))
                    {
                        if (StartPagePrinter(hPrinter))
                        {
                            bool success = WritePrinter(hPrinter, pBytes, count, out _);
                            EndPagePrinter(hPrinter);
                            return success;
                        }
                        EndDocPrinter(hPrinter);
                    }
                }
                catch
                {
                    throw;
                }
                finally
                {
                    ClosePrinter(hPrinter);
                }
            }
            return false;
        }

        public static bool SendStringToPrinter(string printerName, string content, string docName = "")
        {
            docName = string.IsNullOrEmpty(docName) ? "Documento" : docName;

            nint pBytes = Marshal.StringToCoTaskMemAnsi(content);
            try
            {
                return SendBytesToPrinter(printerName, pBytes, content.Length, docName);
            }
            finally
            {
                Marshal.FreeCoTaskMem(pBytes);
            }
        }

        public static bool SendArrayListToPrinter(string printerName, ArrayList content)
        {
            foreach (string line in content)
            {
                SendStringToPrinter(printerName, line + "\n");
            }
            return true;
        }

        public static bool SendEnumerableToPrinter(string printerName, IEnumerable<(int yoffset, int xoffset, string cadena)> content)
        {
            var alPrint = new ArrayList();
            int currentYOffset = 0;
            foreach (var (yoffset, xoffset, cadena) in content)
            {
                if (yoffset != currentYOffset)
                {
                    while (currentYOffset++ < yoffset)
                    {
                        alPrint.Add(" ");
                    }
                    currentYOffset = yoffset;
                }

                string line = cadena.PadLeft(xoffset);
                alPrint.Add(line);
            }

            return SendArrayListToPrinter(printerName, alPrint);
        }

        public bool Open(string printerName)
        {
            if (_hPrinter != nint.Zero)
                Close();

            return OpenPrinter(printerName, out _hPrinter, nint.Zero);
        }

        public bool Close()
        {
            if (_hPrinter == nint.Zero)
                return true;

            var result = ClosePrinter(_hPrinter);
            _hPrinter = nint.Zero;
            return result;
        }

        public bool StartDocument(string docName, string dataType = "RAW")
        {
            if (_hPrinter == nint.Zero)
                throw new InvalidOperationException("Printer not open.");

            var info = new DOCINFOA
            {
                pDocName = docName,
                pDataType = dataType
            };
            return StartDocPrinter(_hPrinter, 1, info);
        }

        public bool EndDocument()
        {
            if (_hPrinter == nint.Zero)
                throw new InvalidOperationException("Printer not open.");

            return EndDocPrinter(_hPrinter);
        }

        public bool StartPage()
        {
            if (_hPrinter == nint.Zero)
                throw new InvalidOperationException("Printer not open.");

            return StartPagePrinter(_hPrinter);
        }

        public bool EndPage()
        {
            if (_hPrinter == nint.Zero)
                throw new InvalidOperationException("Printer not open.");

            return EndPagePrinter(_hPrinter);
        }

        public bool Write(byte[] buffer, int offset, int count)
        {
            if (_hPrinter == nint.Zero)
                throw new InvalidOperationException("Printer not open.");

            // Copiamos los bytes a memoria unmanaged
            var ptr = Marshal.AllocCoTaskMem(count);
            try
            {
                Marshal.Copy(buffer, offset, ptr, count);
                return WritePrinter(_hPrinter, ptr, count, out _);
            }
            finally
            {
                Marshal.FreeCoTaskMem(ptr);
            }
        }

        public void Dispose()
        {
            Close();
        }
    }
}


