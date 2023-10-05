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

    public virtual IEnumerable<Node> GetChildren()
    {
        foreach (var item in ThenStatements)
        {
            yield return item;
        }
        foreach (var item in ElseStatements)
        {
            yield return item;
        }
    }

    public override void Accept(Visitor visitor)
    {
        visitor.VisitIfNode(this);
    }
}
