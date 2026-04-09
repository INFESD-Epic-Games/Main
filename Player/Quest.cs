using System;

namespace SpellFall.Quests
{
    public class Quest
    {
        public string Name;
        public string Description;
        public int RequiredAmount;
        public int CurrentAmount;
        public bool IsCompleted;
        public Action OnComplete;

        public Quest(string name, string description, int requiredAmount, Action onComplete)
        {
            Name = name;
            Description = description;
            RequiredAmount = requiredAmount;
            CurrentAmount = 0;
            IsCompleted = false;
            OnComplete = onComplete;
        }

        public void AddProgress(int amount)
        {
            if (IsCompleted) return;

            CurrentAmount += amount;
            System.Console.WriteLine("Progress added");

            if (CurrentAmount >= RequiredAmount)
            {
                Complete();
            }
        }

        private void Complete()
        {
            IsCompleted = true;
            OnComplete?.Invoke();
        }
    }
}