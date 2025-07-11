﻿using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ElectricFence;

/// <summary>
///     electric floor panel
/// </summary>
public class Building_floor_panel : Building_Trap
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
        return fenceCore.CoreGetDamage(FencePowerComp);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref touchingPawns, "Building_floor_panel", LookMode.Reference);
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
        SpringSub(p);
    }

    private bool springChanceFence(Pawn p)
    {
        return !knowsOfTrapFence(p);
    }

    protected override void SpringSub(Pawn p)
    {
        if (p != null)
        {
            damagePawnFence(p);
        }
    }

    private void damagePawnFence(Pawn p)
    {
        // batteries
        fenceCore.CoreDrainPower(FencePowerComp, 150f);

        var num = Mathf.RoundToInt(getFenceDamage() / (float)4);

        var height = Rand.Value >= 0.666 ? BodyPartHeight.Middle : BodyPartHeight.Top;
        var dinfo = new DamageInfo(DamageDefOf.Stun, num, -1f, -1f, this);

        dinfo.SetBodyRegion(height, BodyPartDepth.Outside);
        p.TakeDamage(dinfo);

        var sparks = new Effecter(DefDatabase<EffecterDef>.GetNamed("ConstructMetal"));
        // If we have a spark effecter

        sparks.EffectTick(p, this);
        sparks.Cleanup();
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