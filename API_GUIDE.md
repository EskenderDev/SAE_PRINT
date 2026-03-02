# SAE.Print API Guide

## Core Interfaces

### IPdfGenerator
Interface for PDF generation.
- `Task<byte[]> GenerateAsync(GenerarDocumentoRequest documento)`: Generates a PDF byte array.

### ITicketGenerator
Interface for ESC/POS ticket generation.
- `Task<byte[]> GenerateAsync(GenerarDocumentoRequest documento)`: Generates an ESC/POS byte array.

### IPrinterTransport
Low-level transport interface for RAW printing.
- `bool Open(string name)`
- `bool Close()`
- `bool Write(byte[] buffer, int offset, int count)`
- `bool StartDocument(string docName, string dataType = "RAW")`
- `bool EndDocument()`

**Implementations**:
- `NativeWindowsPrinterTransport`: Windows-only (via `winspool.drv`).
- `LinuxPrinterTransport`: Linux-only (via `lp` command).

### IPrinterService
High-level service for printing operations.
- `bool PrintText(string printerName, string content, string docName = "")`
- `bool PrintLines(string printerName, IEnumerable<string> lines)`

**Implementation**:
- `PrinterService`: Cross-platform, requires an `IPrinterTransport` implementation.

## Label Component

### ILabelRenderer
The core engine for rendering labels.
- `string GenerateZpl(GlabelsTemplate template, Dictionary<string, string> data)`: Generates a ZPL string.
- `Bitmap RenderToBitmap(GlabelsTemplate template, Dictionary<string, string> data)`: Renders as a GDI+ Bitmap.

## Usage Example (DI)

```csharp
services.AddTransient<IPdfGenerator, PdfGenerator>();
services.AddTransient<ITicketGenerator, TicketGenerator>();

if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    services.AddTransient<IPrinterTransport, NativeWindowsPrinterTransport>();
else
    services.AddTransient<IPrinterTransport, LinuxPrinterTransport>();

services.AddTransient<IPrinterService, PrinterService>();
```
