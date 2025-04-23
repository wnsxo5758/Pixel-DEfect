public class BehaviorTree
{
    private Node root;
    private Blackboard blackboard;

    public BehaviorTree(Node root)
    {
        this.root = root;
        blackboard = new Blackboard();
    }

    public Blackboard Blackboard => blackboard;

    public NodeState Evaluate()
    {
        return root.Evaluate();
    }
}
