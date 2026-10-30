# Graphics and targeting

The ship uses toon shading with cool shadows and restrained edge light, darker lower hull shading, bridge windows, and aft deck markings. The recognizable silhouette and imported color palette are preserved. The original main ship geometry and import settings are restored. The shader has an explicit shadow-caster pass for WebGL.

`ShipVisualStyle` is connected in both build scenes. It shares one material per source color and releases runtime materials when destroyed. Details use ship coordinates so they follow banking and rocking.

The ocean keeps its original animated foam pattern and bright blue/turquoise cartoon palette. Its shader adds subtle distance fading for whitecaps (300–1200 units) and supports the scene haze. Close water and hull foam retain their original character.

The ship has a slightly brighter middle/shadow fill, a gentler lower-hull contrast, and restrained edge light. Sunlight is only slightly warmer, with a small reduction in shadow strength. Linear blue horizon haze starts at 350 units and reaches its full effect at 1900 units, keeping nearby combat clear. Both build scenes use the same atmosphere settings. Cloud meshes, wakes, and quality settings are retained.

## Combat fixes

Pooled bullets are moved to their spawn pose while inactive, with their trails disabled and cleared. Their first trail point is seeded at the muzzle before emission starts. Returning a bullet clears its trail; duplicate returns do not duplicate it in the queue.

Target selection updates the aim point in `Update`; firing happens in `LateUpdate` after aiming updates. Alignment is measured from the current physical muzzle instead of a cached flag. Target changes and time spent turning reset the fire interval, preventing an overdue burst. Dispersion affects projectiles rather than repeatedly moving the tracking point. Shots follow the muzzle axis, with the original near/far dispersion ranges.

## Preview and validation

Use Unity Play Mode for normal iteration. A browser preview requires a WebGL rebuild; the checked-in browser player files have not been replaced by these source changes.

**CIWS → Validate Graphics** checks both ship/ocean shader imports and both scene connections. The project targets Unity 6000.0.85f1. The Unity 6.0 migration and automated regression checks were validated against the migrated project.

`Assets/Editor/CombatRegressionChecks.cs` provides a batch-only regression runner. Run it against a disposable copy with `-batchmode -executeMethod CombatRegressionChecks.Run` and omit `-quit`; the runner exits on completion. It creates its own test scene, exercises real aiming/pooling components over multiple frames, then loads gameplay to verify a live interception. It also saves `ciws-muted-ocean.png` to the system temporary directory.

The checks passed on October 29, 2026: stale lock state with a frozen turret, overdue cooldown, alignment before resuming fire, physical bullet direction, idle/destroyed/inactive-target guards, repeated trail reuse, physics pose reset, duplicate returns, rendered trail continuity, and live gameplay interception. Game runtime errors: zero. Unity's search indexer reported an unrelated editor startup exception.

## Unity 6.0 migration

On October 29, 2026, the original Unity 6.0 settings, package pins, converted materials and texture import metadata were restored before regenerating the editor cache in 6000.0.85f1. All ship, ocean-color, tracer and turret patches were preserved byte-for-byte. The latest water palette retains the bright cartoon blue/turquoise appearance.

One editor-only dependency needs a compatibility update: `com.unity.collab-proxy` is pinned to `2.13.6`. The original `2.6.0` fails to compile in 6000.0.85f1 because `ObjectInfo` is ambiguous between UnityEditor and Codice namespaces. The updated package supports Unity 6.0, and the project compiled successfully with it. The remaining manifest pins match the original repository. Unity resolves the built-in render-pipeline packages and their transitive dependencies to versions compatible with 6000.0.85f1.

The migrated version passed ship shader/scene validation and all combat regression checks, including a live missile interception with zero game runtime errors. No browser player rebuild was performed during migration.

## Minimal visual refinement

The latest pass was previewed in Unity 6000.0.85f1 Play Mode in a disposable project copy, so the open working editor was not interrupted. Both shaders and scene connections passed validation. Close-up and low-horizon renders showed the existing polygon silhouettes, cartoon whitecaps and bright water palette with the gentler shading and atmosphere. Runtime errors: zero. Combat source hashes were checked and remained unchanged.

## Damage control gameplay

The original ship OBJ and import metadata are restored byte-for-byte. Turret bases use darker blue-gray while the upper housings retain their lighter palette.

Four modules are connected in Game.unity: 1 forward turret, 2 aft turret, 3 engines, 4 radar. Missile impacts disable their assigned module. Number keys (including keypad) or HUD cards select one repair crew. Repairs require 20 seconds of gameplay time; switching pauses accumulated progress. Offline turrets cannot fire, engine failure sets the throttle to Stop so the ship coasts down exactly as if stopped, with the throttle locked until repaired; steering still works while it has way on, and radar failure stops the sweep, contacts, direction wedge and ping audio. Repaired modules resume their normal behavior. Repairing engines leaves the throttle stopped until the player advances it. Losing all four modules ends the game, even if one of them is mid-repair; up to three modules down stays recoverable.

`Assets/Editor/ModuleRegressionChecks.cs` runs against a disposable Unity batch-mode copy with `-executeMethod ModuleRegressionChecks.Run` (omit `-quit`). It validates real missile collision damage, four unique module/turret references, engine failure coasting down under a Stop throttle and restored propulsion, offline turret firing guards, radar shutdown with active fading contacts and recovery, single-crew repair timing, selection switching/reselection, paused time, repeat damage, and three-down recovery, and game over once all four are down with one mid-repair. The 6000.0.85f1 run passed with zero runtime errors and rendered the damage-control HUD.

The existing combat regression suite also passed after the module changes, including live missile interception, alignment-before-firing, and pooled tracer continuity.

## Bottom-center module HUD and ship rocking

Module status appears at the bottom center as four separate square console keys in key order: 1 FWD CIWS, 2 AFT CIWS, 3 ENGINES, 4 RADAR. Each dark square has a monospaced `[n]` key hint top-left, a status lamp top-right, the monospaced module name in the middle, and a five-cell bar with a short status along the bottom: white `OK` with all cells lit (matching the radar), red `DOWN` on a red-tinted panel, amber seconds remaining for the active repair, or red seconds for a paused one, with cells lighting as progress builds. The active repair's name gently pulses. Hovering brightens the panel and thickens the border. Clicking a key or pressing its number assigns the crew. The keys are hidden once the game is over.

The game scene intentionally applies two-degree wave roll/pitch plus up to fifteen degrees of turn banking. Wave angles are now assigned absolutely instead of added onto the current angles, preventing cumulative rocking when engine damage bypasses the normal bank update. The module regression checks that engine-off roll stays within its configured amplitude.

## Sky and ocean polish

The sky dome now uses `CIWS/Toon Sky` (`Assets/Resources/ToonSky.mat`), which `Sky` swaps in at start in both scenes. It draws two soft toon bands from a haze horizon that matches the scene fog colour, through a light blue, up to a deeper zenith, plus a small sun disc and glow that follow the directional light. The palette dome's fogged sun, moon and star meshes are hidden; clouds are unchanged. If the material or shader is unavailable, the original scrolling palette dome is kept.

The ocean keeps its cartoon palette and foam, and adds layers that stop open water reading as one solid colour: broad slow swell patches about 160 units across (12% darker, with lighter lulls between), faint light-blue ripples about 14 units across drifting against the foam within 700 units (both sized in world space, independent of the ocean tile mapping), a faint secondary rim around each whitecap, and a tint toward the fog colour from 120 to 1400 units so the water meets the sky haze without a hard horizon line. **CIWS → Validate Graphics** also checks the sky shader.

## Missile waves

Each missile wave picks a new random bearing and spawns 1500 units from the ship in that direction (with the existing 300-unit scatter), so attacks no longer always come from the same side. The radar shows the incoming bearing as before.

## Diorama art direction

The game world (not the HUD) uses a bright, cartoony miniature look inspired by top-down rally games such as art of rally.

- **Ship:** `CIWS/Toon Ship` darkens the original gray palette to a naval tone (`_Tone`) and lights it with three high-contrast bands (deep cool blue shade, mid tone, full sun) under neutral sunlight, plus a little extra light on sky-facing faces. An optional `_Facets` per-face normal mode exists but is off, because screen-space derivatives speckle on the dense hull geometry. Below a ship-space paint line (`_WaterlineY`, 0.15; the sea sits at about -0.6) the hull is red anti-fouling paint (`_HullRed`), with a thin dark boot-top stripe (`_BootTop`) just above it, so a red band shows at the waterline like a real warship.
- **Palette:** cartoon blue water, saturated but not neon, with white foam, a light sky-blue haze for fog and the sky horizon (fog 300–1800 units), a clear blue zenith, and neutral sunlight with softer shadows in both scenes.
- **Camera:** `DioramaCamera` is added to the main camera of every scene at load (no scene wiring). It applies a tilt-shift blur that keeps a band through the middle of the screen sharp, a light grade that slightly raises contrast, and a soft cool vignette. Screen-overlay HUD canvases draw after it and stay sharp. The batch HUD screenshot routes overlays through the camera, so the HUD appears blurred only in that capture.

## Sharp bow

`ShipVisualStyle` replaces the curved plan-view bow taper with one straight wedge per side, from the shoulder (the forward 29% of the hull) to a sharp stem, on runtime copies of the hull meshes. The imported model is split into sibling meshes per colour, so the bow shape is measured across all of them and applied identically. Bow triangles get their own face normals so the wedge sides read as crisp planes. Only geometry at or below deck level is touched; the OBJ file itself is unchanged. The hull model is marked readable so this also works in player builds.
