using UnityEditor.U2D.Aseprite;
using UnityEngine;

public class ColliderManager : MonoBehaviour
{

    private MapData _mapData;
    private string _currentMapID;
    private string playerTag = "Player";

    private void Awake()
    {
        _currentMapID = MapManager.Instance.GetMapName();
        _mapData = Database.Instance.Map.GetMapData(_currentMapID);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Vector2 contactPoint = collision.ClosestPoint(transform.position);

        foreach (Transform child in transform)
        {
            Collider2D childCollider = child.GetComponent<Collider2D>();
            if (collision.CompareTag(playerTag))
            {
                if (!childCollider.OverlapPoint(contactPoint))
                {
                    switch (child.name)
                    {
                        case "Left":
                            OnLeftMap();
                            break;
                        case "Right":
                            OnRightMap();
                            break;
                        case "Up":
                            OnUpMap();
                            break;
                        case "Down":
                            OnDownMap();
                            break;
                        default:
                            break;
                    }
                }

            }
        }
    }

    private void OnLeftMap()
    {
        Debug.Log("왼쪽 맵으로 이동합니다.");
        MapManager.Instance.LoadMap(_mapData.left_map);
    }

    private void OnRightMap()
    {
        Debug.Log("오른쪽 맵으로 이동합니다.");
        MapManager.Instance.LoadMap(_mapData.right_map);
    }

    private void OnUpMap()
    {
        Debug.Log("위쪽 맵으로 이동합니다.");
        MapManager.Instance.LoadMap(_mapData.up_map);
    }

    private void OnDownMap()
    {
        Debug.Log("아래쪽 맵으로 이동합니다.");
        MapManager.Instance.LoadMap(_mapData.down_map);
    }


}
