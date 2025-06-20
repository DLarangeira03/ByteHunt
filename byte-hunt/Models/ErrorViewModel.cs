namespace byte_hunt.Models;

/// <summary>
///     Modelo para exibição de informações de erro.
///     Utilizado para apresentar detalhes sobre erros ocorridos na aplicação.
/// </summary>
public class ErrorViewModel {
    /// <summary>
    ///     Identificador do pedido que gerou o erro.
    ///     Útil para diagnóstico e rastreamento de problemas.
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    ///     Indica se o identificador do pedido deve ser exibido.
    ///     Retorna verdadeiro apenas se o RequestId não for nulo ou vazio.
    /// </summary>
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}