#!/usr/bin/env python3
"""Regenerate src/Primal/scenes/primal spearmaster/ from the hand-drawn art in
src/Primal/art-source/primal spearmaster/.

The source art is six 1024x1024 layers drawn on one shared canvas (so they composite at 0,0).
Rain World wants each scene layer as its own 1366x768 PNG, drawn 1:1 because
PrimalSpearmasterScene forces MenuDepthIllustration.MenuShader.Basic on every layer -- see that
file's header for why (Basic is the only shader that treats a PNG as a plain image rather than a
half-height colour-over-depth-map stack).

The 1024 -> 1366x768 step is a pure translate, no scaling: the union of all drawn content is
(209,214)-(951,852), whose centre lands on the screen centre (683,384) at offset (+103,-149).
That frames the art with even 312px side and 65px top/bottom margins.
"""

from pathlib import Path

from PIL import Image

ROOT = Path(__file__).resolve().parent.parent
SRC = ROOT / "src/Primal/art-source/primal spearmaster"
DEST = ROOT / "src/Primal/scenes/primal spearmaster"

SCREEN = (1366, 768)
# Crop box on the 1024x1024 source that maps it onto the 1366x768 screen at offset (+103,-149).
# PIL fills out-of-bounds area with transparent, which is exactly what the non-backdrop layers want.
BOX = (-103, 149, 1263, 917)

DARKEN = 0.30  # locked-character "Dark" variants; alpha preserved, RGB scaled.


def layer(name):
    return Image.open(SRC / name).convert("RGBA").crop(BOX)


def stack(*layers):
    out = Image.new("RGBA", SCREEN, (0, 0, 0, 0))
    for im in layers:
        out.alpha_composite(im)
    return out


def darken(im):
    r, g, b, a = im.split()
    scale = lambda c: c.point(lambda v: int(v * DARKEN))
    return Image.merge("RGBA", (scale(r), scale(g), scale(b), a))


def main():
    DEST.mkdir(parents=True, exist_ok=True)

    foliage, poles, vines, scug = (layer(n) for n in
                                   ("backgorujnd.PNG", "poles.PNG", "vines.PNG", "scug.PNG"))
    # black.PNG is a uniform opaque black field, so it is regenerated at full screen size rather
    # than translated -- a translated copy would leave transparent gutters at the screen edges.
    backdrop = stack(Image.new("RGBA", SCREEN, (0, 0, 0, 255)), foliage)
    scug_dark = darken(scug)

    written = {
        "background - 4": backdrop,
        "pipes - 3": poles,
        "vines - 2": vines,
        "slugcat - 1": scug,
        "slugcat - 1 - dark": scug_dark,
        "midground": stack(vines, poles),
        "foreground": stack(vines, poles, scug),
        "flat": stack(backdrop, vines, poles, scug),
        "dark flat": stack(backdrop, vines, poles, scug_dark),
        "empty": Image.new("RGBA", SCREEN, (0, 0, 0, 0)),
    }

    for name, im in written.items():
        path = DEST / f"primal spearmaster {name}.png"
        im.save(path, optimize=True)
        print(f"{path.name:<45} {im.size[0]}x{im.size[1]}  bbox={im.getchannel('A').getbbox()}")

    # One "x, y" line per depth layer. Spearmaster's biggest scene has 4; 6 lines is slack.
    for positions in ("positions.txt", "positions_ims.txt"):
        (DEST / positions).write_text("0, 0\n" * 6)
    print("wrote positions.txt / positions_ims.txt")


if __name__ == "__main__":
    main()
