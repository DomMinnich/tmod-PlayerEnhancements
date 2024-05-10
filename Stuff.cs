/*
50 % damage reduction, 
50% damage reduction from projectiles,
3x life regen, 
20% life steal,

*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;

namespace Stuff
{
	// Player class is used to store player data for the mod
    public class Stuff : ModPlayer
	{

	// modify damage taken from npcs (50% damage reduction)
 public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
{
    if (this.Player.name == "test2")
    {
        modifiers.FinalDamage *= 0.5f; // 50% damage reduction
    }
}

// modify damage taken from projectiles (50% damage reduction)
public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
{
	if (this.Player.name == "test2")
	{
		modifiers.FinalDamage *= 0.5f; // 50% damage reduction
	}

	}

// modfily natural life regen (3x life regen)
public override void NaturalLifeRegen(ref float regen)
{
	if (this.Player.name == "test2")
	{
		regen *= 3f; // 3x life regen
	}
}

// add life steal on hit (20% life steal)
public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
{
    if (this.Player.name == "test2")
    { 
        int lifeStealAmount = damageDone / 5 ; // 25% life steal
        this.Player.statLife = Math.Min(this.Player.statLife + lifeStealAmount, this.Player.statLifeMax2);
    }
}
	

	}
	}

