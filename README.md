# Artifact of Doom

A mod for Risk of Rain 2.

Adds an artifact to the game which destroys items of your inventory if you take damage but gives you items if you kill enemies.

## Mods used to create this mod
The root projekt is TinkersSatchel:
https://github.com/ThinkInvis/RoR2-TinkersSatchel

For the UI i took parts of Crashaholics UIModifier mod:
https://github.com/Crashaholic/RoR2UIMod

And the Multiplayer UI support is based on the Example of MiniRPC:
https://github.com/wildbook/R2Mods/tree/develop/MiniRpcLib

## What does this mod do
This mod gives you items after killing some enemies. But you'll loose items if you get hit.

The calculation for the required kills is:
```(totalItems - currentStage * averageItemsPerStage) ^ exponentTriggerItems```
`totalItems` is the current item count of each character. `CurrentStage` is the number of stages completed and `exponentTriggerItems` and `averageItemsPerStage` can be changed in the config file.
For example you've 20 items and are on stage 5. The configured averageItems are 3 (default). That means you've to kill (20-5*3)^2=10 enemies to get one item.

Everytime you get hit you'll loose an item. The only exception is if you've less items then the configured minItemsPerStage.
If that is the case you have a chance that you will not use an item.  
The calculation for this is:
```squareroot(totalItems/(minItemsPerStage*CurrentStage));```
minItemsPerStage can be configured in the setting. For example you've 6 items and are on stage 5. The configured minItemsPerStage are 2 (default).
So you've the calculation:
6/(2*5)=3/5 that means you've a change of 60% to not loose an item.

On the other hand the possibility increases if you've more items than you should have to loose more than one item. The formular to calculate that is:
```(totalItems) / (maxItemsPerStage * currentStage)^exponentailFactorToCalculateSumOfLostItems```
maxItemsPerStage and exponentailFactorToCalculateSumOfLostItems are configurable and have a default value of 7 and 1.5.
For example you've 50 items, are on stage 5 and have the default values so you calculate:
(50 / 7 * 5)^1.5=1.7
So you'll have a 100% chance to loose one item and 70% chance to loose another.

After you lost an item you'll get a short buff that prevents you from loosing another one. The length of that buff depends on the difficulty and can also be configured with the config entitys "timeAfterHitToNotLoose".

In the settings you can specify for each character how many items he'll get if he kills enough enemys and if he has an additional multiplier to the buff after loosing an item. The default settings are that every meele char has a 4 times longer buff and Artificer get double as many items if he kills enough enemies.

I'm not sure if the buff and the character specific settings make the game too easy. So if you think so just change the settings or tell me what the perfect settings are.

## UI
On the left side there are the symbols of the gained items. ***These are only the items which you obtained by the mod.*** On the right side there are all lost items. The UI resets every stage.

## I've an issue:
Please add an issue to my github repository:
https://github.com/SirHamburger/ArtifactOfDoom
You can also reach me in Discord (Sir Hamburger#8447)

## Patchnotes
### Version 0.9.1
* Fixed bug in multiplayer when one ally dies you don't see your gain/lost items
* Added exponential function to items gained
* Added root function to items lost
* Fixed calculation bugs for loosing items (so you will loose less items if your below the minItemCount)

### Version 0.9.0
Added Character specific settings and a timed buff which prevents you from loosing items

### Version 0.8.1 
Updated dependency in manifest.json (inserted Tiler2)

### Version 0.8.0
Initial upload