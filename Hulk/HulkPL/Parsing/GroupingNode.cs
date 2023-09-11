namespace HulkPL;

public class GroupingNode : Node
{
    public Node Expression { get; }

    public GroupingNode(Node expression)
    {
        Expression = expression;
    }

    public override void Accept(Visitor visitor)
    {
        visitor.VisitGroupingNode(this);
    }

    public override IEnumerable<Node> GetChildren()
    {
        yield return Expression;
    }
}
