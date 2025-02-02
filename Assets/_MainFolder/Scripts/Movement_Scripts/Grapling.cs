using NTC.MonoCache;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class Grappling : MonoCache
{
    [Header("References")]
    private PlayerMovementAdvanced pm;
    public Transform cam;
    public Transform gunTip;
    public LayerMask whatIsGrappleable;
    public LineRenderer lr;
    public Animator GrGAnimator;

    [Header("Grappling")]
    public float maxGrappleDistance;
    public float grappleDelayTime;
    public float overshootYAxis;
    public float raycastRadius;
    public float grappleSpeed;
    [SerializeField] private float grappleSpeedBoost = 1.2f; // Значение ускорения при использовании крюка кошки
    [SerializeField] float grappleSpeedBoostDuration = 2f; // Длительность ускорения при использовании крюка кошки

    private Vector3 grapplePoint;

    [Header("Cooldown")]
    public float grapplingCd;
    private float grapplingCdTimer;

    [Header("Input")]
    public KeyCode grappleKey = KeyCode.Mouse1;

    public bool grappling;

    private EventInstance ziplineInstance;

    public KatanasController katanasController;

    private void Start()
    {
        pm = GetComponent<PlayerMovementAdvanced>();
        ziplineInstance = AudioManager.instance.CreateInstance(FMODEvents.Instance.Zipline);


    }

    protected override void Run()
    {
        if (Input.GetKeyDown(grappleKey) && !pm.dashing)
            StartGrapple();

        if (grapplingCdTimer > 0)
            grapplingCdTimer -= Time.deltaTime;

        if (pm.dashing || pm.wallRunning || pm.sliding)
            StopGrapple();
    }

    protected override void LateRun()
    {
        if (grappling)
            lr.SetPosition(0, gunTip.position);
    }

    private void StartGrapple()
    {
        if (grapplingCdTimer > 0) return;

        grappling = true;
        // pm.freeze = true;

        GrGAnimator.SetTrigger("GrappleOn");

        RaycastHit hit;
        if (Physics.SphereCast(cam.position, raycastRadius, cam.forward, out hit, maxGrappleDistance, whatIsGrappleable))
        {
            grapplePoint = hit.point;
            Invoke(nameof(ExecuteGrapple), grappleDelayTime);
        }
        else
        {
            grapplePoint = cam.position + cam.forward * maxGrappleDistance;
            Invoke(nameof(StopGrapple), grappleDelayTime);
        }

        lr.enabled = true;
        lr.SetPosition(1, grapplePoint);
        pm.jumpsRemaining = pm.maxJumpCount;

        AudioManager.instance.PlayOneShot(FMODEvents.Instance.Zipline, this.transform.position);

    }

    private void ExecuteGrapple()
    {
        pm.freeze = false;

        Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);
        float grapplePointRelativeYPos = grapplePoint.y - lowestPoint.y;
        float highestPointOnArc = grapplePointRelativeYPos + overshootYAxis;

        if (grapplePointRelativeYPos < 0) highestPointOnArc = overshootYAxis;


        pm.JumpToPosition(grapplePoint, grappleSpeed);

        Invoke(nameof(StopGrapple), 1f);

        AudioManager.instance.StopInstance(ziplineInstance);


    }

    public void StopGrapple()
    {
        pm.freeze = false;
        grappling = false;
        grapplingCdTimer = grapplingCd;
        lr.enabled = false;
        GrGAnimator.SetTrigger("GrappleOff");

        AudioManager.instance.StopInstance(ziplineInstance);



        pm.ModifySpeed(grappleSpeedBoost, grappleSpeedBoostDuration);



    }

    public bool IsGrappling()
    {
        return grappling;
    }

    public Vector3 GetGrapplePoint()
    {
        return grapplePoint;
    }
}