using System;
using System.Collections.Generic;

namespace SpellFall.Quests
{
    public class QuestManager
    {
        public List<Quest> ActiveQuests = new List<Quest>();

        public void AddQuest(Quest quest)
        {
            ActiveQuests.Add(quest);
        }

        public void AddProgress(string questName, int amount)
        {
            Console.WriteLine("Active quests count: " + ActiveQuests.Count);
            foreach (var quest in ActiveQuests)
            {
                Console.WriteLine("Active quests count: " + ActiveQuests.Count);
                if (quest.Name == questName && !quest.IsCompleted)
                {
                    System.Console.WriteLine("questmanager");
                    quest.AddProgress(amount);
                }
            }
        }

        public bool IsQuestCompleted(string questName)
        {
            foreach (var quest in ActiveQuests)
            {
                if (quest.Name == questName)
                    return quest.IsCompleted;
            }

            return false;
        }
    }
}