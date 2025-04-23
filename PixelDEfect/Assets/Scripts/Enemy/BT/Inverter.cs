public class Inverter : Node
{
    // 인버터 데코레이터: 자식 노드의 결과를 반전
    public Inverter(Node child)
    {
        AddChild(child);
    }

    public override NodeState Evaluate()
    {
        switch (children[0].Evaluate())
        {
            case NodeState.Failure:
                state = NodeState.Success;
                return state;
            case NodeState.Success:
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
