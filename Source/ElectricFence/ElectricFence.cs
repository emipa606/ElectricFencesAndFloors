using RimWorld;
using System;
using Verse;
using System.Collections.Generic;
using System.Diagnostics;
using Verse.AI.Group;
using Verse.Sound;
using UnityEngine;
using Verse.AI;


namespace ElectricFence
{
	/// <summary>
	///  building type electric fence wall
	/// </summary>

	public static class fenceCore
	{

		//
		// Methods
		//

		public static bool CoreKnowsOfTrap (Pawn p, Faction f)
		{
			Lord lord = p.GetLord ();
			if (p != null && p.def.race.Animal && p.Faction == Faction.OfPlayer && !p.InAggroMentalState && !p.HostileTo(f)) { // tamed animals
				return true;
			} else if (p != null && p.def.race.Animal && lord != null && !p.HostileTo(f)) { // caravan animals
				return true;
			} else if (p != null && p.def.race.Animal) {
				return false;
			}
			if (p.guest != null && lord != null && (lord.LordJob is LordJob_FormAndSendCaravan || lord.LordJob is LordJob_AssistColony || lord.LordJob is LordJob_VisitColony)) { // caravans, friendlies, visitors
				return true;
			}
			if (p.guest != null && !p.HostileTo(f)) { // released prisoners
				return true;
			}
			if (p.guest != null && p.guest.IsPrisoner && !p.guest.PrisonerIsSecure && !p.guest.Released && p.HostileTo(f)) { // escaping prisoners
				return false;
			}
			if (p.Faction != null && !p.Faction.HostileTo (f)) { // non hostiles
				return true;
			}
			if (p.Faction.IsPlayer && p.RaceProps.Humanlike) { // player humanlike pawns
				return true;
			}

			return false;
		}

		public static int CoreGetDamage (CompPower FencePowerComp)
		{
			float powerGain = FencePowerComp.PowerNet.CurrentEnergyGainRate ();
			float powerStore = FencePowerComp.PowerNet.CurrentStoredEnergy ();
			int multiplier = 1;
			if (powerStore > 1000) {
				multiplier = Mathf.RoundToInt ((powerStore/1000));
			}
			float powerTotal = powerGain * 60 * multiplier;
			int calcPower = Mathf.RoundToInt (powerTotal);

			// batteries
			int batteryDamage = 0;
			for (int i = 0; i < FencePowerComp.PowerNet.batteryComps.Count; i++) {
				float storedPower = FencePowerComp.PowerNet.batteryComps [i].StoredEnergy;
				if (storedPower > 10000) {
					storedPower = 10000f;
				}
				batteryDamage += Mathf.RoundToInt (storedPower/100);
			}
			calcPower += batteryDamage;
			return calcPower;
		}

		public static int CoreGetPlasmaDamage (CompPower FencePowerComp)
		{
			float powerGain = FencePowerComp.PowerNet.CurrentEnergyGainRate ();
			float powerStore = FencePowerComp.PowerNet.CurrentStoredEnergy ();
			int multiplier = 1;
			if (powerStore > 2000) {
				multiplier = Mathf.RoundToInt ((powerStore/2000));
			}
			float powerTotal = powerGain * 60 * 5 * multiplier;
			if (powerTotal > 100) {
				powerTotal = 100f;
			}
			int calcPower = Mathf.RoundToInt (powerTotal);

			// batteries
			int batteryDamage = 0;
			for (int i = 0; i < FencePowerComp.PowerNet.batteryComps.Count; i++) {
				float storedPower = FencePowerComp.PowerNet.batteryComps [i].StoredEnergy;
				if (storedPower > 5000) {
					storedPower = 5000f;
				}
				batteryDamage += Mathf.RoundToInt (storedPower/100);
			}
			calcPower += batteryDamage;
			return calcPower;
		}

		public static void CoreDrainPower (CompPower FencePowerComp, float drainPowerMax) {
			for (int i = 0; i < FencePowerComp.PowerNet.batteryComps.Count; i++) {
				float storedPower = FencePowerComp.PowerNet.batteryComps [i].StoredEnergy;
				float drainPower = (FencePowerComp.PowerNet.batteryComps [i].Props.storedEnergyMax / 10);
				if (drainPower > (drainPowerMax/10)) {
					drainPower = (drainPowerMax/10);
				}
				if (storedPower > drainPower) {
					FencePowerComp.PowerNet.batteryComps [i].DrawPower (drainPower);
				} else {
					FencePowerComp.PowerNet.batteryComps [i].DrawPower (storedPower);
				}
			}
		}

		public static void CoreAssignPawnDamage (Pawn p, int damage, Thing source, CompPower FencePowerComp, float drainPower) {
			// batteries
			fenceCore.CoreDrainPower(FencePowerComp,drainPower);

			int randomInRange = 1;
			if (damage < 50) {
				randomInRange = 1;
			} else if (damage < 100) {
				randomInRange = 2;
			} else if (damage < 200) {
				randomInRange = 3;
			} else {
				randomInRange = 5;
			}

			BodyPartHeight height = (Rand.Value >= 0.666) ? BodyPartHeight.Middle : BodyPartHeight.Top;

			for (int i = 0; i < randomInRange; i++) {
				if (damage <= 0) {
					break;
				}
				int num2 = Mathf.Max (1, Mathf.RoundToInt (Rand.Value * (float)damage));
				damage -= num2;
				DamageInfo dinfo = new DamageInfo (DamageDefOf.Burn, num2, -1, -1f, source, null, null, DamageInfo.SourceCategory.ThingOrUnknown);
				if (randomInRange > 2) {
					dinfo = new DamageInfo (DamageDefOf.Flame, num2, -1, -1f, source, null, null, DamageInfo.SourceCategory.ThingOrUnknown);
				}
				dinfo.SetBodyRegion (height, BodyPartDepth.Outside);
				p.TakeDamage (dinfo);

				Effecter sparks = null;
				sparks = new Effecter(DefDatabase<EffecterDef>.GetNamed("ConstructMetal"));
				// If we have a spark effecter
				if (sparks != null) {
					for (int y = 0; y < randomInRange; y++) {
						sparks.EffectTick(p, source);
					}
					sparks.Cleanup();
					sparks = null;
				}

			}
		}

	}

	/// <summary>
	///  building type electric fence wall
	/// </summary>

	public class Building_fence : Building_Trap
	{

		//
		// Fields
		//
		private List<Pawn> touchingPawns = new List<Pawn> ();

		public CompPower FencePowerComp {
			get {
				return base.GetComp<CompPower> ();
			}
		}

		//
		// Methods
		//
		private void CheckSpring (Pawn p)
		{
			if (p != null && this.SpringChanceFence (p) && GetFenceDamage () > 0) {
				this.SpringFence (p);
				if (p.Faction == Faction.OfPlayer || p.HostFaction == Faction.OfPlayer) {
					Find.LetterStack.ReceiveLetter ("LetterFriendlyTrapSprungLabel".Translate (new object[] {
						p.NameShortColored
					}), "LetterFriendlyTrapSprung".Translate (new object[] {
						p.NameShortColored
					}), LetterDefOf.NegativeEvent, new TargetInfo (base.Position, base.Map, false), null);
				}
			}
		}

		private int GetFenceDamage ()
		{
			return fenceCore.CoreGetDamage (FencePowerComp);
		}

		public override void ExposeData ()
		{
			base.ExposeData ();
			Scribe_Collections.Look<Pawn> (ref this.touchingPawns, "Building_fence", LookMode.Reference, new object[0]);
		}

		private bool KnowsOfTrapFence (Pawn p)
		{
			return fenceCore.CoreKnowsOfTrap (p, base.Faction);
		}

		private void SpringFence (Pawn p)
		{
			SoundDef.Named ("EnergyShieldBroken").PlayOneShot (new TargetInfo (base.Position, base.Map, false));
			//if (p != null && p.Faction != null) {                
			//	p.Faction.TacticalMemory.TrapRevealed (base.Position, base.Map);
			//}
			this.SpringSub (p);
		}

		private bool SpringChanceFence (Pawn p)
		{
			return !this.KnowsOfTrapFence (p);
		}

		protected override void SpringSub (Pawn p)
		{
			if (p != null) {
				this.DamagePawnFence (p);
			}
		}

		private void DamagePawnFence (Pawn p)
		{
			int num = GetFenceDamage ();
			fenceCore.CoreAssignPawnDamage (p, num, this, FencePowerComp, 1000f);
		}
			
		public override void Tick ()
		{
			List<Thing> thingList = base.Position.GetThingList (base.Map);
			for (int i = 0; i < thingList.Count; i++) {
				Pawn pawn = thingList [i] as Pawn;
				if (pawn != null && !this.touchingPawns.Contains (pawn)) {
					this.touchingPawns.Add (pawn);
					this.CheckSpring (pawn);
				}
			}
			for (int j = 0; j < this.touchingPawns.Count; j++) {
				Pawn pawn2 = this.touchingPawns [j];
				if (!pawn2.Spawned || pawn2.Position != base.Position) {
					this.touchingPawns.Remove (pawn2);
				}
			}
			base.Tick ();
		}
	
	}

	/// <summary>
	///  building type electric fence gate
	/// </summary>

	public class Building_fence_door : Building_Door
	{

		//
		// Fields
		//
		private List<Pawn> touchingPawns = new List<Pawn> ();

		public CompPower FencePowerComp {
			get {
				return base.GetComp<CompPower> ();
			}
		}

		//
		// Methods
		//

		private void CheckSpring (Pawn p)
		{
			if (p != null && this.SpringChanceFence (p) && GetFenceDamage () > 0) {
				this.SpringFence (p);
				if (p.Faction == Faction.OfPlayer || p.HostFaction == Faction.OfPlayer) {
					Find.LetterStack.ReceiveLetter ("LetterFriendlyTrapSprungLabel".Translate (new object[] {
						p.NameShortColored
					}), "LetterFriendlyTrapSprung".Translate (new object[] {
						p.NameShortColored
					}), LetterDefOf.NegativeEvent, new TargetInfo (base.Position, base.Map, false), null);
				}
			}
		}

		private int GetFenceDamage ()
		{
			return fenceCore.CoreGetDamage (FencePowerComp);
		}

		public override void ExposeData ()
		{
			base.ExposeData ();
			Scribe_Collections.Look<Pawn> (ref this.touchingPawns, "Building_fence_door", LookMode.Reference, new object[0]);
		}

		private bool KnowsOfTrapFence (Pawn p)
		{
			return fenceCore.CoreKnowsOfTrap (p, base.Faction);
		}

		private void SpringFence (Pawn p)
		{
			SoundDef.Named ("EnergyShieldBroken").PlayOneShot (new TargetInfo (base.Position, base.Map, false));
			//if (p != null && p.Faction != null) {
			//	p.Faction.TacticalMemory.TrapRevealed (base.Position, base.Map);
			//}
			if (p != null) {
				this.DamagePawnFence (p);
			}
		}

		private bool SpringChanceFence (Pawn p)
		{
			return !this.KnowsOfTrapFence (p);
		}

		private void DamagePawnFence (Pawn p)
		{
			int num = GetFenceDamage ();
			fenceCore.CoreAssignPawnDamage (p, num, this, FencePowerComp, 1000f);
		}

		public override bool PawnCanOpen (Pawn p)
		{
			return true;
		}

		public override void Tick ()
		{
			List<Thing> thingList = base.Position.GetThingList (base.Map);
			for (int i = 0; i < thingList.Count; i++) {
				Pawn pawn = thingList [i] as Pawn;
				if (pawn != null && !this.touchingPawns.Contains (pawn)) {
					this.touchingPawns.Add (pawn);
					this.CheckSpring (pawn);
				}
			}
			for (int j = 0; j < this.touchingPawns.Count; j++) {
				Pawn pawn2 = this.touchingPawns [j];
				if (!pawn2.Spawned || pawn2.Position != base.Position) {
					this.touchingPawns.Remove (pawn2);
				}
			}
			base.Tick ();
		}

	}

	/// <summary>
	///  building type plasma fence wall
	/// </summary>

	public class Building_p_fence : Building_Trap
	{

		//
		// Fields
		//
		private List<Pawn> touchingPawns = new List<Pawn> ();

		public CompPower FencePowerComp {
			get {
				return base.GetComp<CompPower> ();
			}
		}

		//
		// Methods
		//
		private void CheckSpring (Pawn p)
		{
			if (p != null && this.SpringChanceFence (p) && GetFenceDamage () > 0) {
				this.SpringFence (p);
				if (p.Faction == Faction.OfPlayer || p.HostFaction == Faction.OfPlayer) {
					Find.LetterStack.ReceiveLetter ("LetterFriendlyTrapSprungLabel".Translate (new object[] {
						p.NameShortColored
					}), "LetterFriendlyTrapSprung".Translate (new object[] {
						p.NameShortColored
					}), LetterDefOf.NegativeEvent, new TargetInfo (base.Position, base.Map, false), null);
				}
			}
		}

		private int GetFenceDamage ()
		{
			return fenceCore.CoreGetPlasmaDamage (FencePowerComp);
		}

		public override void ExposeData ()
		{
			base.ExposeData ();
			Scribe_Collections.Look<Pawn> (ref this.touchingPawns, "Building_p_fence", LookMode.Reference, new object[0]);
		}

		private bool KnowsOfTrapFence (Pawn p)
		{
			return fenceCore.CoreKnowsOfTrap (p, base.Faction);
		}

		private void SpringFence (Pawn p)
		{
			SoundDef.Named ("EnergyShieldBroken").PlayOneShot (new TargetInfo (base.Position, base.Map, false));
			//if (p != null && p.Faction != null) {
			//	p.Faction.TacticalMemory.TrapRevealed (base.Position, base.Map);
			//}
			this.SpringSub (p);
		}

		private bool SpringChanceFence (Pawn p)
		{
			return !this.KnowsOfTrapFence (p);
		}

		protected override void SpringSub (Pawn p)
		{
			if (p != null) {
				this.DamagePawnFence (p);
			}
		}

		private void DamagePawnFence (Pawn p)
		{
			int num = GetFenceDamage ();
			fenceCore.CoreAssignPawnDamage (p, num, this, FencePowerComp, 500f);
		}

		public override void Tick ()
		{
			List<Thing> thingList = base.Position.GetThingList (base.Map);
			for (int i = 0; i < thingList.Count; i++) {
				Pawn pawn = thingList [i] as Pawn;
				if (pawn != null && !this.touchingPawns.Contains (pawn)) {
					this.touchingPawns.Add (pawn);
					this.CheckSpring (pawn);
				}
			}
			for (int j = 0; j < this.touchingPawns.Count; j++) {
				Pawn pawn2 = this.touchingPawns [j];
				if (!pawn2.Spawned || pawn2.Position != base.Position) {
					this.touchingPawns.Remove (pawn2);
				}
			}
			base.Tick ();
		}

	}

	/// <summary>
	///  building type plasma fence gate
	/// </summary>

	public class Building_p_fence_door : Building_Door
	{

		//
		// Fields
		//
		private List<Pawn> touchingPawns = new List<Pawn> ();

		public CompPower FencePowerComp {
			get {
				return base.GetComp<CompPower> ();
			}
		}

		//
		// Methods
		//

		private void CheckSpring (Pawn p)
		{
			if (p != null && this.SpringChanceFence (p) && GetFenceDamage () > 0) {
				this.SpringFence (p);
				if (p.Faction == Faction.OfPlayer || p.HostFaction == Faction.OfPlayer) {
					Find.LetterStack.ReceiveLetter ("LetterFriendlyTrapSprungLabel".Translate (new object[] {
						p.NameShortColored
					}), "LetterFriendlyTrapSprung".Translate (new object[] {
						p.NameShortColored
					}), LetterDefOf.NegativeEvent, new TargetInfo (base.Position, base.Map, false), null);
				}
			}
		}

		private int GetFenceDamage ()
		{
			return fenceCore.CoreGetPlasmaDamage (FencePowerComp);
		}

		public override void ExposeData ()
		{
			base.ExposeData ();
			Scribe_Collections.Look<Pawn> (ref this.touchingPawns, "Building_p_fence_door", LookMode.Reference, new object[0]);
		}

		private bool KnowsOfTrapFence (Pawn p)
		{
			return fenceCore.CoreKnowsOfTrap (p, base.Faction);
		}

		private void SpringFence (Pawn p)
		{
			SoundDef.Named ("EnergyShieldBroken").PlayOneShot (new TargetInfo (base.Position, base.Map, false));
			//if (p != null && p.Faction != null) {
			//	p.Faction.TacticalMemory.TrapRevealed (base.Position, base.Map);
			//}
			if (p != null) {
				this.DamagePawnFence (p);
			}
		}

		private bool SpringChanceFence (Pawn p)
		{
			return !this.KnowsOfTrapFence (p);
		}

		private void DamagePawnFence (Pawn p)
		{
			int num = GetFenceDamage ();
			fenceCore.CoreAssignPawnDamage (p, num, this, FencePowerComp, 500f);
		}

		public override bool PawnCanOpen (Pawn p)
		{
			return true;
		}

		public override void Tick ()
		{
			List<Thing> thingList = base.Position.GetThingList (base.Map);
			for (int i = 0; i < thingList.Count; i++) {
				Pawn pawn = thingList [i] as Pawn;
				if (pawn != null && !this.touchingPawns.Contains (pawn)) {
					this.touchingPawns.Add (pawn);
					this.CheckSpring (pawn);
				}
			}
			for (int j = 0; j < this.touchingPawns.Count; j++) {
				Pawn pawn2 = this.touchingPawns [j];
				if (!pawn2.Spawned || pawn2.Position != base.Position) {
					this.touchingPawns.Remove (pawn2);
				}
			}
			base.Tick ();
		}

	}

	/// <summary>
	///  electric floor panel
	/// </summary>

	public class Building_floor_panel : Building_Trap
	{

		//
		// Fields
		//
		private List<Pawn> touchingPawns = new List<Pawn> ();

		public CompPower FencePowerComp {
			get {
				return base.GetComp<CompPower> ();
			}
		}

		//
		// Methods
		//
		private void CheckSpring (Pawn p)
		{
			if (p != null && this.SpringChanceFence (p) && GetFenceDamage () > 0) {
				this.SpringFence (p);
				if (p.Faction == Faction.OfPlayer || p.HostFaction == Faction.OfPlayer) {
					Find.LetterStack.ReceiveLetter ("LetterFriendlyTrapSprungLabel".Translate (new object[] {
						p.NameShortColored
					}), "LetterFriendlyTrapSprung".Translate (new object[] {
						p.NameShortColored
					}), LetterDefOf.NegativeEvent, new TargetInfo (base.Position, base.Map, false), null);
				}
			}
		}

		private int GetFenceDamage ()
		{
			return fenceCore.CoreGetDamage (FencePowerComp);
		}

		public override void ExposeData ()
		{
			base.ExposeData ();
			Scribe_Collections.Look<Pawn> (ref this.touchingPawns, "Building_floor_panel", LookMode.Reference, new object[0]);
		}

		private bool KnowsOfTrapFence (Pawn p)
		{
			return fenceCore.CoreKnowsOfTrap (p, base.Faction);
		}

		private void SpringFence (Pawn p)
		{
			SoundDef.Named ("EnergyShieldBroken").PlayOneShot (new TargetInfo (base.Position, base.Map, false));
			//if (p != null && p.Faction != null) {
			//	p.Faction.TacticalMemory.TrapRevealed (base.Position, base.Map);
			//}
			this.SpringSub (p);
		}

		private bool SpringChanceFence (Pawn p)
		{
			return !this.KnowsOfTrapFence (p);
		}

		protected override void SpringSub (Pawn p)
		{
			if (p != null) {
				this.DamagePawnFence (p);
			}
		}

		private void DamagePawnFence (Pawn p)
		{
			// batteries
			fenceCore.CoreDrainPower(FencePowerComp,150f);

			int num = Mathf.RoundToInt(GetFenceDamage () / 4);

			BodyPartHeight height = (Rand.Value >= 0.666) ? BodyPartHeight.Middle : BodyPartHeight.Top;
			DamageInfo dinfo = new DamageInfo (DamageDefOf.Stun, num, -1f, -1f, this, null, null, DamageInfo.SourceCategory.ThingOrUnknown);

			dinfo.SetBodyRegion (height, BodyPartDepth.Outside);
			p.TakeDamage (dinfo);

			Effecter sparks = null;
			sparks = new Effecter(DefDatabase<EffecterDef>.GetNamed("ConstructMetal"));
			// If we have a spark effecter
			if (sparks != null) {
				sparks.EffectTick(p, this);
				sparks.Cleanup();
				sparks = null;
			}
			
		}

		public override void Tick ()
		{
			List<Thing> thingList = base.Position.GetThingList (base.Map);
			for (int i = 0; i < thingList.Count; i++) {
				Pawn pawn = thingList [i] as Pawn;
				if (pawn != null && !this.touchingPawns.Contains (pawn)) {
					this.touchingPawns.Add (pawn);
					this.CheckSpring (pawn);
				}
			}
			for (int j = 0; j < this.touchingPawns.Count; j++) {
				Pawn pawn2 = this.touchingPawns [j];
				if (!pawn2.Spawned || pawn2.Position != base.Position) {
					this.touchingPawns.Remove (pawn2);
				}
			}
			base.Tick ();
		}

	}

}

