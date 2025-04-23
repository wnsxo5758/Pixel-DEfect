public class BehaviorTree
{
    private Node root;

    public BehaviorTree(Node root)
    {
        this.root = root;
        Blackboard = new Blackboard();
    }

    public Blackboard Blackboard { get; set; }

    public NodeState Evaluate()
    {
        return root.Evaluate();
    }
}
