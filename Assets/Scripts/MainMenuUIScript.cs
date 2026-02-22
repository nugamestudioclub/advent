using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MainMenuUIScript : MonoBehaviour
{
    [Serializable]
    public struct SVKVPair<K,V>
    {
        public K Key;
        public V Value;
    }

    [SerializeField] private SVKVPair<GameObject, float>[] m_revealDurations;

    [SerializeField] private InputActionReference m_confirmReference;

    [SerializeField] private string m_targetScene;

    private bool m_canStart;

    private void Start()
    {
        m_canStart = false;

        foreach (var pair in m_revealDurations)
        {
            pair.Key.SetActive(false);
        }

        StartCoroutine(IE_RollOut());
    }

    private void Update()
    {
        if (m_canStart && m_confirmReference.action.WasPerformedThisFrame())
        {
            m_canStart = false;

            SceneManager.LoadScene(m_targetScene);
        }
    }

    private IEnumerator IE_RollOut()
    {
        foreach (var kv in m_revealDurations)
        {
            yield return new WaitForSeconds(kv.Value);

            kv.Key.SetActive(true);
        }

        m_canStart = true;
    }
}
