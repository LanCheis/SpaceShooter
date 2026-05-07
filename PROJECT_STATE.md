# PROJECT_STATE — Space Shooter

Unity 2D space shooter. Core player systems are implemented in code but several require Inspector wiring before they function in-game. **~20% complete** — movement, shooting, weapon levels, and pickup logic exist as scripts; enemies, UI, audio, and game loop are entirely absent.

- **Engine:** Unity 2022.3.47f1 (LTS)
- **Working directory:** `w:\Space Shooter`
- **Render mode:** 2D, orthographic (size 5), Main Camera at default position
- **Scenes:** 1 — [Assets/Scenes/SampleScene.unity](Assets/Scenes/SampleScene.unity)
- **Scripts (game):** 3 — PlayerController, Bullet, PowerUp
- **Source control:** Not a git repository — highest-priority risk

---

## COMPLETED FEATURES

### Player movement
File: [Assets/Space Shooter Assets/Sprites/Player/PlayerController.cs](Assets/Space%20Shooter%20Assets/Sprites/Player/PlayerController.cs)

- Reads `GetAxisRaw` Horizontal/Vertical in `Update()`.
- Sets `rb.linearVelocity = moveInput.normalized * moveSpeed` in `FixedUpdate()` (direct velocity, not force-based). `moveSpeed` default 12.
- Screen clamping: viewport-percent clamp (5%–95%) applied to position every `FixedUpdate()` via `mainCam.WorldToViewportPoint` / `ViewportToWorldPoint`.
- `transform.rotation = Quaternion.identity` — no tilt; rotation is locked flat every frame.
- Rigidbody2D configured at runtime in `Start()`: `gravityScale=0`, `linearDamping=0`, interpolation on, `freezeRotation=true`.

### Weapon system (code only — needs Inspector wiring)
File: [Assets/Space Shooter Assets/Sprites/Player/PlayerController.cs](Assets/Space%20Shooter%20Assets/Sprites/Player/PlayerController.cs)

- `WeaponType` enum: `Neutron` (green, wide-short bullet) or `Laser` (red, tall-thin bullet).
- `weaponLevel` (1–3) controls spread pattern, Chicken Invaders style:
  - Level 1: single center shot
  - Level 2: two parallel shots (±0.2 offset)
  - Level 3: three spread shots (center + ±15° angle)
- Fire input: hold **J** key; cooldown via `fireRate` (default 0.2s).
- `CreateBullet()` instantiates `bulletPrefab` at `firePointCenter.position`, sets `SpriteRenderer.color` and scales the bullet shape by weapon type.
- `UpgradeWeapon(WeaponType)`: if same type, increments level (max 3); if different type, switches and resets to level 1.
- **Requires wiring:** `bulletPrefab` and `firePointCenter` must be assigned in Inspector or shooting silently does nothing.

### Engine particle effects (code only — needs Inspector wiring)
File: [Assets/Space Shooter Assets/Sprites/Player/PlayerController.cs](Assets/Space%20Shooter%20Assets/Sprites/Player/PlayerController.cs)

- `ParticleSystem[] engines` array (assign 6 particle systems via Inspector).
- `startSize` pulses: 1.5 while moving, 0.7 while idle.
- **Requires wiring:** `engines` array must be populated in Inspector.

### Bullet script
File: [Assets/Space Shooter Assets/Sprites/Player/Bullet.cs](Assets/Space%20Shooter%20Assets/Sprites/Player/Bullet.cs)

- On `Start()`: gets `Rigidbody2D`, sets `rb.linearVelocity = transform.up * speed` (default 20).
- Auto-destroys after 3 seconds via `Destroy(gameObject, 3f)`.
- **Missing:** no `OnTriggerEnter2D` — bullets pass through everything; collision with enemies not implemented.

### PowerUp script
File: [Assets/Space Shooter Assets/Sprites/Player/PowerUp.cs](Assets/Space%20Shooter%20Assets/Sprites/Player/PowerUp.cs)

- `OnTriggerEnter2D`: detects `"Player"` tag, calls `player.UpgradeWeapon(weaponType)`, destroys itself.
- Fully self-contained; just needs a prefab with `CircleCollider2D (isTrigger)` and this script in the scene.

---

## IN-PROGRESS / NEEDS WIRING

- **Bullet prefab** — `Bullet.cs` exists but no `.prefab` file. Must be created in the Unity Editor: add a sprite from [Sprites/Projectiles/](Assets/Space%20Shooter%20Assets/Sprites/Projectiles/), `Rigidbody2D` (gravity 0), `CapsuleCollider2D (isTrigger)`, and the `Bullet.cs` script. Assign to `PlayerController.bulletPrefab`.
- **Fire point Transform** — an empty child GameObject at the tip of the ship must be created and assigned to `PlayerController.firePointCenter`.
- **Engine particle systems** — six `ParticleSystem` child objects (use `EngineFireParticle.prefab` from [Prefabs/Effects/](Assets/Space%20Shooter%20Assets/Prefabs/Effects/)) need to be added to the Player and assigned in the `engines` array.

---

## NOT YET STARTED (implied by asset library)

- **Enemy system:** `Enemy.cs`, HP, downward movement, `OnTriggerEnter2D` death → explosion. Sprites ready in [Sprites/Enemys/](Assets/Space%20Shooter%20Assets/Sprites/Enemys/).
- **EnemySpawner:** timed spawner at random X above screen top.
- **GameManager:** singleton for player HP, score, game-over flow.
- **HUD/UI:** Canvas with score and HP using [Sprites/GUI/](Assets/Space%20Shooter%20Assets/Sprites/GUI/) atlas. No Canvas exists in the scene.
- **AudioManager:** BGM via `DST-TowerDefenseTheme.mp3`, SFX for shoot/explode/hit. No `AudioSource` in scene.
- **Background:** starfield `background-1156435.png` and parallax `BG Obj/` sprites unused.
- **Sorting layers:** none configured — needed before parallax, projectiles, and UI are added.
- **Obstacles:** meteor sprites and prefabs ready but no spawner or script.
- **Pickups (gameplay):** sprites exist in [Sprites/Pickups/](Assets/Space%20Shooter%20Assets/Sprites/Pickups/) (shield, health, missile-pod, etc.) but no prefabs or logic beyond the generic `PowerUp.cs`.

---

## KNOWN ISSUES

### Bugs in current code
1. **Bullet has no collision** — [Bullet.cs](Assets/Space%20Shooter%20Assets/Sprites/Player/Bullet.cs) moves and self-destructs on timer but has no `OnTriggerEnter2D`. Bullets are cosmetic until this is added.
2. **Silent fail on missing references** — [PlayerController.cs:74](Assets/Space%20Shooter%20Assets/Sprites/Player/PlayerController.cs#L74) `Shoot()` returns early if `bulletPrefab` or `firePointCenter` is null with no warning. Add a `Debug.LogWarning` or `[RequireComponent]` attribute to catch this in the Editor.
3. **No null check on Rigidbody2D** — `GetComponent<Rigidbody2D>()` in `Start()` is assumed to succeed. Missing component would NRE on frame one.
4. **`Camera.main` is not null-checked** — if the camera is untagged or disabled, `FixedUpdate` will throw on the viewport conversion.
5. **Player BoxCollider2D is mis-sized** — scene value is `{x: 5.04, y: 5.32}` with offset `{0.05, -0.86}`, which does not match the ship sprite outline. Will cause false hit detection once enemies exist.
6. **No player HP / death** — player is immortal. Collisions with enemies or bullets have no effect.

### Project-level risks
- **No source control.** No `.git`, no `.gitignore`. All work is one bad save from being lost.
- **Single scene named `SampleScene`** — Unity's default name. No menu, game-over, or boss scene.
- **Vietnamese comments** throughout the scripts. Fine for a solo project; note for collaborators.

---

## ARCHITECTURE

```
Unity Scene (SampleScene)
├── Main Camera (orthographic, size 5, AudioListener)
└── Player
    ├── Transform           pos ≈ (-0.17, 0.89, 0)
    ├── SpriteRenderer      → ship sprite
    ├── Rigidbody2D         gravity=0, drag=0, freezeRotation=true, interpolate=on
    ├── BoxCollider2D       (mis-sized, see Known Issues)
    └── PlayerController    reads Input → sets velocity → clamps position
                            holds J → Shoot() → CreateBullet()
                            UpgradeWeapon() called by PowerUp pickups
```

Systems that do **not** yet exist:
- `Enemy.cs` + `EnemySpawner.cs`
- `GameManager.cs` (HP, score, game-over)
- `UIManager.cs` + Canvas HUD
- `AudioManager.cs`

---

## FILE STRUCTURE

```
Space Shooter/
├── PROJECT_STATE.md
├── Assembly-CSharp.csproj         Unity-generated, do not edit
├── Space Shooter.sln              Unity-generated solution
├── Assets/
│   ├── Scenes/
│   │   └── SampleScene.unity      Only scene. Contains Main Camera + Player.
│   └── Space Shooter Assets/
│       ├── Fonts/                 Segment7Standard.otf, Xolonium-Regular.ttf (unused)
│       ├── Sound/
│       │   ├── DST-TowerDefenseTheme.mp3   Main BGM (unused)
│       │   ├── cc0/               blaster01–06, laser, plasma, explo, pickup, hit SFX (unused)
│       │   ├── cc-by/             cg1, explode, flaunch, rlaunch SFX (unused)
│       │   └── music cc-by 30/    DST-RailJet, FrozenJam music loops (unused)
│       ├── Sprites/
│       │   ├── background-1156435.png      Starfield (unused)
│       │   ├── BG Obj/            bgobj1–4 parallax props (unused)
│       │   ├── Effects/           EngineFire, EngineFireBlue, Blaster, shield sprites + Explosions/
│       │   ├── Enemys/
│       │   │   ├── Boss/          C7, fx486 boss art (unused)
│       │   │   └── Minions/       e01–e03, KBUM, MK 1K, fxt2/7, etc. (unused)
│       │   ├── GUI/               HUD atlas + Space-Gui-2/ subfolder (unused)
│       │   ├── Obstacles/         meteor1–4 sprites (unused)
│       │   ├── Pickups/           shield, health, missile-pod, laser-precision, etc. (unused)
│       │   ├── Player/
│       │   │   ├── PlayerController.cs   ★ Main player script
│       │   │   ├── Bullet.cs             ★ Bullet movement + lifetime
│       │   │   ├── PowerUp.cs            ★ Weapon upgrade pickup
│       │   │   ├── color.png, lines.png, rocketerColor.png, linesRocketer.png
│       │   ├── Portraits/         character portraits (unused)
│       │   ├── Projectiles/       all blaster/rocket/laser sprites + sparkBlaster anim (unused as prefab)
│       │   └── Trash/             debris sprites (unused)
│       └── Prefabs/
│           ├── Effects/           Mini/Small/Medium/Huge Explosion, EngineFireParticle,
│           │                      explode_2_0…explode_10_0, PlasmaGreenHit
│           ├── Enemy Effects/     AB03/, AB06/ animated explosions + mini trash
│           ├── Meteor Prefabs/    MeteorDamage 1/2, MeteorAnimatedExplo, MeteorAnimatedExploAlter
│           ├── Player Effects/    PSAnimatedExplo 1, PSMiniTrash 1
│           └── Game Space Effect.prefab
├── Packages/                      manifest.json, packages-lock.json
├── ProjectSettings/               Unity 2022.3.47f1 LTS settings
├── Library/  Logs/  Temp/  UserSettings/   Unity-generated, do not commit
```

---

## NEXT TASKS (priority order)

### Immediate (unblock what's already coded)
1. **Initialize git** — add Unity `.gitignore` (ignore `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `*.csproj`, `*.sln`), commit current state.
2. **Create Bullet prefab in Unity Editor** — sprite from [Sprites/Projectiles/](Assets/Space%20Shooter%20Assets/Sprites/Projectiles/), `Rigidbody2D` (gravity 0), `CapsuleCollider2D` (isTrigger), `Bullet.cs`. Assign to `PlayerController.bulletPrefab`.
3. **Add fire point** — empty child Transform at ship nose, assign to `PlayerController.firePointCenter`.
4. **Fix BoxCollider2D** — resize to match the actual ship sprite outline in the Inspector.

### Player (next meaningful additions — see detailed breakdown below)
5. **Player HP + damage + death** — add to `PlayerController`.
6. **Bullet collision** — add `OnTriggerEnter2D` to `Bullet.cs` to damage enemies.
7. **Invincibility frames after hit** — brief blink/flash so one collision doesn't immediately kill.

### Game loop
8. **Enemy.cs + EnemySpawner.cs** — HP, downward movement, death explosion using existing prefabs.
9. **GameManager singleton** — player HP, score, game-over.
10. **HUD** — Canvas with score/HP using GUI atlas.
11. **Audio** — BGM `AudioSource` on Camera; `AudioManager.PlaySfx(clip)` for shoot/explode/hit.

### Polish
12. **Parallax background** — static starfield first, then `BG Obj/` parallax layers.
13. **Sorting layers** — Background / Default / Projectile / Enemy / Player / UI.
14. **Pickups** — health restore, shield, missile-pod prefabs using existing sprites.
15. **Boss encounter** using `Sprites/Enemys/Boss/` art.

---

## PLAYER — NEXT FUNCTIONS TO ADD

These should be added to `PlayerController.cs` in the order listed:

### 1. `TakeDamage(int amount)` — public method
Called by enemy `OnTriggerEnter2D` or bullet collision. Reduces `currentHP`, triggers invincibility frames, and calls `Die()` at zero.
```
public int maxHP = 3;
private int currentHP;
private bool isInvincible = false;

public void TakeDamage(int amount) { ... }
```

### 2. Invincibility frames (`StartInvincibility` coroutine)
After `TakeDamage`, set `isInvincible = true`, blink the `SpriteRenderer` alpha for ~1.5s, then restore. Prevents instant multi-hit death.

### 3. `Die()` — private method
Spawn the player explosion prefab from [Prefabs/Player Effects/](Assets/Space%20Shooter%20Assets/Prefabs/Player%20Effects/) (`PSAnimatedExplo 1`), notify `GameManager` of game-over, and `Destroy(gameObject)`.

### 4. `Shoot()` — add fire SFX hook
Before the method is done, call `AudioManager.PlaySfx(shootClip)` (stubbed with a serialized `AudioClip shootSfx` field for now — wire it to a clip from [Sound/cc0/](Assets/Space%20Shooter%20Assets/Sound/cc0/) like `blaster01.wav`).

### 5. Weapon pickup visual feedback
When `UpgradeWeapon()` is called, briefly flash the ship white (one-frame `SpriteRenderer.color` tween) and play the `pickup1.ogg` SFX. Gives the player feedback that the pickup was registered.

---

## TECHNICAL NOTES

- **Movement model:** Direct `linearVelocity` set each frame — instant response, no glide. If you want a weighted/inertia feel, switch back to `AddForce` with `drag`.
- **Bullet orientation:** `Bullet.cs` fires along `transform.up`. The bullet prefab's local Y axis must point toward the top of the screen (Unity default for sprites is fine). If bullets fly sideways, rotate the prefab's sprite 90°.
- **Input:** Legacy `UnityEngine.Input` / `GetAxisRaw`. No Input Action assets. Switching to the new Input System is a small port — `PlayerController` is compact enough.
- **Coordinate space:** World units; orthographic size 5 → ~17.78 × 10 units at 16:9. Viewport-clamp (5%–95%) keeps the ship fully on screen.
- **Physics:** `gravityScale=0` on the player. All dynamic bodies (bullets, enemies, pickups) must also have `gravityScale=0` or they fall off-screen immediately.
- **Sorting layers:** Not configured. Add Background / Default / Projectile / Enemy / Player / UI before adding parallax or HUD.
- **No tests, no CI, no build pipeline.** Unity Editor Play mode is the only way to run the game.
- **Locale:** All comments are in Vietnamese.
