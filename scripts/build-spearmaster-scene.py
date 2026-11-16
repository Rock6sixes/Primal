#!/usr/bin/env python3
"""Regenerate src/Primal/scenes/primal spearmaster/ from the hand-drawn art in
src/Primal/art-source/primal spearmaster/.

The source art is six 1024x1024 layers drawn on one shared canvas (so they composite at 0,0).
Rain World wants each scene layer as its own 1366x768 PNG, drawn 1:1 because
PrimalSpearmasterScene forces MenuDepthIllustration.MenuShader.Basic on every layer -- see that
file's header for why (Basic is the only shader that treats a PNG as a plain image rather than a
half-height colour-over-depth-map stack).

The 1024 -> 1366x768 step is a scale plus a translate, both derived by measurement rather than
taste. A first attempt placed the art 1:1 and centred the whole drawn area; in-game that ran off
the top of the screen and collided with the name/description text.

**Do not frame this art against a screenshot's own coordinates.** The game draws a portrait layer
roughly 83px HIGHER than the layer's own position in its PNG. Measured by aligning each PNG
composite against a real screenshot of it (best intersection-over-union over candidate shifts):
Artificer dy=-88 at IoU 0.889, Spearmaster dy=-78 at IoU 0.959 -- the ~10px spread is parallax,
which moves a layer by up to 80/depth (~22-38px) depending on when the shot was taken. Mistaking
screen coordinates for PNG coordinates is exactly what made the first correction wrong a second
time, so the target below is expressed purely in PNG space.

In PNG space the known-good Artificer portrait occupies x 401..963, y 132..670 (its full drawn
extent, i.e. anything not pure black). This art is scaled and placed so its own full drawn extent
lands on that same box: source extent (210,214)-(949,850), centre (580,532), scaled by
538/636 = SCALE so the heights match, then centred on TARGET_CENTRE. The result lands on
x 370..996, y 132..670 -- the same vertical band as Artificer, so it clears the title text by the
same margin the reference does, with ~98px of bottom and ~132px of top slack for parallax.

Re-measure and update SRC_EXTENT_CENTRE/SCALE if the source art is ever redrawn.
"""

from pathlib import Path

from PIL import Image

ROOT = Path(__file__).resolve().parent.parent
SRC = ROOT / "src/Primal/art-source/primal spearmaster"
DEST = ROOT / "src/Primal/scenes/primal spearmaster"

SCREEN = (1366, 768)
SOURCE = 1024

SCALE = 0.846               # 538 / 636: source drawn-extent height -> Artificer's drawn-extent height
SRC_EXTENT_CENTRE = (580, 532)
TARGET_CENTRE = (683, 401)  # canvas centre horizontally; Artificer's own drawn-extent centre vertically

# Where the scaled source sits in the PNG, and so the crop box that realises it. PIL fills
# out-of-bounds area with transparent, which is exactly what the non-backdrop layers want.
_OX = round(TARGET_CENTRE[0] - SRC_EXTENT_CENTRE[0] * SCALE)
_OY = round(TARGET_CENTRE[1] - SRC_EXTENT_CENTRE[1] * SCALE)
BOX = (-_OX, -_OY, -_OX + SCREEN[0], -_OY + SCREEN[1])

DARKEN = 0.30  # locked-character "Dark" variants; alpha preserved, RGB scaled.


def layer(name):
    scaled = round(SOURCE * SCALE)
    im = Image.open(SRC / name).convert("RGBA").resize((scaled, scaled), Image.LANCZOS)
    return im.crop(BOX)


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

    # The source's black.PNG (a uniform opaque black field) is deliberately NOT used for the depth
    # layers. The menu draws its rain effect behind the portrait, so an opaque backdrop hides the
    # rain everywhere the backdrop covers -- which is what shipped first and looked wrong. Vanilla's
    # own depth layers are alpha-cut painted shapes (opaque fraction 0.14-0.72 across the stock
    # Artificer/Spearmaster scenes), transparent wherever nothing is painted, precisely so the rain
    # shows through. So the depth backdrop is just the painted foliage, keeping its own alpha.
    backdrop = foliage
    # Flat mode is the exception and matches vanilla the other way: every stock "- Flat" asset is a
    # fully opaque 1366x768 composite, so those do get the black field behind them.
    flat_base = stack(Image.new("RGBA", SCREEN, (0, 0, 0, 255)), foliage)
    scug_dark = darken(scug)

    written = {
        "background - 4": backdrop,
        "pipes - 3": poles,
        "vines - 2": vines,
        "slugcat - 1": scug,
        "slugcat - 1 - dark": scug_dark,
        "midground": stack(vines, poles),
        "foreground": stack(vines, poles, scug),
        "flat": stack(flat_base, vines, poles, scug),
        "dark flat": stack(flat_base, vines, poles, scug_dark),
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
