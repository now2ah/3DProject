using UnityEngine;

public class ZombieHitEffect : MonoBehaviour
{
    private Renderer[] targetRenderers;
    private MaterialPropertyBlock propBlock;

    private static readonly int IsHitID = Shader.PropertyToID("_IsHit");

    [SerializeField] private float flashDuration = 0.2f;

    void Awake()
    {
        targetRenderers = GetComponentsInChildren<Renderer>();
        propBlock = new MaterialPropertyBlock();
    }

    /// <summary>
    /// 외부(총알 등)에서 호출하여 피격 효과를 시작합니다.
    /// </summary>
    public void StartHitFlash()
    {
        foreach (var renderer in targetRenderers)
        {
            renderer.GetPropertyBlock(propBlock);

            propBlock.SetFloat(IsHitID, 1.0f);

            renderer.SetPropertyBlock(propBlock);

            CancelInvoke(nameof(StopHitFlash));
            Invoke(nameof(StopHitFlash), flashDuration);
        }
    }

    private void StopHitFlash()
    {
        foreach (var renderer in targetRenderers)
        {
            renderer.GetPropertyBlock(propBlock);

            propBlock.SetFloat(IsHitID, 0.0f);

            renderer.SetPropertyBlock(propBlock);
        }
    }
}
