namespace byte_hunt.Models.Comparador;

/// <summary>
///     Modelo de vista para a exibição da comparação entre itens.
///     Contém os itens a serem comparados e as linhas de atributos para comparação.
/// </summary>
public class ItemComparisonViewModel {
    /// <summary>
    ///     Lista de itens selecionados para comparação.
    ///     Contém os objetos de Item completos que estão a ser comparados.
    /// </summary>
    public List<Item> Items { get; set; }
    
    /// <summary>
    ///     Lista de linhas de comparação de atributos.
    ///     Cada linha representa um atributo com os valores correspondentes para cada item.
    /// </summary>
    public List<AttrComparisonRow> AttrRows { get; set; }
    
}

/// <summary>
///     Modelo para cada linha de comparação de atributos.
///     Representa um atributo específico e seus valores para cada item comparado.
/// </summary>
public class AttrComparisonRow {
    /// <summary>
    ///     Nome ou chave do atributo a ser comparado.
    ///     Exemplo: "Memória RAM", "Processador", "Preço".
    /// </summary>
    public string Key { get; set; }
    
    /// <summary>
    ///     Lista de valores do atributo para cada item na comparação.
    ///     A posição na lista corresponde à posição do item na lista de Items.
    /// </summary>
    public List<string> Values { get; set; }
    public List<HighlightType> Highlights { get; set; } = new();
}

/// <summary>
///   Enumeração para tipos de destaque de atributos na comparação.
/// </summary>
public enum HighlightType {
    None,
    Best,
    Worst
}