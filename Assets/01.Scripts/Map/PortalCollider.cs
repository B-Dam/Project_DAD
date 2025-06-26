using UnityEngine;

public class PortalCollider : ColliderBase
{


    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(PlayerTag))
        {
            string colliderDir = gameObject.name;

            switch (colliderDir)
            {
                case "Right":
                    // 오른쪽 맵으로 이동하기
                    MoveToRightMap?.Invoke(); // 오른쪽 맵으로 이동하는 메소드 호출
                    break;
                case "Left":
                    // 왼쪽 맵으로 이동하기
                    MoveToLeftMap?.Invoke(); // 왼쪽 맵으로 이동하는 메소드 호출
                    break;
                case "Up":
                    // 위쪽 맵으로 이동하기
                    MoveToUpMap?.Invoke(); // 위쪽 맵으로 이동하는 메소드 호출
                    break;
                case "Down":
                    // 아래쪽 맵으로 이동하기
                    MoveToDownMap?.Invoke(); // 아래쪽 맵으로 이동하는 메소드 호출
                    break;
                default:
                    Debug.Log("얜 또 뭐야;;");
                    break;
            }
        }
    }


}
