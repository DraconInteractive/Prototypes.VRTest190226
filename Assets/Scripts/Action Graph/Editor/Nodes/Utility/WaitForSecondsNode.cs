using System;

[Serializable]
class WaitForSecondsNode : BaseActionNode, IEditorNode
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        base.OnDefinePorts(context);

        context.AddInputPort<float>("Seconds");
    }

    public BaseRTNode CreateRuntimeType() => new WaitForSecondsRTNode();
}