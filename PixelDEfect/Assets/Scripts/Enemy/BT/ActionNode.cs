public class ActionNode : Node
{
    // 기본 액션 노드 클래스
    private System.Func<NodeState> action;

    public ActionNode(System.Func<NodeState> action)
    {
        this.action = action;
    }

    public override NodeState Evaluate()
    {
        switch (action())
        {
            case NodeState.Success:
                state = NodeState.Success;
                return state;
            case NodeState.Failure:
                state = NodeState.Failure;
                return state;
            case NodeState.Running:
                state = NodeState.Running;
                return state;
            default:
                state = NodeState.Failure;
                return state;
        }
    }
}
