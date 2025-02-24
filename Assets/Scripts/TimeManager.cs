using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TimeManager : MonoBehaviour
{
    public static TimeManager instance;

    public int hour = 6; // Start at 6 AM
    public int minute = 0;
    public float timeMultiplier = 1f; // Adjust this to speed up or slow down.
    private float timeElapsed = 0f;
    public float realSecondsPerInGameMinute = 0.8333f; // 0.8333 second in real life = 1 minute in game

    public enum TimePhase { Sunrise, Day, Sunset, Night }
    public TimePhase currentPhase;

    public Light2D sunLight; // Assign the Directional Light

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        timeElapsed += Time.deltaTime * timeMultiplier;

        if (timeElapsed >= realSecondsPerInGameMinute)
        {
            IncrementTime();
            timeElapsed = 0f;
        }
        UpdateDayNightCycle();
    }

    void IncrementTime()
    {
        minute++;

        if (minute >= 60)
        {
            minute = 0;
            hour++;

            if (hour >= 24)
            {
                hour = 0; // Reset to midnight
            }
        }
    }

    void UpdateDayNightCycle()
    {
        
        // Determine current time phase
        if (hour >= 4 && hour < 6)
            currentPhase = TimePhase.Sunrise;
        else if (hour >= 6 && hour < 17)
            currentPhase = TimePhase.Day;
        else if (hour >= 17 && hour < 19)
            currentPhase = TimePhase.Sunset;
        else
            currentPhase = TimePhase.Night;

        // Adjust lighting
        UpdateLighting();
    }

    void UpdateLighting()
    {
        float normalizedTime = (hour * 60 + minute) / 1440f; // Normalize 0-1

        switch (currentPhase)
        {
            case TimePhase.Sunrise:
                sunLight.intensity = Mathf.Lerp(sunLight.intensity, Mathf.Lerp(0.2f, 1.2f, normalizedTime), Time.deltaTime *2f);
                sunLight.color = Color.Lerp(sunLight.color, Color.Lerp(new Color(1f, 0.5f, 0.2f), Color.white, normalizedTime), Time.deltaTime * 2f);
                break;

            case TimePhase.Day:
                sunLight.intensity = Mathf.Lerp(sunLight.intensity, 1.0f, Time.deltaTime * 2f);
                sunLight.color = Color.Lerp(sunLight.color, new Color(1f, 0.95f, 0.85f), Time.deltaTime * 2f);
                break;

            case TimePhase.Sunset:
                sunLight.intensity = Mathf.Lerp(sunLight.intensity, Mathf.Lerp(0.4f, 0.1f, normalizedTime), Time.deltaTime * 2f);
                sunLight.color = Color.Lerp(sunLight.color, Color.Lerp(Color.white, new Color(0.4f, 0.2f, 0.1f), normalizedTime), Time.deltaTime * 2f);
                break;

            case TimePhase.Night:
                sunLight.intensity = Mathf.Lerp(sunLight.intensity, 0.2f, Time.deltaTime * 2f);
                sunLight.color = Color.Lerp(sunLight.color, new Color(0.1f, 0.1f, 0.2f), Time.deltaTime * 2f);
                break;
        }

    }
}
