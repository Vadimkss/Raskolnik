using UnityEngine;

public class InputManager : MonoBehaviour
{
    private GraveWeapon graveWeapon;
    private RevolverWeapon revolverWeapon;

    private float graveHoldTimer = 0f; // Таймер для удержания ЛКМ (гроб)
    private float revolverHoldTimer = 0f; // Таймер для удержания ПКМ (револьвер)
    private float holdTime = 0.5f; // Время удержания для заряженной атаки
    public bool revolverActive = true;

    private void Start()
    {
        graveWeapon = FindObjectOfType<GraveWeapon>();
        revolverWeapon = FindObjectOfType<RevolverWeapon>();
    }

    private void Awake() { revolverActive = true; }
    private void Update()
    {
        // Управление гробом (ЛКМ)
        if (graveWeapon.graveActive)
        {
            if (Input.GetMouseButton(0)) // Удержание ЛКМ
            {
                graveHoldTimer += Time.deltaTime;

                if (graveHoldTimer >= holdTime)
                {
                    graveWeapon.GraveAnimator.SetBool("Throwing", true);
                }
            }

            if (Input.GetMouseButtonUp(0)) // Отпускание ЛКМ
            {
                if (graveHoldTimer >= holdTime)
                {
                    graveWeapon.ChargedGraveAttack();
                    graveWeapon.GraveAnimator.SetBool("Throwing", false);
                }
                else
                {
                    graveWeapon.GraveAttack();
                }

                graveHoldTimer = 0f;
            }
        }

        // Управление револьвером (ПКМ)
        if (revolverActive)
        {
            if (Input.GetMouseButton(1)) // Удержание ПКМ
            {
                revolverHoldTimer += Time.deltaTime;

                if (revolverHoldTimer >= holdTime)
                {
                    revolverWeapon.revolverAnim.SetBool("Spin", true);
                }
            }

            if (Input.GetMouseButtonUp(1)) // Отпускание ПКМ
            {
                if (revolverHoldTimer >= holdTime)
                {
                    revolverWeapon.ChargedRevolverShot();
                    revolverWeapon.revolverAnim.SetBool("Spin", false);
                    revolverWeapon.revolverAnim.SetTrigger("Throwing");
                }
                else
                {
                    revolverWeapon.RevolverShot();
                }

                revolverHoldTimer = 0f;
            }
        }
    }
}
