using UnityEngine;

public class LocusScript : MonoBehaviour
{
    [SerializeField] private BoundsInt m_copyBounds;

    private Vector3Int m_center;

    private void Awake()
    {
        var bl = m_copyBounds.position;
        var tr = m_copyBounds.position + m_copyBounds.size;

        m_center = new Vector3Int(Mathf.FloorToInt((bl.x + tr.x) / 2f), Mathf.FloorToInt((bl.y + tr.y) / 2f));
    }

    public BoundsInt GetCopyBounds() => m_copyBounds;

    public Vector3Int GetCenter() => m_center;

    #region Debug
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireCube(new Vector3(m_copyBounds.x-.5f, m_copyBounds.y-.5f), m_copyBounds.size);
    }
    #endregion
}
