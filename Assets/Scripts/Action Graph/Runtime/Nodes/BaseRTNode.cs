using System;
using System.Collections.Generic;

[Serializable]
public abstract class BaseRTNode
{
    public List<Port> Inputs;
    public List<Port> Outputs;
}

public class Port
{
    public string Name;
    public List<Port> ConnectedPorts;
    public Type Type;
    public object Value;
}