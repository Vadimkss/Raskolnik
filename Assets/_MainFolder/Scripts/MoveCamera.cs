using NTC.MonoCache;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoCache
{
    public Transform cameraPosition;

    protected override void Run()
    {
        transform.position = cameraPosition.position;
    }
}
