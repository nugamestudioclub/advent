using System.Collections.Generic;
using UnityEngine;

public class LitePlayerAudio : MonoBehaviour
{
    public enum SFXType
    {
        Jump,
        Copy,
        Paste
    }

    [SerializeField] private AudioSource m_audioSource;

    [Space]

    [SerializeField] private SVKVPair<SFXType, AudioClip>[] m_mapping;

    private IDictionary<SFXType, AudioClip> m_bakedMap;
    private PlayerScript m_playerScript;
    private ReflectionScript m_reflectionScript;

    private void Awake()
    {
        m_bakedMap = new Dictionary<SFXType, AudioClip>();

        foreach (var entry in m_mapping)
        {
            m_bakedMap[entry.Key] = entry.Value;
        }

        m_playerScript = FindFirstObjectByType<PlayerScript>();
        if (m_playerScript == null)
        {
            Debug.LogError("Player script not found in scene.");
            return;
        }

        m_reflectionScript = FindFirstObjectByType<ReflectionScript>();
        if (m_reflectionScript == null)
        {
            Debug.LogError("Reflection script not found in scene.");
            return;
        }

        m_playerScript.OnSFXPlay += PlaySFXOneShot;
        m_reflectionScript.OnActionPerformed += PlaySFXOneShot;
    }

    private void OnDestroy()
    {
        if (m_playerScript != null) m_playerScript.OnSFXPlay -= PlaySFXOneShot;
        if (m_reflectionScript != null) m_reflectionScript.OnActionPerformed -= PlaySFXOneShot;
    }

    public void PlaySFXOneShot(SFXType type)
    {
        m_audioSource.PlayOneShot(m_bakedMap[type]);
    }
}
