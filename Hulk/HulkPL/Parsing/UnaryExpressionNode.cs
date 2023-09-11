namespace HulkPL;

public class UnaryExpressionNode : Node
{
    public Token Operator { get; set; }
    public Node Expression { get; set; }

    public UnaryExpressionNode(Token op, Node expr)
    {
        this.Operator = op;
        this.Expression = expr;
    }

    public override void Accept(Visitor visitor)
    {
        visitor.VisitUnaryExpressionNode(this);
    }

    public override IEnumerable<Node> GetChildren()
    {
        yield return Expression;
    }
}
