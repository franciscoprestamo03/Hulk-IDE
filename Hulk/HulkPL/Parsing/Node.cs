namespace HulkPL;

public abstract class Node
{
    public abstract void Accept(Visitor visitor);

    public virtual IEnumerable<Node> GetChildren()
    {
        yield break;
    }

}
