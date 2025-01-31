using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShaker : MonoBehaviour
{
    public Transform playerCam;

    public void CameraShake()
    {
        playerCam
            .DOShakePosition(0.03f, .1f, 10, 90f, false, true, ShakeRandomnessMode.Harmonic)
            .SetEase(Ease.InCubic)
            .SetLink(playerCam.gameObject)
            .OnComplete(() => ResetCameraPosition());

        playerCam
            .DOShakeRotation(0.03f, .1f, 10, 90f, true, ShakeRandomnessMode.Harmonic)
            .SetEase(Ease.InCubic)
            .SetLink(playerCam.gameObject);
    }

    public void CameraShakeStronger()
    {
        playerCam
            .DOShakePosition(1f, 2f, 10, 90f, false, true, ShakeRandomnessMode.Harmonic)
            .SetEase(Ease.InCubic)
            .SetLink(playerCam.gameObject)
            .OnComplete(() => ResetCameraPosition());

        playerCam
            .DOShakeRotation(.3f, 5f, 10, 90f, true, ShakeRandomnessMode.Harmonic)
            .SetEase(Ease.InCubic)
            .SetLink(playerCam.gameObject);
    }

    private void ResetCameraPosition()
    {
        playerCam.DOLocalMove(Vector3.zero, 0.1f).SetEase(Ease.Linear);
        
    }
}
