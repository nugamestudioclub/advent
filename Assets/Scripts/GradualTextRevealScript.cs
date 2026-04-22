using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GradualTextRevealScript : MonoBehaviour
{
    [SerializeField] private SVKVPair<GameObject, float>[] m_revealDurations;

    private bool m_isDone;

    private void Awake()
    {
        foreach (var pair in m_revealDurations)
        {
            pair.Key.SetActive(false);
        }
    }

    public void StartRun()
    {
        StartCoroutine(IE_RollOut());
    }

    private IEnumerator IE_RollOut()
    {
        foreach (var kv in m_revealDurations)
        {
            yield return new WaitForSeconds(kv.Value);

            kv.Key.SetActive(true);
        }

        m_isDone = true;
    }

    public bool IsDone() => m_isDone;
}
