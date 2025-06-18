namespace byte_hunt.Models.Comparador;

public class ItemComparisonViewModel {
    public List<Item> Items { get; set; }
    public List<AttrComparisonRow> AttrRows { get; set; }
    
}

//model para cada row de comparacao
public class AttrComparisonRow {
    public string Key { get; set; }
    public List<string> Values { get; set; }
    public List<HighlightType> Highlights { get; set; } = new();
}

public enum HighlightType {
    None,
    Best,
    Worst
}