# Point Setup Guide

This folder is your control center for each screen point.

## 1) Create or copy point config
- Use `layout/points/point-001.json` as template.
- Set real `screen.width` and `screen.height`.
- Add all image slots in `slots`.

## 2) Define slots
For each image block on screen, set:
- `id`: unique slot name
- `order`: display order (0,1,2...)
- `type`: `static` or `rate`
- `image`: file name from point image folder
- `x`, `y`, `width`, `height`: slot position/size on screen

For `type=rate`, also set text boxes:
- `rate.priceText` for main value
- `rate.changeText` for delta value

## 3) Put images to point folder
For point-001:
- `content/points/point-001/images`

## 4) Validate before run
```bash
cd /Users/mr.kadyroff/Documents/led/LedshowYQ/LedImageUpdaterService
python3 layout/tools/validate_layout.py layout/points/point-001.json
```

## 5) Recommended workflow on a new point
1. Set screen size.
2. Add 3 slots (or more) with rough coordinates.
3. Put background images without numbers for rate slots.
4. Validate layout.
5. Fine-tune coordinates and text box rectangles.

## 6) Quick coordinate source from SVG
Use existing helper:
```bash
python3 svg_rect_coords.py image.svg --list
```

Copy needed numbers to slot `x/y/width/height`.
