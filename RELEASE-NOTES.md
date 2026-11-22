TickTracker 4.1.7, packaged as a module for Sonaria Software Launcher. The launcher downloads this file itself and checks it against a SHA-256 built into the launcher, so there is nothing to install by hand.

Fixes the flashing that was left after 4.1.6. The combat alert looks at Roblox whenever another window is in front, even when Roblox is still in plain view (for example after clicking into Discord on another monitor, or onto the Damage Counter). It did that by asking Roblox to draw itself again (PrintWindow), which can make the game flash white. Now:
- if Roblox's top-right corner is in view, it reads the screen there with a plain copy, like the health bar
- only if Roblox is completely hidden behind other windows does it ask Roblox for a picture, where a flash can't be seen
- if Roblox is partly covered and the corner is hidden, it skips that look

With Windows HDR on, the alert may miss a fight while Roblox's corner is in view, since the screen copy comes back brighter. The combat timer is visible on screen in that case.

SHA-256 of `TickTracker-4.1.7.zip`: `44bdba122c1514bcdc8b29ff82309388872d9b4f4b958f89fe86b4664e0c75fa`
