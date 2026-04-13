using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace SpellFall.Engine
{
    public class SoundManager
    {
        public Dictionary<string, SoundEffect> Sounds = new Dictionary<string, SoundEffect>();

        public void Load(ContentManager content, string name, string assetsPath)
        {
            if (!Sounds.ContainsKey(name))
            {
                SoundEffect sound = content.Load<SoundEffect>(assetsPath);
                Sounds.Add(name, sound);
            }
            else
            {
                Console.WriteLine($"Sound '{name}' already loaded!");
            }
        }

        
        public void PlaySound(string name, float volume, float pitch = 0f, float pan = 0f)
        {
            if(Sounds.TryGetValue(name,out SoundEffect soundEffect))
            {
                soundEffect.Play(volume, pitch, pan);
            }
        }   
    }
}
