namespace LLama.Sampling;

/// <summary>
/// A grammar in GBNF form.
/// </summary>
public record class Grammar
{
    public string Gbnf { get; init; }
    public string Root { get; init; }

    public Grammar(string gbnf, string root)
    {
        Gbnf = gbnf;
        Root = root;
    }
}