"""Draws the Nexus images: a 1300x372 page header and a 1920x1080 gallery shot.

Nexus wants a wide header and a 16:9 gallery image, not the square Thunderstore icon,
so the icon drawing is placed square on the left with the title beside it. Both sizes
come out of one renderer with everything measured as a fraction of the height, so the
two stay the same picture at different shapes.

    python tools/make_banner.py
"""
import os
import sys

from PIL import Image, ImageDraw, ImageFont

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
from make_icon import BACKGROUND, FLAME_INNER, scene  # noqa: E402

MEDIA = os.path.join(os.path.dirname(HERE), "media")
SCALE = 2


def font(path, size):
    try:
        return ImageFont.truetype(path, size)
    except OSError:
        return ImageFont.load_default()


def render(w, h, out):
    image = Image.new("RGBA", (w * SCALE, h * SCALE), BACKGROUND)

    art = scene(int(h * SCALE * 0.86), background=(0, 0, 0, 0))
    image.alpha_composite(art, (int(h * SCALE * 0.09), int(h * SCALE * 0.07)))

    draw = ImageDraw.Draw(image)
    title = font("C:/Windows/Fonts/segoeuib.ttf", int(h * SCALE * 0.103))
    sub = font("C:/Windows/Fonts/segoeui.ttf", int(h * SCALE * 0.047))

    x = int(h * SCALE * 1.06)
    draw.text((x, int(h * SCALE * 0.36)), "Cartur's", font=title, fill=(238, 232, 220, 255))
    draw.text((x, int(h * SCALE * 0.47)), "Flooring", font=title, fill=FLAME_INNER)
    draw.text((x, int(h * SCALE * 0.61)), "Floors keep the rain off. Fires sit on them.",
              font=sub, fill=(150, 140, 124, 255))

    image.resize((w, h), Image.LANCZOS).convert("RGB").save(out)
    print("wrote", out, (w, h))


render(1300, 372, os.path.join(MEDIA, "nexus-header.png"))
render(1920, 1080, os.path.join(MEDIA, "nexus-gallery.png"))
