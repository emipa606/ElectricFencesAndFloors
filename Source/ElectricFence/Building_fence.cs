﻿using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.Sound;

namespace ElectricFence;

/// <summary>
///     building type electric fence wall
/// </summary>
public class Building_fence : Building_Trap
{
    //
    // Fields
    //
    private List<Pawn> touchingPawns = [];

    public CompPower FencePowerComp => GetComp<CompPower>();

    //
    // Methods
    //
    private void CheckSpring(Pawn p)
    {
        if (p == null || !SpringChanceFence(p) || GetFenceDamage() <= 0)
        {
            return;
        }

        SpringFence(p);
        if (p.Faction == Faction.OfPlayer || p.HostFaction == Faction.OfPlayer)
        {
            Find.LetterStack.ReceiveLetter("LetterFriendlyTrapSprungLabel".Translate(p.NameShortColored),
                "LetterFriendlyTrapSprung".Translate(p.NameShortColored), LetterDefOf.NegativeEvent,
                new TargetInfo(Position, Map));
        }
    }

    private int GetFenceDamage()
    {
        return fenceCore.CoreGetDamage(FencePowerComp);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref touchingPawns, "Building_fence", LookMode.Reference);
    }

    private bool KnowsOfTrapFence(Pawn p)
    {
        return fenceCore.CoreKnowsOfTrap(p, Faction);
    }

    private void SpringFence(Pawn p)
    {
        SoundDef.Named("EnergyShieldBroken").PlayOneShot(new TargetInfo(Position, Map));
        //if (p != null && p.Faction != null) {                
        //	p.Faction.TacticalMemory.TrapRevealed (base.Position, base.Map);
        //}
        SpringSub(p);
    }

    private bool SpringChanceFence(Pawn p)
    {
        return !KnowsOfTrapFence(p);
    }

    protected override void SpringSub(Pawn p)
    {
        if (p != null)
        {
            DamagePawnFence(p);
        }
    }

    private void DamagePawnFence(Pawn p)
    {
        var num = GetFenceDamage();
        fenceCore.CoreAssignPawnDamage(p, num, this, FencePowerComp, 1000f);
    }

    public override void Tick()
    {
        var thingList = Position.GetThingList(Map);
        foreach (var thing in thingList)
        {
            if (thing is not Pawn pawn || touchingPawns.Contains(pawn))
            {
                continue;
            }

            touchingPawns.Add(pawn);
            CheckSpring(pawn);
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