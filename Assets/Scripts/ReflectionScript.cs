using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ReflectionScript : MonoBehaviour
{
    [Serializable]
    private struct CopyData
    {
        public bool HasData;
        public TileBase[] CopyTilesBuffer;
        public Vector3Int[] CopyPositionsBuffer;
        public Vector3Int BoundsPosition;
    }

    [SerializeField] private Tilemap m_mainGrid;
    [SerializeField] private Tilemap m_reflectionGrid;

    private CopyData m_copyData;

    private Camera m_mainCamera;
    private LocusScript[] m_locii;

    private void Awake()
    {
        m_mainCamera = Camera.main;
        m_locii = FindObjectsByType<LocusScript>(FindObjectsSortMode.None);
    }

    public void CopyRegion()
    {
        if (!FindOnscreenLocus(out var locus))
        {
            Debug.LogWarning("No locus found on screen.");
            return;
        }

        var bounds = locus.GetCopyBounds();

        if (bounds.size.z != 1)
        {
            Debug.LogWarning("Bounds Z size must be 1, otherwise it will not enumerate.");
            return;
        }

        m_copyData = new CopyData();
        m_copyData.CopyTilesBuffer = new TileBase[(bounds.xMax - bounds.xMin) * (bounds.yMax - bounds.yMin)];
        m_copyData.CopyPositionsBuffer = new Vector3Int[(bounds.xMax - bounds.xMin) * (bounds.yMax - bounds.yMin)];
        m_copyData.BoundsPosition = bounds.position;

        // both allPositionsWithin and GetTilesBlockNonAlloc are sequentially consistent
        int index = 0;
        foreach (var pos in bounds.allPositionsWithin) m_copyData.CopyPositionsBuffer[index++] = pos;
        m_mainGrid.GetTilesBlockNonAlloc(bounds, m_copyData.CopyTilesBuffer);

        m_copyData.HasData = true;
    }

    public void PasteRegion()
    {
        if (!FindOnscreenLocus(out var locus))
        {
            Debug.LogWarning("No locus found on screen.");
            return;
        }

        var copy_dest_bounds = locus.GetCopyBounds();

        var changes = new List<TileChangeData>();
        for (int i = 0; i < m_copyData.CopyPositionsBuffer.Length; i++)
        {
            var offset = m_copyData.CopyPositionsBuffer[i] - m_copyData.BoundsPosition;

            var target_position = copy_dest_bounds.position + offset;

            // if the copy-dest bounds dont contain the relative position OR there's a blocking tile in main, go to next
            if (!copy_dest_bounds.Contains(target_position)
                || m_mainGrid.HasTile(target_position)) continue; // TODO: this will bug with decal tiles! is there a way to "tag" them so we can only focus on tagged ones?

            var change_data = new TileChangeData()
            {
                position = target_position,
                tile = m_copyData.CopyTilesBuffer[i]
            };

            changes.Add(change_data);
        }

        m_reflectionGrid.SetTiles(changes.ToArray(), false);

        m_copyData = default;
    }

    public bool HasCopy() => m_copyData.HasData;

    private bool FindOnscreenLocus(out LocusScript valid)
    {
        // not very performant since it checks every locus, even ones obviously too far
        // it's a game jam tho, so K.I.S.S.
        foreach (var locus in m_locii)
        {
            var vp = m_mainCamera.WorldToViewportPoint(m_mainGrid.CellToWorld(locus.GetCenter()));

            // if oob, pass
            if (vp.x < 0 || vp.x > 1) continue;
            if (vp.y < 0 || vp.y > 1) continue;

            valid = locus;

            Debug.Log("Found locus: " + valid);

            return true;
        }

        valid = null;
        return false;
    }
}
