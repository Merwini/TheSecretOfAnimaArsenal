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
    /* Reminder
        Awful = 0
        Poor = 1
        Normal = 2
        Good = 3
        Excellent = 4
        Masterwork = 5
        Legendary = 6
     */

    private int curStageIndex = -1;
    private int ticksRemainingInStage = -1;
    public int TicksForNextStage
    {
        get
        {
            if (curStageIndex == -1) // initial gestation
                return (int)Extension.initialGestationDays * GenDate.TicksPerDay;

            if (curStageIndex == 6) // already at legendary, no more stages
                return int.MaxValue;

            return (int)Extension.qualityGestationDaysList[curStageIndex + 1] * GenDate.TicksPerDay;
        }
    }

    private int ticksUntilNextVirionActivity = -1;
    private int ticksUntilNextVirionDamage = -1;
    private bool virionActive = false;
    public float TendQualityRequirement => Extension.requiredTendQualityPerStage * curStageIndex;

    private bool extracting = false;

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

    public override string LabelInBrackets
    {
        get
        {
            // TODO current quality + Severity
            return null;
        }
    }

    public override void PostAdd(DamageInfo? dinfo)
    {
        base.PostAdd(dinfo);
        ticksRemainingInStage = (int)(Extension.initialGestationDays * GenDate.TicksPerDay);
        curStageIndex = -1;

        ticksUntilNextVirionActivity = ticksRemainingInStage + (Extension.virionAcvtivityDaysRange.RandomInRange() * GenDate.TicksPerDay); // Pre-load an activity with enough padding to also get past initial gestation
    }

    public override void TickInterval(int delta)
    {
        base.TickInterval(delta);

        if (ticksRemainingInStage > 0)
        {
            ticksRemainingInStage = Math.Max(0, ticksRemainingInStage - delta);
        }

        Severity = 1f - (float)ticksRemainingInStage / TicksForNextStage;

        if (ticksRemainingInStage <= 0 && curStageIndex < 6)
        {
            curStageIndex++;
            ticksRemainingInStage = TicksForNextStage;
        }

        if (!virionActive)
        {
            if (ticksUntilNextVirionActivity > 0)
            {

                ticksUntilNextVirionActivity = Math.Max(0, ticksUntilNextVirionActivity - delta);
            }

            if (ticksUntilNextVirionActivity <= 0)
            {
                virionActive = true;
                ticksUntilNextVirionDamage = Extension.virionDamageDaysRange.RandomInRange() * GenDate.TicksPerDay;
            }
        }
        else
        {
            if (ticksUntilNextVirionDamage > 0)
            {
                ticksUntilNextVirionDamage = Math.Max(0, ticksUntilNextVirionDamage - delta);
            }
            if (ticksUntilNextVirionDamage <= 0)
            {
                // TODO virion damages host
            }
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
        SpawnResult(map, pos, !extracting);
        pawn.health.RemoveHediff(this);
    }

    public void Notify_VirionExtracted()
    {
        extracting = true;
        HealthUtility.DamageUntilDead(pawn); // maybe destroy specifically the torso instead?
    }

    public override bool TendableNow(bool ignoreTimer = false)
    {
        return base.TendableNow(ignoreTimer);
        // TODO virion becomes restless at random intervals, requiring tending
    }

    public override void Tended(float quality, float maxQuality, int batchPosition = 0)
    {
        base.Tended(quality, maxQuality, batchPosition);
        // TODO check tend quality again rwquirement for growth stage. If above, virionActive = false and reset timer
        // if below, send a message and a text mote that says quality vs requirement
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

    private void SpawnResult(Map map, IntVec3 pos, bool premature)
    {
        // TODO check pawn spawned here, or in the caller?

        if (curStageIndex == -1 || premature) // maybe give item if stage is Legendary? Am I merciful?
        {
            SpawnEntity(map, pos);
        }
        else
        {
            SpawnItem(map, pos);
        }
    }

    private void SpawnItem(Map map, IntVec3 pos)
    {
        Thing thing = ThingMaker.MakeThing(Extension.producedItem);
        SetThingQuality(thing, curStageIndex);
        thing.stackCount = Extension.producedCount;
        GenPlace.TryPlaceThing(thing, pos, map, ThingPlaceMode.Near);
        Messages.Message("TSOA_VirionComplete".Translate(pawn.Name), new LookTargets(pos, map), MessageTypeDefOf.PositiveEvent);
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
        Messages.Message("TSOA_VirionIncomplete".Translate(pawn.Name), new LookTargets(pos, map), MessageTypeDefOf.NegativeHealthEvent);
    }

    private void SetThingQuality(Thing thing, int index)
    {
        //TODO
        CompQuality comp = thing.TryGetComp<CompQuality>();
        if (comp == null)
            return;

        QualityCategory qual = QualityCategory.Awful;
        for (int i = 0; i <= index; i++)
        {
            qual++;
        }
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
        Scribe_Values.Look(ref ticksRemainingInStage, "ticksRemainingInStage", -1);
        Scribe_Values.Look(ref curStageIndex, "curStageIndex", -1);

        Scribe_Values.Look(ref ticksUntilNextVirionActivity, "ticksUntilNextVirionActivity", -1);
        Scribe_Values.Look(ref ticksUntilNextVirionDamage, "ticksUntilNextVirionDamage", -1);
        Scribe_Values.Look(ref virionActive, "virionActive", false);
    }

}
