using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventDebugger : MonoBehaviour
{
   public string debugMessage = "Hello World";
    public void DebugShooting()
    {
        Debug.Log(debugMessage);

    }
}
