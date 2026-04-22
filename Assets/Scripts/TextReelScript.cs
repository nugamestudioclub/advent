using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TextReelScript : MonoBehaviour
{
    [SerializeField] private SVKVPair<string, float>[] m_introTextDurations;
    [SerializeField] private TextMeshProUGUI m_introText;
    [SerializeField] private SkipTextScript m_skipText;
    [SerializeField] private InputActionReference m_confirmReference;
    
    private bool m_isDone;

    private void Update()
    {
        if (m_confirmReference.action.WasPerformedThisFrame())
        {
            StopAllCoroutines();
            m_isDone = true;
            m_introText.gameObject.SetActive(false);
            m_skipText.gameObject.SetActive(false);
        }
    }

    public void StartRun()
    {
        StartCoroutine(IE_RollText());
    }


    private IEnumerator IE_RollText()
    {
        yield return new WaitForSeconds(2f);

        foreach (var kv in m_introTextDurations)
        {
            yield return new WaitForSeconds(0.75f);

            m_introText.gameObject.SetActive(true);

            m_introText.text = kv.Key;

            yield return new WaitForSeconds(kv.Value);

            m_introText.gameObject.SetActive(false);
        }

        m_skipText.gameObject.SetActive(false);
        m_isDone = true;
    }

    public bool IsDone() => m_isDone;
}
