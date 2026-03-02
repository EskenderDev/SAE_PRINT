# SAE.Print Library

A high-performance, database-independent .NET library for document generation (PDF, Tickets) and printing, including a specialized Label rendering engine.

## Features

- **PDF Generation**: Create professional PDF documents using PDFsharp 6.x.
- **Ticket Generation**: Generate ESC/POS compatible tickets for thermal printers.
- **Label Rendering**: Full engine for rendering G-Labels templates to ZPL, GDI+ Bitmaps, or physical printers.
- **Native Windows Printing**: Support for RAW (ESC/POS/ZPL) printing directly to Windows print queues via P/Invoke.
- **Clean Architecture**: Decoupled interfaces and implementations, making it easy to extend or mock for testing.
- **Zero Database Dependency**: All data is passed via simple request models.

## Installation

```bash
dotnet add package SAE.Print
```

## Quick Start

### Generating a PDF

```csharp
var pdfGenerator = new PdfGenerator(logger);
var request = new GenerarDocumentoRequest { /* ... populate data ... */ };
byte[] pdfBytes = await pdfGenerator.GenerateAsync(request);
File.WriteAllBytes("output.pdf", pdfBytes);
```

### Printing to a Windows Printer (RAW)

```csharp
var transport = new NativeWindowsPrinterTransport();
bool success = transport.Open("Printer Name");
if (success) {
    transport.StartDocument("My Job");
    transport.Write(Encoding.ASCII.GetBytes("^XA^FO50,50^A0N,50,50^FDHello World^FS^XZ"), 0, ...);
    transport.EndDocument();
    transport.Close();
}
```

## Architecture

The project follows Clean Architecture:
- `SAE.Print.Core`: Core interfaces and domain models.
- `SAE.Print.Infrastructure`: Concrete implementations for PDF/Ticket generation and Windows services.
- `SAE.Print.Labels`: Specialized component for label template processing.

## License

This project is licensed under the MIT License.
