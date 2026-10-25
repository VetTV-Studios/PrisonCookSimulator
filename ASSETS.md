# Asset provenance

All audio and art in `PrisonCookSimulator/Assets/Audio` and `PrisonCookSimulator/Assets/Art` are original, procedurally generated for this project by VetTV Studios (no samples, fonts-as-art, or third-party packs). Regenerate from the repo root with:

```
python3 tools/gen_audio.py   # numpy -> 22.05 kHz mono 16-bit WAV
python3 tools/gen_art.py     # Pillow -> 128x128 PNG icons
```

- Audio: click, tick, station_hit, ruined, purchase, service_end, music_menu (loop, ~23 s), music_line (loop, ~22 s)
- Art: station_{prep,range,fryer,steam,window}, recipe_{joe,mac,loaf,beans,king,hash,fish,veg,stew,warden}, ing_{beef,chicken,beans,pasta,bread,veg,potato,sauce,spice,egg,fish,prime}
