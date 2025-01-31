using UnityEngine;
using ParadoxNotion.Design;
using FlowCanvas.Nodes;
using FMODUnity;

[Category("AudioManager")]
public class PlayOneShotNode : CallableActionNode<EventReference, Vector3>
{
    public override void Invoke(EventReference sound, Vector3 worldPos)
    {
        if (AudioManager.instance == null)
        {
            Debug.LogError("AudioManager instance is null. Ensure that AudioManager is initialized.");
            return;
        }

        if (sound.IsNull)
        {
            Debug.LogError("EventReference is null or invalid.");
            return;
        }

        Debug.Log($"Attempting to play sound: {sound.Guid}, at position: {worldPos}");
        AudioManager.instance.PlayOneShot(sound, worldPos);
    }
}
