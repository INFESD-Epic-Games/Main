using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace SpellFall.Sounds
{
    public class Sound
    {
        public string Name;
        public SoundEffect Effect;

        public Sound(string name, SoundEffect effect)
        {
            Name = name;
            Effect = effect;
        }
    }
}