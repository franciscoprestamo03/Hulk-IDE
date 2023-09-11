namespace HulkPL;

public class WhileNode : Node
{
    public Node Condition { get; }
    public List<Node> BodyStatements { get; }

    public WhileNode(Node condition, List<Node> bodyStatements)
    {
        Condition = condition;
        BodyStatements = bodyStatements;
    }

    public override void Accept(Visitor visitor)
    {
        visitor.VisitWhileNode(this);
    }
}
