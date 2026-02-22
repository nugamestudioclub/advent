using Unity.VisualScripting;
using UnityEngine;

public class SnapToPlayerView : MonoBehaviour
{
    private Camera cam;
    [SerializeField] private Transform player;

    private void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        Vector3 viewPos = cam.WorldToViewportPoint(player.position);

        Vector3 camPos = cam.transform.position;

        float width = cam.orthographicSize * cam.aspect * 2f;
        float height = cam.orthographicSize * 2f;

        if (viewPos.x < 0) camPos.x -= width;
        else if (viewPos.x > 1) camPos.x += width;
        else if (viewPos.y < 0) camPos.y -= height;
        else if (viewPos.y > 1) camPos.y += height;

        cam.transform.position = camPos;
    }
}
