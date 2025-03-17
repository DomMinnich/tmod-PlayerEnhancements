using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace FirstTime1000
{
    public class Config : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [Label("Privileged Player Name")]
        [Tooltip("The player who will receive bonus effects")]
        [DefaultValue("Dominic")]
        public string PrivilegedPlayerName { get; set; } = "Dominic";
    }
}