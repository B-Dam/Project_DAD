using System.Collections;
using UnityEngine;

public class CombatAnimationController : MonoBehaviour
{
    public Animator playerAnimator;
    public Animator enemyAnimator;
    
    [Header("일반 공격 시 움직임")]
    public GameObject playerCharacter;
    public GameObject enemyCharacter;
    public float attackMoveDistance = 100f;    // 얼마나 이동할지
    public float attackMoveDuration = 0.2f;  // 얼마나 빠르게

    private void Start()
    {
        var cm = CombatManager.Instance;
        if (cm == null) Debug.LogError("CombatAnimationController: CombatManager.Instance is null");
        else
        {
            cm.OnPlayerSkillUsed += HandlePlayerSkill;
            cm.OnEnemySkillUsed  += HandleEnemySkill;
        }
    }

    private void OnDestroy()
    {
        var cm = CombatManager.Instance;
        if (cm != null)
        {
            cm.OnPlayerSkillUsed -= HandlePlayerSkill;
            cm.OnEnemySkillUsed  -= HandleEnemySkill;
        }
    }

    private void HandlePlayerSkill(CardData data)
    {
        switch (data.displayName)  // 또는 data.id, data.skillName 등에 맞게
        {
            case "물기":
                playerAnimator.SetTrigger("Attack");
                StartCoroutine(PlayerDoAttackStep(playerCharacter.transform));
                break;
            case "으르렁거리기":
                playerAnimator.SetTrigger("Bark");
                break;
            case "웅크리기":
                playerAnimator.SetTrigger("Def");
                break;
            default:
                playerAnimator.SetTrigger("Attack");
                break;
        }
    }
    
    private IEnumerator PlayerDoAttackStep(Transform tf)
    {
        Vector3 startPos = tf.position;
        Vector3 midPos   = startPos + Vector3.right * attackMoveDistance;
        float   halfDur  = attackMoveDuration * 0.5f;
        float   elapsed  = 0f;

        // 앞쪽으로 이동
        while (elapsed < halfDur)
        {
            tf.position = Vector3.Lerp(startPos, midPos, elapsed / halfDur);
            elapsed    += Time.deltaTime;
            yield return null;
        }
        tf.position = midPos;

        yield return new WaitForSeconds(1f);

        // 뒤로 돌아오기
        elapsed = 0f;
        while (elapsed < halfDur)
        {
            tf.position = Vector3.Lerp(midPos, startPos, elapsed / halfDur);
            elapsed    += Time.deltaTime;
            yield return null;
        }
        tf.position = startPos;
    }
    
   
    private void HandleEnemySkill(CardData data)
    {
        // 적도 동일하게 분기 가능
        switch (data.displayName)
        {
            case "할퀴기":
                enemyAnimator.SetTrigger("Attack");
                StartCoroutine(EnemyDoAttackStep(enemyCharacter.transform));
                break;
            default :
                break;
        }
    }
    
    private IEnumerator EnemyDoAttackStep(Transform tf)
    {
        Vector3 startPos = tf.position;
        Vector3 midPos   = startPos + Vector3.left * attackMoveDistance;
        float   halfDur  = attackMoveDuration * 0.5f;
        float   elapsed  = 0f;

        // 앞쪽으로 이동
        while (elapsed < halfDur)
        {
            tf.position = Vector3.Lerp(startPos, midPos, elapsed / halfDur);
            elapsed    += Time.deltaTime;
            yield return null;
        }
        tf.position = midPos;

        yield return new WaitForSeconds(1f);

        // 뒤로 돌아오기
        elapsed = 0f;
        while (elapsed < halfDur)
        {
            tf.position = Vector3.Lerp(midPos, startPos, elapsed / halfDur);
            elapsed    += Time.deltaTime;
            yield return null;
        }
        tf.position = startPos;
    }

}