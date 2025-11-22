using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace nuff.tsoa.arsenal
{
    public class DamageWorker_PsyExtraDamage : DamageWorker_AddInjury
    {
        public override DamageResult Apply(DamageInfo dinfo, Thing victim)
        {
            Pawn pawn = dinfo.Instigator as Pawn;
            PsyExtraDamageExtension extension = dinfo.Weapon?.GetModExtension<PsyExtraDamageExtension>();

            if (pawn == null || extension == null || pawn.GetPsylinkLevel() <= 0)
                return new DamageResult();

            float originalHeat = pawn.psychicEntropy.EntropyValue;
            Log.Message("original heat: " + originalHeat);
            float heatCost = originalHeat * extension.heatConsumedPercent;
            Log.Message("heat cost: " + heatCost);

            if (heatCost == 0)
            {
                return new DamageResult();
            }

            float bonusDamage = heatCost * extension.damagePerHeatConsumed;
            Log.Warning("bonus damage: " + bonusDamage);

            pawn.psychicEntropy.TryAddEntropy(-heatCost, null);

            var newDinfo = new DamageInfo(dinfo);
            newDinfo.Def = extension.damageDef;
            newDinfo.SetAmount(bonusDamage);

            //var newDinfo = new DamageInfo(
            //    extension.damageDef,
            //    bonusDamage,
            //    dinfo.ArmorPenetrationInt,
            //    dinfo.Angle,
            //    dinfo.Instigator,
            //    dinfo.HitPart,
            //    dinfo.Weapon,
            //    dinfo.Category);

            Log.Warning("damage amount: " + newDinfo.Amount);

            return base.Apply(newDinfo, victim);
        }
    }
}
