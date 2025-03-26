using UnityEngine;

public class FlashLight : Item
{
    private void Awake()
    {
        _Initialize();
    }

    protected override void _Initialize()
    {
        _type = EItemType.FLASH_LIGHT;
    }
}
