using UnityEngine;

public class MetricTracking : MonoBehaviour
{
    private static int m_reflections;

    private static float m_startTime;
    private static float m_endTime;

    public static void ResetMetrics()
    {
        m_startTime = Time.time;

        m_reflections = 0;
    }

    public static void EndTimer() => m_endTime = Time.time;

    public static void IncrementReflection() => m_reflections++;

    public static (float time, int count) GetMetrics() => (m_endTime - m_startTime, m_reflections);
}
