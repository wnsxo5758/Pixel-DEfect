public class BehaviorTree
{
    private Node root;
    private bool isPaused = false;

    public BehaviorTree(Node root)
    {
        this.root = root;
        Blackboard = new Blackboard();
    }

    public Blackboard Blackboard { get; set; }

    public void Pause()
    {
        isPaused = true;
    }

    public void Resume()
    {
        isPaused = false;
    }
    
    public NodeState Evaluate()
    {
        // 일시정지 상태면 평가하지 않고 진행 중 상태 반환
        if (isPaused)
            return NodeState.Running;
        
        return root.Evaluate();
    }
}
