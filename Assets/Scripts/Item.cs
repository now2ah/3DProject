using UnityEngine;

public enum EItemType
{
    RIFLE,
    BULLET,
    FLASH_LIGHT,
}

public abstract class Item : MonoBehaviour
{
    protected EItemType _type;
    public EItemType ItemType => _type;

    protected abstract void _Initialize();
}
