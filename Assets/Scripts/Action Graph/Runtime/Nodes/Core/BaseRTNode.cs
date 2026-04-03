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
        graph.PrintDebug(this, "Executing");
        _state = NodeState.Running;
        ExecuteInternal(graph);
    }

    protected abstract void ExecuteInternal(RuntimeActionGraph graph);

    protected void SetComplete() => _state = NodeState.Complete;
    
    protected void SetFailed() => _state = NodeState.Failed;

    public virtual void Reset(RuntimeActionGraph graph) => _state = NodeState.Idle;

    public virtual bool IsPure() => false;
    
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
            graph.PrintDebugError(this, $"Cannot find port {portName} on node");
            value = default;
            return false;
        }

        if (inputPort.Connections.Count > 0)
        {
            // Only ever follow the first connection if multiple exist
            graph.ResolveOutputConnection(inputPort.Connections[0], out var upstreamNode, out var upstreamPort);

            if (upstreamNode == null)
            {
                graph.PrintDebugError(this, $"Cannot find upstream node for port {portName}");
                value = default;
                return false;
            }

            if (upstreamNode.State == NodeState.Idle || IsPure()) // Re-execute pure nodes on every request
                upstreamNode.Execute(graph);

            if (upstreamNode.State == NodeState.Failed)
            {
                value = default;
                return false;
            }

            if (upstreamPort.Value == null)
            {
                graph.PrintDebugError(this, $"No value able to be fetched from upstream, port {portName}");
                value = default;
                return false;
            }

            bool convertable = TryConvertValue(upstreamPort.Value, portName, out value);
            if (!convertable)
            {
                graph.PrintDebugError(this, $"Cannot convert value for port {portName}");
            }
            return convertable;
        }
        else
        {
            if (inputPort.Value == null)
            {
                graph.PrintDebugError(this, $"No value implicit on port {portName}, returning null");
                value = default;
                return true; // null is a valid unset default
            }
            return TryConvertValue(inputPort.Value, portName, out value);
        }
    }

    private bool TryConvertValue<T>(object raw, string portName, out T value)
    {
        // Direct type match or subtype — no conversion needed
        if (raw is T direct)
        {
            value = direct;
            return true;
        }

        // Attempt conversion (handles numeric coercions, string parsing, etc.)
        try
        {
            var t = typeof(T);
            value = t.IsEnum
                ? (T)Enum.ToObject(t, raw)
                : (T)Convert.ChangeType(raw, t);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Port '{portName}' on ({GetType().FullName}): could not convert value of type {raw.GetType().Name} to {typeof(T).Name}. {e.Message}");
            value = default;
            return false;
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

        // Allow writing T to a port declared as a base type (e.g. string to an object port)
        if (outputPort.Type != null && !outputPort.Type.IsAssignableFrom(typeof(T)))
        {
            Debug.LogError($"Tried to set value on output port {portName} on node ({GetType().FullName}), but type {typeof(T).Name} is not assignable to port type {outputPort.Type.Name}");
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

public abstract class BaseLoopRTNode : BaseRTNode
{
    public List<string> LoopBodyNodeIds = new();

    public override void Reset(RuntimeActionGraph graph)
    {
        base.Reset(graph);
        foreach (var id in LoopBodyNodeIds)
            graph.GetNodeById(id)?.Reset(graph);
    }
}