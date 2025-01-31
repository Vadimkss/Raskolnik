using System.Collections.Generic;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    private static TimerManager _instance;
    public static TimerManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var obj = new GameObject("TimerManager");
                _instance = obj.AddComponent<TimerManager>();
                DontDestroyOnLoad(obj);
            }
            return _instance;
        }
    }

    private readonly List<Timer> timers = new List<Timer>();

    public void RegisterTimer(Timer timer)
    {
        timers.Add(timer);
    }

    public void UnregisterTimer(Timer timer)
    {
        timers.Remove(timer);
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        for (int i = timers.Count - 1; i >= 0; i--)
        {
            timers[i].Update(deltaTime);
        }
    }
}