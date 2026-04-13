using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace SpellFall.Engine
{
    public class SoundManager
    {
        public Dictionary<string, SoundEffect> Sounds = new Dictionary<string, SoundEffect>();
        private SoundEffectInstance currentMusic;
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
        public void PlayMusic(string name, bool loop, float volume)
        {
            if (Sounds.TryGetValue(name, out SoundEffect soundEffect))
            {
                currentMusic?.Stop();

                currentMusic = soundEffect.CreateInstance();
                currentMusic.IsLooped = loop;
                currentMusic.Volume = 0.5f;
                currentMusic.Play();
            }   
            else
            {
                Console.WriteLine($"Music '{name}' not found!");
            }
        }   
        public void PauseMusic()
        {
            currentMusic.Pause();
        }
        public void ResumeMusic()
        {
            currentMusic.Resume();
        }
        public void StopMusic()
        {
            currentMusic.Stop();
        }
        public void SetMusicVolume(float volume)
        {
            currentMusic.Volume = volume;
        }
    }
}
