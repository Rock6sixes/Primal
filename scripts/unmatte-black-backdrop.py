#!/usr/bin/env python3
"""Turn a portrait depth layer's flat black field into transparency, in place.

WHY: the character-select menu draws its rain effect *behind* the portrait scene, so any depth
layer that is a fully opaque rectangle hides the rain everywhere it covers -- the rain then only
appears outside the layer, which is the bug this fixes. Vanilla's own depth layers are alpha-cut
painted shapes (opaque fraction 0.14-0.72 across the stock Artificer/Spearmaster scenes),
transparent wherever nothing is painted. Flat-mode "- Flat" assets are the opposite and are
correctly fully opaque -- do not run this on those.

The Primal Spearmaster art is generated from layered sources, so it just omits its black layer
(see build-spearmaster-scene.py). The Primal Artificer art has NO source layers in this repo --
only the flattened PNGs -- so its black field has to be keyed out of the finished image instead,
and this script is the only record of how that was done. It is idempotent: pixels already
transparent stay transparent, and their RGB is already black.

HOW: alpha is ramped on luminance between LO and HI. Those bounds are deliberately tight. The
backdrop is a genuinely flat black field (76% of pixels sit at luminance <= 2, and only 0.8% more
appear by luminance 5), so a low cut separates "field" from "paint" cleanly, while anything the
artist actually painted -- including dark cave walls and shadows at luminance 12+ -- stays fully
opaque and keeps the original look. The short ramp preserves the soft brush edges where the
painting fades into the field, rather than cutting a hard outline around it.
"""

import sys
from pathlib import Path

import numpy as np
from PIL import Image

ROOT = Path(__file__).resolve().parent.parent

DEFAULT_TARGETS = [
    ROOT / "src/Primal/scenes/primal artificer/primal artificer background - 5.png",
    ROOT / "src/Primal/scenes/primal artificer/primal artificer end bkg.png",
]

LO, HI = 2.0, 12.0


def unmatte(path):
    im = Image.open(path).convert("RGBA")
    a = np.asarray(im).astype(np.float32)
    lum = a[:, :, :3].max(axis=2)
    ramp = np.clip((lum - LO) / (HI - LO), 0.0, 1.0)
    a[:, :, 3] *= ramp
    out = Image.fromarray(a.round().clip(0, 255).astype(np.uint8), "RGBA")
    out.save(path, optimize=True)
    return (np.asarray(out)[:, :, 3] > 250).mean()


def main():
    targets = [Path(p) for p in sys.argv[1:]] or DEFAULT_TARGETS
    for path in targets:
        before = (np.asarray(Image.open(path).convert("RGBA"))[:, :, 3] > 250).mean()
        after = unmatte(path)
        print(f"{path.name:<45} opaque-frac {before:.2f} -> {after:.2f}")


if __name__ == "__main__":
    main()
