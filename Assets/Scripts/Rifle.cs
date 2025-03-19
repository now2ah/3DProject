using JetBrains.Annotations;
using UnityEngine;

public class Rifle : Item
{
    public int damage;

    public Rifle(int damage) : base(EItemType.RIFLE)
    {
        this.damage = damage;
    }
}
