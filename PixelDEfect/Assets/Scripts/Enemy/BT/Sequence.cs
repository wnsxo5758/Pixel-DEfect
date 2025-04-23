public class Sequence : Node
{
    // 모든 자식이 성공해야 성공, 하나라도 실패하면 실패
    public Sequence() { }

    public override NodeState Evaluate()
    {
        bool anyChildRunning = false;

        foreach (Node child in children)
        {
            switch (child.Evaluate())
            {
                case NodeState.Failure:
                    state = NodeState.Failure;
                    return state;
                case NodeState.Success:
                    continue;
                case NodeState.Running:
                    anyChildRunning = true;
                    continue;
                default:
                    state = NodeState.Success;
                    return state;
            }
        }

        state = anyChildRunning ? NodeState.Running : NodeState.Success;
        return state;
    }
}
