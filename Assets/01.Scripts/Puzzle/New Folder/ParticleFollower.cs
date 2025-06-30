//using UnityEngine;

//public class ParticleFollower : MonoBehaviour
//{
//    public Transform pointA;
//    public Transform pointB;
//    public float moveSpeed = 2f;

//    private float t = 0f;

//    void Update()
//    {
//        if (pointA && pointB)
//        {
//            t += Time.deltaTime * moveSpeed;
//            transform.position = Vector3.Lerp(pointA.position, pointB.position, Mathf.PingPong(t, 1f));
//        }
//    }
//}
