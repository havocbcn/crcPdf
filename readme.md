Library to read and write lowlevel pdf's, like ITextSharp, but free.

Very alpha version, doesn't fully implement Pdf standard. Please report any problem found.

### Features
-   Pages
-   Text
-   Images
-   Base fonts
-   TrueType fonts
-   TrueType fonts subsetting

## Examples

### Text
Support for positioning, rotating and set text.
```csharp
SharpPdf pdf = new SharpPdf();
pdf.Catalog.Pages
  .AddPage()                        
     .SetFont("Times roman", 12, false, false)
     .SetPosition(10, 15)
     .AddLabel("Hello World"); 	
```

### Basic fonts
Support for pdf base fonts: Time new Roman, Courier, Helvetica, ZapfDingbats and Symbols. Any of those fonts are already included in any Pdf reader.

```csharp
SharpPdf pdf = new SharpPdf();
pdf.Catalog.Pages
  .AddPage()                        
     .SetFont("Times roman", 12, false, false)
     .SetPosition(10, 15)
     .AddLabel("Hello World"); 	
```

### TrueType fonts
Library can read and use ttf fonts located with the application or in typical OS forldes. For speed considerations, the filename of the font must be used.

```csharp
SharpPdf pdf = new SharpPdf();
pdf.Catalog.Pages
  .AddPage()                        
     .SetFont("OpenSans-Regular", 12)
     .SetPosition(10, 15)
     .AddLabel("Hello World"); 	
```

### TrueType subsetings / embedding fonts
Maybe you want to include the font inside the Pdf or you want to use unicode characters, so the font must be included inside the Pdf:

```csharp
SharpPdf pdf = new SharpPdf();
pdf.Catalog.Pages
  .AddPage()                        
     .SetFont("OpenSans-Regular", 12, Embedded.Yes)
     .SetPosition(10, 15)
     .AddLabel("Α α:Alpha. Β β: Beta. Γ γ: Gamma. Δ δ: Delta"); 	
```

### Images
Includes an image.

```csharp
SharpPdf pdf = new SharpPdf();
pdf.Catalog.Pages
  .AddPage()        
                                                              // widh, 0, 0, height, X, Y
    .CurrentTransformationMatrix(300, 0, 0, 500, 50, 100)
    .AddImage("samples/image.jpg")
```

[![Codacy Badge](https://api.codacy.com/project/badge/Grade/e0969b64ccbf42aa8011a605a5fc2770)](https://app.codacy.com/manual/havocbcn/SharpPDF?utm_source=github.com&utm_medium=referral&utm_content=havocbcn/SharpPDF&utm_campaign=Badge_Grade_Dashboard)
