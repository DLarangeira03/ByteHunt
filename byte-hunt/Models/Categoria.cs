using System.ComponentModel.DataAnnotations;

namespace byte_hunt.Models;

public class Categoria
{
    //Atributos da classe
    [Key]
    public int Id { get; set; } //identificador
    
    [Required(ErrorMessage = "O nome da categoria é obrigatório.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 50 caracteres.")]
    [Display(Name = "Nome da Categoria")]
    public string Nome { get; set; } //nome da categoria
    
    [Display(Name = "Descrição")]
    public string? Descricao { get; set; } //Descrição da categoria (caso seja necessário)
    
    public ICollection<Item> Itens { get; set; } = new List<Item>();// Lista de itens associados a esta categoria
}