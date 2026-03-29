using System;
using System.Linq;
using Unity.GraphToolkit.Editor;
using UnityEngine;

[Serializable]
public class FindGameObjectByNameRTNode : BaseRTNode
{
    protected override void ExecuteInternal(RuntimeActionGraph graph)
    {
        if (!TryGetInput<string>("Name", graph, out var nameStr))
        {
            Debug.LogError("Cannot find name port on find go by name node");
            return;
        }
        TrySetOutput("Object", GameObject.Find(nameStr));
    }
}

[Serializable]
public class FindGameObjectsByTagRTNode : BaseRTNode
{
    protected override void ExecuteInternal(RuntimeActionGraph graph)
    {
        if (!TryGetInput<string>("Tag", graph, out var tagStr))
        {
            Debug.LogError("Cannot find tag port on find objects node");
            return;
        }
        
        TrySetOutput("Objects", GameObject.FindGameObjectWithTag(tagStr));
    }
}

[Serializable]
public class FindEnemiesByNameRTNode : BaseRTNode
{
    protected override void ExecuteInternal(RuntimeActionGraph graph)
    {
        TrySetOutput("Enemies", Array.Empty<TestEnemy>());
    }
}

[Serializable]
public class FindTargetsByNameRTNode : BaseRTNode
{
    protected override void ExecuteInternal(RuntimeActionGraph graph)
    {
        if (!TryGetInput<string>("Name", graph, out var nameStr))
        {
            Debug.LogError("Cannot find name port on find targets node");
            return;
        }
        TrySetOutput("Targets", Target.All.Where(x => x.gameObject.name == nameStr));
    }
}