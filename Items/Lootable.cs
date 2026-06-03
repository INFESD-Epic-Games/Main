namespace SpellFall.Items
{
    public interface ILootable
    {
        string LootTierName { get; }
        void ApplyLootTier(string tierName);
    }
}