namespace HulkPL;

public class IfNode : Node
{
    public Node Condition { get; }
    public List<Node> ThenStatements { get; }
    public List<Node> ElseStatements { get; }

    public IfNode(Node condition, List<Node> thenStatements, List<Node> elseStatements)
    {
        Condition = condition;
        ThenStatements = thenStatements;
        ElseStatements = elseStatements;
    }

    public override void Accept(Visitor visitor)
    {
        visitor.VisitIfNode(this);
    }
}
