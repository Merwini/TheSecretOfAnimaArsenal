### THE SECRET OF ANIMA - ARSENAL

## NOTE FOR MODDERS

All of the custom weapon and armor mechanics in this mod are xml-configurable.
To use them, just have this mod as a required item for your mod, and add the correct xml to your defs either directly or as patches.

## CompPsyShield
Applied to apparel. When worn by a pawn with a psylink, creates a shield that blocks incoming damage, and produces neural heat in the wearer.

**How to use:**
Add a tsoa.arsenal.CompProperties_PsyShield to the apparel's `<comps>`

**Looks like:**
```xml
<ThingDef>
    (the rest of your def up here)
    <comps>
        <li Class="tsoa.arsenal.CompProperties_PsyShield">
		<heatPerDamage>1</heatPerDamage>
                <resetHeatPercent>0.8</resetHeatPercent>
                <resetDelayTicks>120</resetDelayTicks>
                <minDrawSize>1.2</minDrawSize>
                <maxDrawSize>1.55</maxDrawSize>
                <qualityHeatMultipliers>
                    <li>
                        <key>QualityCategory.Awful</key>
                        <value>1.2</value>
                    </li>
                    <li>
                        <key>QualityCategory.Poor</key>
                        <value>1.1</value>
                    </li>
                    <li>
                        <key>QualityCategory.Normal</key>
                        <value>1</value>
                    </li>
                    <li>
                        <key>QualityCategory.Good</key>
                        <value>0.9</value>
                    </li>
                    <li>
                        <key>QualityCategory.Excellent</key>
                        <value>0.8</value>
                    </li>
                    <li>
                        <key>QualityCategory.Masterwork</key>
                        <value>0.65</value>
                    </li>
                    <li>
                        <key>QualityCategory.Legendary</key>
                        <value>0.5</value>
                    </li>
                </qualityHeatMultipliers>
	</li>
    </comps>
</ThingDef>
```

**What each node controls:**
heatPerDamage - The baseline points of neural heat given to the wearer per point of damage blocked. This is further modified by qualityHeatMultipliers, see below. If not defined, defaults to 1. Example: if set to 3, and the shield blocks 10 damage, the wearer will accumulate 30 neural heat.

resetHeatPercent - If the psy shield is broken due to taking damage that puts the wearer at or above their neural heat limit, this number controls at what percentage of their neural heat limit the shield can reset. If not defined, defaults to 0.8 (80%). Example: if set to 0.8, and the wearer has 100 neural heat limit, and the shield breaks, the shield will reset and begin blocking damage again when neural heat falls to 80.

resetDelayTicks - A number of ticks (60 ticks is one second at 1x speed) before the shield can try to reset. Intended to prevent situations where a wearer's neural heat is right around the resetHeatPercent so the shield keeps breaking and resetting with every attack. If not defined, defaults to 120 ticks.

minDrawSize and maxDrawSize - psy shield visually shrinks in size as the wearer gets closer to their neural heat limit. These numbers define the largest it can be (maxDrawSize at 0 neural heat) and the smallest it can be (minDrawSize at 100% of limit). minDrawSize defaults to 1.2, and maxDrawSize defaults to 1.55.

qualityHeatMultipliers - psy shield heat per damage is modified by the quality of the armor. This node is a dictionary mapping different quality levels to different multipliers. Example: if heatPerDamage is set to 3, and the shield blocks 10 damage, and the apparel quality is Poor, the wearer will recieve 30 neural heat. But if the apparel quality is legendary, the wearer will instead recieve 15 neural heat.


## Entropy Extra Damage
Applied to a weapon. When wielded by a pawn with a psylink, can deal extra damage based on accumulated neural heat, disperse neural heat on successful attacks, or both.

**How to use:**
Add a tsoa.arsenal.EntropyDamageExtension to your weapon's `<modExtensions>`, AND add an `<extraMeleeDamages>` to the weapons Tool(s), with TSOA_EntropyExtraDamage as its DamageDef.

**Looks like:**
```xml
<ThingDef>
    (the rest of your def up here)
    <tools>
        <li>
            <label>edge</label>
            <capacities>
                <li>Cut</li>
            </capacities>
            <power>14</power>
            <cooldownTime>1.6</cooldownTime>
            <armorPenetration>0.25</armorPenetration>
            <extraMeleeDamages>
                <li>
                    <def>TSOA_EntropyExtraDamage</def>
                    <amount>1</amount>
                </li>
            </extraMeleeDamages>
        </li>
    </tools>
    <modExtensions>
        <li Class="tsoa.arsenal.EntropyDamageExtension">
            <damageDef>TSOA_EntropicDischarge</damageDef>
            <entropyCounted>0.1</entropyCounted>
            <entropySpent>0.1</entropySpent>
            <damagePerEntropy>1</damagePerEntropy>
        </li>
    </modExtensions>
</ThingDef>
```

**What each node controls:**
extraMeleeDamages - in the extraMeleeDamages, the def is the DamageDef that will be used to apply the extra damage. For this to work, it must be TSOA_EntropyExtraDamage. You may also add other extraMeleeDamages if you want, it will not interfere. THE AMOUNT HERE DOES NOT MATTER, ANY NON-ZERO AMOUNT WILL WORK. This damage is not actually applied, it only exists to trigger the Worker that applies the damage, all of the actual values are calculated using the data in the EntropyDamageExtension.

*EntropyDamageExtension*
damageDef - what DamageDef the extra damage will be applied as. Can be any damage type you like. I use TSOA_EntropicDischarge, my own custom DamageDef.

entropyCounted - What percentage of the wielder's neural heat, as a float, will be counted when detemrining extra damage. If not defined, defaults to 0%. Example: If set to 0.1 (10%), and the wielder has 60 accumulated neural heat, the Worker will count 6 neural heat for bonus damage, multiplied by damagePerEntropy (see below).

damagePerEntropy - How many points of damage will be applied per point of neural heat counted by entropyCounted. If not defined, defaults to 0. Example: If set to 2, and entropyCounted is counting 6 from the example above, would apply (6 x 2 = 12) points of damage on a successful attack.

entropySpent - What percentage of the wielder's accumulated neural heat will be dispersed / removed on a successful attack. If not defined, defaults to 0%. Does not need to match entropyCounted, and leaving it at 0 means the weapon will do extra damage from neural heat, but not remove any neural heat. Example: If set to 0.1 (10%), and the wielder has 60 accumulated neural heat, on a successful attack the wielder will remove (60 x 0.1 = 6) neural heat, leaving 54 remaining.


## Psy Scaling
Applied to weapons and apparel. Gives a damage bonus to weapons, or an armor bonus to apparel, based on a percentage of the wielder/wearer's Psychic Sensitivity stat.

**How to use:**
Add a `<TSOA_PsyScaling>` node to the `<stateBases>` for the equipment.

**Looks like:**
```xml
<ThingDef>
    (the rest of your def up here)
    <statBases>
        (other stat bases)
        <TSOA_PsyScaling>0.25</TSOA_PsyScaling>
    </statBases>
</ThingDef>
```

**What each node controls:**
TSOA_PsyScaling - A percentage of the equipment user's Psychic Sensitivity that will be used when calculating the damage or armor bonuses, written as a float value. Does not scale values down if Sensitivity is below 100%. Example: If a weapon has 0.25 (25%) PsyScaling, and the wielder has 180% Psychic Sensitivity, the wielder's Sensitivity is 80% above normal, so will add (80% x 0.25 = 20%) to the weapon's damage. So a weapon that normally does 10 damage would now do 12.