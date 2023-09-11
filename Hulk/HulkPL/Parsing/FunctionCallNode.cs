namespace HulkPL;

public class FunctionCallNode : Node
{
    public string Name { get; }
    public List<Node> Arguments { get; }

    public FunctionCallNode(string name, List<Node> arguments)
    {
        Name = name;
        Arguments = arguments;
    }

    public override void Accept(Visitor visitor)
    {
        visitor.VisitFunctionCallNode(this);
    }

    public override IEnumerable<Node> GetChildren()
    {
        foreach (var argument in Arguments)
        {
            yield return argument;
        }
    }
}
