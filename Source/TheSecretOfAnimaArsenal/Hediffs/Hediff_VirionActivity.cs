using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Sound;
using RimWorld;
using LudeonTK;
using UnityEngine;

namespace tsoa.arsenal;

public class Hediff_VirionActivity : Hediff
{
    // Separating this into its own hediff because Hediff_Virion's Hediff entry in Health tab is already crowded enough. Just no way to also have it tell the player about Activity

    public const float damageTimeFactor = 0.9f; // need to balance. maybe 0.95?

    public float tendQualityRequirement; // set by Hediff_Virion before add
    public FloatRange virionDamageDaysRange; // set by Hediff_Virion before add

    private int ticksUntilNextVirionDamage = -1;
    private float timeFactor = 1f;

    public int TicksUntilNextVirionDamage
    {
        get
        {
            return (int)(virionDamageDaysRange.RandomInRange * GenDate.TicksPerDay * timeFactor);
        }
    }

    public override void PostAdd(DamageInfo? dinfo)
    {
        base.PostAdd(dinfo);
        ticksUntilNextVirionDamage = TicksUntilNextVirionDamage;
    }

    public override void PostRemoved()
    {
        base.PostRemoved();
        Hediff_Virion originalHediff = pawn.health.hediffSet.GetFirstHediffOfDef(TSOAA_DefOf.TSOA_VirionHediff) as Hediff_Virion;
        originalHediff?.Notify_ActivityEnded();
    }

    public override void TickInterval(int delta)
    {
        base.TickInterval(delta);

        if (ticksUntilNextVirionDamage > 0)
        {
            ticksUntilNextVirionDamage = Math.Max(0, ticksUntilNextVirionDamage - delta);
        }
        if (ticksUntilNextVirionDamage <= 0)
        {
            DoVirionDamage();
            timeFactor *= damageTimeFactor;
            ticksUntilNextVirionDamage = TicksUntilNextVirionDamage;
        }
    }

    public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
    {
        base.Notify_PawnDied(dinfo, culprit);
        pawn.health.RemoveHediff(this);
    }

    public override bool TendableNow(bool ignoreTimer = false)
    {
        return true;
    }

    public override void Tended(float quality, float maxQuality, int batchPosition = 0)
    {
        if (quality >= tendQualityRequirement)
        {
            MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, "TSOA_VirionTendSuccessMote".Translate());
            Messages.Message("TSOA_VirionTendSuccessMessage".Translate(pawn.LabelShort), pawn, MessageTypeDefOf.PositiveEvent);

            pawn.health.RemoveHediff(this);
        }
        else
        {
            MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, "TSOA_VirionTendFailureMote".Translate(quality.ToStringPercent(), tendQualityRequirement.ToStringPercent()), Color.red);
            Messages.Message("TSOA_VirionTendFailureMessage".Translate(pawn.LabelShort), pawn, MessageTypeDefOf.NegativeHealthEvent);
        }
    }

    private void DoVirionDamage()
    {
        if (!pawn.SpawnedOrAnyParentSpawned)
        {
            return;
        }

        EffecterDefOf.MeatExplosionSmall.Spawn(pawn.Position, pawn.Map).Cleanup();
        SoundDefOf.Execute_Cut.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));

        BodyPartRecord torso = pawn.RaceProps.body.corePart;
        if (torso == null)
        {
            return;
        }

        List<BodyPartRecord> candidates = new List<BodyPartRecord>();
        foreach (BodyPartRecord part in torso.GetDirectChildParts())
        {
            if (part.depth == BodyPartDepth.Inside)
            {
                candidates.Add(part);
            }
        }

        BodyPartRecord targetPart;

        if (candidates.Count > 0 && Rand.Bool) // 50% chance to hit torso vs a random organ
        {
            targetPart = candidates.RandomElement();
        }
        else
        {
            targetPart = torso;
        }

        float damageAmount = Rand.Range(4f, 10f); // organs have 15-20 hitPoints, on average fatal damage will happen if the same organ is hit 3 times, but could happen in 2

        DamageInfo dinfo = new DamageInfo(
            DamageDefOf.Cut,
            damageAmount,
            0f,
            -1f,
            instigator: null,
            hitPart: targetPart
        );

        pawn.TakeDamage(dinfo);
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        if (!DebugSettings.godMode)
        {
            yield break;
        }

        yield return new Command_Action
        {
            defaultLabel = "Dev: Virion damage next tick",
            action = () =>
            {
                ticksUntilNextVirionDamage = 0;
            }
        };

        yield return new Command_Action
        {
            defaultLabel = "Dev: Log ticks until next damage",
            action = () =>
            {
                Log.Message("Ticks until next virion damage: " + ticksUntilNextVirionDamage);
            }
        };
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref ticksUntilNextVirionDamage, "ticksUntilNextVirionDamage", -1);
        Scribe_Values.Look(ref timeFactor, "timeFactor", 1f);
        Scribe_Values.Look(ref tendQualityRequirement, "tendQualityRequirement", 0f);
        Scribe_Values.Look(ref virionDamageDaysRange, "virionDamageDaysRange");
    }
}
