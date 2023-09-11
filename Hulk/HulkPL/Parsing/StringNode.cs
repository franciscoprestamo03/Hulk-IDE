namespace HulkPL;

public class StringNode : Node
{
    public string Value { get; }

    public StringNode(string value)
    {
        this.Value = value;
    }

    public override void Accept(Visitor visitor)
    {
        visitor.VisitStringNode(this);
    }
}
