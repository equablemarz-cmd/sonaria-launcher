Damage Counter 1.0.4, packaged as a module for Sonaria Software Launcher. The launcher downloads this file itself and checks it against a SHA-256 built into the launcher, so there is nothing to install by hand.

Stops the flashing on every PC.
- It looks at the game 10 times a second again, the same speed as 1.0.0. 1.0.1 to 1.0.3 looked 20 times a second, and grabbing the screen that often could make things flash on some PCs. The automatic 20-a-second mode is gone.
- It now takes a plain copy of the screen's pixels. Up to 1.0.3 the screen copy also asked Windows to redraw every see-through window (the counter's own panel, TickTracker's badge, game overlays) on each copy, a known cause of flicker. It doesn't need those windows, so it no longer asks for them.

Counting is the same as 1.0.3's 10-a-second mode: a hit counts after two clear sightings, cut-off numbers are ignored, and the total rolls up quickly. The 3D panel is unchanged.

SHA-256 of `DamageCounter-1.0.4.zip`: `664f6649f559603e6b72ff4b5999e0f90ecf7a2a6791cbf5100b79fdb80f6456`
