//using UnityEngine;

//[RequireComponent(typeof(Camera))]
//public class FixedAspectRatio : MonoBehaviour
//{
//    public float targetAspect = 40f / 9f;
//    int lastW, lastH;

//    void OnEnable() { Apply(); }
//    void Update() { if (Screen.width != lastW || Screen.height != lastH) Apply(); }

//    void Apply()
//    {
//        lastW = Screen.width; lastH = Screen.height;
//        var cam = GetComponent<Camera>();
//        float windowAspect = (float)Screen.width / Screen.height;

//        if (windowAspect > targetAspect)
//        {
//            // 가로가 너무 넓음 → 좌우 검은 띠(필라박스)
//            float scale = targetAspect / windowAspect;
//            cam.rect = new Rect((1f - scale) * 0.5f, 0f, scale, 1f);
//        }
//        else
//        {
//            // 세로가 너무 김 → 위아래 검은 띠(레터박스)
//            float scale = windowAspect / targetAspect;
//            cam.rect = new Rect(0f, (1f - scale) * 0.5f, 1f, scale);
//        }
//    }
//}
