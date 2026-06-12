#!/usr/bin/env python3
import argparse
import io
import json
import subprocess
import tempfile
from pathlib import Path
from typing import Dict, Optional

SUPPORTED = [".jpg", ".jpeg", ".png", ".webp", ".bmp", ".svg"]


def load_rates_json(path: Optional[Path]) -> Optional[dict]:
    if path is None:
        return None
    if not path.exists():
        raise SystemExit(f"Rates json not found: {path}")

    data = json.loads(path.read_text(encoding="utf-8"))
    usd = data.get("USD", {})
    eur = data.get("EUR", {})

    return {
        "usd_buy": str(usd.get("buy", "")),
        "usd_sell": str(usd.get("sell", "")),
        "eur_buy": str(eur.get("buy", "")),
        "eur_sell": str(eur.get("sell", "")),
    }


def find_ordered_images(source_dir: Path) -> Dict[int, Path]:
    found: Dict[int, Path] = {}
    for n in (1, 2, 3):
        for ext in SUPPORTED:
            p = source_dir / f"{n}{ext}"
            if p.exists():
                found[n] = p
                break
    return found


def _list_candidate_images(source_dir: Path) -> list[Path]:
    files: list[Path] = []
    for p in source_dir.iterdir():
        if not p.is_file():
            continue
        if p.name.lower() == "required_files.txt":
            continue
        if p.suffix.lower() in SUPPORTED:
            files.append(p)
    return files


def pick_input_order(source_dir: Path, preferred_files: Optional[Dict[int, str]] = None) -> Dict[int, Path]:
    numbered = find_ordered_images(source_dir)
    if all(i in numbered for i in (1, 2, 3)):
        return numbered

    if preferred_files:
        preferred_map: Dict[int, Path] = {}
        ok = True
        for i in (1, 2, 3):
            name = preferred_files.get(i)
            if not name:
                ok = False
                break
            p = source_dir / name
            if not p.exists():
                ok = False
                break
            preferred_map[i] = p
        if ok:
            return preferred_map

    candidates = _list_candidate_images(source_dir)
    if len(candidates) < 3:
        raise SystemExit(f"Need at least 3 input images in {source_dir}. Found: {len(candidates)}")

    # Fallback: first 3 by upload/mtime order.
    candidates.sort(key=lambda p: (p.stat().st_mtime, p.name.lower()))
    return {1: candidates[0], 2: candidates[1], 3: candidates[2]}


def _has_cmd(cmd: str) -> bool:
    from shutil import which
    return which(cmd) is not None


def _load_svg_as_image(svg_path: Path):
    Image, _, _, _, _ = _load_pillow()
    with tempfile.NamedTemporaryFile(suffix=".png", delete=False) as tmp:
        tmp_png = Path(tmp.name)

    try:
        # Prefer Inkscape on Windows and cross-platform environments.
        if _has_cmd("inkscape"):
            cmd = [
                "inkscape",
                str(svg_path),
                "--export-type=png",
                f"--export-filename={tmp_png}",
            ]
            subprocess.run(cmd, check=True, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
            return Image.open(tmp_png).convert("RGBA")

        # Optional fallback for Linux/macOS if available.
        if _has_cmd("rsvg-convert"):
            cmd = ["rsvg-convert", str(svg_path), "-o", str(tmp_png)]
            subprocess.run(cmd, check=True, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
            return Image.open(tmp_png).convert("RGBA")

        # macOS fallback.
        if _has_cmd("sips"):
            cmd = ["sips", "-s", "format", "png", str(svg_path), "--out", str(tmp_png)]
            subprocess.run(cmd, check=True, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
            return Image.open(tmp_png).convert("RGBA")

        raise SystemExit(
            "SVG input detected, but no converter found. Install Inkscape and add it to PATH, "
            "or convert SVGs to JPG/PNG before compose."
        )
    finally:
        if tmp_png.exists():
            try:
                tmp_png.unlink()
            except OSError:
                pass


def load_image_any(path: Path):
    ext = path.suffix.lower()
    if ext == ".svg":
        return _load_svg_as_image(path)

    Image, _, _ = _load_pillow()
    return Image.open(path).convert("RGBA")


def _load_pillow():
    try:
        from PIL import Image, ImageOps, ImageColor, ImageDraw, ImageFont
        return Image, ImageOps, ImageColor, ImageDraw, ImageFont
    except ModuleNotFoundError as exc:
        raise SystemExit(
            "Missing dependency Pillow. Install with: pip3 install -r layout/tools/requirements.txt"
        ) from exc


def _format_rate_value(value: Optional[object]) -> str:
    if value is None:
        return ""
    text = str(value).strip()
    if not text:
        return ""
    try:
        return f"{float(text):.2f}"
    except ValueError:
        return text


def render_fit(img, w: int, h: int, fit: str):
    Image, ImageOps, _, _, _ = _load_pillow()
    if fit == "contain":
        out = Image.new("RGBA", (w, h), (0, 0, 0, 0))
        resized = ImageOps.contain(img, (w, h), Image.Resampling.LANCZOS)
        x = (w - resized.width) // 2
        y = (h - resized.height) // 2
        out.paste(resized, (x, y), resized if resized.mode == "RGBA" else None)
        return out
    if fit == "fill":
        return img.resize((w, h), Image.Resampling.LANCZOS)
    return ImageOps.fit(img, (w, h), method=Image.Resampling.LANCZOS)


def _workspace_root() -> Path:
    return Path(__file__).resolve().parents[2]


def auto_build_config(point: str, canvas_w: int, canvas_h: int, preferred_files: Optional[Dict[int, str]] = None) -> dict:
    root = _workspace_root()
    source_dir_rel = Path("content") / "points" / point / "images"
    output_rel = Path("content") / "points" / point / "output" / "final.jpg"
    source_dir = root / source_dir_rel

    found = pick_input_order(source_dir, preferred_files)

    raw_widths = []
    for i in (1, 2, 3):
        img = load_image_any(found[i])
        try:
            if img.height <= 0:
                raw_widths.append(1.0)
            else:
                raw_widths.append((img.width * canvas_h) / img.height)
        finally:
            img.close()

    total = sum(raw_widths)
    if total <= 0:
        raw_widths = [1.0, 1.0, 1.0]
        total = 3.0

    scale = canvas_w / total
    widths = [max(1, int(round(w * scale))) for w in raw_widths]
    diff = canvas_w - sum(widths)
    widths[-1] += diff
    if widths[-1] < 1:
        widths[-1] = 1

    x = 0
    slots = []
    for idx, order in enumerate((1, 2, 3)):
        w = widths[idx]
        if idx == 2:
            w = canvas_w - x

        slots.append({
            "order": order,
            "file": found[order].name,
            "x": x,
            "y": 0,
            "width": w,
            "height": canvas_h,
            "fit": "fill",
        })
        x += w

    return {
        "pointId": point,
        "canvas": {
            "width": canvas_w,
            "height": canvas_h,
            "background": "#000000",
        },
        "images": slots,
        "sourceDir": str(source_dir_rel).replace("\\", "/"),
        "outputFile": str(output_rel).replace("\\", "/"),
    }


def preferred_files_from_config(config_path: Path) -> Dict[int, str]:
    if not config_path.exists():
        return {}

    cfg = json.loads(config_path.read_text(encoding="utf-8"))
    result: Dict[int, str] = {}
    for slot in cfg.get("images", []):
        try:
            order = int(slot.get("order"))
        except Exception:
            continue
        if order in (1, 2, 3) and slot.get("file"):
            result[order] = str(slot["file"])
    return result


def _draw_rate_overlay(canvas, cfg: dict, rate1: Optional[str], rate2: Optional[str], rates: Optional[dict]) -> None:
    if not rate1 and not rate2 and not rates:
        return

    layout = cfg.get("ratesLayout")
    if not isinstance(layout, dict):
        return

    _, _, ImageColor, ImageDraw, ImageFont = _load_pillow()
    draw = ImageDraw.Draw(canvas)

    if rates:
        values_by_key = {
            "usd": {
                "buy": _format_rate_value(rates.get("usd_buy")),
                "sell": _format_rate_value(rates.get("usd_sell")),
            },
            "eur": {
                "buy": _format_rate_value(rates.get("eur_buy")),
                "sell": _format_rate_value(rates.get("eur_sell")),
            },
        }
    else:
        values_by_key = {
            "usd": {
                "buy": _format_rate_value(rate1),
                "sell": "",
            },
            "eur": {
                "buy": _format_rate_value(rate2),
                "sell": "",
            },
        }

    blocks = layout.get("blocks", [])
    for block in blocks:
        key = str(block.get("key", "")).strip().lower()
        field_values = values_by_key.get(key)
        if not field_values:
            continue

        # Clear only strict numeric field areas to preserve labels, flags and decorative elements.
        for clear_rect in block.get("clearRects", []):
            x = int(clear_rect.get("x", 0))
            y = int(clear_rect.get("y", 0))
            w = int(clear_rect.get("width", 0))
            h = int(clear_rect.get("height", 0))
            if w <= 0 or h <= 0:
                continue
            fill = clear_rect.get("fill", "#000000")
            draw.rectangle([x, y, x + w, y + h], fill=ImageColor.getrgb(fill))

        for field in block.get("fields", []):
            field_name = str(field.get("name", "")).strip().lower()
            value_text = field_values.get(field_name, "")
            if not value_text:
                continue

            x = int(field.get("x", 0))
            y = int(field.get("y", 0))
            color = field.get("color", "#FFFFFF")
            font_size = int(field.get("fontSize", 16))
            font_name = field.get("font", "arial.ttf")
            align = str(field.get("align", "left")).strip().lower()

            try:
                font = ImageFont.truetype(font_name, font_size)
            except Exception:
                font = ImageFont.load_default()

            text_x = x
            if align in ("right", "center"):
                box_w = int(field.get("width", 0))
                if box_w > 0:
                    bbox = draw.textbbox((0, 0), value_text, font=font)
                    tw = bbox[2] - bbox[0]
                    if align == "right":
                        text_x = x + max(0, box_w - tw)
                    else:
                        text_x = x + max(0, (box_w - tw) // 2)

            draw.text((text_x, y), value_text, fill=ImageColor.getrgb(color), font=font)


def compose_from_config_data(cfg: dict, output_override: Optional[Path], rate1: Optional[str], rate2: Optional[str], rates: Optional[dict]) -> Path:
    Image, _, ImageColor, _, _ = _load_pillow()
    root = _workspace_root()

    source_dir = (root / cfg["sourceDir"]).resolve()
    output_file = (root / cfg["outputFile"]).resolve()
    if output_override:
        output_file = output_override

    canvas_w = int(cfg["canvas"]["width"])
    canvas_h = int(cfg["canvas"]["height"])
    bg = cfg["canvas"].get("background", "#000000")
    bg_rgba = ImageColor.getrgb(bg) + (255,)

    found = find_ordered_images(source_dir)

    canvas = Image.new("RGBA", (canvas_w, canvas_h), bg_rgba)

    slots = sorted(cfg["images"], key=lambda s: int(s["order"]))
    for slot in slots:
        order = int(slot["order"])
        if "file" in slot and slot["file"]:
            src = source_dir / slot["file"]
            if not src.exists() and order in found:
                src = found[order]
        else:
            if order not in found:
                raise SystemExit(f"Missing input image for order {order} in {source_dir}")
            src = found[order]

        if not src.exists():
            raise SystemExit(f"Missing input file: {src}")

        x = int(slot["x"])
        y = int(slot["y"])
        w = int(slot["width"])
        h = int(slot["height"])
        fit = slot.get("fit", "cover")

        img = load_image_any(src)
        try:
            prepared = render_fit(img, w, h, fit)
            canvas.paste(prepared, (x, y), prepared)
        finally:
            img.close()

    _draw_rate_overlay(canvas, cfg, rate1, rate2, rates)

    output_file.parent.mkdir(parents=True, exist_ok=True)
    canvas.convert("RGB").save(output_file, format="JPEG", quality=92, optimize=True)
    return output_file


def compose_from_config(config_path: Path, width_override: Optional[int], height_override: Optional[int], output_override: Optional[Path], rate1: Optional[str], rate2: Optional[str], rates: Optional[dict]) -> Path:
    cfg = json.loads(config_path.read_text(encoding="utf-8"))
    if width_override:
        cfg["canvas"]["width"] = int(width_override)
    if height_override:
        cfg["canvas"]["height"] = int(height_override)
    return compose_from_config_data(cfg, output_override, rate1, rate2, rates)


def main() -> None:
    parser = argparse.ArgumentParser(description="Compose 1/2/3 images into one screen canvas")
    parser.add_argument("--config", default="layout/points/megapark.compose.json", help="Path to compose config")
    parser.add_argument("--point", default="megapark", help="Point id for auto mode (content/points/<point>/images)")
    parser.add_argument("--auto-layout", action="store_true", help="Auto-place 3 ordered images into full screen width")
    parser.add_argument("--save-generated-config", action="store_true", help="Save auto-generated config to layout/points/<point>.compose.generated.json")
    parser.add_argument("--width", type=int, help="Override canvas width")
    parser.add_argument("--height", type=int, help="Override canvas height")
    parser.add_argument("--output", help="Override output file path")
    parser.add_argument("--rate1", help="Optional test text for first rate block")
    parser.add_argument("--rate2", help="Optional test text for second rate block")
    parser.add_argument("--rates-json", help="JSON with USD/EUR buy/sell values")
    args = parser.parse_args()

    output_override = Path(args.output).resolve() if args.output else None

    rates = load_rates_json(Path(args.rates_json).resolve()) if args.rates_json else None

    if args.auto_layout:
        if not args.width or not args.height:
            raise SystemExit("Auto mode requires --width and --height")

        config_path = Path(args.config).resolve()
        preferred = preferred_files_from_config(config_path)
        cfg = auto_build_config(args.point, int(args.width), int(args.height), preferred)
        if args.save_generated_config:
            gen_cfg = _workspace_root() / "layout" / "points" / f"{args.point}.compose.generated.json"
            gen_cfg.write_text(json.dumps(cfg, indent=2, ensure_ascii=True) + "\n", encoding="utf-8")
            print(f"Generated config saved: {gen_cfg}")

        out = compose_from_config_data(cfg, output_override, args.rate1, args.rate2, rates)
    else:
        config_path = Path(args.config).resolve()
        out = compose_from_config(config_path, args.width, args.height, output_override, args.rate1, args.rate2, rates)

    print(f"Composed image saved: {out}")


if __name__ == "__main__":
    main()
