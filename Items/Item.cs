using Microsoft.Xna.Framework.Graphics;

namespace SpellFall.Items;

public abstract class Item
{
    public string Name;
    public string Description;
    public LootTier Tier;
    public Texture2D Icon;
    public bool IsUnique;
}

public class WeaponItem : Item
{
    public int Damage;
}