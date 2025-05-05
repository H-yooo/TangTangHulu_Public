using UnityEngine;

public class EffectAutoDestroy : MonoBehaviour
{
    public void DestroySelf()
    {
        Destroy(transform.root.gameObject); // 전체 Effect 프리팹 제거
    }
}
