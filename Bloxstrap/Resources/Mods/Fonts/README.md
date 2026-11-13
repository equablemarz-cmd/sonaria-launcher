# Fonts in this folder

`SonariaUIRegular.ttf` and `SonariaUIBold.ttf` are modified versions of Source Sans 3 by Adobe (Regular and Bold), which is licensed under the SIL Open Font License 1.1. The licence is in `SourceSans-LICENSE.md`.

What was changed: every glyph, spacing and kerning value is 12% larger inside the same line height, and the hinting instructions were removed. The fonts were renamed "Sonaria UI" because the licence reserves the name "Source" for unmodified versions.

The launcher uses them to set Roblox's own menus and chat in the font Creatures of Sonaria uses, at a size that matches the fonts they replace. They are made by `make_ui_font.py` in this folder from the files in Adobe's `source-sans` package.
