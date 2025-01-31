using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class RS_FMODE_Events : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RS_Footsteps1()
    {
        AudioManager.instance.PlayOneShot(FMODEvents.Instance.RS_Step1, this.transform.position);
    }

    public void RS_Footsteps2()
    {
        AudioManager.instance.PlayOneShot(FMODEvents.Instance.RS_Step2, this.transform.position);
    }

    public void RS_Moving()
    {
        AudioManager.instance.PlayOneShot(FMODEvents.Instance.RS_Moving, this.transform.position);
    }

    public void RS_Hihat()
    {
        AudioManager.instance.PlayOneShot(FMODEvents.Instance.HiHat, this.transform.position);
    }
}
