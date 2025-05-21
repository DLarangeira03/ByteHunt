using System.ComponentModel.DataAnnotations;

namespace byte_hunt.Models;

public class Categoria
{
    //Atributos da classe
    [Key]
    public int Id { get; set; } //identificador
    
    [Required]
    public string Nome { get; set; } //nome da categoria
    
    public string? Descricao { get; set; } //Descrição da categoria (caso seja necessário)
    
    public ICollection<Item> Itens { get; set; } = new List<Item>();// Lista de itens associados a esta categoria
}