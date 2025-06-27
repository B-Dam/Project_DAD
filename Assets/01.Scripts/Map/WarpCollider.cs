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
                    // ������ ������ �̵��ϴ� �޼ҵ� ȣ��
                    MoveToRightMap?.Invoke();
                    break;
                case "Left":
                    // ���� ������ �̵��ϴ� �޼ҵ� ȣ��
                    MoveToLeftMap?.Invoke(); 
                    break;
                case "Up":
                    // ���� ������ �̵��ϴ� �޼ҵ� ȣ��
                    MoveToUpMap?.Invoke(); 
                    break;
                case "Down":
                    // �Ʒ��� ������ �̵��ϴ� �޼ҵ� ȣ��
                    MoveToDownMap?.Invoke(); 
                    break;
                default:
                    break;
            }

            
        }
    }


}
