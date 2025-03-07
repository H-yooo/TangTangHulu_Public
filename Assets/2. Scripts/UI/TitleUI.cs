using System.Collections;
using UnityEngine;

public class TitleUI : MonoBehaviour
{
    public Animator ownerAnimator;
    public Animator customerShadowAnimator;
    public Animator customerAnimator;

    private float idleChangeInterval = 3f; // 상태 체크 간격

    private void Start()
    {
        StartCoroutine(IdleStateRoutine());
    }

    private IEnumerator IdleStateRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(idleChangeInterval);

            int randomValue = Random.Range(1, 4); // 1~3 중 랜덤 값
            if (randomValue == 1)
            {
                ownerAnimator.SetTrigger("Owner2");
                customerShadowAnimator.SetTrigger("CustomerShadow2");
                customerAnimator.SetTrigger("Customer2");
            }
            else
            {
                ownerAnimator.SetTrigger("Owner1");
                customerShadowAnimator.SetTrigger("CustomerShadow1");
                customerAnimator.SetTrigger("Customer1");
            }

        }
    }

}
