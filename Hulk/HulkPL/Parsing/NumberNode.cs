namespace HulkPL;

public class NumberNode : Node
{
    public double Value { get; set; }

    public NumberNode(double value)
    {
        this.Value = value;
    }

    public override void Accept(Visitor visitor)
    {
        visitor.VisitNumberNode(this);
    }
}
