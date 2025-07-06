using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace ElectricFence;

/// <summary>
///     building type electric fence wall
/// </summary>
public static class fenceCore
{
    //
    // Methods
    //

    public static bool CoreKnowsOfTrap(Pawn p, Faction f)
    {
        var lord = p.GetLord();
        if (p != null && p.def.race.Animal && p.Faction == Faction.OfPlayer && !p.InAggroMentalState &&
            !p.HostileTo(f))
        {
            // tamed animals
            return true;
        }

        if (p != null && p.def.race.Animal && lord != null && !p.HostileTo(f))
        {
            // caravan animals
            return true;
        }

        if (p != null && p.def.race.Animal)
        {
            return false;
        }

        if (p?.guest != null && lord is
                { LordJob: LordJob_FormAndSendCaravan or LordJob_AssistColony or LordJob_VisitColony })
        {
            // caravans, friendlies, visitors
            return true;
        }

        if (p?.guest != null && !p.HostileTo(f))
        {
            // released prisoners
            return true;
        }

        if (p?.guest is { IsPrisoner: true, PrisonerIsSecure: false, Released: false } &&
            p.HostileTo(f))
        {
            // escaping prisoners
            return false;
        }

        if (p?.Faction != null && !p.Faction.HostileTo(f))
        {
            // non hostiles
            return true;
        }

        return p?.Faction is { IsPlayer: true } && p.RaceProps.Humanlike;
        // player humanlike pawns
    }

    public static int CoreGetDamage(CompPower fencePowerComp)
    {
        var powerGain = fencePowerComp.PowerNet.CurrentEnergyGainRate();
        var powerStore = fencePowerComp.PowerNet.CurrentStoredEnergy();
        var multiplier = 1;
        if (powerStore > 1000)
        {
            multiplier = Mathf.RoundToInt(powerStore / 1000);
        }

        var powerTotal = powerGain * 60 * multiplier;
        var calcPower = Mathf.RoundToInt(powerTotal);

        // batteries
        var batteryDamage = 0;
        foreach (var compPowerBattery in fencePowerComp.PowerNet.batteryComps)
        {
            var storedPower = compPowerBattery.StoredEnergy;
            if (storedPower > 10000)
            {
                storedPower = 10000f;
            }

            batteryDamage += Mathf.RoundToInt(storedPower / 100);
        }

        calcPower += batteryDamage;
        return calcPower;
    }

    public static int CoreGetPlasmaDamage(CompPower fencePowerComp)
    {
        var powerGain = fencePowerComp.PowerNet.CurrentEnergyGainRate();
        var powerStore = fencePowerComp.PowerNet.CurrentStoredEnergy();
        var multiplier = 1;
        if (powerStore > 2000)
        {
            multiplier = Mathf.RoundToInt(powerStore / 2000);
        }

        var powerTotal = powerGain * 60 * 5 * multiplier;
        if (powerTotal > 100)
        {
            powerTotal = 100f;
        }

        var calcPower = Mathf.RoundToInt(powerTotal);

        // batteries
        var batteryDamage = 0;
        foreach (var compPowerBattery in fencePowerComp.PowerNet.batteryComps)
        {
            var storedPower = compPowerBattery.StoredEnergy;
            if (storedPower > 5000)
            {
                storedPower = 5000f;
            }

            batteryDamage += Mathf.RoundToInt(storedPower / 100);
        }

        calcPower += batteryDamage;
        return calcPower;
    }

    public static void CoreDrainPower(CompPower fencePowerComp, float drainPowerMax)
    {
        foreach (var compPowerBattery in fencePowerComp.PowerNet.batteryComps)
        {
            var storedPower = compPowerBattery.StoredEnergy;
            var drainPower = compPowerBattery.Props.storedEnergyMax / 10;
            if (drainPower > drainPowerMax / 10)
            {
                drainPower = drainPowerMax / 10;
            }

            compPowerBattery.DrawPower(storedPower > drainPower ? drainPower : storedPower);
        }
    }

    public static void CoreAssignPawnDamage(Pawn p, int damage, Thing source, CompPower fencePowerComp,
        float drainPower)
    {
        // batteries
        CoreDrainPower(fencePowerComp, drainPower);

        int randomInRange;
        switch (damage)
        {
            case < 50:
                randomInRange = 1;
                break;
            case < 100:
                randomInRange = 2;
                break;
            case < 200:
                randomInRange = 3;
                break;
            default:
                randomInRange = 5;
                break;
        }

        var height = Rand.Value >= 0.666 ? BodyPartHeight.Middle : BodyPartHeight.Top;

        for (var i = 0; i < randomInRange; i++)
        {
            if (damage <= 0)
            {
                break;
            }

            var num2 = Mathf.Max(1, Mathf.RoundToInt(Rand.Value * damage));
            damage -= num2;
            var damageInfo = new DamageInfo(DamageDefOf.Burn, num2, -1, -1f, source);
            if (randomInRange > 2)
            {
                damageInfo = new DamageInfo(DamageDefOf.Flame, num2, -1, -1f, source);
            }

            damageInfo.SetBodyRegion(height, BodyPartDepth.Outside);
            p.TakeDamage(damageInfo);

            var sparks = new Effecter(DefDatabase<EffecterDef>.GetNamed("ConstructMetal"));
            // If we have a spark effecter

            for (var y = 0; y < randomInRange; y++)
            {
                sparks.EffectTick(p, source);
            }

            sparks.Cleanup();
        }
    }
}