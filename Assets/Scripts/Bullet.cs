using UnityEngine;

public class Bullet : Item
{
    public int amount;

    private void Awake()
    {
        _Initialize();
    }

    protected override void _Initialize()
    {
        _type = EItemType.BULLET;
        amount = 3;
    }
}
