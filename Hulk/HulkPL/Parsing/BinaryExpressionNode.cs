namespace HulkPL;

public class BinaryExpressionNode : Node
{
    public Node Left { get; set; }
    public Token Operator { get; set; }
    public Node Right { get; set; }

    public BinaryExpressionNode(Node left, Token op, Node right)
    {
        this.Left = left;
        this.Operator = op;
        this.Right = right;
    }

    public override void Accept(Visitor visitor)
    {
        visitor.VisitBinaryExpressionNode(this);
    }

    public override IEnumerable<Node> GetChildren()
    {
        yield return Left;
        yield return Right;
    }
}
