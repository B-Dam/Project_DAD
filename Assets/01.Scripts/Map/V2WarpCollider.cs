using UnityEngine;

public class V2WarpCollider : V2ColliderBase
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(PlayerTag))
        {
            string warpDir = this.gameObject.name;  // Right, Left 등

            V2MapMove(warpDir);
        }
    }
}