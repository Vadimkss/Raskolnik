using UnityEngine;
using ParadoxNotion.Design;
using FlowCanvas.Nodes;

[Category("Screen")]
public class GetScreenCenter : PureFunctionNode<Vector2>
{

    public override Vector2 Invoke()
    {
        return new Vector2(Screen.width/2, Screen.height/2);
    }
}
