using NTC.MonoCache;
using UnityEngine;

public class MoveCamera : MonoCache
{
    public Transform cameraPosition;

    protected override void Run()
    {
        transform.position = cameraPosition.position;
    }
}
