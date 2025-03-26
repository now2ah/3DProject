using JetBrains.Annotations;
using UnityEngine;

public class Rifle : Item
{
    public int damage;

    private void Awake()
    {
        _Initialize();
    }

    protected override void _Initialize()
    {
        _type = EItemType.RIFLE;
        damage = 3;
    }
}
