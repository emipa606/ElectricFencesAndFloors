using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace ElectricFence
{
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

            if (p?.guest != null && lord != null && (lord.LordJob is LordJob_FormAndSendCaravan ||
                                                     lord.LordJob is LordJob_AssistColony ||
                                                     lord.LordJob is LordJob_VisitColony))
            {
                // caravans, friendlies, visitors
                return true;
            }

            if (p?.guest != null && !p.HostileTo(f))
            {
                // released prisoners
                return true;
            }

            if (p?.guest != null && p.guest.IsPrisoner && !p.guest.PrisonerIsSecure && !p.guest.Released &&
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

            if (p?.Faction != null && p.Faction.IsPlayer && p.RaceProps.Humanlike)
            {
                // player humanlike pawns
                return true;
            }

            return false;
        }

        public static int CoreGetDamage(CompPower FencePowerComp)
        {
            var powerGain = FencePowerComp.PowerNet.CurrentEnergyGainRate();
            var powerStore = FencePowerComp.PowerNet.CurrentStoredEnergy();
            var multiplier = 1;
            if (powerStore > 1000)
            {
                multiplier = Mathf.RoundToInt(powerStore / 1000);
            }

            var powerTotal = powerGain * 60 * multiplier;
            var calcPower = Mathf.RoundToInt(powerTotal);

            // batteries
            var batteryDamage = 0;
            foreach (var compPowerBattery in FencePowerComp.PowerNet.batteryComps)
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

        public static int CoreGetPlasmaDamage(CompPower FencePowerComp)
        {
            var powerGain = FencePowerComp.PowerNet.CurrentEnergyGainRate();
            var powerStore = FencePowerComp.PowerNet.CurrentStoredEnergy();
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
            foreach (var compPowerBattery in FencePowerComp.PowerNet.batteryComps)
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

        public static void CoreDrainPower(CompPower FencePowerComp, float drainPowerMax)
        {
            foreach (var compPowerBattery in FencePowerComp.PowerNet.batteryComps)
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

        public static void CoreAssignPawnDamage(Pawn p, int damage, Thing source, CompPower FencePowerComp,
            float drainPower)
        {
            // batteries
            CoreDrainPower(FencePowerComp, drainPower);

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
                var dinfo = new DamageInfo(DamageDefOf.Burn, num2, -1, -1f, source);
                if (randomInRange > 2)
                {
                    dinfo = new DamageInfo(DamageDefOf.Flame, num2, -1, -1f, source);
                }

                dinfo.SetBodyRegion(height, BodyPartDepth.Outside);
                p.TakeDamage(dinfo);

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
}