using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;

namespace Stuff
{
    public class InfiniteReach : GlobalTile
    {
        public override bool CanKillTile(int i, int j, int type, ref bool blockDamaged)
        {
            Player player = Main.LocalPlayer;
            // Only apply infinite reach to the privileged player
            if (player.name == ModContent.GetInstance<Config>().PrivilegedPlayerName)
            {
                // instance to check if infinite reach is enabled
                Stuff modPlayer = player.GetModPlayer<Stuff>();

                // Method to check if infinite reach is enabled (we'll need to expose this in Stuff)
                if (modPlayer.IsInfiniteReachEnabled())
                {
                    // Calculate distance to tile
                    float playerX = player.position.X + (float)(player.width / 2);
                    float playerY = player.position.Y + (float)(player.height / 2);
                    float tileX = i * 16 + 8;
                    float tileY = j * 16 + 8;
                    
                    // Allow tile destruction at any distance
                    return true;
                }
            }
            
            // Use default behavior otherwise
            return base.CanKillTile(i, j, type, ref blockDamaged);
        }

        public override bool CanPlace(int i, int j, int type)
        {
            Player player = Main.LocalPlayer;
            // Only apply infinite reach to the privileged player
            if (player.name == ModContent.GetInstance<Config>().PrivilegedPlayerName)
            {
                // instance to check if infinite reach is enabled
                Stuff modPlayer = player.GetModPlayer<Stuff>();

                // Method to check if infinite reach is enabled
                if (modPlayer.IsInfiniteReachEnabled())
                {
                    // Allow tile placement at any distance
                    return true;
                }
            }
            
            // Use default behavior otherwise
            return base.CanPlace(i, j, type);
        }

        public override void SetStaticDefaults()
        {
            // Register any necessary static defaults if needed
        }
    }
}