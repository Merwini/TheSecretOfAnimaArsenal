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
using tsoa.core;

namespace tsoa.arsenal;

public class Hediff_Virion : Hediff
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
    private bool hasQuality; // store this so I don't have to keep checking for QualityComp, saves some CPU cycles
    private int MaxStage
    {
        get
        {
            if (hasQuality)
            {
                return 6;
            }

            return 0;
        }
    }

    private int InitialGestationTicks => (int)(Extension.initialGestationDays * GenDate.TicksPerDay);

    public int TicksUntilNextStage
    {
        get
        {
            if (curStageIndex == -1) // initial gestation
                return (int)(Extension.initialGestationDays * GenDate.TicksPerDay);

            if (curStageIndex == 6) // already at legendary, no more stages
                return int.MaxValue;

            return (int)(Extension.qualityGestationDaysList[curStageIndex] * GenDate.TicksPerDay);
        }
    }

    private int ticksUntilNextVirionActivity = -1;
    private bool virionActive = false;
    public float TendQualityRequirement => Extension.requiredTendQualityPerStage * (curStageIndex + 1);
    public int RandomTicksUntilNextVirionActivity => (int)(Extension.virionActivityDaysRange.RandomInRange * GenDate.TicksPerDay);

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

    public override string LabelInBrackets
    {
        get
        {
            string stageLabel = curStageIndex switch
            {
                -1 => "TSOA_GestatingStage".Translate(),
                0 => "QualityCategory_Awful".Translate(),
                1 => "QualityCategory_Poor".Translate(),
                2 => "QualityCategory_Normal".Translate(),
                3 => "QualityCategory_Good".Translate(),
                4 => "QualityCategory_Excellent".Translate(),
                5 => "QualityCategory_Masterwork".Translate(),
                6 => "QualityCategory_Legendary".Translate(),
                _ => "ERROR"
            };
            return $"{stageLabel} {Severity.ToStringPercent()}";
        }
    }

    public override void PostAdd(DamageInfo? dinfo)
    {
        base.PostAdd(dinfo);

        if (Extension == null)
        {
            Log.Error("Added Hediff_Virion, but implanted Virion did not have valid extension");
            pawn.health.RemoveHediff(this);
            return;
        }

        ticksRemainingInStage = InitialGestationTicks;
        curStageIndex = -1;

        ticksUntilNextVirionActivity = RandomTicksUntilNextVirionActivity; // Pre-load an activity

        hasQuality = Extension.producedItem.HasComp<CompQuality>();
    }

    public override void TickInterval(int delta)
    {
        base.TickInterval(delta);

        if (ticksRemainingInStage > 0)
        {
            ticksRemainingInStage = Math.Max(0, ticksRemainingInStage - delta);
        }

        Severity = 1f - (float)ticksRemainingInStage / TicksUntilNextStage;

        if (ticksRemainingInStage <= 0 && curStageIndex < MaxStage)
        {
            curStageIndex++;
            ticksRemainingInStage = TicksUntilNextStage;
        }

        if (!virionActive)
        {
            if (ticksUntilNextVirionActivity > 0)
            {

                ticksUntilNextVirionActivity = Math.Max(0, ticksUntilNextVirionActivity - delta);
            }

            if (ticksUntilNextVirionActivity <= 0)
            {
                StartVirionActivity();
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

    public void Notify_ActivityEnded()
    {
        virionActive = false;
        ticksUntilNextVirionActivity = RandomTicksUntilNextVirionActivity;
    }

    private void StartVirionActivity()
    {
        Hediff_VirionActivity existingActivity = pawn.health.hediffSet.GetFirstHediffOfDef(TSOAA_DefOf.TSOA_VirionActiveHediff) as Hediff_VirionActivity;
        if (existingActivity != null)
        {
            Log.Error("Hediff_Virion: tried to start virion activity but one already exists on pawn " + pawn.Name);
            return;
        }

        virionActive = true;
        Hediff_VirionActivity hediff = HediffMaker.MakeHediff(TSOAA_DefOf.TSOA_VirionActiveHediff, pawn) as Hediff_VirionActivity;
        hediff.tendQualityRequirement = TendQualityRequirement;
        hediff.virionDamageDaysRange = Extension.virionDamageDaysRange;
        pawn.health.AddHediff(hediff);
        Messages.Message("TSOA_VirionActiveMessage".Translate(pawn.LabelShort), pawn, MessageTypeDefOf.NegativeHealthEvent);
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

        List<List<VirionSpawnEntry>> entitiesForVirion = Extension.entitySpawnByStage ?? VirionExtension.MetalhorrorDefaults;

        if (entitiesForVirion.NullOrEmpty())
        {
            Log.Error($"Hediff_Virion: No entitySpawnByStage for Virion producing {Extension.producedItem.defName}");
            return;
        }

        if (entitiesForVirion.Count < curStageIndex + 1)
        {
            Log.Error($"Hediff_Virion: Not enough stages in entitySpawnByStage for Virion producing {Extension.producedItem.defName}");
            return;
        }

        List<VirionSpawnEntry> entitiesForStage = entitiesForVirion[curStageIndex + 1];

        if (entitiesForStage.NullOrEmpty())
        {
            Log.Error("Hediff_Virion: No spawn entries for stage " + curStageIndex + " for Virion producing " + Extension.producedItem.defName);
            return;
        }

        foreach (VirionSpawnEntry spawnEntry in entitiesForStage)
        {
            for (int i = 0; i < spawnEntry.count; i++)
            {
                entity = PawnGenerator.GeneratePawn(new PawnGenerationRequest(spawnEntry.kind, Faction.OfEntities));
                if (spawnEntry.forcedLifeStageIndex >= 0)
                {
                    entity.ageTracker.LockCurrentLifeStageIndex(spawnEntry.forcedLifeStageIndex);
                }
                GenSpawn.Spawn(entity, pos, map);
            }
        }

        Messages.Message("TSOA_VirionIncomplete".Translate(pawn.Name), new LookTargets(pos, map), MessageTypeDefOf.NegativeHealthEvent);
    }

    private void SetThingQuality(Thing thing, int index)
    {
        if (!hasQuality)
            return;

        CompQuality comp = thing.TryGetComp<CompQuality>();
        if (comp == null)
            return;

        comp.SetQuality((QualityCategory)Math.Clamp(index, 0, 6), null);
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        if (!DebugSettings.godMode)
        {
            yield break;
        }

        yield return new Command_Action
        {
            defaultLabel = "Dev: Progress virion 1 day",
            action = () =>
            {
                ticksRemainingInStage -= GenDate.TicksPerDay;
            }
        };

        yield return new Command_Action
        {
            defaultLabel = "Dev: Virion activity next tick",
            action = () =>
            {
                ticksUntilNextVirionActivity = 0;
            }
        };

        yield return new Command_Action
        {
            defaultLabel = "Dev: Log ticks until next activity",
            action = () =>
            {
                Log.Message("Ticks until next virion activity: " + ticksUntilNextVirionActivity);
            }
        };
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Defs.Look(ref virionDef, "virionDef");
        Scribe_Values.Look(ref ticksRemainingInStage, "ticksRemainingInStage", -1);
        Scribe_Values.Look(ref curStageIndex, "curStageIndex", -1);
        Scribe_Values.Look(ref hasQuality, "hasQuality", false);

        Scribe_Values.Look(ref ticksUntilNextVirionActivity, "ticksUntilNextVirionActivity", -1);
        Scribe_Values.Look(ref virionActive, "virionActive", false);
    }
}
