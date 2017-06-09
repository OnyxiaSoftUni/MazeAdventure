using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Engine;

namespace MazeAdventure
{
    public partial class MazeAdventure : Form
    {
        private Player _player;
        private Monster _currentMonster;

        public MazeAdventure()
        {
            InitializeComponent();

            _player = new Player(10, 10, 20, 0);
            MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
            _player.Inventory.Add(new InventoryItem(
                World.ItemByID(World.ITEM_ID_RUSTY_SWORD), 1));

           UpdatePlayerStats();
        }

        private void UpdatePlayerStats()
        {
            lblHitPoints.Text = _player.CurrentHitPoints.ToString();
            lblGold.Text = _player.Gold.ToString();
            lblExperience.Text = _player.ExperiencePoints.ToString();
            lblLevel.Text = _player.Level.ToString();
        }

        private void btnNorth_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToNorth);
        }

        private void btnEast_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToEast);
        }

        private void btnSouth_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToSouth);
        }

        private void btnWest_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToWest);
        }

        private void MoveTo(Location newLocation)
        {
            
            if (!_player.HasRequiredItemToEnterThisLocation(newLocation))
            {
                rtbMessages.Text += "You must have a " + newLocation.ItemRequiredToEnter.Name +
                                    " to enter this location." + Environment.NewLine;
                return;
            }

            //Update the player's current location
            _player.CurrentLocation = newLocation;

            //Show/hide available movements buttons
            btnNorth.Visible = (newLocation.LocationToNorth != null);
            btnEast.Visible = (newLocation.LocationToEast != null);
            btnSouth.Visible = (newLocation.LocationToSouth != null);
            btnWest.Visible = (newLocation.LocationToWest != null);

            //Display current location name and description
            rtbLocation.Text = newLocation.Name + Environment.NewLine;
            rtbLocation.Text += newLocation.Description + Environment.NewLine;

            //Completely heal the player
            _player.CurrentHitPoints = _player.MaximumHitPoints;

            //Update Hit Points in UI
            lblHitPoints.Text = _player.CurrentHitPoints.ToString();

            //Does the location have a quest?
            if (newLocation.QuestAvailableHere != null)
            {          
                //See if the player already has the quest and if they have completed it
                bool playerAlreadyHasQuest = _player.HasThisQuest(newLocation.QuestAvailableHere);
                bool playerAlreadyCompletedQuest = _player.CompletedThisQuest(newLocation.QuestAvailableHere);
                if (playerAlreadyHasQuest)
                {
                    //if not completed the quest yet
                    if (!playerAlreadyCompletedQuest)
                    {
                      //See if the player has all needed to complete the quest
                        bool playerHasAllItemsToCompleteQuest =
                            _player.HasAllQuestCompletionItem(newLocation.QuestAvailableHere);

                        //The player has all item required
                        if (playerHasAllItemsToCompleteQuest)
                        {
                            //Display massage
                            rtbMessages.Text += Environment.NewLine;
                            rtbMessages.Text += "You complete the " + newLocation.QuestAvailableHere.Name +
                                                " quest." + Environment.NewLine;

                            //Remove quest items from inventory
                           _player.RemoveQuestCompletionItem(newLocation.QuestAvailableHere);

                            //Give quest rewards
                            rtbMessages.Text += "You receive: " + Environment.NewLine;
                            rtbMessages.Text += newLocation.QuestAvailableHere.RewardExperiencePoints.ToString() +
                                                "experience points" + Environment.NewLine;
                            rtbMessages.Text += newLocation.QuestAvailableHere.RewardGold.ToString() + "gold" +
                                                Environment.NewLine;
                            rtbMessages.Text += newLocation.QuestAvailableHere.RewardItem.Name +
                                                Environment.NewLine;

                            _player.ExperiencePoints += newLocation.QuestAvailableHere.RewardExperiencePoints;
                            _player.Gold += newLocation.QuestAvailableHere.RewardGold;

                            //Add the reward
                           _player.AddItemToInventory(newLocation.QuestAvailableHere.RewardItem);

                            //Mark the quest as completed
                            _player.MarkQuestCompleted(newLocation.QuestAvailableHere);
                        }
                    }
                }
                else
                {
                    //The player does not already have the quest
                    //Display the messages
                    rtbMessages.Text += "You receive the " + newLocation.QuestAvailableHere.Name + " quest." +
                                        Environment.NewLine;
                    rtbMessages.Text += newLocation.QuestAvailableHere.Description + Environment.NewLine;
                    rtbMessages.Text += "To complete it, return with:" + Environment.NewLine;
                    foreach (QuestCompletionItem qci in newLocation.QuestAvailableHere.QuestCompletionItems)
                    {
                        if (qci.Quantity == 1)
                        {
                            rtbMessages.Text += qci.Quantity.ToString() + " " + qci.Details.Name +
                                                Environment.NewLine;
                        }
                        else
                        {
                            rtbMessages.Text += qci.Quantity.ToString() + " " + qci.Details.NamePlural +
                                                Environment.NewLine;
                        }
                    }
                    rtbMessages.Text += Environment.NewLine;

                    //Add the quest to the playe's quest list
                    _player.Quests.Add(new PlayerQuest(newLocation.QuestAvailableHere));
                }
            }

            //Does the location have a monster?
            if (newLocation.MonsterLivingHere != null)
            {
                rtbMessages.Text += "You see a " + newLocation.MonsterLivingHere.Name + Environment.NewLine;

                //Make a new monster,using the values from the standart monster in the World.Monster list
                Monster standartMonster = World.MonsterByID(newLocation.MonsterLivingHere.ID);

                _currentMonster = new Monster(standartMonster.ID, standartMonster.Name,
                    standartMonster.MaximumDamage, standartMonster.RewardExperiencePoints,
                    standartMonster.RewardGold, standartMonster.CurrentHitPoints, standartMonster.MaximumHitPoints);

                foreach (LootItem lootItem in standartMonster.LootTable)
                {
                    _currentMonster.LootTable.Add(lootItem);
                }

                cboWeapons.Visible = true;
                cboPotions.Visible = true;
                btnUseWeapon.Visible = true;
                btnUsePotion.Visible = true;
            }
            else
            {
                _currentMonster = null;

                cboWeapons.Visible = false;
                cboPotions.Visible = false;
                btnUseWeapon.Visible = false;
                btnUsePotion.Visible = false;
            }

            //Update player's inventory list
            UpdateInventoryListUI();

            //Refresh player's quest list
            UpdateQuestListInUI();

            //Update player's weapons combobox
            UpdateWeaponsListInUI();

            //Update player's potion combobox
            UpdatePotionListInUI();

            //Update player's stats
            UpdatePlayerStats();
        }

        private void UpdateInventoryListUI()
        {
            dgvInventory.RowHeadersVisible = false;

            dgvInventory.ColumnCount = 2;
            dgvInventory.Columns[0].Name = "Name";
            dgvInventory.Columns[0].Width = 197;
            dgvInventory.Columns[1].Name = "Quantity";

            dgvInventory.Rows.Clear();

            foreach (InventoryItem inventoryItem in _player.Inventory)
            {
                if (inventoryItem.Quantity > 0)
                {
                    dgvInventory.Rows.Add(new[] { inventoryItem.Details.Name, inventoryItem.Quantity.ToString() });
                }
            }
        }

        private void UpdateQuestListInUI()
        {
            dgvQuests.RowHeadersVisible = false;

            dgvQuests.ColumnCount = 2;
            dgvQuests.Columns[0].Name = "Name";
            dgvQuests.Columns[0].Width = 197;
            dgvQuests.Columns[1].Name = "Done?";

            dgvQuests.Rows.Clear();

            foreach (PlayerQuest playerQuest in _player.Quests)
            {
                dgvQuests.Rows.Add(new[] { playerQuest.Details.Name, playerQuest.IsCompleted.ToString() });
            }
        }

        private void UpdateWeaponsListInUI()
        {
            List<Weapon> weapons = new List<Weapon>();

            foreach (InventoryItem inventoryItem in _player.Inventory)
            {
                if (inventoryItem.Details is Weapon)
                {
                    if (inventoryItem.Quantity > 0)
                    {
                        weapons.Add((Weapon)inventoryItem.Details);
                    }
                }
            }

            if (weapons.Count == 0)
            {
                //The player doesn't have any weapons,so hide the weapon combobox and the "Use button
                cboWeapons.Visible = false;
                btnUseWeapon.Visible = false;
            }
            else
            {
                cboWeapons.DataSource = weapons;
                cboWeapons.DisplayMember = "Name";
                cboWeapons.ValueMember = "ID";

                cboWeapons.SelectedIndex = 0;
            }
        }

        private void UpdatePotionListInUI()
        {
            List<HealingPotion> healingPotion = new List<HealingPotion>();

            foreach (InventoryItem inventoryItem in _player.Inventory)
            {
                if (inventoryItem.Details is HealingPotion)
                {
                    if (inventoryItem.Quantity > 0)
                    {
                        healingPotion.Add((HealingPotion)inventoryItem.Details);
                    }
                }
            }

            if (healingPotion.Count == 0)
            {
                //The player doesn't have any potion, hide potion combobox and "Use" button
                cboPotions.Visible = false;
                btnUsePotion.Visible = false;
            }
            else
            {
                cboPotions.DataSource = healingPotion;
                cboPotions.DisplayMember = "Name";
                cboPotions.ValueMember = "ID";

                cboPotions.SelectedIndex = 0;
            }

        }

        private void btnUseWeapon_Click(object sender, EventArgs e)
        {
            //Get the currently selected weapon from cboWeapons ComboBox
            Weapon currentWeapon = (Weapon) cboWeapons.SelectedItem;

            //Determinate the amount of damage to the monster
            int damageToMonster =
                RandomNumberGenerator.NumberBetween(currentWeapon.MinimumDamage, currentWeapon.MaximumDamage);

            //Aplay the damage to the monster's Current Hit Points
            _currentMonster.CurrentHitPoints -= damageToMonster;

            //Display message
            rtbMessages.Text += "You hit the " + _currentMonster.Name + " for " + damageToMonster.ToString() +
                                " points." + Environment.NewLine;

            //Check if the monster is dead
            if (_currentMonster.CurrentHitPoints <= 0)
            {
                //Monster is dead
                rtbMessages.Text += Environment.NewLine;
                rtbMessages.Text += "You defeated the " + _currentMonster.Name + Environment.NewLine;

                //Give player experience points for killing the monster
                _player.ExperiencePoints += _currentMonster.RewardExperiencePoints;
                rtbMessages.Text += "You defeated the " + _currentMonster.RewardExperiencePoints.ToString() +
                                    " experience points" + Environment.NewLine;

                //Give player gold for killing the monster
                _player.Gold += _currentMonster.RewardGold;
                rtbMessages.Text += "You receive " + _currentMonster.RewardGold.ToString() + " gold" +
                                    Environment.NewLine;
                //Get random loot item from the monster
                List<InventoryItem> lootedItems = new List<InventoryItem>();

                //Add items to the list, comparing a random number to the drop percentage
                foreach (LootItem lootItem in _currentMonster.LootTable)
                {
                    if (RandomNumberGenerator.NumberBetween(1,100) <= lootItem.DropPercentage)
                    {
                        lootedItems.Add(new InventoryItem(lootItem.Details, 1));
                    }
                }

                //If no items were randomly selected, then add the default loot item
                if (lootedItems.Count == 0)
                {
                    foreach (LootItem lootItem in _currentMonster.LootTable)
                    {
                        if (lootItem.IsDefaultItem)
                        {
                            lootedItems.Add(new InventoryItem(lootItem.Details,1));
                        }
                    }
                }

                //Add the looted items to the player's inventory
                foreach (InventoryItem ii in lootedItems)
                {
                    _player.AddItemToInventory(ii.Details);

                    if (ii.Quantity == 1)
                    {
                        rtbMessages.Text += "You loot " + ii.Quantity.ToString() + " " + ii.Details.Name +
                                            Environment.NewLine;
                    }
                    else
                    {
                        rtbMessages.Text += "You loot " + ii.Quantity.ToString() + " " + ii.Details.NamePlural +
                                            Environment.NewLine;
                    }
                }

                //Update player information and inventory controls
                UpdatePlayerStats();
                UpdateInventoryListUI();
                UpdateWeaponsListInUI();
                UpdatePotionListInUI();

                //Add a blank line to the message box
                rtbMessages.Text += Environment.NewLine;

                //Move player to current location (to heal player and create a new monster to fight
                MoveTo(_player.CurrentLocation);
            }
            else
            {
                //Monster is still alive
                //Determine the amount of damage the monster does to the player
                int damageToPlayer = RandomNumberGenerator.NumberBetween(0, _currentMonster.MaximumDamage);

                //Display massage
                rtbMessages.Text += "The " + _currentMonster.Name + " did " + damageToPlayer.ToString() + " points of damage." + Environment.NewLine;

                //Substract damage from player
                _player.CurrentHitPoints -= damageToPlayer;

                //Update player data in UI
                lblHitPoints.Text = _player.CurrentHitPoints.ToString();

                if (_player.CurrentHitPoints <=0)
                {
                    //Display message
                    rtbMessages.Text += "The " + _currentMonster.Name + " killed you." + Environment.NewLine;

                    //Move player to "Home
                    MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
                }
            }
        }

        private void btnUsePotion_Click(object sender, EventArgs e)
        {
            //Get the currently selected potion from the comboBox
            HealingPotion potion = (HealingPotion) cboPotions.SelectedItem;

            //Add healing amound to the player's current hit points
            _player.CurrentHitPoints += potion.AmountToHeal;

            //Current points cannot exceed player's MaximumHitPoints
            if (_player.CurrentHitPoints > _player.MaximumHitPoints)
            {
                _player.CurrentHitPoints = _player.MaximumHitPoints;
            }

            //Remove the potion from the player's inventory
            foreach (InventoryItem ii in _player.Inventory)
            {
                if (ii.Details.ID == potion.ID)
                {
                    ii.Quantity--;
                    break;
                }
            }

            //Display message
            rtbMessages.Text += "You drink a " + potion.Name + Environment.NewLine;

            //Monster gets their turn to attack

            //Determine the amount of damege monster does to the player
            int damageToPlayer = RandomNumberGenerator.NumberBetween(0, _currentMonster.MaximumDamage);

            //Display message
            rtbMessages.Text += "The " + _currentMonster.Name + " did " + damageToPlayer.ToString() +
                                " points of damage." + Environment.NewLine;
            
            //Substract damage from player
            _player.CurrentHitPoints -= damageToPlayer;

            if (_player.CurrentHitPoints <= 0)
            {
                //Display message
                rtbMessages.Text += "The " + _currentMonster.Name + " killed you." + Environment.NewLine;

                //Move player to "Home
                MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
            }

            // Refresh player data in UI
            //Update player information and inventory controls
            lblHitPoints.Text = _player.CurrentHitPoints.ToString();

            UpdateInventoryListUI();
            UpdatePotionListInUI();
        }

        private void ScrollToBottomOfMessages()
        {
            rtbMessages.SelectionStart = rtbMessages.Text.Length;
            rtbMessages.ScrollToCaret();
        }
    }
}
