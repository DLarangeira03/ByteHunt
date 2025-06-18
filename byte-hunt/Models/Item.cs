using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace byte_hunt.Models;

public class Item
{ 
    //Atributos da classe
    [Key]
    public int Id { get; set; } //identificador
    
    [Required]
    public string Nome { get; set; } //nome do item
    
    [Required]
    public string Marca { get; set; } //marca do item
    
    public string? Descricao { get; set; } //descrição do item
    
    public decimal? Preco { get; set; } //preço do item

    public string? FotoItem { get; set; } //foto do item
    
    public string? AttrsJson { get; set; } // JSON de atributos do item
    
    public int CategoriaId { get; set; } //chave estrangeira
    
    [ForeignKey("CategoriaId")]
    [ValidateNever]
    public Categoria Categoria { get; set; } //categoria do item
    
    public ICollection<Comparacao> Comparacoes { get; set; } = new List<Comparacao>(); //Relação muitos-para-muitos com Comparacao   

        
    
}