namespace byte_hunt.Models.Popups;

/// <summary>
///     Modelo para popups de confirmação.
///     Utilizado para solicitar confirmação do utilizador antes de realizar ações importantes.
/// </summary>
public class ConfirmPopupViewModel : PopupViewModel {
    
    /// <summary>
    ///     Texto da pergunta de confirmação.
    /// </summary>
    public string Pergunta { get; set; } = "Tem certeza que deseja continuar?";
    
    /// <summary>
    ///     Texto do botão de confirmação.
    /// </summary>
    public string ConfirmBtnText { get; set; } = "Sim";
    
    /// <summary>
    ///     Texto do botão de cancelamento.
    /// </summary>
    public string CancelBtnText { get; set; } = "Não";
    
    /// <summary>
    ///     URL para onde o formulário será submetido após confirmação.
    ///     Define o destino da ação quando o utilizador confirma.
    /// </summary>
    public string ActionUrl { get; set; }
    
    /// <summary>
    ///     Método HTTP a ser utilizado na submissão do formulário.
    ///     Por predefinição, utiliza o método "POST".
    /// </summary>
    public string FormMethod { get; set; } = "POST";

}