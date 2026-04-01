using UnityEngine;

public class FirstRTNode : BaseRTNode
{
    public override bool IsPure() => true;

    protected override void ExecuteInternal(RuntimeActionGraph graph)
    {
        if (!TryGetInput("Array", graph, out object[] array))
        {
            Debug.LogError("Unable to get array input to First node");
            SetFailed();
            return;
        }

        if (array == null || array.Length == 0)
        {
            Debug.LogError("Cant get first element in array size 0 / null");
            SetFailed();
            return;
        }

        TrySetOutput("First", array[0]);
    }
}