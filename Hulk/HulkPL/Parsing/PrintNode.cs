namespace HulkPL;

public class PrintNode : Node
{
    public Node Expression { get; }

    public PrintNode(Node expression)
    {
        Expression = expression;
    }

    public override void Accept(Visitor visitor)
    {
        visitor.VisitPrintNode(this);
    }

    public override IEnumerable<Node> GetChildren()
    {
        yield return Expression;
    }
}