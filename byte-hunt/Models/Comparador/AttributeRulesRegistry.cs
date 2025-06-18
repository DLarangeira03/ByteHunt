using System.Collections.Generic;

namespace byte_hunt.Models.Comparador;

public static class AttributeRulesRegistry {
    public static Dictionary<string, AttributeComparer.AttributeRule> Rules = new() {
        {
            "Memória RAM", new() {
                Key = "Memória RAM", Direction = AttributeComparer.ComparisonDirection.HigherIsBetter, Unit = "GB"
            }
        }, {
            "Armazenamento", new() {
                Key = "Armazenamento", Direction = AttributeComparer.ComparisonDirection.HigherIsBetter, Unit = "GB"
            }
        }, {
            "Autonomia da Bateria", new() {
                Key = "Autonomia da Bateria", Direction = AttributeComparer.ComparisonDirection.HigherIsBetter,
                Unit = "mAh"
            }
        }, {
            "Peso", new() {
                Key = "Peso", Direction = AttributeComparer.ComparisonDirection.LowerIsBetter, Unit = "g"
            }
        }, {
            "Ecrã", new() {
                Key = "Ecrã", Direction = AttributeComparer.ComparisonDirection.HigherIsBetter, Unit = "\""
            }
        }, {
            "Tamanho do Ecrã", new() {
                Key = "Tamanho do Ecrã", Direction = AttributeComparer.ComparisonDirection.HigherIsBetter, Unit = "\""
            }
        }, {
            "Câmera", new() {
                Key = "Câmera", Direction = AttributeComparer.ComparisonDirection.HigherIsBetter, Unit = "MP"
            }
        }, {
            "Brilho", new() {
                Key = "Brilho", Direction = AttributeComparer.ComparisonDirection.HigherIsBetter, Unit = "nits"
            }
        }, {
            "Contraste", new() {
                Key = "Contraste", Direction = AttributeComparer.ComparisonDirection.HigherIsBetter
            }
        }, {
            "Tempo de Resposta", new() {
                Key = "Tempo de Resposta", Direction = AttributeComparer.ComparisonDirection.LowerIsBetter, Unit = "ms"
            }
        }, {
            "Taxa de Atualização", new() {
                Key = "Taxa de Atualização", Direction = AttributeComparer.ComparisonDirection.HigherIsBetter,
                Unit = "Hz"
            }
        }, {
            "Preço", new() {
                Key = "Preço", Direction = AttributeComparer.ComparisonDirection.LowerIsBetter, Unit = "€"
            }
        }, {
            "Resolução", new() {
                Key = "Resolução",
                Direction = AttributeComparer.ComparisonDirection.HigherIsBetter
            }
        },
    };
}