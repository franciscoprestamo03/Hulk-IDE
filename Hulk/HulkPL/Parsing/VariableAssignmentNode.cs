namespace HulkPL;

public class VariableAssignmentNode : Node
{
    public string Name { get; set; }
    public Node Value { get; }

    public VariableAssignmentNode(string name, Node value)
    {
        Name = name;
        Value = value;
    }

    public override void Accept(Visitor visitor)
    {
        visitor.VisitVariableAssignmentNode(this);
    }

    public override IEnumerable<Node> GetChildren()
    {
        yield return Value;
    }
}
