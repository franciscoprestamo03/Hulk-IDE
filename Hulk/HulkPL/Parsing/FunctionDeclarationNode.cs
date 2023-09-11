namespace HulkPL;

public class FunctionDeclarationNode : Node
{
    public string Name { get; }
    public List<Token> Parameters { get; }
    public List<Node> Body { get; }
    public Node? ReturnNode { get; }

    public FunctionDeclarationNode(string name, List<Token> parameters, List<Node> body, Node? returnNode)
    {
        Name = name;
        Parameters = parameters;
        Body = body;
        ReturnNode = returnNode;
    }

    public override void Accept(Visitor visitor)
    {
        visitor.VisitFunctionDeclarationNode(this);
    }
}
