using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace byte_hunt.Models;

public class Item
{ 
    //Atributos da classe
    [Key]
    public int Id { get; set; } //identificador
    
    [Required]
    public string Nome { get; set; } //nome do item
    
    
    public string Marca { get; set; } //marca do item
    
    [Required]
    public string Descricao { get; set; } //descrição do item
    
    public decimal Preco { get; set; } //preço do item
    
    public string FotoItem {get;set;} //foto do item
    
    [Required]
    public int CategoriaId { get; set; } //chave estrangeira
    
    [ForeignKey("CategoriaId")]
    public Categoria Categoria { get; set; } //categoria do item
    
    public ICollection<Comparacao> Comparacoes { get; set; } = new List<Comparacao>(); //Relação muitos-para-muitos com Comparacao   

        
    
}