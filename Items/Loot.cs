using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Microsoft.Xna.Framework.Graphics;
using SpellFall.Collision;
using SpellFall.Engine;

namespace SpellFall.Items
{
    public class LootTier
    {
        public string Name;
        public float BaseWeight;
        public float RarityFactor;

        public LootTier(string name, float weight, float rarityFactor)
        {
            Name = name;
            BaseWeight = weight;
            RarityFactor = rarityFactor;
        }
    }
    public class Loot : GameObject
    {
        public CircleCollider circleCollider { get; private set; }
        private Random rng = new Random();
        
        private List<LootTier> lootTable = new List<LootTier>()
        {
            new LootTier("Rusty", 50f, 1f),
            new LootTier("Common", 30f, 1f),
            new LootTier("Uncommon", 15f, 2f),
            new LootTier("Rare", 8f, 3f),
            new LootTier("Epic", 4f, 4f),
            new LootTier("Legendary", 2f, 5f),
            new LootTier("Mythic", 1f, 6f),
        };

        public Loot()
        {
            
        }


        float GetModifiedWeight(LootTier entry, float luck)
        {
            return entry.BaseWeight * (1+ luck * entry.RarityFactor * 1f);
        }

        // Gets the rarity of the item
        public LootTier GetRandomRarity(float luck)
        {
            float totalWeight = 0f;

            List<float> modifiedWeights = new List<float>();

            // Calculate weights
            foreach (var entry in lootTable)
            {
                float w = GetModifiedWeight(entry, luck);
                modifiedWeights.Add(w);
                totalWeight += w;
            }

            // Roll
            double roll = rng.NextDouble() * totalWeight;

            float cumulative = 0f;

            for (int i = 0; i < lootTable.Count; i++)
            {
                cumulative += modifiedWeights[i];

                if (roll <= cumulative)
                {
                    Console.WriteLine(lootTable[i].Name);
                    return lootTable[i];
                }
            }

            // fallback
            Console.WriteLine("fallback");
            return lootTable[0];
        }
    }
}