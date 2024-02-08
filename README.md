![](https://i.imgur.com/FONmSL9.png)

# Emergency Dice

This mod adds five new items (and more coming soon):
* Emergency Die
* The Gambler
* Chronos
* The Sacrificer
* Pathfinding blobs (not a dice item)

## Emergency Die

This is an item you can buy from the store.
It is a one time use only die, which teleports you to the ship with all your items.
There's one catch though. It has a 50/50 chance of working.

* Rolling 6 will return you to the ship with all the crewmates standing near you (they need to be very close).
* Rolling 4 or 5 will return you to the ship.
* Rolling 3 has a mixed effect. It can be bad, it can be good.
* Rolling 2 will cause something bad to happen.
* You don't want to roll a 1.

## The Gambler

A new scrap item with rolling outcomes similar to the Emergency die,
but with a larger pool of effects and a bigger chance for a bad outcome.

* Rolling 6 will cause a Great effect.
* Rolling 5 will cause a Good effect.
* Rolling 4 will cause either a Good or a Mixed effect.
* Rolling 3 will cause a Bad effect.
* Rolling 2 will cause either a Bad or an Awful effect.
* Rolling 1 will cause an Awful effect.

You can either sell it, or use it.

## Chronos

Similar to The Gambler. Has a higher chance of getting a great effect,
but the outcomes change when the time passes. As the night
comes, the chances to roll a bad/awful effect increase.

* Rolling 6 will cause a Great effect.
* Rolling 5 will cause a Good or a Great effect.
* Rolling 4 will cause either a Bad, Good or a Great effect.
* Rolling 3 will cause a Bad effect.
* Rolling 2 will cause either a Bad or an Awful effect.
* Rolling 1 will cause an Awful effect.

You can either sell it, or use it.


## The Sacrificer

Guarantees a return to the ship, and a bad effect.

* Rolling 6 will cause a Bad effect.
* Rolling 5 will cause a Bad effect.
* Rolling 4 will cause a Bad effect.
* Rolling 3 will cause a Bad or an Awful effect.
* Rolling 2 will cause an Awful effect.
* Rolling 1 will cause two Awful effects.

## Effects

### Positive

* Gives you Pathfinding Blobs - a new item which shows everyone the way to the main entrance when used
* Gives you a shotgun with ammo
* Heals all alive players and restores their batteries in every item
* Teleports you to the ship with your items
* Spawns about 3-9 (random) scrap items beneath you with a lower weight.
* Revives every dead player
* Gives you infinite stamina
* Gives everyone infinite stamina
* More to come

### Negative

* Spawns a jester in a popped state, and pops all already existing jesters.
* Creates a lot of fake fire exits
* Spawns an armageddon outside
* Spawns a lot of bee hives outside
* Spawns a lot of bugs inside
* Detonates a random player
* Blocks all fire exits
* Closes all open doors and locks them
* Jumpscares you (can be changed in the settings to be mild)
* Spawns a lot of mines inside
* Spawns a lot of mines outside
* Changes your speech, and makes it harder to communicate
* Causes coilheads to sometimes ignore your stare, which allows them to move while being watched.
* Creates turrets near the ship
* Teleports you inside
* Turns off all lights permanently
* Creates a lot of turrets
* Spawns a lot of worms outside
* Spawns a lot of zombies inside
* Teleports you to the ship, but leaves a zombie in your place
* Makes you emit an annoying sound periodically
* More to come


### Config
Be sure to check out the plugin's config file.
It is called Emergency Dice.cfg and it should
be generated when you run the game with the mod for the first time.

You can modify which effects you want or not and more.

### Emergency die shop alias
Some mods (like Lethal Things mod) might use conflicting item names in the shop.
To solve this I added few aliases for the emergency die in the shop.

Try these aliases in the shop:
* emergencydie
* dice
* die
* edie

### Chat commands
I included some chat commands in the mod, mainly for myself to ease debugging.
If you want to use them you have to set

"Allow chat commands = true"

in the config.

To spawn dice use:
* ~edice dice num

where num is an id of the die.
ID's start from 1.

If you want to force an effect, use
* ~edice effect effectName

You can find effectNames on github in the source code.
Look for the folder named Effects, and inside you will find all of them.
Pick the one you want, and get the name from the line

public string Name => ""

### Special thanks
* Thanks Simmmmms for contributing some code and finding bugs!
* Thank you Kayden21 for reporting bugs



### All players need this mod
