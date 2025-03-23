using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;

namespace Stuff
{
    public enum MessageType : byte
    {
        SpawnItem
    }

    public class Mod : Mod
    {
        public static Mod Instance;

        public override void Load()
        {
            Instance = this;
        }

        public override void Unload()
        {
            Instance = null;
        }

        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            MessageType msgType = (MessageType)reader.ReadByte();
            switch (msgType)
            {
                case MessageType.SpawnItem:
                    int itemID = reader.ReadInt32();
                    int quantity = reader.ReadInt32();
                    int posX = reader.ReadInt32();
                    int posY = reader.ReadInt32();
                    // Get the player who sent the packet (ensures proper item placement dimensions)
                    Player player = Main.player[whoAmI];
                    for (int i = 0; i < quantity; i++)
                    {
                        Item.NewItem(new EntitySource_Misc("Stuff:SpawnItem"), posX, posY, player.width, player.height, itemID);
                    }
                    break;
                default:
                    Logger.WarnFormat("Mod: Unknown message type: {0}", msgType);
                    break;
            }
        }
    }
}
