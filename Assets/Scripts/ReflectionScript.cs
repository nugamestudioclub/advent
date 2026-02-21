using UnityEngine;
using UnityEngine.Tilemaps;

public class ReflectionScript : MonoBehaviour
{
    [SerializeField] private Transform m_testpoint;

    [SerializeField] private Tilemap m_mainGrid;
    [SerializeField] private Tilemap m_reflectionGrid;

    [SerializeField] private LocusScript[] m_locii;

    private TileBase[,] m_copyBuffer;
    private bool m_hasCopy;

    private Camera m_mainCamera;

    private void Awake()
    {
        m_mainCamera = Camera.main;
        m_locii = FindObjectsByType<LocusScript>(FindObjectsSortMode.None);
    }

    private void CopyRegion()
    {
        if (FindOnscreenLocus(out var locus))
        {
            Debug.LogWarning("No locus found on screen.");
            return;
        }

        var bounds = locus.GetCopyBounds();
        var locus_cellpos = m_mainGrid.WorldToCell(locus.GetPosition());

        m_copyBuffer = new TileBase[bounds.x, bounds.y];
        for (int x = 0; x < bounds.x; ++x)
        {
            for (int y = 0; y < bounds.y; ++y)
            {
                var cell_pos = new Vector3Int(locus_cellpos.x + x, locus_cellpos.y + y);

                if (!m_mainGrid.HasTile(cell_pos)) continue;

                m_copyBuffer[x, y] = m_mainGrid.GetTile(cell_pos);
            }
        }

        m_hasCopy = true;
    }

    private void PasteRegion()
    {
        if (FindOnscreenLocus(out var locus))
        {
            Debug.LogWarning("No locus found on screen.");
            return;
        }


        var locus_cellpos = m_mainGrid.WorldToCell(locus.GetPosition());

        for (int x = 0; x < m_copyBuffer.GetLength(0); ++x)
        {
            for (int y = 0; y < m_copyBuffer.GetLength(1); ++y)
            {
                var target_position = new Vector3Int(locus_cellpos.x + x, locus_cellpos.y + y);

                if (m_mainGrid.HasTile(target_position)) continue;

                
                /*
                 * TODO
                m_reflectionGrid.tile
                m_copyBuffer[x, y] = m_mainGrid.GetTile(cell_pos);
                */
            }
        }
    }

    private bool FindOnscreenLocus(out LocusScript valid)
    {
        // not very performant since it checks every locus, even ones obviously too far
        // it's a game jam tho, so K.I.S.S.
        foreach (var locus in m_locii)
        {
            var vp = m_mainCamera.WorldToViewportPoint(locus.GetPosition());

            // if oob, pass
            if (vp.x < 0 || vp.x > 1) continue;
            if (vp.y < 0 || vp.y > 1) continue;

            valid = locus;

            return true;
        }

        valid = null;
        return false;
    }
}
