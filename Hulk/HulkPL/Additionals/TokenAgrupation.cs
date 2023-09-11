namespace HulkPL;
public class TokenAgrupation
{
    public TokenGroup Group { get; private set; }
    public string Value { get; private set; }

    public TokenAgrupation(TokenGroup group, string value)
    {
        this.Group = group;
        this.Value = value;
    }
}

