using UnityEngine;
using UnityEngine.Tilemaps;
using static ReflectionScript;

public class TileParticleSystemScript : MonoBehaviour
{
    [SerializeField] private ReflectionScript m_reflScript;

    [Space]

    [SerializeField] private int m_poolSize;
    [SerializeField] private GameObject m_effectSystem;

    private void Awake()
    {
        m_reflScript.OnReflectionTilesChanged += OnReflectionChange;

        for (int i = 0; i < m_poolSize; i++)
        {
            var go = GameObject.Instantiate(m_effectSystem, transform);
            go.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (m_reflScript) m_reflScript.OnReflectionTilesChanged -= OnReflectionChange;
    }

    private void OnReflectionChange(Vector3Int[] cell_changes, Tilemap map, CellChangeType type)
    {
        foreach (var pos in cell_changes)
        {
            var go = FindAvailableOrMake();
            go.transform.position = map.GetCellCenterWorld(pos);
            go.SetActive(true);
        }
    }

    private GameObject FindAvailableOrMake()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (!transform.GetChild(i).gameObject.activeInHierarchy) return transform.GetChild(i).gameObject;
        }

        return GameObject.Instantiate(m_effectSystem, transform);
    }
}
