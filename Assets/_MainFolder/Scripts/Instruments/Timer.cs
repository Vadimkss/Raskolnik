using System;
using UnityEngine;

public class Timer
{
    private float delay;
    private float duration;
    private float actionFrequency;
    private Action callback;
    private Action onComplete;

    private float timer;
    private float actionTimer;
    private bool isRunning;

    public Timer(float delay, float duration, float actionFrequency, Action callback, Action onComplete = null)
    {
        this.delay = delay;
        this.duration = duration;
        this.actionFrequency = actionFrequency;
        this.callback = callback;
        this.onComplete = onComplete;
    }

    public void Start()
    {
        if (isRunning)
            return;

        timer = 0f;
        actionTimer = 0f;
        isRunning = true;

        // Добавляем в менеджер для обновления\n        TimerManager.Instance.RegisterTimer(this);
    }

    public void Update(float deltaTime)
    {
        if (!isRunning)
            return;

        timer += deltaTime;

        // Wait for delay
        if (timer < delay)
            return;

        // Execute actions at specified frequency
        actionTimer += deltaTime;
        if (actionTimer >= actionFrequency)
        {
            callback?.Invoke();
            actionTimer = 0f;
        }

        // Check if duration is over
        if (timer >= delay + duration)
        {
            Stop();
        }
    }

    public void Stop()
    {
        if (!isRunning)
            return;

        isRunning = false;
        TimerManager.Instance.UnregisterTimer(this);
        onComplete?.Invoke();
    }
}