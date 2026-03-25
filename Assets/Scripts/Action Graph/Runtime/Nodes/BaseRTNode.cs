using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = System.Object;

[Serializable]
public abstract class BaseRTNode
{
    // Consider making this a dictionary?
    public List<Port> Inputs = new();
    public List<Port> Outputs = new();

    public enum NodeState
    {
        Idle,
        Running,
        Complete,
        Failed
    }

    private NodeState _state;
    public NodeState State => _state;

    public void Execute()
    {
        _state = NodeState.Running;
        ExecuteInternal();
    }

    protected abstract void ExecuteInternal();

    protected void SetComplete() => _state = NodeState.Complete;
    protected void SetFailed() => _state = NodeState.Failed;

    protected Port GetInputPort(string name) => Inputs.FirstOrDefault(x => x.Name == name);
    protected Port GetOutputPort(string name) => Outputs.FirstOrDefault(x => x.Name == name);

    protected void DefExecNext()
    {
        SetComplete();
        GetOutputPort("Exec")?.ConnectedPorts[0]?.Node.Execute();
    }
    
    protected bool TryGetInput<T>(string portName, out T value)
    {
        var inputPort = Inputs.FirstOrDefault(x => x.Name == portName);
        if (inputPort == null)
        {
            Debug.LogError($"Tried to get value on input port {portName}, but it doesn't exist on node ({GetType().FullName})");
            value = default;
            return false;
        }

        if (inputPort.Type != typeof(T))
        {
            Debug.LogError($"Tried to get value on input port {portName} on node ({GetType().FullName}), but requested type ({typeof(T).FullName}) does not match port type ({inputPort.Type.FullName}");
            value = default;
            return false;
        }

        if (inputPort.ConnectedPorts.Count > 0)
        {
            // Only ever follow the first connection if multiple exist
            var upstreamPort = inputPort.ConnectedPorts[0];
            var upstreamNode = upstreamPort.Node;

            if (upstreamNode.State == NodeState.Idle)
                upstreamNode.Execute(); // Recurse: upstream may itself call TryGetInput

            if (upstreamNode.State == NodeState.Failed)
            {
                value = default;
                return false;
            }

            if (upstreamPort.Value == null)
            {
                value = default;
                return false;
            }

            value = (T)upstreamPort.Value;
            return true;
        }
        else
        {
            value = (T)inputPort.Value;
            return true;
        }
    }
    
    protected bool TrySetOutput<T>(string portName, T value)
    {
        var outputPort = Outputs.FirstOrDefault(x => x.Name == portName);
        if (outputPort == null)
        {
            Debug.LogError($"Tried to set value on output port {portName}, but it doesn't exist on this node ({GetType().FullName})");
            return false;
        }

        if (outputPort.Type != typeof(T))
        {
            Debug.LogError($"Tried to set value on output port {portName} on node ({GetType().FullName}), but requested type ({typeof(T).FullName}) does not match port type ({outputPort.Type.FullName}");
            return false;
        }

        outputPort.Value = value;
        return true;
    }
}

public class Port
{
    public string Name;
    public List<Port> ConnectedPorts = new();
    public Type Type;
    public object Value;

    public BaseRTNode Node;
}