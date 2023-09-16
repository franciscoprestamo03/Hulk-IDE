namespace HulkPL;

public class FunctionDeclarationNode : Node
{
    public string Name { get; }
    public List<VariableDeclarationNode> Parameters;
    public List<Node>? Body { get; }
    public Node ReturnNode { get; }

    public Type type  { get; }

    public FunctionDeclarationNode(string name, List<VariableDeclarationNode> parameters, List<Node>? body, Node returnNode,Type ptype)
    {
        Name = name;
        Parameters = parameters;
        Body = body;
        ReturnNode = returnNode;
        type = ptype;
    }

    public override void Accept(Visitor visitor)
    {
        visitor.VisitFunctionDeclarationNode(this);
    }
}
