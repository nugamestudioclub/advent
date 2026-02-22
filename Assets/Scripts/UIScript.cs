using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class UIScript : MonoBehaviour
{
    [SerializeField] private ReflectionScript m_reflScript;
    [SerializeField] private Transform m_gridParent;
    [SerializeField] private Vector2 m_gridSize = new Vector2(95, 95);
    [SerializeField] private GridLayoutGroup m_gridLayout;
    [SerializeField] private Image m_gridItem;
    [SerializeField] private int m_poolCount = 25;

    [Header("Ornament")]

    [SerializeField] private TextMeshProUGUI m_numerator;
    [SerializeField] private TextMeshProUGUI m_denominator;

    private int m_totalCount;
    private int m_count;

    private List<Image> m_pool;

    private void Awake()
    {
        m_pool = new List<Image>();

        m_totalCount = FindObjectsByType<OrnamentScript>(FindObjectsSortMode.None).Length;

        for (int i = 0; i < m_poolCount; i++)
        {
            MakeNew(false);
        }

        OrnamentScript.OnOrnamentCollected += OrnamentCollected;
        m_reflScript.OnCopyDataChange += ReflectionTilesChanged;

        RefreshOrnamentText();
    }

    private void OnDestroy()
    {
        OrnamentScript.OnOrnamentCollected -= OrnamentCollected;
        m_reflScript.OnCopyDataChange -= ReflectionTilesChanged;
    }

    private void ReflectionTilesChanged(bool is_clear, TileBase[] tile_data, Vector3Int[] pos_data, Tilemap map)
    {
        foreach(var child in m_pool)
        {
            child.color = Color.clear;
            child.gameObject.SetActive(false);
        }

        if (is_clear) return;

        // get bottom-leftmost item
        // get x max and y max
        var lowest = new Vector3Int(999, 999, 999);
        float x_max = -999;
        float y_max = -999;
        for (int i = 0; i < pos_data.Length; ++i)
        {
            var pos = pos_data[i];

            if (pos.x < lowest.x && pos.y < lowest.y) lowest = pos;

            if (pos.x > x_max) x_max = pos.x;
            if (pos.y > y_max) y_max = pos.y;
        }

        // offset it so that xmax and ymax are set with an origin of 0, 0, 0
        x_max -= lowest.x;
        y_max -= lowest.y;

        float maximal = Mathf.Max(x_max, y_max) + 1;

        m_gridLayout.cellSize = new Vector2(m_gridSize.x / maximal, m_gridSize.y / maximal);

        var list = EnableNumber(pos_data.Length);

        // clear all
        foreach (Image image in list) image.sprite = null;

        // for each position, enable with a sprite
        for (int i = 0; i < pos_data.Length; i++)
        {
            var o_pos = pos_data[i] - lowest;

            var image = list[o_pos.x + o_pos.y * ((int)x_max+1)];

            image.color = tile_data[i] == null ? Color.clear : Color.white;
        }
    }

    private IList<Image> EnableNumber(int count)
    {
        var list = new List<Image>();

        int current = 0;

        // find items in our pool we can use
        foreach (var child in m_pool)
        {
            if (current >= count) return list;

            if (!child.gameObject.activeInHierarchy)
            {
                child.gameObject.SetActive(true);
                list.Add(child);

                current++;
            }
        }

        // if there are more things we need, make the rest
        while (current < count)
        {
            list.Add(MakeNew(true));

            current++;
        }

        return list;
    }

    private Image MakeNew(bool state)
    {
        var go = GameObject.Instantiate(m_gridItem, m_gridParent);
        go.gameObject.SetActive(state);

        go.name = "Item " + m_pool.Count;

        m_pool.Add(go);

        return go;
    }

    private void OrnamentCollected()
    {
        ++m_count;

        RefreshOrnamentText();
    }

    private void RefreshOrnamentText()
    {
        m_numerator.text = m_count.ToString();
        m_denominator.text = m_totalCount.ToString();
    }
}
