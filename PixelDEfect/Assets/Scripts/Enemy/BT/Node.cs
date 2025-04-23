using System.Collections.Generic;

public enum NodeState
{
    Running, // 실행 중
    Success, // 성공
    Failure  // 실패
}

public abstract class Node
{
    protected NodeState state;
    protected List<Node> children = new List<Node>();
    protected Node parent;
    
    // 노드가 실행될 때 호출되는 메서드
    public abstract NodeState Evaluate();
    
    // 노드 메서드
    public void AddChild(Node child)
    {
        children.Add(child);
        child.parent = this;
    }
}
