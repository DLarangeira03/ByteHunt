namespace byte_hunt.Models.Comparador;

/// <summary>
///     Modelo de vista para a seleção de itens para comparação.
///     Utilizado na interface onde o utilizador escolhe quais itens deseja comparar.
/// </summary>
public class ItemCompareSelectViewModel {
    /// <summary>
    ///     Lista de todos os itens disponíveis para seleção.
    ///     Permite ao utilizador escolher quais itens deseja incluir na comparação.
    /// </summary>
    public List<Item> AllItems { get; set; }
}