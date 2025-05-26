namespace byte_hunt.Models.Popups;

public class ConfirmPopupViewModel : PopupViewModel {
    
    public string Pergunta { get; set; } = "Tem certeza que deseja continuar?";
    public string ConfirmBtnText { get; set; } = "Sim";
    public string CancelBtnText { get; set; } = "Não";
    public string ActionUrl { get; set; }
    public string FormMethod { get; set; } = "POST";

}