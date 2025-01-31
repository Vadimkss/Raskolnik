using UnityEngine;
using ParadoxNotion.Design;
using FlowCanvas.Nodes;

[Category("Debug")]
public class DebugRaycast : CallableActionNode<Vector3, Vector3, Color, float>
{

    public override void Invoke(Vector3 origin, Vector3 direction, Color color, float duration)
    {
        Debug.DrawRay(origin, direction * 100f, color, duration);
    }
}
