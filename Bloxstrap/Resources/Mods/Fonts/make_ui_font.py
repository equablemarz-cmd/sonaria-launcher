"""Makes the launcher's 'Sonaria UI' fonts: Source Sans 3 drawn 12% larger inside the same line box.
The OFL asks that a changed version does not keep the reserved name 'Source', so it is renamed."""
import sys
from fontTools.ttLib import TTFont
from fontTools.ttLib.scaleUpem import scale_upem
SCALE = 1.12
def build(src, dst, style):
    f = TTFont(src)
    upem = f['head'].unitsPerEm
    keep = {k: getattr(f['hhea'], k) for k in ('ascent', 'descent', 'lineGap')}
    keep2 = {k: getattr(f['OS/2'], k) for k in ('sTypoAscender', 'sTypoDescender', 'sTypoLineGap', 'usWinAscent', 'usWinDescent')}
    # hinting instructions are tied to the original outlines: drop them (the text is drawn large enough not to need them)
    for tag in ('fpgm', 'prep', 'cvt ', 'hdmx', 'VDMX', 'LTSH', 'gasp'):
        if tag in f: del f[tag]
    glyf = f['glyf']
    for name in glyf.keys():
        g = glyf[name]
        if hasattr(g, 'program'): 
            from fontTools.ttLib.tables.ttProgram import Program
            g.program = Program(); g.program.fromBytecode(b'')
    scale_upem(f, int(round(upem * SCALE)))      # every outline, spacing and kerning value x1.12 ...
    f['head'].unitsPerEm = upem                  # ... measured against the original em = drawn 12% larger
    for k, v in keep.items(): setattr(f['hhea'], k, v)          # same line height as before
    for k, v in keep2.items(): setattr(f['OS/2'], k, v)
    fam = 'Sonaria UI'
    full = f'{fam} {style}'
    name = f['name']
    for rec in list(name.names):
        if rec.nameID in (1, 16): rec.string = fam
        elif rec.nameID in (2, 17): rec.string = style
        elif rec.nameID == 3: rec.string = f'{full}; modified from Source Sans 3'
        elif rec.nameID == 4: rec.string = full
        elif rec.nameID == 6: rec.string = full.replace(' ', '')
        elif rec.nameID in (18, 20, 21, 22, 25): name.names.remove(rec)
        elif rec.nameID == 10: rec.string = 'Source Sans 3 by Adobe, drawn 12% larger for Roblox menus and chat. Renamed as the SIL Open Font License asks of modified versions.'
    f.save(dst)
    g = TTFont(dst)
    print(dst.split('/')[-1], 'upem', g['head'].unitsPerEm, 'capH', g['OS/2'].sCapHeight, 'xH', g['OS/2'].sxHeight, 'ascent', g['hhea'].ascent, 'descent', g['hhea'].descent,
          'glyphs', len(g.getGlyphOrder()), 'names', sorted({(r.nameID, r.toUnicode()[:40]) for r in g['name'].names if r.nameID in (1, 2, 4, 6)}))
T = sys.argv[2].rstrip('/') + '/'    # folder holding SourceSans3-Regular.ttf and SourceSans3-Bold.ttf
build(T + 'SourceSans3-Regular.ttf', sys.argv[1] + '/SonariaUIRegular.ttf', 'Regular')
build(T + 'SourceSans3-Bold.ttf', sys.argv[1] + '/SonariaUIBold.ttf', 'Bold')
