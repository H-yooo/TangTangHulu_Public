using UnityEngine;

public class EffectAutoDestroy : MonoBehaviour
{
    public void DestroySelf()
    {
        Destroy(transform.root.gameObject); // ��ü Effect ������ ����
    }
}
