using UnityEngine;

public class FirstRTNode : BaseRTNode
{
    public override bool IsPure() => true;

    protected override void ExecuteInternal(RuntimeActionGraph graph)
    {
        if (!TryGetInput("Array", graph, out object[] array) || array == null || array.Length == 0)
        {
            SetFailed();
            return;
        }

        TrySetOutput("First", array[0]);
    }
}