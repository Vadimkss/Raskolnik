using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class GravePickup : MonoBehaviour
{
    public string playerTag = "Player";  // “ег игрока, назначьте его через инспектор
    private GraveWeapon grWeapon;

    private void Awake()
    {
    grWeapon = FindObjectOfType<GraveWeapon>();    
    }

    void OnTriggerEnter(Collider other)
    {
        // ѕроверка, что столкновение произошло с игроком
        if (other.CompareTag(playerTag))
        {
            Debug.Log("√роб подобран");
            grWeapon.graveActive = true;
            grWeapon.GraveAnimator.SetBool("GraveActive", true);
            Destroy(gameObject);

            // ћожете добавить дополнительную логику дл€ того, чтобы игрок "подобрал" гроб, например:
            // player.GetComponent<PlayerInventory>().AddGrave(this);
        }
    }
}
