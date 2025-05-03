using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace byte_hunt.Models;

public class Comparacao
{
    //Atributos da classe
    [Key]
    public int Id { get; set; } //identificador
    
    [Required]
    public DateTime Data { get; set; } //Data da comparação
    
    [Required]
    public int UtilizadorId { get; set; }// Chave estrangeira
    
    [ForeignKey("UtilizadorId")]
    public Utilizador Utilizador { get; set; } //utilizador que realiza a comparação
    
    public ICollection<Item> Itens { get; set; } = new List<Item>();//Relação com os itens comparados
}