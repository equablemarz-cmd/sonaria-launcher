TickTracker 4.1.6, packaged as a module for Sonaria Software Launcher. The launcher downloads this file itself and checks it against a SHA-256 built into the launcher, so there is nothing to install by hand.

Fixes flashing. TickTracker reads your health bar off the screen 60 times a second. Up to 4.1.5 each of those screen copies also asked Windows to redraw every see-through window (TickTracker's own badge, the Damage Counter, game overlays), which made the game and its menus flicker on some PCs. 4.1.6 takes a plain copy of the screen's pixels instead, so Windows has nothing extra to redraw. Reading speed, the countdown and the combat alert are unchanged.

SHA-256 of `TickTracker-4.1.6.zip`: `d8685857be1ed2f23cd6480cc989736df8a364b62adff60d820c8e6b4eeaa9a9`
