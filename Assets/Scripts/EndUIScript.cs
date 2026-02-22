using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class EndUIScript : MonoBehaviour
{
    [SerializeField] private TextReelScript m_reelScript;
    [SerializeField] private GradualTextRevealScript m_gradualReveal;

    [Space]

    [SerializeField] private TextMeshProUGUI m_timeText;
    [SerializeField] private TextMeshProUGUI m_reflectionsText;

    [Space]

    [SerializeField] private InputActionReference m_confirmReference;

    [SerializeField] private string m_targetScene;

    private bool m_canSwap;

    private void Start()
    {
        m_canSwap = false;

        var (time, count) = MetricTracking.GetMetrics();

        m_timeText.text = "TIME " + TimeSpan.FromSeconds(time).ToString(@"hh\:mm\:ss"); ;
        m_reflectionsText.text = count + " REFLECTIONS PLACED";

        StartCoroutine(IE_Roll());
    }

    private void Update()
    {
        if (m_canSwap && m_confirmReference.action.WasPerformedThisFrame())
        {
            m_canSwap = false;

            SceneManager.LoadScene(m_targetScene);
        }
    }

    private IEnumerator IE_Roll()
    {
        m_reelScript.StartRun();

        yield return new WaitUntil(() => m_reelScript.IsDone());

        m_gradualReveal.StartRun();

        yield return new WaitUntil(() => m_gradualReveal.IsDone());

        m_canSwap = true;
    }

}
