# SAE.Print Library

A high-performance, database-independent .NET library for document generation (PDF, Tickets) and printing, including a specialized Label rendering engine.

## Features

- **PDF Generation**: Create professional PDF documents using PDFsharp 6.x.
- **Ticket Generation**: Generate ESC/POS compatible tickets for thermal printers.
- **Label Rendering**: Full engine for rendering G-Labels templates to ZPL, GDI+ Bitmaps, or physical printers.
- **Cross-Platform Printing**: 
  - **Windows**: Native RAW printing via Win32 P/Invoke (`NativeWindowsPrinterTransport`).
  - **Linux**: RAW printing via CUPS `lp` command (`LinuxPrinterTransport`).
- **Clean Architecture**: Decoupled interfaces and implementations.
- **Zero Database Dependency**: All data is passed via request models.

## Installation

```bash
dotnet add package SAE.Print
```

## Quick Start

### Generating a PDF

```csharp
var pdfGenerator = new PdfGenerator();
var request = new GenerarDocumentoRequest { /* ... */ };
byte[] pdfBytes = await pdfGenerator.GenerateAsync(request);
```

### Printing (Cross-Platform)

```csharp
using SAE.Print.Infrastructure.Services;

// 1. Choose transport based on OS
IPrinterTransport transport = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) 
    ? new NativeWindowsPrinterTransport() 
    : new LinuxPrinterTransport();

// 2. Use the generic PrinterService
var printerService = new PrinterService(transport);

// 3. Print RAW content (ZPL, ESC/POS, etc.)
printerService.PrintText("Printer_Name", "^XA^FO50,50^A0N,50,50^FDHello World^FS^XZ");
```

## Architecture

The project follows Clean Architecture:
- `SAE.Print.Core`: Core interfaces and domain models.
- `SAE.Print.Infrastructure`: Concrete implementations for PDF/Ticket generation and OS-specific services.
- `SAE.Print.Labels`: Specialized component for label template processing.

## License

This project is licensed under the MIT License.
