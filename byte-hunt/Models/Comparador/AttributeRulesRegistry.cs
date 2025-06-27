using System.Collections.Generic;

namespace byte_hunt.Models.Comparador;

/// <summary>
///     Registo estático de regras de comparação para diferentes atributos.
///     Define como cada tipo de atributo deve ser comparado e destacado.
/// </summary>
public static class AttributeRulesRegistry {
    /// <summary>
    ///     Dicionário de regras para diferentes atributos de itens.
    ///     A chave é o nome do atributo e o valor é a regra de comparação.
    ///     Cada regra define se valores mais altos ou mais baixos são melhores,
    ///     e a unidade de medida do atributo quando aplicável.
    /// </summary>
    public static Dictionary<string, AttributeComparer.AttributeRule> Rules = new() {
        { "Armazenamento", new() {
            Key = "Armazenamento", Direction = AttributeComparer.ComparisonDirection.HigherIsBetter, 
            Unit = "GB"
        } },
        { "Autonomia", new() {
            Key = "Autonomia", Direction = AttributeComparer.ComparisonDirection.HigherIsBetter, 
            Unit = "mAh"
        } },
        { "Bluetooth", new() {
            Key = "Bluetooth", Direction = AttributeComparer.ComparisonDirection.HigherIsBetter
        } },
        { "BoostClock", new() {
            Key = "BoostClock", Direction = AttributeComparer.ComparisonDirection.HigherIsBetter, 
            Unit = "MHz"
        } },
        { "Bus Width", new() {
            Key = "Bus Width", Direction = AttributeComparer.ComparisonDirection.HigherIsBetter, 
            Unit = "bit"
        } },
        { "CacheL3", new() {
            Key = "CacheL3", Direction = AttributeComparer.ComparisonDirection.HigherIsBetter, 
            Unit = "MB"
        } },
        { "Câmara frontal", new() {
            Key = "Câmara frontal", Direction = AttributeComparer.ComparisonDirection.HigherIsBetter, 
            Unit = "MP"
        } },
        { "Câmara principal", new() {
            Key = "Câmara principal", Direction = AttributeComparer.ComparisonDirection.HigherIsBetter, 
            Unit = "MP"
        } },
        { "Câmara traseira", new() {
            Key = "Câmara traseira", Direction = AttributeComparer.ComparisonDirection.HigherIsBetter, 
            Unit = "MP"
        } },
        { "Carregamento", new() {
            Key = "Carregamento", Direction = AttributeComparer.ComparisonDirection.HigherIsBetter, 
            Unit = "W"
        } },
        { "Carregamento wireless", new() {
            Key = "Carregamento wireless", Direction = AttributeComparer.ComparisonDirection.HigherIsBetter, 
            Unit = "W"
        } },
        { "CacheL2", new() {
            Key = "CacheL2", Direction = AttributeComparer.ComparisonDirection.HigherIsBetter, 
            Unit = "MB"
        } },
        { "Cores", new() {
            Key = "Cores", Direction = AttributeComparer.ComparisonDirection.HigherIsBetter
        } },
        { "Cores (E)", new() {
            Key = "Cores (E)", Direction = AttributeComparer.ComparisonDirection.HigherIsBetter
        } },
        { "Cores (P)", new() {
            Key = "Cores (P)", Direction = AttributeComparer.ComparisonDirection.HigherIsBetter
        } },
        { "CUDA", new() {
            Key = "CUDA", Direction = AttributeComparer.ComparisonDirection.HigherIsBetter
        } },
        { "DDR", new() {
            Key = "DDR", Direction = AttributeComparer.ComparisonDirection.HigherIsBetter
        } },
        { "DDR5", new() {
            Key = "DDR5", Direction = AttributeComparer.ComparisonDirection.HigherIsBetter, 
            Unit = "MHz"
        } },
        { "Frequência base", new() {
            Key = "Frequência base", Direction = AttributeComparer.ComparisonDirection.HigherIsBetter, 
            Unit = "GHz"
        } },
        { "Frequência base (E)", new() {
            Key = "Frequência base (E)", Direction = AttributeComparer.ComparisonDirection.HigherIsBetter, 
            Unit = "GHz"
        } },
        { "Frequência base (P)", new() {
            Key = "Frequência base (P)", Direction = AttributeComparer.ComparisonDirection.HigherIsBetter, 
            Unit = "GHz"
        } },
        { "Frequência turbo", new() {
            Key = "Frequência turbo", Direction = AttributeComparer.ComparisonDirection.HigherIsBetter, 
            Unit = "GHz"
        } },
        { "Largura de banda", new() {
            Key = "Largura de banda", Direction = AttributeComparer.ComparisonDirection.HigherIsBetter, 
            Unit = "GB/s"
        } },
        { "LAN", new() {
            Key = "LAN", Direction = AttributeComparer.ComparisonDirection.HigherIsBetter, 
            Unit = "Gbps"
        } },
        { "M.2", new() {
            Key = "M.2", Direction = AttributeComparer.ComparisonDirection.HigherIsBetter, 
            Unit = "slots"
        } },
        { "Memória máxima", new() {
            Key = "Memória máxima", Direction = AttributeComparer.ComparisonDirection.HigherIsBetter, 
            Unit = "GB"
        } },
        { "PCIe", new() {
            Key = "PCIe", Direction = AttributeComparer.ComparisonDirection.HigherIsBetter
        } },
        { "Peso", new() {
            Key = "Peso", Direction = AttributeComparer.ComparisonDirection.LowerIsBetter, 
            Unit = "g"
        } },
        { "Peso com suporte", new() {
            Key = "Peso com suporte", Direction = AttributeComparer.ComparisonDirection.LowerIsBetter, 
            Unit = "kg"
        } },
        { "Peso sem suporte", new() {
            Key = "Peso sem suporte", Direction = AttributeComparer.ComparisonDirection.LowerIsBetter, 
            Unit = "kg"
        } },
        { "RAM", new() {
            Key = "RAM", Direction = AttributeComparer.ComparisonDirection.HigherIsBetter, 
            Unit = "GB"
        } },
        { "Resposta", new() {
            Key = "Resposta", Direction = AttributeComparer.ComparisonDirection.LowerIsBetter, 
            Unit = "ms"
        } },
        { "Resolução", new() {
            Key = "Resolução", Direction = AttributeComparer.ComparisonDirection.HigherIsBetter
        } },
        { "Shaders", new() {
            Key = "Shaders", Direction = AttributeComparer.ComparisonDirection.HigherIsBetter
        } },
        { "Tamanho do ecrã", new() {
            Key = "Tamanho do ecrã", Direction = AttributeComparer.ComparisonDirection.HigherIsBetter, 
            Unit = "\""
        } },
        { "Taxa de atualização", new() {
            Key = "Taxa de atualização", Direction = AttributeComparer.ComparisonDirection.HigherIsBetter, 
            Unit = "Hz"
        } },
        { "TDP", new() {
            Key = "TDP", Direction = AttributeComparer.ComparisonDirection.LowerIsBetter, 
            Unit = "W"
        } },
        { "TGP GPU", new() {
            Key = "TGP GPU", Direction = AttributeComparer.ComparisonDirection.HigherIsBetter, 
            Unit = "W"
        } },
        { "Tensor Cores", new() {
            Key = "Tensor Cores", Direction = AttributeComparer.ComparisonDirection.HigherIsBetter
        } },
        { "Threads", new() {
            Key = "Threads", Direction = AttributeComparer.ComparisonDirection.HigherIsBetter
        } },
        { "Thunderbolt 4", new() {
            Key = "Thunderbolt 4", Direction = AttributeComparer.ComparisonDirection.HigherIsBetter, 
            Unit = "ports"
        } },
        { "VRAM", new() {
            Key = "VRAM", Direction = AttributeComparer.ComparisonDirection.HigherIsBetter, 
            Unit = "GB"
        } },
        { "Zoom ótico", new() {
            Key = "Zoom ótico", Direction = AttributeComparer.ComparisonDirection.HigherIsBetter,
            Unit = "x"
        } }
    };
}