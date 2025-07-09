using System.Collections;
using UnityEngine;

public class CombatAnimationController : MonoBehaviour
{
    [Header("애니메이터")]
    public Animator playerAnimator;
    public Animator enemyAnimator;
    
    [Header("일반 공격 시 움직임")]
    public GameObject playerCharacter;
    public GameObject enemyCharacter;
    public float attackMoveDistance = 100f;    // 얼마나 이동할지
    public float attackMoveDuration = 0.2f;  // 얼마나 빠르게
    
    [Header("공격 모션 딜레이")]
    [SerializeField] private float attackPause = 1f;  
    
    [Header("UI 제어용")]
    [SerializeField] private CanvasGroup gameUIGroup;
    [SerializeField] private GameObject retryPanel;
    
    private bool isEnemyMoving;
    private bool isPlayerMoving;
    
    private Coroutine playerMoveCoroutine;
    private Coroutine enemyMoveCoroutine;

    private void Start()
    {
        var cm = CombatManager.Instance;
        if (cm == null) Debug.LogError("CombatAnimationController: CombatManager.Instance is null");
        else
        {
            cm.OnPlayerSkillUsed += HandlePlayerSkill;
            cm.OnEnemySkillUsed  += HandleEnemySkill;
            cm.OnPlayerHit       += HandlePlayerHit;
            cm.OnEnemyHit        += HandleEnemyHit;
            cm.OnPlayerDeath     += HandlePlayerDeath;
            cm.OnEnemyDeath      += HandleEnemyDeath;
            cm.OnCombatStart     += HandleCombatStart;
        }
    }

    private void OnDestroy()
    {
        var cm = CombatManager.Instance;
        if (cm != null)
        {
            cm.OnPlayerSkillUsed -= HandlePlayerSkill;
            cm.OnEnemySkillUsed  -= HandleEnemySkill;
            cm.OnPlayerHit       -= HandlePlayerHit;
            cm.OnEnemyHit        -= HandleEnemyHit;
            cm.OnPlayerDeath     -= HandlePlayerDeath;
            cm.OnEnemyDeath      -= HandleEnemyDeath;
            cm.OnCombatStart     -= HandleCombatStart;
        }
    }

    private void HandlePlayerSkill(CardData data)
    {
        switch (data.displayName)  // 또는 data.id, data.skillName 등에 맞게
        {
            case "물기":
                playerAnimator.SetTrigger("Attack");
                // 이전 코루틴이 돌고 있으면 중단
                if (playerMoveCoroutine != null)
                    StopCoroutine(playerMoveCoroutine);
                // 새 코루틴 시작하고 레퍼런스 저장
                playerMoveCoroutine = StartCoroutine(PlayerDoAttackStep(playerCharacter.transform));
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
        if (isPlayerMoving) yield break;
        isPlayerMoving = true;
        
        Vector3 startPos = tf.localPosition;
        Vector3 midPos   = startPos + Vector3.right * attackMoveDistance;
        float   halfDur  = attackMoveDuration * 0.5f;
        float   elapsed  = 0f;

        // 앞쪽으로 이동
        while (elapsed < halfDur)
        {
            tf.localPosition = Vector3.Lerp(startPos, midPos, elapsed / halfDur);
            elapsed    += Time.deltaTime;
            yield return null;
        }
        tf.localPosition = midPos;

        yield return new WaitForSeconds(attackPause);

        // 뒤로 돌아오기
        elapsed = 0f;
        while (elapsed < halfDur)
        {
            tf.localPosition = Vector3.Lerp(midPos, startPos, elapsed / halfDur);
            elapsed    += Time.deltaTime;
            yield return null;
        }
        tf.localPosition = startPos;
        
        isPlayerMoving = false;
    }
    
   
    private void HandleEnemySkill(CardData data)
    {
        switch(data.ownerID)
        {
            case 1001 :
            case 1004 :
                switch (data.displayName)
                {
                    case "할퀴기":
                        enemyAnimator.SetTrigger("Attack");
                        if (!isEnemyMoving)
                            enemyMoveCoroutine = StartCoroutine(EnemyDoAttackStep(enemyCharacter.transform));
                        break;
                    case "움찔움찔":
                        enemyAnimator.SetTrigger("Twtich"); break;
                    case "마지막 발악":
                        enemyAnimator.SetTrigger("Enrage"); break;
                    default:
                        break;
                }
                break;
        };
    }
    
    private IEnumerator EnemyDoAttackStep(Transform tf)
    {
        if (isEnemyMoving) yield break;
        isEnemyMoving = true;
        
        Vector3 startPos = tf.localPosition;
        Vector3 midPos   = startPos + Vector3.left * attackMoveDistance;
        float   halfDur  = attackMoveDuration * 0.5f;
        float   elapsed  = 0f;

        // 앞쪽으로 이동
        while (elapsed < halfDur)
        {
            tf.localPosition = Vector3.Lerp(startPos, midPos, elapsed / halfDur);
            elapsed    += Time.deltaTime;
            yield return null;
        }
        tf.localPosition = midPos;

        yield return new WaitForSeconds(attackPause);

        // 뒤로 돌아오기
        elapsed = 0f;
        while (elapsed < halfDur)
        {
            tf.localPosition = Vector3.Lerp(midPos, startPos, elapsed / halfDur);
            elapsed    += Time.deltaTime;
            yield return null;
        }
        tf.localPosition = startPos;

        isEnemyMoving = false;
    }
    
    private void HandlePlayerHit()
    {
        playerAnimator.SetTrigger("Hit");
    }

    private void HandleEnemyHit()
    {
        enemyAnimator.SetTrigger("Hit");
    }
    
    private void HandlePlayerDeath()
    {
        // GameUI 잠금
        LockGameUI();
        // 남아 있는 Hit 트리거 클리어
        playerAnimator.ResetTrigger("Hit");
        // Die 트리거 활성
        playerAnimator.SetTrigger("Die");
        StartCoroutine(ShowRetryAfterDelay());
    }
    
    private void LockGameUI()
    {
        // 모든 버튼/슬롯을 비활성화
        gameUIGroup.interactable     = false;
        gameUIGroup.blocksRaycasts   = false;
    }
    
    private void HandleCombatStart()
    {
        if (CombatDataHolder.IsRetry)
        {
            playerAnimator.SetTrigger("Retry");
            // 한 번만 실행되도록 플래그 리셋
            CombatDataHolder.IsRetry = false;
        }
    }
    
    private IEnumerator ShowRetryAfterDelay()
    {
        // 3초 이후 재시작 패널 활성화
        yield return new WaitForSeconds(3f);
        retryPanel.SetActive(true);
        Time.timeScale = 0;
    }

    private void HandleEnemyDeath()
    {
        // GameUI 잠금
        LockGameUI();
        // Die 트리거 활성
        enemyAnimator.SetTrigger("Die");
        StartCoroutine(DelayedBattleEnd());
    }
    
    private IEnumerator DelayedBattleEnd()
    {
        yield return new WaitForSeconds(3f);
        // 기존 OnBattleEnd 이벤트 호출
        CombatDataHolder.LastTrigger?.OnBattleEnd();
    }
}