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
                    // ������ ������ �̵��ϱ�
                    MoveToRightMap?.Invoke(); // ������ ������ �̵��ϴ� �޼ҵ� ȣ��
                    break;
                case "Left":
                    // ���� ������ �̵��ϱ�
                    MoveToLeftMap?.Invoke(); // ���� ������ �̵��ϴ� �޼ҵ� ȣ��
                    break;
                case "Up":
                    // ���� ������ �̵��ϱ�
                    MoveToUpMap?.Invoke(); // ���� ������ �̵��ϴ� �޼ҵ� ȣ��
                    break;
                case "Down":
                    // �Ʒ��� ������ �̵��ϱ�
                    MoveToDownMap?.Invoke(); // �Ʒ��� ������ �̵��ϴ� �޼ҵ� ȣ��
                    break;
                default:
                    Debug.Log("�� �� ����;;");
                    break;
            }
        }
    }


}
