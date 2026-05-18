using Microsoft.Xna.Framework.Graphics;

namespace SpellFall.Items;

public interface IItem
{
    public string Name { get; }
    public string Description { get; }
    public string Rarity { get; }
    public Texture2D Icon { get; }
    public bool IsUnique { get; }
}