using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Customer : MonoBehaviour
{
    [SerializeField] private Animator animator; // 손님의 애니메이터
    private float defaultMoveSpeed = 10.0f;
    private float moveSpeed = 10.0f;
    private float defaultAnimatorSpeed = 1f;
    private float moveoutdelay = 1f;
    private float targetPositionY = 2.8f; // 목표 위치
    private float exitPositionX = 5.0f; // 나갈 때의 X 위치

    private bool isWaiting = false; // 손님이 대기 중인지 확인하는 플래그

    // Fever 모드 활성화/비활성화
    public void CustomerFeverMode(bool isActive)
    {
        if (isActive)
        {
            moveSpeed = defaultMoveSpeed * 2f; // 이동 속도 증가
            animator.speed = defaultAnimatorSpeed * 2f; // 애니메이터 속도 증가
            moveoutdelay = 0.5f;
        }
        else
        {
            moveSpeed = defaultMoveSpeed; // 이동 속도 복원
            animator.speed = defaultAnimatorSpeed; // 애니메이터 속도 복원
            moveoutdelay = 1f;
        }
    }

    public bool IsWaiting()
    {
        return isWaiting;
    }

    public void SetIdleState()
    {
        // 대기 상태 설정
        isWaiting = true;
        animator.SetBool("isWalk", false);
        animator.SetBool("isIdle", true);
    }

    public void MoveCustomerToPosition()
    {
        isWaiting = false; // 목표 위치로 이동할 때는 대기 상태 해제

        animator.SetBool("isIdle", false); // Idle 상태 해제
        animator.SetBool("isWalk", true); // 걷기 애니메이션 시작

        StartCoroutine(MoveCustomerCoroutine(targetPositionY));
    }

    private IEnumerator MoveCustomerCoroutine(float targetY)
    {
        Vector3 targetPosition = new Vector3(transform.position.x, targetY, transform.position.z);

        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            // 이동
            animator.SetBool("isIdle", false);
            animator.SetBool("isWalk", true);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        // 목표 위치에 도달한 후 위치 정확히 설정
        transform.position = targetPosition;
        animator.SetBool("isWalk", false);
        animator.SetBool("isIdle", true);

        CustomerManager.Instance.OnCustomerArrived(this);
        InputManager.Instance.BlockInput(false);
    }

    // 만족 애니메이션 플레이 메서드
    public void PlaySatisfactionAnimation(bool isSatisfy)
    {
        animator.SetBool("isIdle", false); // Idle 상태 해제
        if (isSatisfy)
        {
            animator.SetTrigger("Satisfy");
        }
        else
        {
            animator.SetTrigger("Dissatisfy");
        }
    }

    // 에니메이션 이벤트로 호출
    public void MoveOut()
    {
        animator.SetTrigger("Exit");
        StartCoroutine(MoveCustomerOutCoroutine(moveoutdelay));
    }

    private IEnumerator MoveCustomerOutCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        Vector3 exitPosition = new Vector3(exitPositionX, transform.position.y, transform.position.z);

        while (Vector3.Distance(transform.position, exitPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, exitPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        // 나가기 완료 후 CustomerManager에 알림
        CustomerManager.Instance.OnCustomerExited(this);
    }
}