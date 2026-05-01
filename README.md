## About

You are a stylish blob person with big eyes and a big heart, and you've woken up deep underground with water rapidly rising.  Follow the blue orb and collect as many trapped animals as you can while keeping ahead of the rising water.

Hold logs to float; place logs for ladders.  Rough grey walls collapse when touched.  Campfires give you a powerup to briefly stop time.  The glowing butterflies, when activated, cut an straight line through the ground to the blue orb as a shortcut.

[🎮  Play on itch.io](https://uncannyforest.itch.io/aquifer-ascent)

## Implementation

Caves are generated live (and infinitely) using a modified random walk algorithm on a 3D hexagonal grid.  8 floating point parameters (width, height, slope, curviness, etc) are lerped randomly over time between 12 biome types.  Includes a cursor tracking a recommended path to ensure walkability.  Random seed is available after playing to return and replay later.

Caves have a smooth look owing to a few dozen 3D model tiles on a [dual-grid system](https://x.com/OskSta/status/1448248658865049605).  For more information see [my Hex Grid System repo](https://github.com/uncannyforest/Hex-Grid-System).

## Built with

Unity 2020.3.48f1

## Credits

Character voiced by my monk parakeet Crackers; background piano is mine.

Many thanks to [Annie](https://github.com/annie-kat) for helping me get started on this in 2020 when I was new to Unity. She helped with the initial code for glowing orbs and holding objects.

Third-party assets used: [Free Low Poly Desert Pack](https://assetstore.unity.com/packages/3d/environments/free-low-poly-desert-pack-106709),
[Low poly alien world](https://assetstore.unity.com/packages/3d/environments/low-poly-alien-world-132329),
[Simple Low Poly Nature Pack](https://assetstore.unity.com/packages/3d/environments/landscapes/simple-low-poly-nature-pack-157552),
[Simplistic Low Poly Nature](https://assetstore.unity.com/packages/3d/environments/simplistic-low-poly-nature-93894),
[Stylized Nature Kit Lite](https://assetstore.unity.com/packages/3d/environments/stylized-nature-kit-lite-176906)
