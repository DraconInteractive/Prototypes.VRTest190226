using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class RuntimeActionGraph
{
    public List<BaseRTNode> Nodes = new();

    public RuntimeActionGraph Clone()
    {
        var clone = new RuntimeActionGraph();
        var portMap = new Dictionary<Port, Port>();

        // First pass: clone nodes and their ports (no connections yet)
        foreach (var node in Nodes)
        {
            var nodeClone = (BaseRTNode)Activator.CreateInstance(node.GetType());
            nodeClone.Inputs = new List<Port>();
            nodeClone.Outputs = new List<Port>();

            foreach (var port in node.Inputs)
            {
                var portClone = new Port { Name = port.Name, Type = port.Type, Value = port.Value, Node = nodeClone, ConnectedPorts = new List<Port>() };
                nodeClone.Inputs.Add(portClone);
                portMap[port] = portClone;
            }
            foreach (var port in node.Outputs)
            {
                var portClone = new Port { Name = port.Name, Type = port.Type, Value = port.Value, Node = nodeClone, ConnectedPorts = new List<Port>() };
                nodeClone.Outputs.Add(portClone);
                portMap[port] = portClone;
            }

            clone.Nodes.Add(nodeClone);
        }

        // Second pass: rewire connections using the port map
        foreach (var node in Nodes)
        {
            foreach (var port in node.Inputs.Concat(node.Outputs))
            {
                var portClone = portMap[port];
                foreach (var connected in port.ConnectedPorts)
                    portClone.ConnectedPorts.Add(portMap[connected]);
            }
        }

        return clone;
    }

    public void Execute()
    {
        var startNode = Nodes.OfType<StartRTNode>().FirstOrDefault();
        if (startNode == null)
        {
            Debug.LogError("RuntimeActionGraph has no StartRTNode.");
            return;
        }
        startNode.Execute();
    }
}
