using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class V2MapBase : MonoBehaviour
{
    /// <summary>
    /// 새로 만들 스크립트 요약 (V2)
    /// 1. 맵 별로 좌표 영역 설정 V
    /// 2. 지정한 좌표 안에 있으면 currentMapID를 해당 맵으로 변경 V
    /// 2-1. 처음 시작했을 땐 001로 설정 V
    /// 2-2. 콜라이더로 이동시키면 currentMapID를 변경 V
    /// 2-3. 변경 방법 : 현재 맵(mapData)의 dir_map 변수에 따르기 V
    /// 3. 맵 방향에 맞는 콜라이더 오브젝트에 맵 이동 타일 그리기 - (001, 002 V)
    /// 4. 스토리 버전과 퍼즐 버전이 있는 맵은 bool 값으로 처리해서 따로 띄우기 -
    /// </summary>

    // 타일맵 설정
    public List<Tilemap> tilemaps;
    private Dictionary<string, BoundsInt> mapBounds = new Dictionary<string, BoundsInt>();

    // 퍼즐 버전 맵 클리어 여부
    public bool isClear105 = false;
    public bool isClear108 = false;

    private void Start()
    {
        // 타일맵들 좌표 영역 설정
        foreach (var tilemap in tilemaps)
        {
            string mapName = tilemap.gameObject.name;
            BoundsInt bounds = tilemap.cellBounds;
            mapBounds[mapName] = bounds;  // 각 맵의 좌표 영역 정보를 딕셔너리에 저장
        }
    }

    // 맵 아이디를 키로 검색하면 해당 맵의 좌표값 반환
    public BoundsInt GetBounds(string mapID)
    {
        return mapBounds.ContainsKey(mapID) ? mapBounds[mapID] : new BoundsInt();
    }

    // 콜라이더 관련 기능

    // 맵을 이동해서 currentMapID를 변경했을 때 mapData를 업데이트하는 함수

}
