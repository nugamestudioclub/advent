using UnityEngine;

public class LocusScript : MonoBehaviour
{
    [SerializeField] private Vector2Int m_copyBounds;

    public Vector2Int GetCopyBounds() => m_copyBounds;

    public Vector3 GetPosition() => transform.position;
}
