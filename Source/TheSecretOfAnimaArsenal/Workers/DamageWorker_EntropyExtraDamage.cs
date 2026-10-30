using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace tsoa.arsenal;

public class DamageWorker_EntropyExtraDamage : DamageWorker_AddInjury
{
    public override DamageResult Apply(DamageInfo dinfo, Thing victim)
    {
        Pawn pawn = dinfo.Instigator as Pawn;

        EntropyDamageExtension extension = dinfo.Weapon?.GetModExtension<EntropyDamageExtension>();
        
        if (pawn == null || victim is not Pawn || extension == null || pawn.GetPsylinkLevel() <= 0)
            return new DamageResult();

        float originalHeat = pawn.psychicEntropy.EntropyValue;
        float heatCounted = originalHeat * extension.entropyCounted;

        if (heatCounted == 0)
            return new DamageResult();

        float heatSpent = originalHeat * extension.entropySpent;
        float bonusDamage = heatCounted * extension.damagePerEntropy;

        pawn.psychicEntropy.TryAddEntropy(-heatSpent, null, false);

        // make sure a misconfigured damageDef doesn't create an infinite loop
        DamageDef newDamage = extension.damageDef != TSOAA_DefOf.TSOA_EntropyExtraDamage ? extension.damageDef : TSOAA_DefOf.TSOA_EntropicDischarge;

        DamageInfo newDinfo = new DamageInfo(dinfo);
        newDinfo.Def = newDamage;
        newDinfo.SetAmount(bonusDamage);
        newDinfo.SetIgnoreArmor(true);

        if (DebugSettings.godMode)
        {
            Log.Message($"[TSOA] DamageWorker_PsyExtraDamage: {pawn.LabelShort} consumed {heatSpent} heat (from {originalHeat}) to deal {bonusDamage} extra {newDamage.LabelCap} damage to {victim.LabelShort} with penetration: {dinfo.ArmorPenetrationInt}");
        }

        return newDamage.Worker.Apply(newDinfo, victim);
    }
}
