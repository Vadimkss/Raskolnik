using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuzzleFlashHolder : MonoBehaviour
{
    [SerializeField] private Transform muzzleFlash;

    private void Update()
    {
        if (muzzleFlash != null)
        {
            muzzleFlash.position = transform.position;
            muzzleFlash.rotation = transform.rotation;
        }
    }
}
