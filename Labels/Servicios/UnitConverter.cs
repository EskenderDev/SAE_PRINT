namespace SAE.Print.Labels.Servicios
{
    public static class UnitConverter
    {
        private const double PointsPerInch = 72.0;
        private const double PointsPerMillimeter = 2.834645669;
        private const double PointsPerCentimeter = 28.34645669;

        public static float PointsToPixels(double points, float dpi = 72f)
            => (float)(points * dpi / PointsPerInch);

        public static double PointsToMillimeters(double points)
            => points / PointsPerMillimeter;

        public static double PointsToCentimeters(double points)
            => points / PointsPerCentimeter;

        public static double MillimetersToPoints(double millimeters)
            => millimeters * PointsPerMillimeter;

        public static double ParseMeasurement(string measurement, string defaultUnit = "pt")
        {
            if (string.IsNullOrWhiteSpace(measurement))
                return 0;

            measurement = measurement.Trim().ToLower();

            // Detectar dónde empieza la unidad (primer caracter no numérico)
            int unitIndex = -1;
            for (int i = 0; i < measurement.Length; i++)
            {
                if (!char.IsDigit(measurement[i]) && measurement[i] != '.' && measurement[i] != '-')
                {
                    unitIndex = i;
                    break;
                }
            }

            string numberPart;
            string unitPart;

            if (unitIndex == -1)
            {
                // No hay unidad → usar default
                numberPart = measurement;
                unitPart = defaultUnit;
            }
            else
            {
                numberPart = measurement.Substring(0, unitIndex);
                unitPart = measurement.Substring(unitIndex);
            }

            // Intentar parsear el número
            if (!double.TryParse(numberPart, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out double value))
            {
                value = 0; // fallback seguro
            }

            // Normalizar unidad
            unitPart = unitPart switch
            {
                "pt" => "pt",
                "in" => "in",
                "mm" => "mm",
                "cm" => "cm",
                "px" => "px",
                _ => defaultUnit
            };

            // Convertir a puntos
            double result = unitPart switch
            {
                "pt" => value,
                "in" => value * PointsPerInch,
                "mm" => value * PointsPerMillimeter,
                "cm" => value * PointsPerCentimeter,
                "px" => value * PointsPerInch / 96, // 96 DPI
                _ => value
            };

            // 🔒 Nunca permitir negativos
            return Math.Max(0, result);
        }
    }

}
