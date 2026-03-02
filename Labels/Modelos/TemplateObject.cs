namespace SAE.Print.Labels.Modelos
{
    public abstract class TemplateObject
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public bool LockAspectRatio { get; set; }
        public TransformationMatrix Matrix { get; set; } = new TransformationMatrix();
        public ShadowEffect Shadow { get; set; }
        public bool Rotate { get; set; }
        public float RotationAngle { get; set; } = 0f;
    }

    public class TextObject : TemplateObject
    {
        public string Content { get; set; }
        public string FontFamily { get; set; }
        public double FontSize { get; set; }
        public string Color { get; set; }
        public TextAlignment Alignment { get; set; }
        public VerticalAlignment VerticalAlignment { get; set; }
        public bool FontItalic { get; set; }
        public bool FontUnderline { get; set; }
        public string FontWeight { get; set; }
        public double LineSpacing { get; set; }
        public bool AutoShrink { get; set; }
        public WrapMode WrapMode { get; set; }
    }

    public class BarcodeObject : TemplateObject
    {
        public string Data { get; set; }
        public string BarcodeType { get; set; }
        public bool ShowText { get; set; }
        public bool Checksum { get; set; }
        public string Color { get; set; }
        public string Backend { get; set; }
    }

    public class BoxObject : TemplateObject
    {
        public string FillColor { get; set; }
        public string LineColor { get; set; }
        public double LineWidth { get; set; }
    }

    public class LineObject : TemplateObject
    {
        public double Dx { get; set; }
        public double Dy { get; set; }
        public string LineColor { get; set; }
        public double LineWidth { get; set; }
    }

    public class EllipseObject : TemplateObject
    {
        public string FillColor { get; set; }
        public string LineColor { get; set; }
        public double LineWidth { get; set; }
    }

    public class ImageObject : TemplateObject
    {
        public string Source { get; set; }
        public new bool LockAspectRatio { get; set; } = true;
    }

    public enum TextAlignment { Left, Center, Right }
    public enum VerticalAlignment { Top, Middle, Bottom }
    public enum WrapMode { Word, Character, None }
}
