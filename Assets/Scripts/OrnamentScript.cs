using System;
using UnityEngine;

public class OrnamentScript : MonoBehaviour
{
    public static event Action OnOrnamentCollected;

    [SerializeField] private Transform m_objectTransform;
    [SerializeField] private float m_degreesPerSecond = 45f;
    [SerializeField] private float m_translationScale = 1f;

    [Space]

    [SerializeField] private Transform m_toDisable;
    [SerializeField] private ParticleSystem m_collectSystem;
    [SerializeField] private ParticleSystem m_emissionSystem;

    private Vector3 m_startingPosition;

    private void Awake()
    {
        m_startingPosition = m_objectTransform.position;
    }

    private void Update()
    {
        m_objectTransform.SetPositionAndRotation(
            m_startingPosition + Vector3.up * Mathf.Sin(Time.time) * m_translationScale,
            Quaternion.Euler(0f, (m_degreesPerSecond * Time.time) % 360, 0f)
            );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        OnOrnamentCollected?.Invoke();

        m_toDisable.gameObject.SetActive(false);
        m_objectTransform.gameObject.SetActive(false);

        m_collectSystem.Play();
        m_emissionSystem.Stop();
    }
}
