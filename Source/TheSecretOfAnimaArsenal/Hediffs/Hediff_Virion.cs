using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;
using Verse.Sound;
using LudeonTK;

namespace tsoa.arsenal;

public class Hediff_Virion : HediffWithComps
{
    private int TicksToSpawn => (int)(Extension.gestationDays * GenDate.TicksPerDay);
    private int ticksRemaining = -1;

    internal ThingDef virionDef;

    internal VirionExtension extension;
    internal VirionExtension Extension
    {
        get
        {
            if (extension == null)
            {
                extension = virionDef.GetModExtension<VirionExtension>();
            }
            return extension;
        }
    }

    internal List<PawnKindDef> spawnedEntities;

    private bool fullyGestated = false; // don't need to save, no ticks between when it is set and when it is read

    public override void PostAdd(DamageInfo? dinfo)
    {
        base.PostAdd(dinfo);
        ticksRemaining = TicksToSpawn;
    }

    public override void TickInterval(int delta)
    {
        base.TickInterval(delta);

        if (ticksRemaining > 0)
        {
            ticksRemaining = Math.Max(0, ticksRemaining - delta);
        }

        Severity = 1f - (float)ticksRemaining / TicksToSpawn;

        if (ticksRemaining <= 0 && pawn.SpawnedOrAnyParentSpawned)
        {
            fullyGestated = true;
            HealthUtility.DamageUntilDead(pawn); // maybe destroy the torso instead?
        }
    }

    public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
    {
        if (!pawn.SpawnedOrAnyParentSpawned)
        {
            Messages.Message("TSOA_VirionNotSpawned".Translate(pawn.Name), MessageTypeDefOf.NegativeHealthEvent);
            return;
        }

        Map map = pawn.MapHeld;
        IntVec3 pos = pawn.PositionHeld;

        DoEmergingEffects(map, pos);
        if (fullyGestated)
        {
            SpawnItem(map, pos);
            Messages.Message("TSOA_VirionComplete".Translate(pawn.Name), new LookTargets(pos, map), MessageTypeDefOf.PositiveEvent);
        }
        else
        {
            SpawnEntity(map, pos);
            Messages.Message("TSOA_VirionIncomplete".Translate(pawn.Name), new LookTargets(pos, map), MessageTypeDefOf.NegativeHealthEvent);
        }
        pawn.health.RemoveHediff(this);
    }

    public override bool TendableNow(bool ignoreTimer = false)
    {
        return base.TendableNow(ignoreTimer);
        // TODO virion becomes restless at random intervals, requiring tending
    }

    public override void Tended(float quality, float maxQuality, int batchPosition = 0)
    {
        base.Tended(quality, maxQuality, batchPosition);
        // TODO
    }

    private void DoEmergingEffects(Map map, IntVec3 pos)
    {
        EffecterDefOf.MetalhorrorEmerging.Spawn(pos, map).Cleanup();
        CellRect cellRect = new CellRect(pos.x, pos.z, 3, 3);
        for (int i = 0; i < 5; i++)
        {
            IntVec3 randomCell = cellRect.RandomCell;
            if (randomCell.InBounds(map) && GenSight.LineOfSight(randomCell, pos, map))
            {
                FilthMaker.TryMakeFilth(randomCell, map, (i % 2 == 0) ? ThingDefOf.Filth_Blood : ThingDefOf.Filth_GrayFlesh);
            }
        }
        SoundDefOf.Crunch.PlayOneShot(new TargetInfo(pos, map));
    }

    private void SpawnItem(Map map, IntVec3 pos)
    {
        Thing thing = ThingMaker.MakeThing(Extension.producedItem);
        SetThingQuality(thing);
        thing.stackCount = Extension.producedCount;
        GenPlace.TryPlaceThing(thing, pos, map, ThingPlaceMode.Near);
    }

    private void SpawnEntity(Map map, IntVec3 pos)
    {
        Pawn entity;
        PawnKindDef kindDef;

        if (Extension.spawnedEntities.NullOrEmpty())
        {
            entity = PawnGenerator.GeneratePawn(PawnKindDefOf.Metalhorror, Faction.OfEntities);
            entity.ageTracker.LockCurrentLifeStageIndex(CurStageIndex); // I think this should make it so that stage 1 spawns a larva, 2 a juvenile, and 3 an adult
            pawn.ageTracker.AgeBiologicalTicks = this.ageTicks;
            pawn.ageTracker.AgeChronologicalTicks = this.ageTicks;

        }
        else if (Extension.spawnedEntities.Count < CurStageIndex + 1)
        {
            entity = PawnGenerator.GeneratePawn(Extension.spawnedEntities.Last(), Faction.OfEntities);
        }
        else
        {
            entity = PawnGenerator.GeneratePawn(Extension.spawnedEntities[CurStageIndex], Faction.OfEntities);
        }

        GenSpawn.Spawn(entity, pos, map);
    }

    private void SetThingQuality(Thing thing)
    {
        //TODO
        CompQuality comp = thing.TryGetComp<CompQuality>();
        if (comp == null)
            return;

        QualityCategory qual = QualityCategory.Awful;
        qual++;
        comp.SetQuality(qual, null);
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (Gizmo gizmo in base.GetGizmos())
        {
            yield return gizmo;
        }
        if (DebugSettings.godMode)
        {
            yield return new Command_Action
            {
                defaultLabel = "Dev: Progress virion 1 day",
                action = () =>
                {
                    ticksRemaining -= GenDate.TicksPerDay;
                }
            };
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Defs.Look(ref virionDef, "virionDef");
        Scribe_Values.Look(ref ticksRemaining, "ticksRemaining");
    }

}
