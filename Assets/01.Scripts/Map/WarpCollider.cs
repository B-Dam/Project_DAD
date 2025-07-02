using UnityEngine;

public class WarpCollider : ColliderBase
{
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(PlayerTag))
        {
            string warpDir = gameObject.name;  // Right, Left µî

            MapMove(warpDir);
        }
    }
}
