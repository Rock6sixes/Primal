#!/usr/bin/env python3
"""Rebuild the Primal Artificer scene's flattened plates from its five component layers.

Unlike the Spearmaster art there is no separate art-source/ tree for Artificer -- the component
layers ARE the source, and several of the shipped files are just flattened stacks of them. That was
verified exactly, not assumed: before this script existed, each shipped plate matched a stack of
components to within rounding (max channel diff <= 16 on 0.00% of pixels).

    flat       = background-5 + vines-4 + ground + slugcat-1 + chains, over opaque black
    dark flat  = the same with the dark (locked) slugcat
    end bkg    = background-5 + vines-4
    end a      = ground + slugcat-1 + chains

WHAT THIS FIXES: the plates were previously stacked with the chains behind the slugcat and the
ground in front of it, so the ground cut across Artificer's legs and the chains were hidden behind
its arms. The correct read, per the reference artwork, is ground BEHIND the character and chains IN
FRONT of it. The ORDER list below is the single place that is expressed for the flattened plates;
the live (non-flattened) select and alt-ending scenes get the same ordering by which component file
each vanilla layer slot maps to, in PrimalArtificerScene.cs's FileMap.

Component files keep their original vanilla-derived names even though two of them no longer sit in
the slot their name suggests -- "smoke - 2" is the chains and "fg - 0" is the ground. They are just
opaque keys; FileMap is where the mapping is stated.
"""

from pathlib import Path

from PIL import Image

ROOT = Path(__file__).resolve().parent.parent
DEST = ROOT / "src/Primal/scenes/primal artificer"
PREFIX = "primal artificer "
SCREEN = (1366, 768)

BACKGROUND = "background - 5"
SPIKES = "vines - 4"
GROUND = "fg - 0"       # despite the name: the ground/grass ring, belongs BEHIND the character
SLUGCAT = "slugcat - 1"
SLUGCAT_DARK = "slugcat - 1 - dark"
CHAINS = "smoke - 2"    # despite the name: the yellow chains, belong IN FRONT of the character

# Back to front.
ORDER = [BACKGROUND, SPIKES, GROUND, SLUGCAT, CHAINS]


def load(name):
    return Image.open(DEST / f"{PREFIX}{name}.png").convert("RGBA")


def stack(names, opaque):
    out = Image.new("RGBA", SCREEN, (0, 0, 0, 255) if opaque else (0, 0, 0, 0))
    for name in names:
        out.alpha_composite(load(name))
    return out


def main():
    dark_order = [SLUGCAT_DARK if n == SLUGCAT else n for n in ORDER]
    plates = {
        # Flat-mode assets are fully opaque 1366x768 composites, matching every stock "- Flat".
        "flat": stack(ORDER, opaque=True),
        "dark flat": stack(dark_order, opaque=True),
        # Depth plates stay alpha-cut so the menu's rain shows through -- see
        # unmatte-black-backdrop.py for why that matters.
        "end bkg": stack([BACKGROUND, SPIKES], opaque=False),
        "end a": stack([GROUND, SLUGCAT, CHAINS], opaque=False),
    }
    for name, im in plates.items():
        path = DEST / f"{PREFIX}{name}.png"
        im.save(path, optimize=True)
        print(f"{path.name:<45} {im.size[0]}x{im.size[1]}")


if __name__ == "__main__":
    main()
