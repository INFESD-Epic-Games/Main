using System;
using System.Collections.Generic;
using System.Linq;

namespace SpellFall.Quests
{
    public class QuestManager
    {
        public List<Quest> ActiveQuests = new List<Quest>();

        public void AddQuest(Quest quest)
        {
            if (quest == null)
            {
                return;
            }

            if (ActiveQuests.Any(q => q.Name == quest.Name))
            {
                return;
            }

            ActiveQuests.Add(quest);
        }

        public bool HasActiveQuest(string questName)
        {
            return ActiveQuests.Any(q => q.Name == questName && !q.IsCompleted);
        }

        public void AddProgress(string questName, int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            foreach (var quest in ActiveQuests)
            {
                if (quest.Name == questName && !quest.IsCompleted)
                {
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