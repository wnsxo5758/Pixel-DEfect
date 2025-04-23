public class Selector : Node
{
    // 셀렉터 노드: 하나라도 성공하면 성공, 모두 실패해야 실패
    public Selector() { }

    public override NodeState Evaluate()
    {
        foreach (Node child in children)
        {
            switch (child.Evaluate())
            {
                case NodeState.Failure:
                    continue;
                case NodeState.Success:
                    state = NodeState.Success;
                    return state;
                case NodeState.Running:
                    state = NodeState.Running;
                    return state;
                default:
                    continue;
            }
        }

        state = NodeState.Failure;
        return state;
    }
}
