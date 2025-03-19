using UnityEngine;

public enum EItemType
{
    RIFLE,
}

public abstract class Item : MonoBehaviour
{
    protected EItemType _type;

    public Item(EItemType type)
    {
        _type = type;
    }
}
