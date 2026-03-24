using System;
using Unity.GraphToolkit.Editor;
using UnityEngine;

public interface IEditorNode
{
    public BaseRTNode CreateRuntimeType();
}