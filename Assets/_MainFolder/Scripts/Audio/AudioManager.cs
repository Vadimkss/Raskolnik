using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class AudioManager : MonoBehaviour
{
    private List<EventInstance> eventInstances;

    public static AudioManager instance { get; private set; }

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Найдено больше одного AudioManager на сцене");
            return;
        }
        instance = this;

        eventInstances = new List<EventInstance>();
    }

    public void PlayOneShot(EventReference sound, Vector3 worldPos)
    {
        RuntimeManager.PlayOneShot(sound, worldPos);
    }

    public EventInstance CreateInstance(EventReference eventReference)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference);
        eventInstances.Add(eventInstance);
        return eventInstance;
    }

    public void StopInstance(EventInstance eventInstance)
    {
        if (eventInstances.Contains(eventInstance))
        {
            eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            eventInstances.Remove(eventInstance);
        }
    }

    public void StopAllInstances()
    {
        foreach (EventInstance eventInstance in eventInstances)
        {
            eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }
        eventInstances.Clear();
    }

    private void OnDestroy()
    {
        StopAllInstances();
    }

    // --- Методы для работы с таймлайном ---

    // Воспроизведение таймлайна с указанием позиции
    public EventInstance PlayTimeline(EventReference eventReference, Vector3 worldPos)
    {
        EventInstance timelineInstance = RuntimeManager.CreateInstance(eventReference);

        // Устанавливаем 3D-позицию звука
        timelineInstance.set3DAttributes(RuntimeUtils.To3DAttributes(worldPos));

        timelineInstance.start();
        eventInstances.Add(timelineInstance);
        return timelineInstance;
    }

    // Остановка таймлайна
    public void StopTimeline(EventInstance timelineInstance)
    {
        if (eventInstances.Contains(timelineInstance))
        {
            timelineInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            timelineInstance.release();
            eventInstances.Remove(timelineInstance);
        }
    }
    // Пауза таймлайна
    public void PauseTimeline(EventInstance timelineInstance, bool paused)
    {
        if (timelineInstance.isValid())
        {
            timelineInstance.setPaused(paused);
        }
    }

    // Установка позиции на таймлайне (в миллисекундах)
    public void SetTimelinePosition(EventInstance timelineInstance, int positionMilliseconds)
    {
        if (timelineInstance.isValid())
        {
            timelineInstance.setTimelinePosition(positionMilliseconds);
        }
    }

    // Получение текущей позиции на таймлайне (в миллисекундах)
    public int GetTimelinePosition(EventInstance timelineInstance)
    {
        int position = 0;
        if (timelineInstance.isValid())
        {
            timelineInstance.getTimelinePosition(out position);
        }
        return position;
    }
}
