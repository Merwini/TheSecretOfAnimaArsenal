using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tsoa.core;
using Verse;

namespace tsoa.arsenal;

public class EntropyDamageExtension : DefModExtension
{
    public float entropyCounted = 0;

    public float entropySpent = 0;

    public float damagePerEntropy = 0;

    public DamageDef damageDef;

    public override void ResolveReferences(Def parentDef)
    {
        base.ResolveReferences(parentDef);
        
        if (entropyCounted <= 0)
        {
            Log.Error("[TSOA] EntropyDamageExtension has no entropyCounted and will do nothing.");
        }

        if (damageDef == null)
        {
            damageDef = TSOAA_DefOf.TSOA_EntropicDischarge;
            Log.Error("[TSOA] EntropyDamageExtension had no damageDef defined. Defaulted to EntropicDischarge.");
        }
    }
}
