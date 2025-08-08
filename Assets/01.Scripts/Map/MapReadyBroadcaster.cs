using System;
using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class MapsReadyBroadcaster : MonoBehaviour
{
    public static event Action OnMapsReady;

    private IEnumerator Start()
    {
        // 모든 Awake/Start가 한 번 돌고 난 뒤
        yield return null;
        
        Debug.Log("[MapsReady] Maps & Confiner ready.");
        OnMapsReady?.Invoke();
    }
}