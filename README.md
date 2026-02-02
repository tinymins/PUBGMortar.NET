# 🎯 PUBG Mortar

A Windows desktop application that helps calculate mortar distance settings in PUBG based on horizontal distance and elevation angle.

## ✨ Features

- 🖱️ Global hotkey support - works even when the game is in focus
- 📐 Automatic scale calibration using in-game map grid
- 📏 Horizontal distance measurement
- 📐 Elevation angle detection
- 🧮 Physics-based mortar distance calculation
- 🌙 Modern dark theme UI

## 🚀 Quick Start

### Requirements

- Windows 10/11
- .NET 10.0 Runtime
- **Run as Administrator** (required for global hotkeys to work with elevated game processes)

### Installation

1. Download the latest release
2. Extract to any folder
3. **Right-click → Run as Administrator**

## 🎮 How to Use

### Hotkeys

| Hotkey | Function |
|--------|----------|
| **Alt+Q** | Start measurement / Close overlay (if overlay is visible) |
| **Ctrl+Alt+Q** | Full measurement with scale reset |
| **Alt+Left Click** | Set measurement point |

### Measurement Steps

1. **Start Measurement** - Press `Alt+Q`
2. **Set Scale** (first time only) - Click two points on the 100m grid line on the map
3. **Set Your Position** - Click on your location on the map
4. **Set Target Position** - Click on the target location on the map
5. **Get Elevation Angle** - Aim at the target in-game and click
6. **Read Result** - Set your mortar to the displayed distance
7. **Close/Continue** - Press `Alt+Q` to close overlay or start next measurement

> 💡 **Tip**: After the first measurement, `Alt+Q` will skip the scale setup and use the previous calibration. Use `Ctrl+Alt+Q` if you need to recalibrate (e.g., changed map zoom level).

## 🔬 How It Works

### 1. Scale Calibration 📏

The program converts pixel distances to real game meters using the map's 100m grid:

```
ScaleFactor = 100m / PixelDistance
```

By clicking two points on a 100m grid line, we establish the conversion ratio for the current map zoom level.

### 2. Horizontal Distance Calculation 📐

Once calibrated, the horizontal distance between any two points is:

```
Distance (m) = PixelDistance × ScaleFactor
```

### 3. Elevation Angle Detection 🎯

This is the clever part! When you aim at a target in PUBG:

- **Screen Center** (Y = 719 pixels for 1440p) = **0° elevation** (looking straight)
- **Screen Top** (Y = 0) = **+26.19° elevation** (looking up)
- **Screen Bottom** = **Negative elevation** (looking down)

The formula:
```
ElevationAngle = (CenterY - ClickY) × MaxDegree / CenterY
```

This captures the height difference between you and the target.

### 4. Mortar Distance Formula 🧮

The mortar follows projectile physics. When there's an elevation difference, we need to compensate:

$$R = \frac{L + \tan(\beta) \cdot (M - \sqrt{M^2 - 2LM\tan(\beta) - L^2})}{\tan^2(\beta) + 1}$$

Where:
- **R** = Mortar distance setting (what we want)
- **L** = Horizontal distance to target
- **β** = Elevation angle
- **M** = Maximum mortar range (700m)

For targets on the same level (β = 0°), R simply equals L.

## ⚠️ Important Notes

### Resolution & FOV

The default calibration values are for:
- Resolution: **2560 × 1440**
- Default game FOV

If you use a different resolution, you may need to adjust `CenterPixelY` and `MAX_DEGREE` in the code.

### Administrator Privileges

The application **must run as Administrator** because:
- PUBG runs with elevated privileges
- Windows blocks input hooks from lower-privilege processes
- Without admin rights, hotkeys won't work when the game is focused

## 🛠️ Tech Stack

- **Framework**: .NET 10.0 / WPF
- **Pattern**: MVVM with CommunityToolkit.Mvvm
- **Hotkeys**: H.Hooks library for global keyboard/mouse hooks

## 📝 License

BSD 3-Clause License - Feel free to use and modify!

## 🤝 Contributing

Issues and PRs are welcome! If you find calibration values for other resolutions, please share them.

---

Made with ❤️ for PUBG players who want precise mortar strikes! 💥
