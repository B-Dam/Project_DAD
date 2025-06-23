using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    CardData upcoming;

    void Start()
    {
        SetupNextSkill();
        TurnManager.Instance.OnEnemyTurnStart += Act;
    }

    void SetupNextSkill()
    {
        var skills   = DataManager.Instance.GetEnemySkills();
        upcoming     = skills[Random.Range(0, skills.Length)];
        PreviewUI.Instance.Show(upcoming);
    }

    public void Act()  // 이걸 TurnManager.OnEnemyTurnStart 에 연결
    {
        // 적 행동 발동
        CombatManager.Instance.ApplySkill(upcoming, isPlayer:false);
        // 바로 다음 스킬 미리보기 준비
        SetupNextSkill();
        // 다시 플레이어 턴으로 전환
        TurnManager.Instance.StartPlayerTurn();
    }
}