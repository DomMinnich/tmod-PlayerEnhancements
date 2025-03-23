/*
50 % damage reduction, 
50% damage reduction from projectiles,
3x life regen, 
20% life steal,
infinite reach using L key,
random item using J key,
search item using K key,
Right and Left arrow keys to navigate items within the search,
Up and Down arrow keys to navigate quantity presets 
(1, 20, 200, 2000) within the search),
Escape to cancel the search,

*/
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace Stuff
{
    public class Stuff : ModPlayer
    {
        // Key bindings configuration
        private readonly Keys RandomItemKey = Keys.OemOpenBrackets; // [ key
        private readonly Keys ItemSearchKey = Keys.P;
        private readonly Keys InfiniteReachKey = Keys.OemCloseBrackets;
        
        // Track key states
        private bool randomItemKeyPreviouslyPressed = false;
        private bool itemSearchKeyPreviouslyPressed = false;
        private bool infiniteReachKeyPreviouslyPressed = false;
        private bool infiniteReachEnabled = false; // toggle for infinite reach
        
        private bool isAwaitingItemInput = false;
        private string currentInput = "";
        private List<int> matchingItems = new List<int>();
        private int selectedItemIndex = 0;
        private bool isQuantityInput = false;
        private string quantityInput = "";
        private int[] quantityPresets = { 1, 20, 200, 2000 };
        private int selectedQuantityIndex = 0;
        private int displayStartIndex = 0;

        public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
        {
            if (CheckPrivilegedPlayer())
            {
                modifiers.FinalDamage *= 0.5f; // 50% damage reduction
            }
        }

        public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
        {
            if (CheckPrivilegedPlayer())
            {
                modifiers.FinalDamage *= 0.5f; // 50% damage reduction
            }
        }

        public override void NaturalLifeRegen(ref float regen)
        {
            if (CheckPrivilegedPlayer())
            {
                regen *= 3f; // 3x life regen
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (CheckPrivilegedPlayer())
            { 
                int lifeStealAmount = damageDone / 5; // 20% life steal
                this.Player.statLife = Math.Min(this.Player.statLife + lifeStealAmount, this.Player.statLifeMax2);
            }
        }

        public override void ResetEffects()
        {
            if (infiniteReachEnabled)
            {
                Player.tileRangeX = 999;
                Player.tileRangeY = 999;
            }
        }
        
        public override void PostUpdate()
        {
            // Random item key handling
            bool randomItemKeyCurrentlyPressed = Main.keyState.IsKeyDown(RandomItemKey);
            if (randomItemKeyCurrentlyPressed && !randomItemKeyPreviouslyPressed)
            {
                if (CheckPrivilegedPlayer() && !isAwaitingItemInput)
                {
                    // Generate a random item ID
                    int randomItemID = Main.rand.Next(1, 5500);
                    if (randomItemID < ItemID.Count)
                    {
                        // Spawn the item using our helper method (handles multiplayer)
                        SpawnItem(randomItemID, 1);
                    }
                }
            }
            randomItemKeyPreviouslyPressed = randomItemKeyCurrentlyPressed;

            // Item search key handling
            bool itemSearchKeyCurrentlyPressed = Main.keyState.IsKeyDown(ItemSearchKey);
            if (itemSearchKeyCurrentlyPressed && !itemSearchKeyPreviouslyPressed)
            {
                if (CheckPrivilegedPlayer())
                {
                    if (!isAwaitingItemInput)
                    {
                        isAwaitingItemInput = true;
                        isQuantityInput = false;
                        currentInput = "";
                        quantityInput = "";
                        matchingItems.Clear();
                        selectedItemIndex = 0;
                        displayStartIndex = 0;
                        selectedQuantityIndex = 0;
                        ClearChat();
                        // Main.NewText("Type item name to search...", Color.Yellow);
                    }
                    else
                    {
                        isAwaitingItemInput = false;
                        ClearChat();
                    }
                }
            }
            itemSearchKeyPreviouslyPressed = itemSearchKeyCurrentlyPressed;
            
            // Infinite reach key handling
            bool infiniteReachKeyCurrentlyPressed = Main.keyState.IsKeyDown(InfiniteReachKey);
            if (infiniteReachKeyCurrentlyPressed && !infiniteReachKeyPreviouslyPressed)
            {
                if (CheckPrivilegedPlayer())
                {
                    infiniteReachEnabled = !infiniteReachEnabled;
                    // Optionally display a message:
                    // Main.NewText(infiniteReachEnabled ? "Infinite reach enabled" : "Infinite reach disabled", Color.LightGreen);
                }
            }
            infiniteReachKeyPreviouslyPressed = infiniteReachKeyCurrentlyPressed;

            // Process item search input if active
            if (isAwaitingItemInput)
            {
                if (Main.keyState.IsKeyDown(Keys.Enter))
                {
                    Main.chatRelease = false;
                }
                HandleItemSearchInput();
            }
        }

        /// <summary>
        /// Helper method that spawns the item. If in multiplayer client mode, sends a packet to the server.
        /// </summary>
        private void SpawnItem(int itemID, int quantity)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                var packet = Stuff.Instance.GetPacket();
                packet.Write((byte)MessageType.SpawnItem);
                packet.Write(itemID);
                packet.Write(quantity);
                packet.Write((int)Player.position.X);
                packet.Write((int)Player.position.Y);
                packet.Send();
            }
            else
            {
                for (int i = 0; i < quantity; i++)
                {
                    Item.NewItem(Player.GetSource_GiftOrReward(), (int)Player.position.X, (int)Player.position.Y, 
                        Player.width, Player.height, itemID);
                }
            }
        }
        
        private void HandleItemSearchInput()
        {
            var keyState = Main.keyState;
            var oldKeyState = Main.oldKeyState;

            if (keyState.IsKeyDown(Keys.Escape) && !oldKeyState.IsKeyDown(Keys.Escape))
            {
                isAwaitingItemInput = false;
                isQuantityInput = false;
                ClearChat();
                // Main.NewText("Item search cancelled.", Color.Red);
                return;
            }

            if (keyState.IsKeyDown(Keys.Enter) && !oldKeyState.IsKeyDown(Keys.Enter))
            {
                if (matchingItems.Count > 0)
                {
                    int itemID = matchingItems[selectedItemIndex];
                    string itemName = Lang.GetItemNameValue(itemID);
                    int quantity = isQuantityInput ? quantityPresets[selectedQuantityIndex] : 1;
                    ClearChat();
                    // Spawn the selected item the desired number of times
                    for (int i = 0; i < quantity; i++)
                    {
                        SpawnItem(itemID, 1);
                    }
                    // Main.NewText($"Received: {quantity}x {itemName}!", Color.Green);
                    isAwaitingItemInput = false;
                    isQuantityInput = false;
                }
                return;
            }

            if (keyState.IsKeyDown(Keys.Left) && !oldKeyState.IsKeyDown(Keys.Left))
            {
                if (matchingItems.Count > 0)
                {
                    selectedItemIndex = (selectedItemIndex - 1 + matchingItems.Count) % matchingItems.Count;
                    UpdateDisplayWindow();
                    ShowCurrentSuggestion();
                }
                return;
            }

            if (keyState.IsKeyDown(Keys.Right) && !oldKeyState.IsKeyDown(Keys.Right))
            {
                if (matchingItems.Count > 0)
                {
                    selectedItemIndex = (selectedItemIndex + 1) % matchingItems.Count;
                    UpdateDisplayWindow();
                    ShowCurrentSuggestion();
                }
                return;
            }

            if (keyState.IsKeyDown(Keys.Up) && !oldKeyState.IsKeyDown(Keys.Up))
            {
                if (matchingItems.Count > 0)
                {
                    isQuantityInput = true;
                    selectedQuantityIndex = (selectedQuantityIndex - 1 + quantityPresets.Length) % quantityPresets.Length;
                    ShowCurrentQuantity();
                }
                return;
            }

            if (keyState.IsKeyDown(Keys.Down) && !oldKeyState.IsKeyDown(Keys.Down))
            {
                if (matchingItems.Count > 0)
                {
                    isQuantityInput = true;
                    selectedQuantityIndex = (selectedQuantityIndex + 1) % quantityPresets.Length;
                    ShowCurrentQuantity();
                }
                return;
            }

            foreach (Keys key in Enum.GetValues(typeof(Keys)))
            {
                if (keyState.IsKeyDown(key) && !oldKeyState.IsKeyDown(key))
                {
                    if (key == Keys.Back && !isQuantityInput && currentInput.Length > 0)
                    {
                        currentInput = currentInput.Substring(0, currentInput.Length - 1);
                        UpdateMatchingItems();
                        return;
                    }
                    char? c = KeyToChar(key, keyState.IsKeyDown(Keys.LeftShift) || keyState.IsKeyDown(Keys.RightShift));
                    if (c.HasValue && !isQuantityInput)
                    {
                        currentInput += c.Value;
                        UpdateMatchingItems();
                        return;
                    }
                }
            }
        }

        private void UpdateDisplayWindow()
        {
            const int displayCount = 5;
            displayStartIndex = Math.Max(0, selectedItemIndex - displayCount / 2);
            displayStartIndex = Math.Min(displayStartIndex, Math.Max(0, matchingItems.Count - displayCount));
        }

        private void UpdateMatchingItems()
        {
            matchingItems.Clear();
            selectedItemIndex = 0;
            displayStartIndex = 0;

            if (currentInput.Length > 0)
            {
                string searchTerm = currentInput.ToLower();
                for (int i = 1; i < ItemID.Count; i++)
                {
                    string itemName = Lang.GetItemNameValue(i).ToLower();
                    if (itemName.Contains(searchTerm))
                    {
                        matchingItems.Add(i);
                    }
                }
            }
            ShowCurrentSearch();
        }

        private void ShowCurrentSearch()
        {
            ClearChat();
            Main.NewText($"Search: {currentInput}", Color.Yellow);
            if (matchingItems.Count > 0)
            {
                ShowCurrentSuggestion();
            }
            else if (currentInput.Length > 0)
            {
                Main.NewText("No matching items found.", Color.Red);
            }
        }

        private void ShowCurrentQuantity()
        {
            if (matchingItems.Count > 0 && isQuantityInput)
            {
                ClearChat();
                string itemName = Lang.GetItemNameValue(matchingItems[selectedItemIndex]);
                int quantity = quantityPresets[selectedQuantityIndex];
                Main.NewText($"Selected: {itemName}", Color.LightBlue);
                Main.NewText($"Quantity: {quantity}", Color.Yellow);
                Main.NewText($"Item {selectedItemIndex + 1} of {matchingItems.Count}", Color.Gray);
            }
        }

        private void ShowCurrentSuggestion()
        {
            if (matchingItems.Count > 0)
            {
                Main.NewText($"Item {selectedItemIndex + 1} of {matchingItems.Count}", Color.Gray);
                StringBuilder sb = new StringBuilder();
                sb.Append("Matching items: ");
                string selectedItemName = Lang.GetItemNameValue(matchingItems[selectedItemIndex]);
                sb.Append("[c/FFFF00:" + selectedItemName + "]");
                int remainingItemCount = Math.Min(4, matchingItems.Count - 1);
                if (remainingItemCount > 0)
                {
                    sb.Append(", ");
                    List<int> itemsToShow = new List<int>();
                    for (int offset = 1; offset <= remainingItemCount; offset++)
                    {
                        int nextIndex = (selectedItemIndex + offset) % matchingItems.Count;
                        if (!itemsToShow.Contains(nextIndex) && nextIndex != selectedItemIndex)
                        {
                            itemsToShow.Add(nextIndex);
                        }
                    }
                    if (itemsToShow.Count < remainingItemCount)
                    {
                        for (int offset = 1; itemsToShow.Count < remainingItemCount; offset++)
                        {
                            int prevIndex = (selectedItemIndex - offset + matchingItems.Count) % matchingItems.Count;
                            if (!itemsToShow.Contains(prevIndex) && prevIndex != selectedItemIndex)
                            {
                                itemsToShow.Add(prevIndex);
                            }
                        }
                    }
                    for (int i = 0; i < itemsToShow.Count; i++)
                    {
                        string itemName = Lang.GetItemNameValue(matchingItems[itemsToShow[i]]);
                        sb.Append(itemName);
                        if (i < itemsToShow.Count - 1)
                            sb.Append(", ");
                    }
                }
                if (matchingItems.Count > 5)
                {
                    sb.Append($" and {matchingItems.Count - 5} more...");
                }
                Main.NewText(sb.ToString(), Color.White);
            }
        }

        private char? KeyToChar(Keys key, bool shift)
        {
            if (key >= Keys.A && key <= Keys.Z)
            {
                char c = (char)('a' + (key - Keys.A));
                return shift ? char.ToUpper(c) : c;
            }
            if (key >= Keys.D0 && key <= Keys.D9 && !shift)
                return (char)('0' + (key - Keys.D0));
            if (key == Keys.Space)
                return ' ';
            if (shift)
            {
                switch (key)
                {
                    case Keys.D1: return '!';
                    case Keys.D2: return '@';
                    case Keys.D3: return '#';
                    case Keys.D4: return '$';
                    case Keys.D5: return '%';
                    case Keys.D6: return '^';
                    case Keys.D7: return '&';
                    case Keys.D8: return '*';
                    case Keys.D9: return '(';
                    case Keys.D0: return ')';
                    case Keys.OemMinus: return '_';
                    case Keys.OemPlus: return '+';
                }
            }
            else
            {
                switch (key)
                {
                    case Keys.OemMinus: return '-';
                    case Keys.OemPlus: return '=';
                }
            }
            return null;
        }
        
        private void ClearChat()
        {
            for (int i = 0; i < 20; i++) 
            {
                Main.NewText(" ", Color.Transparent);
            }
        }
        
        private bool CheckPrivilegedPlayer()
        {
            return this.Player.name == ModContent.GetInstance<Config>().PrivilegedPlayerName;
        }        
        
        public bool IsInfiniteReachEnabled()
        {
            return infiniteReachEnabled;
        }
    }
}
