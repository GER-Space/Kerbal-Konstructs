"""
Builds the Instance editor's square icon buttons (GameData/KerbalKonstructs/Assets/instance*.png, 96 x 96, with
_hover and _down states) from the SVG glyphs in this folder, in the style of the Editor Mode buttons:
the button background of each state is taken from the editorModeSave button (its glyph painted over with the
state's fill color), the glyph is rendered like the Editor Mode glyphs (64 unit SVG at 76 px, placed at 10, 10).

Needs rsvg-convert and Pillow. Run from the repository root:  python3 Docs/Icons/InstanceEditor/make_icons.py
"""
import os
import subprocess
import tempfile

from PIL import Image

ASSETS = "GameData/KerbalKonstructs/Assets"
HERE = os.path.dirname(os.path.abspath(__file__))
STATES = ["", "_hover", "_down"]
ICONS = {
    "instanceCopyPos": "instance_copyPos.svg",
    "instancePastePos": "instance_pastePos.svg",
    "instanceSnap": "instance_snap.svg",
}
GLYPH_SIZE = 76
GLYPH_OFFSET = (10, 10)
# the area of the editorModeSave glyph inside the frame
INNER = (7, 7, 89, 89)


def background(state):
    image = Image.open(os.path.join(ASSETS, "editorModeSave" + state + ".png")).convert("RGBA")
    fill = image.getpixel((8, 48))
    pixels = image.load()
    for y in range(INNER[1], INNER[3]):
        for x in range(INNER[0], INNER[2]):
            pixels[x, y] = fill
    return image


def glyph(svg):
    with tempfile.TemporaryDirectory() as tmp:
        png = os.path.join(tmp, "glyph.png")
        subprocess.run(["rsvg-convert", "-w", str(GLYPH_SIZE), "-h", str(GLYPH_SIZE), "-o", png, os.path.join(HERE, svg)], check=True)
        return Image.open(png).convert("RGBA")


def main():
    for name, svg in ICONS.items():
        g = glyph(svg)
        for state in STATES:
            button = background(state)
            button.alpha_composite(g, GLYPH_OFFSET)
            button.save(os.path.join(ASSETS, name + state + ".png"))
            print("wrote", name + state + ".png")


if __name__ == "__main__":
    main()
