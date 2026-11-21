Damage Counter 1.0.1, packaged as a module for Sonaria Software Launcher. The launcher downloads this file itself and checks it against a SHA-256 built into the launcher, so there is nothing to install by hand.

New since 1.0.0: the total moves sooner after a hit.
- It looks at the game 20 times a second instead of 10, and drops back to 10 by itself if the PC is taking too long over each look, so it never weighs on a slower computer.
- A hit counts as soon as it has been seen clearly a couple of times (about 0.15 seconds), instead of three times at 10 a second.
- The total rolls up to its new value faster.
- A number cut off by something in front of it (a "21" out of a "210") is no longer counted as a hit of its own.

Checked against every fight in the top 5 fliers video: it now catches about a quarter more hits than 1.0.0, mostly fast repeated hits it used to merge.

SHA-256 of `DamageCounter-1.0.1.zip`: `d7ea5bc961576fdb91858435125afbce6f05b73d67c2e0ad33d6a20e454f243f`
