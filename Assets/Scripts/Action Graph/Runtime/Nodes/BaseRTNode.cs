using System;
using System.Collections.Generic;

[Serializable]
public abstract class BaseRTNode
{
    // Consider making this a dictionary?
    public List<Port> Inputs;
    public List<Port> Outputs;

    public enum NodeState
    {
        Idle,
        Running,
        Complete,
        Failed
    }

    private NodeState _state;
    public NodeState State =>  _state;

    public void Execute()
    {
        _state = NodeState.Running;
        ExecuteInternal();
    }

    protected void ExecuteInternal()
    {
    }
}

public class Port
{
    public string Name;
    public List<Port> ConnectedPorts;
    public Type Type;
    public object Value;

    public BaseRTNode Node;
}