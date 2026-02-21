using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ReflectionScript : MonoBehaviour
{
    public enum CellChangeType { Created, Destroyed }

    public delegate void TilesStateChange(Vector3Int[] cell_changes, Tilemap map, CellChangeType type);
    public event TilesStateChange OnReflectionTilesChanged;

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

    private bool m_hasReflectionPlaced;

    private Camera m_mainCamera;
    private LocusScript[] m_locii;

    private void Awake()
    {
        m_mainCamera = Camera.main;
        m_locii = FindObjectsByType<LocusScript>(FindObjectsSortMode.None);

        m_reflectionGrid.origin = m_mainGrid.origin;
        m_reflectionGrid.size = m_mainGrid.size;
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

        locus.Pulse();
    }

    public void PasteRegion()
    {
        if (!FindOnscreenLocus(out var locus))
        {
            Debug.LogWarning("No locus found on screen.");
            return;
        }

        // check to see if we have a reflection, and clear it if we do
        // also pulses the event if cells change
        CheckClearReflection();

        var copy_dest_bounds = locus.GetCopyBounds();

        // keep track of cell changes in this func
        var event_cell_changes = new List<Vector3Int>();

        // go through every position, and if a valid reflection tile is possible, add it to the tile change list
        var changes = new List<TileChangeData>();
        for (int i = 0; i < m_copyData.CopyPositionsBuffer.Length; i++)
        {
            // remap our position to be correctly placed with respect to the dest locus' position
            var offset = m_copyData.CopyPositionsBuffer[i] - m_copyData.BoundsPosition;
            var target_position = copy_dest_bounds.position + offset;

            // if the copy-dest bounds dont contain the relative position OR we dont have a copy tile OR there's a blocking tile in main, go to next
            if (!copy_dest_bounds.Contains(target_position)
                || m_copyData.CopyTilesBuffer[i] == null
                || m_mainGrid.HasTile(target_position)) continue; // TODO: this will bug with decal tiles! is there a way to "tag" them so we can only focus on tagged ones?

            // log the change data
            var change_data = new TileChangeData()
            {
                position = target_position,
                tile = m_copyData.CopyTilesBuffer[i],
                transform = Matrix4x4.identity, // needed to exist in right location
                color = Color.white // needed to be visible
            };
            changes.Add(change_data);

            event_cell_changes.Add(target_position); // log the addition
        }

        // update the grid
        m_reflectionGrid.SetTiles(changes.ToArray(), false);

        // invoke that we added some cells
        OnReflectionTilesChanged?.Invoke(event_cell_changes.ToArray(), m_reflectionGrid, CellChangeType.Created);

        // change cache statuses
        m_copyData.HasData = false;
        m_hasReflectionPlaced = true;

        locus.Pulse();
    }

    private void CheckClearReflection()
    {
        // clear any reflections that have been placed
        if (m_hasReflectionPlaced)
        {
            // get the count of cells in the reflection grid for proper array size
            var grid_bounds = m_reflectionGrid.cellBounds;
            int cell_count = m_reflectionGrid.GetTilesRangeCount(grid_bounds.position, grid_bounds.position + grid_bounds.size);
            
            // make the cell position array
            var cell_positions = new Vector3Int[cell_count];
            var cells_discard = new TileBase[cell_count];

            // fill it with the positions of cells (discarding tile data)
            // NOTE: You need to have an array for the tiles argument, otherwise positions is filled with 0s. Nice.
            m_reflectionGrid.GetTilesRangeNonAlloc(grid_bounds.position, grid_bounds.position + grid_bounds.size, cell_positions, cells_discard);

            // clear it all
            m_reflectionGrid.ClearAllTiles();
            m_hasReflectionPlaced = false; // mark as disposed

            // invoke that we cleared a range of tiles
            OnReflectionTilesChanged?.Invoke(cell_positions.ToArray(), m_reflectionGrid, CellChangeType.Destroyed);
        }
    }

    public bool HasCopy() => m_copyData.HasData;

    private bool FindOnscreenLocus(out LocusScript valid)
    {
        // not very performant since it checks every locus, even ones obviously too far
        // it's a game jam tho, so K.I.S.S.
        foreach (var locus in m_locii)
        {
            var vp = m_mainCamera.WorldToViewportPoint(locus.GetWorldPositionOfCenter());

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
