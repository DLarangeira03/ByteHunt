using System.Text;

namespace byte_hunt.Models.Comparador;

public class AttributeComparer {
    public enum ComparisonDirection {
        HigherIsBetter,
        LowerIsBetter
    }

    public class AttributeRule {
        public string Key { get; set; } = string.Empty;
        public ComparisonDirection Direction { get; set; }
        public string? Unit { get; set; } 
    }

    public static List<HighlightType> Compare(List<string> values, AttributeRule rule) {
        
        // Casos de Sim e nao
        if (values.All(v => v.Trim().Equals("Sim", StringComparison.OrdinalIgnoreCase) ||
                            v.Trim().Equals("Não", StringComparison.OrdinalIgnoreCase))) {
            return values.Select(v =>
                v.Trim().Equals("Sim", StringComparison.OrdinalIgnoreCase)
                    ? HighlightType.Best
                    : HighlightType.None
            ).ToList();
        }

        // Comparacao numerica
        var parsed = values.Select(TryParseValue).ToList();

        if (parsed.Any(p => p == null))
            return values.Select(_ => HighlightType.None).ToList();

        var min = parsed.Min();
        var max = parsed.Max();

        return parsed.Select(p => {
            if (rule.Direction == ComparisonDirection.HigherIsBetter) {
                if (p == max) return HighlightType.Best;
                if (p == min) return HighlightType.Worst;
            } else {
                if (p == min) return HighlightType.Best;
                if (p == max) return HighlightType.Worst;
            }
            return HighlightType.None;
        }).ToList();
    }

    private static decimal? TryParseValue(string input) {
        var cleaned = input
            .Normalize(NormalizationForm.FormKD)
            .Replace("€", "")
            .Replace("\"", "")
            .Replace("\u00A0", "")
            .Replace("\u200B", "")
            .Trim();
        
        // parse especial pra resolucoes
        if (cleaned.Contains('x') && cleaned.Count(c => c == 'x') == 1) {
            var parts = cleaned.Split('x');
            if (int.TryParse(parts[0].Trim(), out var width) && int.TryParse(parts[1].Trim(), out var height))
                return width * height;
        }

        return decimal.TryParse(
            cleaned,
            System.Globalization.NumberStyles.Number,
            System.Globalization.CultureInfo.InvariantCulture,
            out var value
        ) ? value : null;
    }
}
