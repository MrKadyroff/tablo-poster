#!/usr/bin/env python3
import argparse
import json
from pathlib import Path


def load_json(path: Path):
    return json.loads(path.read_text(encoding="utf-8"))


def main() -> int:
    parser = argparse.ArgumentParser(description="Validate point layout config and image files")
    parser.add_argument("config", help="Path to point config JSON")
    args = parser.parse_args()

    cfg_path = Path(args.config)
    if not cfg_path.exists():
        print(f"ERROR: config not found: {cfg_path}")
        return 1

    cfg = load_json(cfg_path)

    errors = []
    warnings = []

    point_id = cfg.get("pointId")
    screen = cfg.get("screen", {})
    slots = cfg.get("slots", [])

    if not point_id:
        errors.append("pointId is required")

    width = screen.get("width")
    height = screen.get("height")
    if not isinstance(width, int) or width <= 0:
        errors.append("screen.width must be positive integer")
    if not isinstance(height, int) or height <= 0:
        errors.append("screen.height must be positive integer")

    content_root = cfg.get("contentRoot")
    if not content_root:
        errors.append("contentRoot is required")
        content_dir = None
    else:
        content_dir = (cfg_path.parent.parent.parent / content_root).resolve()
        if not content_dir.exists():
            warnings.append(f"contentRoot folder not found: {content_dir}")

    seen_ids = set()
    seen_order = set()
    for i, slot in enumerate(slots):
        sid = slot.get("id")
        order = slot.get("order")
        stype = slot.get("type")

        if not sid:
            errors.append(f"slots[{i}].id is required")
        elif sid in seen_ids:
            errors.append(f"duplicate slot id: {sid}")
        else:
            seen_ids.add(sid)

        if not isinstance(order, int):
            errors.append(f"slots[{i}].order must be integer")
        elif order in seen_order:
            errors.append(f"duplicate slot order: {order}")
        else:
            seen_order.add(order)

        if stype not in ("static", "rate"):
            errors.append(f"slots[{i}].type must be static|rate")

        for key in ("x", "y", "width", "height"):
            if key not in slot:
                errors.append(f"slots[{i}].{key} is required")

        if stype == "rate" and "rate" not in slot:
            errors.append(f"slots[{i}] type=rate requires 'rate' object")

        image = slot.get("image")
        if image and content_dir:
            img_path = content_dir / image
            if not img_path.exists():
                warnings.append(f"missing image file for slot '{sid}': {img_path}")

    print(f"Point: {point_id}")
    print(f"Screen: {width}x{height}")
    print(f"Slots: {len(slots)}")

    if errors:
        print("\nERRORS:")
        for e in errors:
            print(f"- {e}")

    if warnings:
        print("\nWARNINGS:")
        for w in warnings:
            print(f"- {w}")

    if not errors and not warnings:
        print("\nOK: layout config is valid and all files found.")

    return 1 if errors else 0


if __name__ == "__main__":
    raise SystemExit(main())
