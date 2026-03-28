using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public abstract class BaseRTNode
{
    public string NodeId;
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

    public void Execute(RuntimeActionGraph graph)
    {
        _state = NodeState.Running;
        ExecuteInternal(graph);
    }

    protected abstract void ExecuteInternal(RuntimeActionGraph graph);

    protected void SetComplete() => _state = NodeState.Complete;
    protected void SetFailed() => _state = NodeState.Failed;

    protected Port GetInputPort(string name) => Inputs.FirstOrDefault(x => x.Name == name);
    protected Port GetOutputPort(string name) => Outputs.FirstOrDefault(x => x.Name == name);

    protected void DefExecNext(RuntimeActionGraph graph)
    {
        SetComplete();
        var execPort = GetOutputPort("Exec");
        if (execPort == null || execPort.Connections.Count == 0) return;
        graph.ResolveInputConnection(execPort.Connections[0], out var nextNode, out _);
        nextNode?.Execute(graph);
    }

    protected bool TryGetInput<T>(string portName, RuntimeActionGraph graph, out T value)
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
            Debug.LogError($"Tried to get value on input port {portName} on node ({GetType().FullName}), but requested type ({typeof(T).FullName}) does not match port type ({inputPort.Type.FullName})");
            value = default;
            return false;
        }

        if (inputPort.Connections.Count > 0)
        {
            // Only ever follow the first connection if multiple exist
            graph.ResolveOutputConnection(inputPort.Connections[0], out var upstreamNode, out var upstreamPort);

            if (upstreamNode == null)
            {
                value = default;
                return false;
            }

            if (upstreamNode.State == NodeState.Idle)
                upstreamNode.Execute(graph);

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

            value = ConvertValue<T>(upstreamPort.Value);
            return true;
        }
        else
        {
            value = ConvertValue<T>(inputPort.Value);
            return true;
        }
    }

    private static T ConvertValue<T>(object raw)
    {
        var t = typeof(T);
        if (t.IsEnum)
            return (T)Enum.ToObject(t, raw);
        return (T)Convert.ChangeType(raw, t);
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
            Debug.LogError($"Tried to set value on output port {portName} on node ({GetType().FullName}), but requested type ({typeof(T).FullName}) does not match port type ({outputPort.Type.FullName})");
            return false;
        }

        outputPort.Value = value;
        return true;
    }
}

[Serializable]
public class Port
{
    public string Name;
    public string NodeId;
    public List<string> Connections = new(); // format: "nodeId__portName"
    public string TypeName; // Assembly-qualified type name, serialized in place of Type
    public object Value;

    [JsonIgnore]
    private Type _type;

    [JsonIgnore]
    public Type Type
    {
        get => _type ??= TypeName != null ? Type.GetType(TypeName) : null;
        set { _type = value; TypeName = value?.AssemblyQualifiedName; }
    }
}

public abstract class BaseContextRTNode : BaseRTNode
{
    public List<string> BlockIds = new();
}

public abstract class BaseBlockRTNode : BaseRTNode
{
    public string ContextNodeID;
}