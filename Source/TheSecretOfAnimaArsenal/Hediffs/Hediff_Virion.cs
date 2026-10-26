using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        Gestating = 0
        Awful = 1
        Poor = 2
        Normal = 3
        Good = 4
        Excellent = 5
        Masterwork = 6
        Legendary = 7
     */

    private int growthStageIndex;
    public int GrowthStageIndex => growthStageIndex;

    private bool hasQuality; // store this so I don't have to keep checking for QualityComp, saves some CPU cycles
    private bool hasQualitySet = false;
    public bool HasQuality
    {
        get
        {
            if (!hasQualitySet) // fix after reload, so I don't have to serialize
            {
                hasQuality = Extension.producedItem.HasComp<CompQuality>();
                hasQualitySet = true;
            }
            return hasQuality;
        }
    }
    private int MaxStage => HasQuality ? 7 : 1;
    public bool IsFullyGestated => growthStageIndex >= MaxStage;

    private int ticksUntilNextStage;
    public int TicksUntilNextStage => ticksUntilNextStage;
    public int TicksForCurrentStage => (int)(Extension.gestationDaysList[growthStageIndex] * GenDate.TicksPerDay);
    public bool StageComplete => TicksUntilNextStage <= 0;
    public bool ShouldTick => !pawn.Suspended; // I dunno if I'll ever need to handle modded reasons a pawn shouldn't have its gestation ticking

    private int ticksUntilNextActivity;
    public int TicksUntilNextActivity => ticksUntilNextActivity;
    private bool virionActive = false;
    public int RandomTicksUntilNextVirionActivity => (int)(Extension.virionActivityDaysRange.RandomInRange * GenDate.TicksPerDay);
    public float TendQualityRequirement => Extension.requiredTendQualityPerStage * growthStageIndex;

    public bool CanExtractNow => GrowthStageIndex >= 1;
    private bool extracting = false; // doesn't need saving because it is set and read in the same method call
    private bool ShouldSpawnItem => IsFullyGestated || (extracting && growthStageIndex >= 1); // has reached max stage, or is extracting medically and has finished gestation

    internal RecipeDef virionRecipeDef;

    internal VirionExtension extension;
    internal VirionExtension Extension
    {
        get
        {
            if (extension == null)
            {
                extension = virionRecipeDef?.GetModExtension<VirionExtension>();
            }
            return extension;
        }
    }

    public override string GetInspectString()
    {
        if (DebugSettings.godMode)
        {
            return $"{TicksUntilNextStage} ticks until next stage";
        }
        return null;
    }

    public override string LabelInBrackets
    {
        get
        {
            string stageLabel;

            if (HasQuality)
            {
                stageLabel = growthStageIndex switch
                {
                    0 => "TSOA_GestatingStage".Translate(),
                    1 => "QualityCategory_Awful".Translate(),
                    2 => "QualityCategory_Poor".Translate(),
                    3 => "QualityCategory_Normal".Translate(),
                    4 => "QualityCategory_Good".Translate(),
                    5 => "QualityCategory_Excellent".Translate(),
                    6 => "QualityCategory_Masterwork".Translate(),
                    7 => "QualityCategory_Legendary".Translate(),
                    _ => "ERROR"
                };
            }
            else
            {
                stageLabel = growthStageIndex switch
                {
                    0 => "TSOA_GestatingStage".Translate(),
                    1 => "",
                    _ => "ERROR"
                };
            }

            string progress = IsFullyGestated ? "Complete".Translate() :  (1f - (float)TicksUntilNextStage / TicksForCurrentStage).ToStringPercent();

            return $"{Extension.producedItem.label} - {stageLabel} {progress}";
        }
    }

    public override bool ShouldRemove => false;


    public override void PostAdd(DamageInfo? dinfo)
    {
        base.PostAdd(dinfo);

        if (Extension == null)
        {
            Log.Error("Added Hediff_Virion, but implanted Virion did not have valid extension. Removing it.");
            pawn.health.RemoveHediff(this);
            return;
        }

        growthStageIndex = 0;
        SetStageEndTick(0);
        SetNextActivityTick();
    }

    public override void TickInterval(int delta)
    {
        base.TickInterval(delta);

        if (IsFullyGestated || !ShouldTick)
            return;

        ticksUntilNextStage -= delta;

        if (TicksUntilNextStage <= 0)
        {
            growthStageIndex++;
            if (!IsFullyGestated)
            {
                SetStageEndTick(TicksUntilNextStage);
            }
            else
            {
                CompleteGestation();
            }
        }


        if (!virionActive)
        {
            ticksUntilNextActivity -= delta;

            if (TicksUntilNextActivity <= 0)
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
            pawn.health.RemoveHediff(this);
        }
        else
        {
            DoEscapeOrExtraction();
        }
    }

    public override void PostRemoved()
    {
        base.PostRemoved();

        RemoveActivityHediff();
    }

    public void Notify_VirionExtracted()
    {
        extracting = true;
        DoEscapeOrExtraction();
        HealthUtility.DamageUntilDead(pawn); // maybe destroy specifically the torso instead?
        // if the player finds a way to cheat death, so be it
    }

    // called after stage has already advanced. leftover will be a negative number, from delta ticks > 1 overshooting the end of a stage
    private void SetStageEndTick(int leftover)
    {
        ticksUntilNextStage = TicksForCurrentStage + leftover;
    }

    private void SetNextActivityTick()
    {
        ticksUntilNextActivity = RandomTicksUntilNextVirionActivity;
    }

    private void StartVirionActivity()
    {
        Hediff_VirionActivity existingActivity = pawn.health.hediffSet.GetFirstHediffOfDef(TSOAA_DefOf.TSOA_VirionActiveHediff) as Hediff_VirionActivity;
        if (existingActivity != null)
        {
            virionActive = true;
            Log.Error("Hediff_Virion: tried to start virion activity but one already exists on pawn " + pawn.Name);
            return;
        }

        Hediff_VirionActivity hediff = HediffMaker.MakeHediff(TSOAA_DefOf.TSOA_VirionActiveHediff, pawn) as Hediff_VirionActivity;
        hediff.tendQualityRequirement = TendQualityRequirement;
        hediff.virionDamageDaysRange = Extension.virionDamageDaysRange;
        pawn.health.AddHediff(hediff);

        virionActive = true;

        // TODO should be a letter instead?
        Messages.Message("TSOA_VirionActiveMessage".Translate(pawn.LabelShort), pawn, MessageTypeDefOf.NegativeHealthEvent);
    }

    public void Notify_ActivityEnded()
    {
        virionActive = false;
        if (!IsFullyGestated)
        {
            SetNextActivityTick();
        }
    }

    public void CompleteGestation()
    {
        ticksUntilNextStage = int.MaxValue;
        ticksUntilNextActivity = int.MaxValue;
        if (virionActive)
        {
            RemoveActivityHediff();
            virionActive = false;
        }
    }

    private void DoEscapeOrExtraction()
    {
        Map map = pawn.MapHeld;
        IntVec3 pos = pawn.PositionHeld;

        DoEmergingEffects(map, pos);

        SpawnResult(map, pos, ShouldSpawnItem);

        pawn.health.RemoveHediff(this);
    }

    private void DoEmergingEffects(Map map, IntVec3 pos)
    {
        EffecterDefOf.MetalhorrorEmerging.Spawn(pos, map).Cleanup();
        CellRect cellRect = CellRect.CenteredOn(pos, 2);
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

    private void SpawnResult(Map map, IntVec3 pos, bool shouldSpawnItem)
    {
        if (!shouldSpawnItem)
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
        SetThingQuality(thing, growthStageIndex - 1); // stage 0 in gestating
        thing.stackCount = Extension.producedCount;
        GenPlace.TryPlaceThing(thing, pos, map, ThingPlaceMode.Near);
        string message = extracting ? "TSOA_VirionExtracted" : "TSOA_VirionDiedComplete";
        Messages.Message(message.Translate(pawn.Name), new LookTargets(pos, map), MessageTypeDefOf.PositiveEvent);
    }

    private void SpawnEntity(Map map, IntVec3 pos)
    {
        Pawn entity;

        List<List<VirionSpawnEntry>> entitiesForVirion = Extension.entitySpawnByStage ?? VirionExtension.MetalhorrorDefaults;

        if (entitiesForVirion.NullOrEmpty())
        {
            Log.Error($"Hediff_Virion: No entitySpawnByStage for Virion producing {Extension.producedItem.defName}");
            return;
        }

        if (entitiesForVirion.Count < growthStageIndex + 1)
        {
            Log.Error($"Hediff_Virion: Not enough stages in entitySpawnByStage for Virion producing {Extension.producedItem.defName}");
            return;
        }

        List<VirionSpawnEntry> entitiesForStage = entitiesForVirion[growthStageIndex];

        if (entitiesForStage.NullOrEmpty())
        {
            Log.Error("Hediff_Virion: No spawn entries for stage " + growthStageIndex + " for Virion producing " + Extension.producedItem.defName);
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
        if (!HasQuality)
            return;

        CompQuality comp = thing.TryGetComp<CompQuality>();
        if (comp == null)
            return;

        comp.SetQuality((QualityCategory)Mathf.Clamp(index, 0, 6), null);
    }

    public void RemoveActivityHediff()
    {
        Hediff_VirionActivity activity = pawn.health.hediffSet.GetFirstHediffOfDef(TSOAA_DefOf.TSOA_VirionActiveHediff) as Hediff_VirionActivity;
        if (activity != null)
        {
            pawn.health.RemoveHediff(activity);
        }
        virionActive = false;
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        if (!DebugSettings.godMode)
        {
            yield break;
        }

        yield return new Command_Action
        {
            defaultLabel = "Dev: Advanced gestation time by 1 day",
            action = () =>
            {
                ticksUntilNextStage -= GenDate.TicksPerDay;
            }
        };

        yield return new Command_Action
        {
            defaultLabel = "Dev: Virion activity next tick",
            action = () =>
            {
                ticksUntilNextActivity = 1;
            }
        };

        yield return new Command_Action
        {
            defaultLabel = "Dev: Log ticks until next activity",
            action = () =>
            {
                Log.Message("Ticks until next virion activity: " + TicksUntilNextActivity);
            }
        };
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Defs.Look(ref virionRecipeDef, "virionRecipeDef");
        Scribe_Values.Look(ref ticksUntilNextStage, "ticksUntilNextStage", int.MaxValue);
        Scribe_Values.Look(ref growthStageIndex, "curStageIndex", 0);

        Scribe_Values.Look(ref ticksUntilNextActivity, "ticksUntilNextActivity", 0);
        
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            virionActive = pawn.health.hediffSet.HasHediff(TSOAA_DefOf.TSOA_VirionActiveHediff);
        }
    }
}
