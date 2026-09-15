"""Draws the 256x256 Thunderstore icon.

Thunderstore rejects anything that is not exactly 256x256 PNG, and there is no art
asset to crop from, so the icon is drawn: rain stopping dead on a plank floor
overhead, and a fire burning on the plank floor below it. One picture, both features.

    python tools/make_icon.py

`scene()` is also imported by make_banner.py so the Nexus images are the same drawing
at a different shape rather than a second piece of art to keep in step.
"""
import os

from PIL import Image, ImageDraw, ImageFilter

HERE = os.path.dirname(os.path.abspath(__file__))
OUT = os.path.join(os.path.dirname(HERE), "package", "icon.png")

SIZE = 256
SCALE = 4  # drawn large and downsampled, so the edges are not stair-stepped

BACKGROUND = (38, 34, 30, 255)
PLANK = (122, 88, 56, 255)
PLANK_DARK = (92, 65, 40, 255)
PLANK_EDGE = (46, 33, 21, 255)
RAIN = (138, 178, 206, 255)
FLAME_OUTER = (214, 96, 28, 255)
FLAME_INNER = (247, 191, 66, 255)
EMBER = (120, 60, 26, 255)


def planks(draw, s, top, height, seams):
    """A run of floorboards seen edge-on: one slab, darker seams seen as gaps."""
    left, right = 16 * s, (SIZE - 16) * s
    draw.rectangle([left, top, right, top + height], fill=PLANK, outline=PLANK_EDGE,
                   width=int(3 * s))
    draw.rectangle([left, top, right, top + height * 0.28], fill=PLANK_DARK)
    span = (right - left) / (seams + 1)
    for i in range(1, seams + 1):
        x = left + span * i
        draw.line([(x, top), (x, top + height)], fill=PLANK_EDGE, width=int(3 * s))


def flame(draw, cx, base, width, height, colour):
    """A teardrop: wide round belly at the bottom pinched to a point at the top."""
    points = []
    steps = 60
    for side in (1, -1):
        edge = []
        for i in range(steps + 1):
            t = i / steps                      # 0 at the base, 1 at the tip
            w = (width / 2) * (1 - t) ** 0.7 * (1 + 0.55 * (1 - t) * t * 4)
            edge.append((cx + side * w, base - t * height))
        points += edge if side == 1 else list(reversed(edge))
    draw.polygon(points, fill=colour)


def scene(px, background=BACKGROUND):
    """The whole picture at px by px. Everything is measured in 256ths, so the
    drawing is the same at any size."""
    s = px / SIZE
    image = Image.new("RGBA", (px, px), background)
    draw = ImageDraw.Draw(image)

    # Rain, falling into the top third and stopping on the floor above - the whole
    # point of the mod is that it does not get through.
    for x, length in ((44, 46), (78, 62), (112, 40), (150, 58), (186, 44), (214, 60)):
        y0 = (20 + (x % 17)) * s
        draw.line([(x * s, y0), (x * s - 9 * s, y0 + length * s)], fill=RAIN, width=int(5 * s))

    planks(draw, s, 104 * s, 30 * s, 5)        # the floor overhead; rain lands on this

    # Splash: rain breaking sideways where it hits, so the stop reads as a stop.
    for x in (60, 120, 190):
        draw.line([(x * s, 102 * s), (x * s + 11 * s, 95 * s)], fill=RAIN, width=int(4 * s))
        draw.line([(x * s, 102 * s), (x * s - 11 * s, 95 * s)], fill=RAIN, width=int(4 * s))

    planks(draw, s, 206 * s, 30 * s, 5)        # the floor the fire stands on

    glow = Image.new("RGBA", image.size, (0, 0, 0, 0))
    ImageDraw.Draw(glow).ellipse([96 * s, 168 * s, 160 * s, 216 * s], fill=(196, 92, 24, 90))
    image.alpha_composite(glow.filter(ImageFilter.GaussianBlur(14 * s)))

    draw = ImageDraw.Draw(image)
    draw.ellipse([100 * s, 198 * s, 156 * s, 212 * s], fill=EMBER)
    flame(draw, 128 * s, 206 * s, 54 * s, 62 * s, FLAME_OUTER)
    flame(draw, 128 * s, 204 * s, 28 * s, 36 * s, FLAME_INNER)
    return image


if __name__ == "__main__":
    scene(SIZE * SCALE).resize((SIZE, SIZE), Image.LANCZOS).save(OUT)
    print("wrote", OUT)
