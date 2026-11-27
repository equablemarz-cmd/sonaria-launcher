TickTracker 4.3.0, packaged as a module for Sonaria Software Launcher. The launcher downloads this file itself and checks it against a SHA-256 built into the launcher, so there is nothing to install by hand.

A new tick clock. Natural regen is a fixed 15-second clock that never moves, but TickTracker used to count from whichever heal it last accepted, so one stray heal near the countdown (Healing Pulse, mud, food, a healer) pulled the whole countdown over to itself. Now it weighs up every rhythm the heals could belong to and shows the one with the most ticks behind it:
- a stray heal near the countdown no longer drags it
- a rhythm three ticks have landed on is only dropped for one that is clearly better backed up
- bleed, full health or a hit at the same moment: the countdown carries on through them
- if the first heal it counted from wasn't a tick, it switches once that heal's own tick fails to show
- coming back from a menu on the same creature carries the countdown on
- something passing over the health bar for a moment is no longer taken for a hit and a heal

Tested in simulation only (made-up ticks, stray heals and hits), not against recorded gameplay.

SHA-256 of `TickTracker-4.3.0.zip`: `672cdb32a07663fc6e4fa04ad182f07d825e81876426212af8348a693db13d70`
