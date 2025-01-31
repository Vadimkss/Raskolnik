using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeStopMotion : MonoBehaviour
{
    public Animator Animator;
    public int FPS = 24;
    private float _time;

    private void OnValidate()
    {
        if (!Animator)
        {
            Animator = GetComponent<Animator>();
        }
    }


    // Update is called once per frame
    void Update()
    {
        _time += Time.deltaTime;
        var UpdateTime = 1f/FPS;
        Animator.speed = 0;

        if (_time > UpdateTime)
        {
            _time -= UpdateTime;
            Animator.speed = UpdateTime / Time.deltaTime;
        }

    }
}
