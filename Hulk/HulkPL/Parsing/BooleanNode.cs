namespace HulkPL;

public class BooleanNode : Node
{
    public bool Value { get; }

    public BooleanNode(bool value)
    {
        Value = value;
    }

    public override void Accept(Visitor visitor)
    {
        visitor.VisitBooleanNode(this);
    }
}
