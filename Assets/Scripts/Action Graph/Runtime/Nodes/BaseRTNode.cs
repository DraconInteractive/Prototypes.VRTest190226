using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = System.Object;

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

    protected abstract void ExecuteInternal();

    protected Port GetInputPort(string name) => Inputs.FirstOrDefault(x => x.Name == name);
    protected Port GetOutputPort(string name) => Outputs.FirstOrDefault(x => x.Name == name);
    
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

        // Check if connected, if not, just return default
        if (inputPort.ConnectedPorts.Count > 0)
        {
            // Recursively assess inputs
            // For now we're only ever going to assess the first input connection if there are multiple
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
    public List<Port> ConnectedPorts;
    public Type Type;
    public object Value;

    public BaseRTNode Node;
}