# SAE.Print API Guide

This guide describes the core interfaces and how to extend the `SAE.Print` library.

## Core Interfaces

### `IPdfGenerator`
Responsible for converting a `GenerarDocumentoRequest` into a PDF byte array.
- **Namespace**: `SAE.Print.Core.Interfaces`
- **Method**: `Task<byte[]> GenerateAsync(GenerarDocumentoRequest request)`

### `ITicketGenerator`
Generates ESC/POS commands or raw text for thermal tickets.
- **Namespace**: `SAE.Print.Core.Interfaces`
- **Method**: `Task<byte[]> GenerateAsync(GenerarDocumentoRequest request)`

### `IPrinterService`
A high-level service to manage print jobs.
- **Namespace**: `SAE.Print.Core.Interfaces`

### `IPrinterTransport`
Low-level abstraction for sending bytes to a physical device.
- **Implementations**:
  - `NativeWindowsPrinterTransport`: Uses Win32 `winspool.drv` for RAW printing.

## Label Component

The `SAE.Print.Labels` namespace contains a complex engine for handling G-Labels templates.

### `ILabelRenderer`
The main interface for processing labels.
- `GenerateZpl(...)`: Produces Zebra Programming Language code.
- `PrintLabel(...)`: Sends the label to a GDI+ printer.
- `RenderToImage(...)`: Renders the label to a GDI+ `Image` or `Bitmap`.

## Customization

To add a new document type:
1. Implement a new generator interface in `Core`.
2. Provide the concrete implementation in `Infrastructure`.
3. Register it in your Dependency Injection container.

```csharp
services.AddTransient<IPdfGenerator, MyCustomPdfGenerator>();
```
