namespace HulkPL;

public class VariableDeclarationNode : Node
{
    public string Name { get; }
    public Node Initializer { get; }

    public VariableType VarType { get; }

    public VariableDeclarationNode(string name, Node initializer,VariableType type = VariableType.Implicit)
    {
        Name = name;
        Initializer = initializer;
        VarType = type;
    }

    public override void Accept(Visitor visitor)
    {
        visitor.VisitVariableDeclarationNode(this);
    }

    public override IEnumerable<Node> GetChildren()
    {
        yield return Initializer;
    }
}
