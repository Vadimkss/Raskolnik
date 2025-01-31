using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Stamina : MonoBehaviour
    
{
    public float playerStamina;
    public float staminaReduction = 1f;
    public float maxStamina = 100f;
    public float staminaOutlay = 100f;
    void Start()
    {
        playerStamina = maxStamina;

    }

    
    void Update()
    {
        if (playerStamina < maxStamina)
        {
         playerStamina += staminaReduction;


        }
    }
}
