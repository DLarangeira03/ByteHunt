namespace byte_hunt.Models.Popups;

/// <summary>
///     Modelo para popups informativos.
///     Utilizado para exibir mensagens simples ou avisos ao utilizador.
/// </summary>
public class InfoPopupViewModel : PopupViewModel{
    /// <summary>
    ///     Texto da mensagem a ser exibida no popup.
    /// </summary>
    public string? Mensagem { get; set; } = "Isto é um popup";
}