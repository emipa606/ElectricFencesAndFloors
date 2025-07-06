using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.Sound;

namespace ElectricFence;

/// <summary>
///     building type plasma fence gate
/// </summary>
public class Building_p_fence_door : Building_Door
{
    //
    // Fields
    //
    private List<Pawn> touchingPawns = [];

    private CompPower FencePowerComp => GetComp<CompPower>();

    //
    // Methods
    //

    private void checkSpring(Pawn p)
    {
        if (p == null || !springChanceFence(p) || getFenceDamage() <= 0)
        {
            return;
        }

        springFence(p);
        if (p.Faction == Faction.OfPlayer || p.HostFaction == Faction.OfPlayer)
        {
            Find.LetterStack.ReceiveLetter("LetterFriendlyTrapSprungLabel".Translate(p.NameShortColored),
                "LetterFriendlyTrapSprung".Translate(p.NameShortColored), LetterDefOf.NegativeEvent,
                new TargetInfo(Position, Map));
        }
    }

    private int getFenceDamage()
    {
        return fenceCore.CoreGetPlasmaDamage(FencePowerComp);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref touchingPawns, "Building_p_fence_door", LookMode.Reference);
    }

    private bool knowsOfTrapFence(Pawn p)
    {
        return fenceCore.CoreKnowsOfTrap(p, Faction);
    }

    private void springFence(Pawn p)
    {
        SoundDef.Named("EnergyShieldBroken").PlayOneShot(new TargetInfo(Position, Map));
        //if (p != null && p.Faction != null) {
        //	p.Faction.TacticalMemory.TrapRevealed (base.Position, base.Map);
        //}
        if (p != null)
        {
            damagePawnFence(p);
        }
    }

    private bool springChanceFence(Pawn p)
    {
        return !knowsOfTrapFence(p);
    }

    private void damagePawnFence(Pawn p)
    {
        var num = getFenceDamage();
        fenceCore.CoreAssignPawnDamage(p, num, this, FencePowerComp, 500f);
    }

    public override bool PawnCanOpen(Pawn p)
    {
        return true;
    }

    protected override void Tick()
    {
        var thingList = Position.GetThingList(Map);
        foreach (var thing in thingList)
        {
            if (thing is not Pawn pawn || touchingPawns.Contains(pawn))
            {
                continue;
            }

            touchingPawns.Add(pawn);
            checkSpring(pawn);
        }

        for (var j = 0; j < touchingPawns.Count; j++)
        {
            var pawn2 = touchingPawns[j];
            if (!pawn2.Spawned || pawn2.Position != Position)
            {
                touchingPawns.Remove(pawn2);
            }
        }

        base.Tick();
    }
}