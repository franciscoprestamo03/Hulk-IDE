namespace HulkPL;

public class VariableReferenceNode : Node
{
    public string Name { get; }

    public VariableReferenceNode(string name)
    {
        Name = name;
    }

    public override void Accept(Visitor visitor)
    {
        visitor.VisitVariableReferenceNode(this);
    }
}