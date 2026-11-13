TickTracker 4.1.3, packaged as a module for Sonaria Software Launcher. The launcher downloads this file itself and checks it against a SHA-256 built into the launcher, so there is nothing to install by hand.

Fixed since 4.1.2: heal ticks were missed whenever the game showed the health number on the bar (for example "3036 (61%)"). The number covers the middle of the bar, so TickTracker either lost the bar or read it as stuck at about 41%. It now reads the bar from the rows the number does not cover. It also no longer lets a wrongly sized "bar" it once found distort the next reading.

SHA-256 of `TickTracker-4.1.3.zip`: `09f3233ef8b04a0b216f7519e09a3ff5c5bf94cd2c3f06a852654fa73635c372`
