using System.Linq;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    CardData upcoming;
    int turnCount = 1;

    void Start()
    {
        SetupNextSkill();
        TurnManager.Instance.OnEnemyTurnStart += Act;
    }

    void SetupNextSkill()
    {
        var skills = DataManager.Instance.GetEnemySkills();
        int ownerID = DataManager.Instance.enemyData.ownerID;
        int enemyHP = CombatManager.Instance.enemyHp;

        switch (ownerID)
        {
            // 튜토리얼 송곳니
            case 1001 : upcoming = skills[0]; break;
            // 송곳니
            case 1004 :
                if (enemyHP <= 40) upcoming = skills.FirstOrDefault(s => s.displayName == "마지막 발악");
                else
                {
                    // 2턴 주기: 홀수턴엔 할퀴기, 짝수턴엔 움찔움찔
                    if (turnCount % 2 == 1)
                        upcoming = skills.FirstOrDefault(s => s.displayName == "할퀴기");
                    else
                        upcoming = skills.FirstOrDefault(s => s.displayName == "움찔움찔");

                    turnCount++;
                }
                break;
            default : upcoming = skills[Random.Range(0, skills.Length)]; break;
        }
        if (upcoming == null && skills.Length > 0)
            upcoming = skills[Random.Range(0, skills.Length)];
        
        // PreviewUI 출력
        PreviewUI.Instance.Show(upcoming);
    }   

    public void Act()  // 이걸 TurnManager.OnEnemyTurnStart 에 연결
    {
        // 적 행동 발동
        CombatManager.Instance.ApplySkill(upcoming, isPlayer:false);
        
        // 바로 다음 스킬 미리보기 준비
        SetupNextSkill();
    }
}