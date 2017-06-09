using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
   public class Player:LivingCreature
    {
        public int Gold { get; set; }
        public int ExperiencePoints { get; set; }
        public int Level { get { return ((ExperiencePoints / 100) + 1); }}
        public List<InventoryItem> Inventory { get; set; }
        public List<PlayerQuest> Quests { get; set; }
        public Location CurrentLocation { get; set; }

        public Player(int currentHitPoints, int maximumHitPoints, int gold, int experiencePoints) : base(currentHitPoints, maximumHitPoints)
        {
            Gold = gold;
            ExperiencePoints = experiencePoints;

            Inventory = new List<InventoryItem>();
            Quests = new List<PlayerQuest>();
            
           
        }

        public bool HasRequiredItemToEnterThisLocation(Location location)
        {
            if (location.ItemRequiredToEnter ==null)
            {
                return true;
            }

            //See if the player has the required item in their inventory
            foreach (InventoryItem inventoryItem in Inventory)
            {
                if (inventoryItem.Details.ID == location.ItemRequiredToEnter.ID)
                {
                    return true;
                }
            }

            //not found the requited item
            return false;
        }

        public bool HasThisQuest(Quest quest)
        {
            foreach (PlayerQuest playerQuest in Quests)
            {
                if (playerQuest.Details.ID == quest.ID)
                {
                    return true;
                }
            }
            return false;
        }

        public bool CompletedThisQuest(Quest quest)
        {
            foreach (PlayerQuest playerQuest in Quests)
            {
                if (playerQuest.Details.ID==quest.ID)
                {
                    return playerQuest.IsCompleted;
                }
            }
            return false;
        }

        public bool HasAllQuestCompletionItem(Quest quest)
        {
            //See if the player has all the item needed to complete the quest here
            foreach (QuestCompletionItem qci in quest.QuestCompletionItems)
            {
                bool foundItemInPlayersInventory = false;

                //Check to see quantity at the inventory
                foreach (InventoryItem ii in Inventory)
                {
                    if (ii.Details.ID==qci.Details.ID)
                    {
                        foundItemInPlayersInventory = true;

                        //Are there enough of this item to complete the quest
                        if (ii.Quantity<qci.Quantity)
                        {
                            return false;
                        }
                    }
                }

                //The player does not have any of this items
                if (!foundItemInPlayersInventory)
                {
                    return false;
                }
            }

            //Have all needed
            return true;
        }

        public void RemoveQuestCompletionItem(Quest quest)
        {
            foreach (QuestCompletionItem qci in quest.QuestCompletionItems)
            {
                foreach (InventoryItem ii in Inventory)
                {
                    if (ii.Details.ID == qci.Details.ID)
                    {
                        ii.Quantity -= qci.Quantity;
                        break;
                    }
                }
            }
        }

        public void AddItemToInventory(Item itemToAdd)
        {
            foreach (InventoryItem ii in Inventory)
            {
                if (ii.Details.ID == itemToAdd.ID)
                {
                    ii.Quantity++;

                    return;
                }
            }
            Inventory.Add(new InventoryItem(itemToAdd,1));
        }

        public void MarkQuestCompleted(Quest quest)
        {
            //Find the quest in the player's quest list
            foreach (PlayerQuest pq in Quests)
            {
                if (pq.Details.ID == quest.ID)
                {
                    //Mark it as completed
                    pq.IsCompleted = true;

                    return;
                }
            }
        }


    }
}
