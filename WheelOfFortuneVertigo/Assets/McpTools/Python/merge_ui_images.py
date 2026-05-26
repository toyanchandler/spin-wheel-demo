#!/usr/bin/env python3
import json
import sys
from pathlib import Path

from PIL import Image


def unity_sprite_crop(source, rect):
    image = Image.open(source).convert("RGBA")
    x = int(round(rect["x"]))
    y = int(round(rect["y"]))
    width = int(round(rect["width"]))
    height = int(round(rect["height"]))
    top = image.height - y - height
    return image.crop((x, top, x + width, top + height))


def tint(image, color):
    r, g, b, a = color
    red, green, blue, alpha = image.split()
    red = red.point(lambda v: int(v * r))
    green = green.point(lambda v: int(v * g))
    blue = blue.point(lambda v: int(v * b))
    alpha = alpha.point(lambda v: int(v * a))
    return Image.merge("RGBA", (red, green, blue, alpha))


def main():
    if len(sys.argv) != 2:
        raise SystemExit("Usage: merge_ui_images.py manifest.json")

    manifest_path = Path(sys.argv[1])
    manifest = json.loads(manifest_path.read_text(encoding="utf-8-sig"))
    output_width = int(manifest["width"])
    output_height = int(manifest["height"])
    canvas = Image.new("RGBA", (output_width, output_height), (0, 0, 0, 0))

    for layer in manifest["layers"]:
        target = layer["targetRect"]
        width = max(1, int(round(target["width"])))
        height = max(1, int(round(target["height"])))
        if layer.get("solid"):
            r, g, b, a = layer["color"]
            sprite = Image.new(
                "RGBA",
                (width, height),
                (
                    int(round(r * 255)),
                    int(round(g * 255)),
                    int(round(b * 255)),
                    int(round(a * 255)),
                ),
            )
        else:
            sprite = unity_sprite_crop(layer["source"], layer["textureRect"])
            sprite = sprite.resize((width, height), Image.Resampling.LANCZOS)
            sprite = tint(sprite, layer["color"])
        canvas.alpha_composite(sprite, (int(round(target["x"])), int(round(target["y"]))))

    output = Path(manifest["output"])
    output.parent.mkdir(parents=True, exist_ok=True)
    canvas.save(output)
    print(json.dumps({"output": str(output), "size": [output_width, output_height], "layers": len(manifest["layers"])}))


if __name__ == "__main__":
    main()
