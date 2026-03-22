using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using LudeonTK;

namespace tsoa.arsenal;

public class DamageWorker_EntropyExtraDamage : DamageWorker_AddInjury
{
    public override DamageResult Apply(DamageInfo dinfo, Thing victim)
    {
        if (!(victim is Pawn victimPawn))
        {
            return new DamageResult();
        }
        Pawn pawn = dinfo.Instigator as Pawn;

        float heatConsumedPercent = dinfo.Weapon?.GetStatValueAbstract(TSOAA_DefOf.TSOA_EntropyDamage) ?? 0;

        if (pawn == null || victim == null || heatConsumedPercent == 0 || pawn.GetPsylinkLevel() <= 0)
            return new DamageResult();

        float originalHeat = pawn.psychicEntropy.EntropyValue;
        float heatCost = originalHeat * heatConsumedPercent;

        if (heatCost == 0)
        {
            return new DamageResult();
        }

        float bonusDamage = heatCost * dinfo.Amount;

        pawn.psychicEntropy.TryAddEntropy(-heatCost, null);

        DamageInfo newDinfo = new DamageInfo(dinfo);
        newDinfo.Def = TSOAA_DefOf.TSOA_EntropicDischarge;
        newDinfo.SetAmount(bonusDamage);
        newDinfo.SetIgnoreArmor(true);

        if (DebugSettings.godMode)
        {
            Log.Message($"[TSOA] DamageWorker_PsyExtraDamage: {pawn.LabelShort} consumed {heatCost} heat (from {originalHeat}) to deal {bonusDamage} extra {TSOAA_DefOf.TSOA_EntropyExtraDamage.label} damage to {victim.LabelShort} with penetration: {dinfo.ArmorPenetrationInt}");
        }

        return base.Apply(newDinfo, victim);
    }
}
