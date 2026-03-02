using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace SAE.Print.Labels.Modelos
{
    public class GlabelsTemplate
    {
        [JsonPropertyName("brand")]
        public string Brand { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("part")]
        public string Part { get; set; }

        [JsonPropertyName("size")]
        public string Size { get; set; }

        [JsonPropertyName("productUrl")]
        public string ProductUrl { get; set; }

        [JsonPropertyName("labelRectangle")]
        public LabelRectangle LabelRectangle { get; set; }

        [JsonPropertyName("objects")]
        public List<TemplateObject> Objects { get; set; } = new List<TemplateObject>();

        [JsonPropertyName("variables")]
        public List<TemplateVariable> Variables { get; set; } = new List<TemplateVariable>();

        [JsonPropertyName("lastModified")]
        public DateTime LastModified { get; set; } = DateTime.Now;

        [JsonPropertyName("filePath")]
        public string FilePath { get; set; }

        [JsonIgnore]
        public Dictionary<string, IncrementalState> IncrementalStates { get; set; } = new Dictionary<string, IncrementalState>();
    }

    public class LabelRectangle
    {
        [JsonPropertyName("width")]
        public double Width { get; set; }

        [JsonPropertyName("height")]
        public double Height { get; set; }

        [JsonPropertyName("round")]
        public double Round { get; set; }

        [JsonPropertyName("xWaste")]
        public double XWaste { get; set; }

        [JsonPropertyName("yWaste")]
        public double YWaste { get; set; }

        [JsonPropertyName("layout")]
        public Layout Layout { get; set; }
    }

    public class Layout
    {
        [JsonPropertyName("dx")]
        public double Dx { get; set; }

        [JsonPropertyName("dy")]
        public double Dy { get; set; }

        [JsonPropertyName("nx")]
        public int Nx { get; set; }

        [JsonPropertyName("ny")]
        public int Ny { get; set; }

        [JsonPropertyName("x0")]
        public double X0 { get; set; }

        [JsonPropertyName("y0")]
        public double Y0 { get; set; }
    }

    public class TransformationMatrix
    {
        [JsonPropertyName("a")]
        public double A { get; set; } = 1;  // Scale X

        [JsonPropertyName("b")]
        public double B { get; set; } = 0;  // Shear Y

        [JsonPropertyName("c")]
        public double C { get; set; } = 0;  // Shear X

        [JsonPropertyName("d")]
        public double D { get; set; } = 1;  // Scale Y

        [JsonPropertyName("e")]
        public double E { get; set; } = 0;  // Translate X

        [JsonPropertyName("f")]
        public double F { get; set; } = 0;  // Translate Y

        [JsonIgnore]
        public bool IsIdentity => A == 1 && B == 0 && C == 0 && D == 1 && E == 0 && F == 0;

        // Constructor por defecto (matriz identidad)
        public TransformationMatrix() { }

        // Constructor con parámetros
        public TransformationMatrix(double a, double b, double c, double d, double e, double f)
        {
            A = a;
            B = b;
            C = c;
            D = d;
            E = e;
            F = f;
        }

        // Método para crear una matriz de rotación
        public static TransformationMatrix CreateRotationMatrix(double angleInDegrees)
        {
            var angleInRadians = angleInDegrees * Math.PI / 180.0;
            var cos = Math.Cos(angleInRadians);
            var sin = Math.Sin(angleInRadians);

            return new TransformationMatrix(
                cos, -sin,
                sin, cos,
                0, 0
            );
        }

        // Método para crear una matriz de escala
        public static TransformationMatrix CreateScaleMatrix(double scaleX, double scaleY)
        {
            return new TransformationMatrix(
                scaleX, 0,
                0, scaleY,
                0, 0
            );
        }

        // Método para crear una matriz de traslación
        public static TransformationMatrix CreateTranslationMatrix(double translateX, double translateY)
        {
            return new TransformationMatrix(
                1, 0,
                0, 1,
                translateX, translateY
            );
        }
    }

    public class ShadowEffect
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        [JsonPropertyName("color")]
        public string Color { get; set; }

        [JsonPropertyName("opacity")]
        public double Opacity { get; set; }

        [JsonPropertyName("offsetX")]
        public double OffsetX { get; set; }

        [JsonPropertyName("offsetY")]
        public double OffsetY { get; set; }
    }

    public class TemplateVariable
    {
        [JsonPropertyName("name")]
        [XmlAttribute("name")]
        public string Name { get; set; }

        [JsonPropertyName("type")]
        [XmlAttribute("type")]
        public string Type { get; set; }

        [JsonPropertyName("initialValue")]
        [XmlAttribute("initialValue")]
        public string InitialValue { get; set; }

        [JsonPropertyName("increment")]
        [XmlAttribute("increment")]
        public string Increment { get; set; } = "never";  // "never", "per_copy", "per_page", "per_session"

        [JsonPropertyName("stepSize")]
        [XmlAttribute("stepSize")]
        public int StepSize { get; set; } = 0;

        [JsonIgnore]
        [XmlIgnore]
        public int CurrentValue { get; set; }

        [JsonIgnore]
        [XmlIgnore]
        public bool IsIncremental => Increment != "never" && StepSize != 0;

        // Método para inicializar el valor actual
        public void Initialize()
        {
            if (int.TryParse(InitialValue, out int initial))
            {
                CurrentValue = initial;
            }
            else
            {
                CurrentValue = 0;
            }
        }

        // Método para incrementar el valor
        public void IncrementStep()
        {

            CurrentValue += StepSize;

        }

        // Método para obtener el valor formateado
        public string GetFormattedValue()
        {
            return CurrentValue.ToString();
        }

        // Método para reiniciar al valor inicial
        public void Reset()
        {
            Initialize();
        }
    }


    // Clase para manejar el estado incremental de la plantilla
    public class IncrementalState
    {
        [JsonPropertyName("variableName")]
        public string VariableName { get; set; }

        [JsonPropertyName("lastValue")]
        public int LastValue { get; set; }

        [JsonPropertyName("lastUsed")]
        public DateTime LastUsed { get; set; }

        [JsonPropertyName("totalIncrements")]
        public int TotalIncrements { get; set; }

        public void Update(int newValue)
        {
            LastValue = newValue;
            LastUsed = DateTime.Now;
            TotalIncrements++;
        }
    }

    // Clase helper para manejo de variables incrementales
    public static class IncrementalVariableHelper2
    {
        public static void InitializeIncrementalVariables(GlabelsTemplate template)
        {
            foreach (var variable in template.Variables)
            {
                if (variable.Increment != "never")
                {
                    variable.Initialize();

                    if (!template.IncrementalStates.ContainsKey(variable.Name))
                    {
                        template.IncrementalStates[variable.Name] = new IncrementalState
                        {
                            VariableName = variable.Name,
                            LastValue = variable.CurrentValue,
                            LastUsed = DateTime.Now,
                            TotalIncrements = 0
                        };
                    }
                }
            }
        }

        public static Dictionary<string, string> ProcessIncrementalVariables(
            GlabelsTemplate template,
            Dictionary<string, string> originalData,
            int currentCopy = 1)
        {
            var processedData = new Dictionary<string, string>(originalData);

            foreach (var variable in template.Variables)
            {
                if (variable.Increment != "never")
                {
                    // Calcular valor basado en el modo de incremento
                    int calculatedValue = variable.Increment switch
                    {
                        "per_copy" => variable.CurrentValue + ((currentCopy - 1) * variable.StepSize),
                        "per_page" => variable.CurrentValue + ((currentCopy - 1) * variable.StepSize),
                        "per_session" => variable.CurrentValue,
                        _ => variable.CurrentValue
                    };

                    processedData[variable.Name] = calculatedValue.ToString();

                    // Actualizar estado
                    if (template.IncrementalStates.ContainsKey(variable.Name))
                    {
                        template.IncrementalStates[variable.Name].Update(calculatedValue);
                    }
                }
            }

            return processedData;
        }

        public static void UpdateIncrementalVariables(GlabelsTemplate template, int copiesPrinted = 1)
        {
            foreach (var variable in template.Variables)
            {
                if (variable.Increment == "per_copy" || variable.Increment == "per_page")
                {
                    // Solo actualizar el valor base si es por copia/página
                    for (int i = 0; i < copiesPrinted; i++)
                    {
                        variable.IncrementStep();
                    }
                }
            }
        }

        public static void ResetAllIncrementalVariables(GlabelsTemplate template)
        {
            foreach (var variable in template.Variables)
            {
                if (variable.Increment != "never")
                {
                    variable.Reset();
                }
            }

            template.IncrementalStates.Clear();
        }

        public static Dictionary<string, object> GetIncrementalVariablesInfo(GlabelsTemplate template)
        {
            var info = new Dictionary<string, object>();

            foreach (var variable in template.Variables.Where(v => v.Increment != "never"))
            {
                info[variable.Name] = new
                {
                    Type = variable.Type,
                    InitialValue = variable.InitialValue,
                    CurrentValue = variable.CurrentValue,
                    IncrementMode = variable.Increment,
                    StepSize = variable.StepSize,
                    State = template.IncrementalStates.ContainsKey(variable.Name)
                        ? template.IncrementalStates[variable.Name]
                        : null
                };
            }

            return info;
        }

        public static bool HasIncrementalVariables(GlabelsTemplate template)
        {
            return template.Variables.Any(v => v.Increment != "never");
        }
    }
}
