using System.Collections.Generic;
using UnityEngine;

public class HoleClaimRegistry : MonoBehaviour
{
    //boxId -> 점유한 HoleTrigger
    private static readonly Dictionary<int, HoleTrigger> holeClaimed = new();

    /// <summary>
    /// 박스(boxID)를 아직 누구도 점유하지 않았다면 claimer로 점유하고 true 반환
    /// 이미 점유되어 있다면 false 반환
    /// </summary>
    public static bool TryClaim(int boxId, HoleTrigger claimer)
    {
        if(holeClaimed.ContainsKey(boxId))
        {
            // 이미 점유됨
            return false;
        }
        holeClaimed[boxId] = claimer;
        return true;
    }

    /// <summary>
    /// claimer가 해당 박스를 점유 중일 때만 해제.
    /// </summary>
    public static void Release(int boxId, HoleTrigger claimer)
    {
        if (holeClaimed.TryGetValue(boxId, out var currentClaimer) && currentClaimer == claimer)
        {
            holeClaimed.Remove(boxId);
        }
    }

    /// <summary>
    /// 필요 시 (씬 리셋 등) 전체 초기화
    ///</summary>
    public static void ClearAllClaims()
    {
        holeClaimed.Clear();
    }
}
