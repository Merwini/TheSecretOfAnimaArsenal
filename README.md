### THE SECRET OF ANIMA - ARSENAL

## NOTE FOR MODDERS

All of the custom weapon and armor mechanics in this mod are xml-configurable.
To use them, just have this mod as a required item for your mod, and add the correct xml to your defs either directly or as patches.

## CompPsyShield
When worn by a pawn with a psylink, creates a shield that blocks incoming damage, and produces neural heat in the wearer.

**How to use:**
Add a CompProperties_PsyShield to the apparel's `<comps>`

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
