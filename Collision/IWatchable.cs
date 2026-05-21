using Microsoft.Xna.Framework;

namespace SpellFall.Collision;

public interface IWatchable
{
    public bool IsWatched { get; set; }
}