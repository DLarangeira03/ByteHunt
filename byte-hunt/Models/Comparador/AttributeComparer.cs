using System.Text;

namespace byte_hunt.Models.Comparador;

/// <summary>
///     Classe responsável por comparar atributos de diferentes itens.
///     Permite determinar quais valores são melhores ou piores com base em regras definidas.
/// </summary>
public class AttributeComparer {
    /// <summary>
    ///     Define a direção da comparação para um atributo.
    ///     Indica se valores mais altos ou mais baixos são considerados melhores.
    /// </summary>
    public enum ComparisonDirection {
        /// <summary>
        ///     Valores mais altos são considerados melhores.
        ///     Exemplos: Memória RAM, Armazenamento, Resolução.
        /// </summary>
        HigherIsBetter,
        
        /// <summary>
        ///     Valores mais baixos são considerados melhores.
        ///     Exemplos: Preço, Peso, Tempo de Resposta.
        /// </summary>
        LowerIsBetter
    }

    /// <summary>
    ///     Define uma regra para comparação de um atributo específico.
    ///     Contém a chave do atributo, a direção da comparação e a unidade de medida.
    /// </summary>
    public class AttributeRule {
        /// <summary>
        ///     Nome ou identificador do atributo.
        ///     Exemplo: "Memória RAM", "Preço", "Peso".
        /// </summary>
        public string Key { get; set; } = string.Empty;
        
        /// <summary>
        ///     Direção da comparação para este atributo.
        ///     Define se valores mais altos ou mais baixos são melhores.
        /// </summary>
        public ComparisonDirection Direction { get; set; }
        
        /// <summary>
        ///     Unidade de medida do atributo.
        ///     Exemplo: "GB", "€", "kg".
        /// </summary>
        public string? Unit { get; set; } 
    }

    /// <summary>
    ///     Compara uma lista de valores de atributos e determina quais são os melhores ou piores.
    ///     Suporta comparações booleanas (Sim/Não) e numéricas.
    /// </summary>
    /// <param name="values">Lista de valores a serem comparados.</param>
    /// <param name="rule">Regra que define como os valores devem ser comparados.</param>
    /// <returns>Lista de tipos de destaque para cada valor (Melhor, Pior ou Nenhum).</returns>
    public static List<HighlightType> Compare(List<string> values, AttributeRule rule) {
        
        // Case onde todos os valores são "Sim" ou "Não"
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
        
        // Verifica se todos os valores são nulos ou se a conversão falhou
        if (parsed.Any(p => p == null))
            return values.Select(_ => HighlightType.None).ToList();
        
        // Converte para decimal
        var min = parsed.Min();
        var max = parsed.Max();
        
        // Retorna os destaques baseados na regra de comparação
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

    /// <summary>
    ///     Tenta converter uma string em um valor decimal para comparação.
    ///     Realiza limpeza de símbolos e formatos especiais.
    /// </summary>
    /// <param name="input">String contendo o valor a ser convertido.</param>
    /// <returns>Valor decimal ou null se a conversão não for possível.</returns>
    private static decimal? TryParseValue(string input) {
        // Normaliza a string removendo espaços, símbolos e caracteres especiais
        var cleaned = input
            .Normalize(NormalizationForm.FormKD)
            .Replace("€", "")
            .Replace("\"", "")
            .Replace("\u00A0", "")
            .Replace("\u200B", "")
            .Trim();
        
        // parse especial pra resoluções
        if (cleaned.Contains('x') && cleaned.Count(c => c == 'x') == 1) {
            var parts = cleaned.Split('x');
            if (int.TryParse(parts[0].Trim(), out var width) && int.TryParse(parts[1].Trim(), out var height))
                return width * height;
        }
        
        // Retorna o valor decimal se a conversão for bem-sucedida
        return decimal.TryParse(
            cleaned,
            System.Globalization.NumberStyles.Number,
            System.Globalization.CultureInfo.InvariantCulture,
            out var value
        ) ? value : null;
    }
}
