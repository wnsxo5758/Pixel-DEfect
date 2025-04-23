public class ConditionNode : Node
{
    // 조건 노드
    private System.Func<bool> condition;

    public ConditionNode(System.Func<bool> condition)
    {
        this.condition = condition;
    }

    public override NodeState Evaluate()
    {
        state = condition() ? NodeState.Success : NodeState.Failure;
        return state;
    }
}
