using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class RuntimeActionGraph
{
    public List<BaseRTNode> Nodes = new();

    private static readonly JsonSerializerSettings JsonSettings = new()
    {
        TypeNameHandling = TypeNameHandling.Auto,
        Formatting = Formatting.Indented
    };

    public string Serialize() => JsonConvert.SerializeObject(this, JsonSettings);

    public static RuntimeActionGraph Deserialize(string json) =>
        JsonConvert.DeserializeObject<RuntimeActionGraph>(json, JsonSettings);

    public BaseRTNode GetNodeById(string id) =>
        Nodes.FirstOrDefault(n => n.NodeId == id);

    // Resolves a connection string stored on an input port → finds the upstream node and its output port.
    public void ResolveOutputConnection(string connection, out BaseRTNode node, out Port port)
    {
        ParseConnection(connection, out var nodeId, out var portName);
        node = GetNodeById(nodeId);
        port = node?.Outputs.FirstOrDefault(p => p.Name == portName);
    }

    // Resolves a connection string stored on an output port → finds the downstream node and its input port.
    public void ResolveInputConnection(string connection, out BaseRTNode node, out Port port)
    {
        ParseConnection(connection, out var nodeId, out var portName);
        node = GetNodeById(nodeId);
        port = node?.Inputs.FirstOrDefault(p => p.Name == portName);
    }

    private static void ParseConnection(string connection, out string nodeId, out string portName)
    {
        var sep = connection.IndexOf("__", StringComparison.Ordinal);
        nodeId = connection[..sep];
        portName = connection[(sep + 2)..];
    }

    public RuntimeActionGraph Clone()
    {
        var clone = new RuntimeActionGraph();

        foreach (var node in Nodes)
        {
            var nodeClone = (BaseRTNode)Activator.CreateInstance(node.GetType());
            nodeClone.NodeId = node.NodeId;
            nodeClone.Inputs = node.Inputs.Select(p => new Port
            {
                Name = p.Name,
                NodeId = p.NodeId,
                Connections = new List<string>(p.Connections),
                Type = p.Type,
                Value = p.Value
            }).ToList();
            nodeClone.Outputs = node.Outputs.Select(p => new Port
            {
                Name = p.Name,
                NodeId = p.NodeId,
                Connections = new List<string>(p.Connections),
                Type = p.Type,
                Value = p.Value
            }).ToList();
            clone.Nodes.Add(nodeClone);
        }

        return clone;
    }

    public void Execute()
    {
        var startNode = Nodes.OfType<StartRTNode>().FirstOrDefault();
        if (startNode == null)
        {
            Debug.LogError($"RuntimeActionGraph has no StartRTNode. Nodes: {Nodes.Count}");
            return;
        }
        startNode.Execute(this);
    }
}
