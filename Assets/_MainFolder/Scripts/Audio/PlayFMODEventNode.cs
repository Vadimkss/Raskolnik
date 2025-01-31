using UnityEngine;
using ParadoxNotion.Design;
using FlowCanvas.Nodes;
using FMODUnity;

[Category("AudioManager")]
public class PlayFMODEventNode : CallableActionNode<EventReference, Vector3>
{
    public override void Invoke(EventReference soundEvent, Vector3 position)
    {
        AudioManager.instance.PlayOneShot(soundEvent, position);
    }
}
