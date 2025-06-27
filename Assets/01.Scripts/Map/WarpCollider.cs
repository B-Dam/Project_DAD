using UnityEngine;

public class WarpCollider : ColliderBase
{


    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(PlayerTag))
        {
            string warpDir = gameObject.name;

            switch (warpDir)
            {
                case "Right":
                    // 오른쪽 맵으로 이동하는 메소드 호출
                    MoveToRightMap?.Invoke();
                    break;
                case "Left":
                    // 왼쪽 맵으로 이동하는 메소드 호출
                    MoveToLeftMap?.Invoke(); 
                    break;
                case "Up":
                    // 위쪽 맵으로 이동하는 메소드 호출
                    MoveToUpMap?.Invoke(); 
                    break;
                case "Down":
                    // 아래쪽 맵으로 이동하는 메소드 호출
                    MoveToDownMap?.Invoke(); 
                    break;
                default:
                    break;
            }

            
        }
    }


}
