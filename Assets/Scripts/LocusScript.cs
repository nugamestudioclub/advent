using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LocusScript : MonoBehaviour
{
    [SerializeField] private BoundsInt m_copyBounds;
    [SerializeField] private Tilemap m_grid;
    [SerializeField] private ParticleSystem m_system;
    [SerializeField] private LineRenderer m_renderer;

    private Vector3Int m_centerCell;

    private void Awake()
    {
        ComputeCenter();
        UpdateVisuals();
    }

    public void Pulse()
    {
        StopAllCoroutines();
        StartCoroutine(IE_AnimatePulse());
    }

    private IEnumerator IE_AnimatePulse()
    {
        m_renderer.widthMultiplier = 2f;
        yield return new WaitForSeconds(0.25f);
        m_renderer.widthMultiplier = 1f;
    }

    private void UpdateVisuals()
    {
        var bounds = GetCopyBounds();

        var shape_module = m_system.shape;
        shape_module.position = m_grid.WorldToCell(m_centerCell + new Vector3Int(1, 1)) 
            + new Vector3(0.125f, 0.125f); // visual correction
        shape_module.scale = bounds.size;

        var tr_offset = bounds.size;

        var bl = m_grid.WorldToCell(bounds.position);
        var br = bl + new Vector3Int(tr_offset.x, 0);
        var tr = m_grid.WorldToCell(bounds.position + tr_offset);
        var tl = bl + new Vector3Int(0, tr_offset.y);

        m_renderer.positionCount = 4;
        m_renderer.SetPositions(new Vector3[]
        {
            bl, br, tr, tl
        });
    }

    public BoundsInt GetCopyBounds() => m_copyBounds;

    public Vector3 GetWorldPositionOfCenter() => m_grid.GetCellCenterWorld(m_centerCell);

    private void ComputeCenter()
    {
        var bl = m_copyBounds.position;
        var tr = m_copyBounds.position + m_copyBounds.size;

        m_centerCell = new Vector3Int(Mathf.FloorToInt((bl.x + tr.x) / 2f), Mathf.FloorToInt((bl.y + tr.y) / 2f));
    }

    #region Debug
    private void OnDrawGizmos()
    {
        ComputeCenter();
        var bounds = GetCopyBounds();

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(GetWorldPositionOfCenter(), bounds.size);

        // center
        Gizmos.DrawWireSphere(GetWorldPositionOfCenter(), 0.25f);

        // start
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(m_grid.GetCellCenterWorld(bounds.position), 0.25f);

        // end
        Gizmos.color = Color.red;
        // sub 1, 1, 0 from bounds bc they go UP TO the limit, not including (i.e. 0-indexed)
        Gizmos.DrawWireSphere(m_grid.GetCellCenterWorld(bounds.position + bounds.size - new Vector3Int(1, 1, 0)), 0.25f);
    }
    #endregion
}
