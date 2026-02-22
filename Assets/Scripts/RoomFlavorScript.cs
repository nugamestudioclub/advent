using UnityEngine;

public class RoomFlavorScript : MonoBehaviour
{
    [SerializeField] private string m_text;
    [SerializeField] private bool m_isDefault = false;

    private UIScript m_uiScript;

    private void Awake()
    {
        m_uiScript = FindFirstObjectByType<UIScript>();

        if (m_uiScript == null)
        {
            Debug.LogError("UI Script not found.");
            return;
        }

        if (m_isDefault ) m_uiScript.SetFlavor(m_text);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        m_uiScript.SetFlavor(m_text);
    }
}
