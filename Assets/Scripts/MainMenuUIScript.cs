using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MainMenuUIScript : MonoBehaviour
{
    [SerializeField] private TextReelScript m_reelScript;
    [SerializeField] private GradualTextRevealScript m_gradualReveal;

    [SerializeField] private InputActionReference m_confirmReference;

    [SerializeField] private string m_targetScene;

    private bool m_canStart;

    private void Start()
    {
        m_canStart = false;

        StartCoroutine(IE_Roll());
    }

    private void Update()
    {
        if (m_canStart && m_confirmReference.action.WasPerformedThisFrame())
        {
            m_canStart = false;

            SceneManager.LoadScene(m_targetScene);
        }
    }

    private IEnumerator IE_Roll()
    {
        m_reelScript.StartRun();

        yield return new WaitUntil(() => m_reelScript.IsDone());

        m_gradualReveal.StartRun();

        yield return new WaitUntil(() => m_gradualReveal.IsDone());

        m_canStart = true;
    }

}
