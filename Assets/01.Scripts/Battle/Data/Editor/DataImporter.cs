#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class DataImporter : MonoBehaviour
{
    // CSV 경로
    private const string cardCsvPath = "Assets/07.Resources/skillcardDB.csv";

    // SO를 저장할 폴더
    private const string cardSoFolder = "Assets/06.ScriptableObjects/Cards";

    [MenuItem("Battle/Import Cards From CSV")]
    public static void ImportCards()
    {
        // 대상 폴더가 없으면 생성
        if (!Directory.Exists(cardSoFolder))
            Directory.CreateDirectory(cardSoFolder);

        // CSV 읽기
        var lines = File.ReadAllLines(cardCsvPath);
        
        // 첫 줄은 헤더이므로 i=1부터
        for (int i = 1; i < lines.Length; i++)
        {
            var cols = lines[i].Split(',');
            
            string id = cols[0].Trim();
            string assetPath = $"{cardSoFolder}/{id}.asset";

            // 기존에 있으면 불러오고, 없으면 새로 생성
            var so = AssetDatabase.LoadAssetAtPath<CardData>(assetPath)
                     ?? ScriptableObject.CreateInstance<CardData>();

            // 필드에 CSV 값 할당
            so.cardId             = cols[0].Trim();
            so.displayName        = cols[1].Trim();
            so.ownerType          = (OwnerType)System.Enum.Parse(typeof(OwnerType), cols[2].Trim(), true);
            so.typePrimary        = (CardTypePrimary)System.Enum.Parse(typeof(CardTypePrimary), cols[3].Trim(), true);
            
            // 보조 분류 빈 문자열은 None 처리
            string sec = cols[4].Trim();
            if (string.IsNullOrEmpty(sec))
            {
                so.typeSecondary = CardTypeSecondary.None;
            }
            else
            {
                so.typeSecondary = (CardTypeSecondary)System.Enum.Parse(
                    typeof(CardTypeSecondary), sec, true);
            }
            
            // 숫자 필드 (effectAttack, effectDefense, effectDebuff, effectTurn)
            so.effectAttackValue  = int.Parse(cols[5]);
            so.effectDefenseValue = int.Parse(cols[6]);
            so.effectDebuffValue  = int.Parse(cols[7]);
            so.effectTurnValue    = int.Parse(cols[8]);
            
            so.effectText = cols[9];
            
            // 소유자 타입이 플레이어인 경우
            if (so.ownerType == OwnerType.player)
            {
                // Player면 CSV에서 파싱
                if (!int.TryParse(cols[10].Trim(), out so.rank))
                    so.rank = 1;  // 실패 시 기본 1성으로
                so.animationID = cols[11].Trim();
            }
            else
            {
                // Player 외에는 rank=1, animationID="" 처럼 기본 지정
                so.rank        = 1;
                so.animationID = "";
            }
            
            // icon 과 costAP 는 CSV에 없으니 에디터에서 수동 할당

            // SO 에셋 생성 또는 갱신
            AssetDatabase.CreateAsset(so, assetPath);
            EditorUtility.SetDirty(so);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("CardData SO 생성 완료!");
    }
}
#endif