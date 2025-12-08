using UnityEngine;

public class ItemCtrl : MonoBehaviour
{
    private void Start()
    {
        // 15초 후, 자동 파괴
        Destroy(gameObject, 15f);
    }
}
