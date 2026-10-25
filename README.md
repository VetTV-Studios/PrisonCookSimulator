# Prison Cook Simulator

WinUI 3 / Windows App SDK kitchen-contract game for Windows 10/11. You run the line at fictional **Ironwood C.F.** Plan the menu, work every station, survive inspections and the warden tasting.

Original title. Not a licensed prison or cooking brand.

## Modes

- **Kitchen contract** — 8 weeks, 7 days each. Buy crates, pick two recipes, scrub the floor, buy upgrades, then open the line.
- **Rush line** — no budget. Plate until three trays die. Session best score is tracked.

## The line

Tickets demand stations in order: **Prep → Range → Fryer → Steam → Window**. Timing and matching trustees raise quality. Wrong station dumps the tray. **Pit / scrub** buys hygiene and burns ticket time. Hygiene under 45 on an inspection day pulls the contract. Morale at 0 does the same.

Flags:
- Wednesday often brings the inspector
- Thursday can freeze a trustee on the block
- Sunday (and some Fridays) the warden tastes

## Build

1. Windows 10 1809+ or Windows 11
2. Visual Studio 2022/2026 with **.NET desktop development** and **Windows application development (WinUI)**
3. .NET 8 SDK
4. Open `PrisonCookSimulator.sln`, set platform to **x64**, F5

If packaging identity complains, set this in the `.csproj` for unpackaged debug:

```xml
<WindowsPackageType>None</WindowsPackageType>
```

## Controls

Keys **1–5** hit Prep, Range, Fryer, Steam, Window. **P** or **Esc** pauses and resumes the line.

**Settings** (main menu): music volume, SFX volume, mute, difficulty (Easy 0.75× / Normal / Hard 1.3× ticket clock), fullscreen. Stored in save.json.

Save file: `%LocalAppData%\VetTV\PrisonCookSimulator\save.json` (Rush best score and contract progress). Under MSIX, Windows redirects this path into the package's LocalCache folder.

## itch.io build (unpackaged, self-contained)

```powershell
cd PrisonCookSimulator
dotnet publish -c Release -p:Platform=x64   -p:Unpackaged=true -p:PublishProfile=win-x64
dotnet publish -c Release -p:Platform=ARM64 -p:Unpackaged=true -p:PublishProfile=win-arm64
```

Output goes to `bin\publish\itch-win-x64\` and `bin\publish\itch-win-arm64\`. Zip each folder and upload it to itch.io (or `butler push`).

## Store packaging

Identity in `Package.appxmanifest` is `Studio27.PrisonCookSimulator` / `CN=Studio27`. Replace with your Partner Center reservation before upload.

Logos live in `PrisonCookSimulator/Assets/` (AppIcon.ico is the exe/window icon):

- StoreLogo.png (50×50)
- Square44x44Logo.png, Square71x71Logo.png, Square150x150Logo.png, Square310x310Logo.png
- Wide310x150Logo.png
- SplashScreen.png (620×300)

Suggested Store category: **Simulation**.

## Publisher

VetTV Studios — https://github.com/VetTV-Studios
