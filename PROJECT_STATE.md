# PROJECT_STATE — Space Shooter

Unity 2D space shooter. Core player systems are implemented and the bullet prefab exists on disk. The only remaining Inspector wiring before shooting works is dragging the prefab into the `bulletPrefab` field. **~25% complete** — movement, directional engines, shooting, weapon levels, and pickup logic all work in code; enemies, UI, audio, and game loop are entirely absent.

- **Engine:** Unity 2022.3.47f1 (LTS)
- **Working directory:** `w:\Space Shooter`
- **Render mode:** 2D, orthographic (size 5), Main Camera at default position
- **Scenes:** 1 — [Assets/Scenes/SampleScene.unity](Assets/Scenes/SampleScene.unity)
- **Scripts (game):** 3 — PlayerController, Bullet, PowerUp
- **Prefabs (game):** 1 — Bullet.prefab
- **Source control:** Not a git repository — highest-priority risk

---

## COMPLETED FEATURES

### Player movement
File: [Assets/Space Shooter Assets/Sprites/Player/PlayerController.cs](Assets/Space%20Shooter%20Assets/Sprites/Player/PlayerController.cs)

- Reads `GetAxisRaw` Horizontal/Vertical in `Update()`.
- Sets `rb.linearVelocity = moveInput.normalized * moveSpeed` in `FixedUpdate()`. `moveSpeed` default 12.
- Screen clamping: viewport-percent clamp (5%–95%) in `FixedUpdate()`.
- `transform.rotation = Quaternion.identity` — no tilt; rotation locked flat every frame.
- Rigidbody2D configured at runtime in `Start()`: `gravityScale=0`, `linearDamping=0`, interpolation on, `freezeRotation=true`.

### Directional engine particles (code only — needs Inspector wiring)
File: [Assets/Space Shooter Assets/Sprites/Player/PlayerController.cs](Assets/Space%20Shooter%20Assets/Sprites/Player/PlayerController.cs)

- `ParticleSystem[] engines` array — 6 slots, each mapped explicitly to WASD input:

| Index | Position | Fires on |
|---|---|---|
| [0] | top-left | D and/or S |
| [1] | left | D only |
| [2] | bottom-left | D and/or W |
| [3] | top-right | A and/or S |
| [4] | right | A only |
| [5] | bottom-right | A and/or W |

- `SetEngine(index, activation)` lerps `startSize` between 0.3 (idle) and 1.8 (full thrust).
- Multiple engines ignite simultaneously when the same key covers them (e.g. D fires [0], [1], [2] at once).
- **Requires wiring:** 6 particle systems must be assigned in Inspector in the order above.

### Weapon system + shooting
File: [Assets/Space Shooter Assets/Sprites/Player/PlayerController.cs](Assets/Space%20Shooter%20Assets/Sprites/Player/PlayerController.cs)

- Fire input: hold **J** key; cooldown driven by `attackSpeed` (shots per second, default 5). Set higher = faster.
- Fire position: `GetFirePosition()` — uses `firePointCenter.position` if assigned; otherwise auto-calculates from `SpriteRenderer.bounds.extents.y` (tip of ship sprite). `firePointCenter` is optional.
- `WeaponType` enum: `Neutron` (green, wide-short) or `Laser` (red, tall-thin).
- `weaponLevel` (1–3) spread pattern:
  - Level 1: single center shot
  - Level 2: two parallel shots (±0.2 offset)
  - Level 3: three spread shots (center + ±15° angle)
- `CreateBullet()` instantiates `bulletPrefab`, sets `SpriteRenderer.color`, scales bullet shape by weapon type, and stamps `bullet.damage = weaponLevel`.
- `UpgradeWeapon(WeaponType)`: same type → increments level (max 3); different type → switches and resets to level 1.
- **Requires wiring:** `bulletPrefab` must be assigned in Inspector (drag `Bullet.prefab`). `firePointCenter` is optional.

### Bullet
File: [Assets/Space Shooter Assets/Sprites/Player/Bullet.cs](Assets/Space%20Shooter%20Assets/Sprites/Player/Bullet.cs)
Prefab: [Assets/Space Shooter Assets/Prefabs/Bullet.prefab](Assets/Space%20Shooter%20Assets/Prefabs/Bullet.prefab)

- `speed` (default 20), `damage` (default 1 — overwritten by `PlayerController` to `weaponLevel` at spawn).
- On `Start()`: sets `rb.linearVelocity = transform.up * speed`.
- Auto-destroys after 3 seconds.
- `OnTriggerEnter2D`: destroys self on "Enemy" tag hit. `Enemy.TakeDamage(damage)` call is present but commented out — uncomment when `Enemy.cs` exists.
- Prefab components: SpriteRenderer (white.png, yellowish default tint), Rigidbody2D (gravity 0, continuous collision), BoxCollider2D (isTrigger, size 0.3×0.5), Bullet script.

### PowerUp script
File: [Assets/Space Shooter Assets/Sprites/Player/PowerUp.cs](Assets/Space%20Shooter%20Assets/Sprites/Player/PowerUp.cs)

- `OnTriggerEnter2D`: detects `"Player"` tag, calls `player.UpgradeWeapon(weaponType)`, destroys itself.
- Self-contained; just needs a prefab with `CircleCollider2D (isTrigger)` and this script.

---

## NEEDS INSPECTOR WIRING (before playtest)

| What | Where | Action |
|---|---|---|
| Bullet prefab | `PlayerController.bulletPrefab` | Drag `Prefabs/Bullet.prefab` |
| Engine particles [0–5] | `PlayerController.engines` | Drag 6 particle system child GameObjects in order (see table above) |
| Fire point (optional) | `PlayerController.firePointCenter` | Leave empty for auto, or drag a child Transform at ship nose |

---

## NOT YET STARTED (implied by asset library)

- **Player HP / damage / death** — `PlayerController` has no `TakeDamage()`, no `Die()`, no invincibility frames. Player is currently immortal.
- **Enemy system:** `Enemy.cs`, HP, downward movement, death → explosion. Sprites ready in [Sprites/Enemys/](Assets/Space%20Shooter%20Assets/Sprites/Enemys/).
- **EnemySpawner:** timed spawner at random X above screen top.
- **GameManager:** singleton for player HP, score, game-over flow.
- **HUD/UI:** Canvas with score and HP using [Sprites/GUI/](Assets/Space%20Shooter%20Assets/Sprites/GUI/) atlas. No Canvas in scene.
- **AudioManager:** BGM via `DST-TowerDefenseTheme.mp3`, SFX for shoot/explode/hit. No `AudioSource` in scene.
- **Background:** starfield and parallax `BG Obj/` sprites unused.
- **Sorting layers:** none configured — needed before parallax, projectiles, and UI are layered.
- **Obstacles:** meteor sprites and prefabs ready, no spawner or script.
- **Pickups (gameplay):** sprites exist in [Sprites/Pickups/](Assets/Space%20Shooter%20Assets/Sprites/Pickups/) but no prefabs (only the generic `PowerUp.cs` logic).

---

## KNOWN ISSUES

### Code bugs
1. **No player HP / death** — player is immortal. No `TakeDamage()`, no `Die()`. Must be added before enemies are meaningful.
2. **`Bullet.OnTriggerEnter2D` is a stub** — [Bullet.cs:14](Assets/Space%20Shooter%20Assets/Sprites/Player/Bullet.cs#L14) destroys the bullet but the `Enemy.TakeDamage(damage)` call is commented out. Wire it when `Enemy.cs` is ready.
3. **No null check on Rigidbody2D** — [PlayerController.cs:33](Assets/Space%20Shooter%20Assets/Sprites/Player/PlayerController.cs#L33) `GetComponent<Rigidbody2D>()` assumed to succeed; missing component = NRE frame 1.
4. **`Camera.main` not null-checked** — [PlayerController.cs:144](Assets/Space%20Shooter%20Assets/Sprites/Player/PlayerController.cs#L144) viewport conversion throws if camera is untagged.
5. **Player BoxCollider2D is mis-sized** — scene value `{x: 5.04, y: 5.32}` with offset `{0.05, -0.86}` does not match the ship sprite. Will cause false hit detection once enemies exist.
6. **`attackSpeed = 0` would divide by zero** — [PlayerController.cs:72](Assets/Space%20Shooter%20Assets/Sprites/Player/PlayerController.cs#L72) `1f / attackSpeed`. Add `Mathf.Max(0.1f, attackSpeed)` guard if this field is exposed to untrusted input.

### Project-level risks
- **No source control.** No `.git`, no `.gitignore`. All work is one bad save from loss.
- **Single scene named `SampleScene`** — Unity default. No menu, game-over, or boss scene.
- **Vietnamese comments** throughout. Fine for solo; note for collaborators.
- **"Enemy" tag not yet created** in Unity's Tag Manager — `Bullet.OnTriggerEnter2D` silently won't fire until the tag exists and is assigned to enemy GameObjects.

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
    └── PlayerController
            reads Input → sets velocity → clamps position
            J held → Shoot() → CreateBullet() → Instantiate(Bullet.prefab)
            engines[0-5] → SetEngine() → startSize lerp per direction
            UpgradeWeapon() called by PowerUp pickups

Runtime spawned:
└── Bullet (from Bullet.prefab)
    ├── SpriteRenderer      white.png, tinted at spawn
    ├── Rigidbody2D         gravity=0, continuous collision
    ├── BoxCollider2D       isTrigger, 0.3×0.5
    └── Bullet              velocity=transform.up*speed, destroys on Enemy hit or 3s timeout
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
├── Assets/
│   ├── Scenes/
│   │   └── SampleScene.unity
│   └── Space Shooter Assets/
│       ├── Fonts/                 Segment7Standard.otf, Xolonium-Regular.ttf (unused)
│       ├── Sound/
│       │   ├── DST-TowerDefenseTheme.mp3   Main BGM (unused)
│       │   ├── cc0/               blaster, laser, plasma, explo, pickup, hit SFX (unused)
│       │   ├── cc-by/             additional SFX (unused)
│       │   └── music cc-by 30/    DST-RailJet, FrozenJam loops (unused)
│       ├── Sprites/
│       │   ├── background-1156435.png      Starfield (unused in scene)
│       │   ├── BG Obj/            parallax props (unused)
│       │   ├── Effects/           EngineFire, EngineFireBlue, Blaster, shield + Explosions/
│       │   ├── Enemys/            Boss/ + Minions/ sprites (unused)
│       │   ├── GUI/               HUD atlas + Space-Gui-2/ (unused)
│       │   ├── Obstacles/         meteor1–4 (unused)
│       │   ├── Pickups/           shield, health, missile-pod, etc. (unused)
│       │   ├── Player/
│       │   │   ├── PlayerController.cs   ★ Main player script
│       │   │   ├── Bullet.cs             ★ Bullet movement + collision
│       │   │   ├── PowerUp.cs            ★ Weapon upgrade pickup
│       │   │   └── color.png, lines.png, rocketerColor.png, linesRocketer.png
│       │   ├── Portraits/         (unused)
│       │   ├── Projectiles/       blaster/rocket/laser sprites + sparkBlaster anim (unused as prefabs)
│       │   └── Trash/             debris sprites (unused)
│       └── Prefabs/
│           ├── Bullet.prefab               ★ Ready to assign to PlayerController
│           ├── white.png                   Bullet sprite source
│           ├── Effects/                    Mini/Small/Medium/Huge Explosion, EngineFireParticle, explode_*
│           ├── Enemy Effects/              AB03/, AB06/ animated explosions
│           ├── Meteor Prefabs/             MeteorDamage, MeteorAnimatedExplo
│           ├── Player Effects/             PSAnimatedExplo 1, PSMiniTrash 1
│           └── Game Space Effect.prefab
├── Packages/
└── ProjectSettings/
```

---

## NEXT TASKS (priority order)

### Immediate (unblock playtest)
1. **Initialize git** — add Unity `.gitignore`, commit.
2. **Wire Bullet prefab** — drag `Prefabs/Bullet.prefab` into `PlayerController.bulletPrefab` in Inspector.
3. **Wire engine particles** — assign 6 particle system children in `engines[]` array (in order: top-left → left → bottom-left → top-right → right → bottom-right).
4. **Fix BoxCollider2D** — resize to match ship sprite outline.
5. **Add "Enemy" tag** in Unity Tag Manager (Edit → Project Settings → Tags and Layers).

### Player (next code additions)
6. **`TakeDamage(int amount)`** — reduce HP, trigger invincibility frames, call `Die()` at 0.
7. **`StartInvincibility()` coroutine** — blink `SpriteRenderer` for ~1.5s, set `isInvincible` flag.
8. **`Die()`** — spawn `PSAnimatedExplo 1` from [Prefabs/Player Effects/](Assets/Space%20Shooter%20Assets/Prefabs/Player%20Effects/), notify GameManager, destroy player.
9. **Shoot SFX hook** — add `AudioClip shootSfx` field, call `AudioManager.PlaySfx()` in `Shoot()` (wire to `cc0/blaster01.wav`).

### Game loop
10. **`Enemy.cs` + `EnemySpawner.cs`** — HP, downward movement, death explosion.
11. **Uncomment** `Enemy.TakeDamage(damage)` in [Bullet.cs:16](Assets/Space%20Shooter%20Assets/Sprites/Player/Bullet.cs#L16).
12. **`GameManager` singleton** — player HP, score, game-over.
13. **HUD** — Canvas with score/HP using GUI atlas.
14. **Audio** — BGM on Camera; `AudioManager.PlaySfx(clip)`.

### Polish
15. **Parallax background** — static starfield first, then `BG Obj/` layers.
16. **Sorting layers** — Background / Default / Projectile / Enemy / Player / UI.
17. **Pickup prefabs** — health, shield, missile-pod using existing sprites + `PowerUp.cs`.
18. **Boss encounter** — `Sprites/Enemys/Boss/` art.

---

## TECHNICAL NOTES

- **Attack speed:** `attackSpeed` = shots per second (default 5). Internally: `cooldown = 1f / attackSpeed`. Guard against 0 if ever driven by data.
- **Bullet damage scales with level:** `damage = weaponLevel` stamped in `CreateBullet()`. Level 1 = 1 dmg, level 3 = 3 dmg. Adjust in `CreateBullet()` if you want a different curve.
- **Fire point auto-calc:** `GetFirePosition()` reads `SpriteRenderer.bounds.extents.y` at runtime. This is the live rendered bounds, so it's correct even after sprite swap or scale change. If `firePointCenter` is assigned, that takes priority.
- **Bullet orientation:** fires along `transform.up`. The bullet prefab's local Y must point toward the screen top (Unity default for sprites). If bullets fly sideways, rotate the prefab sprite 90°.
- **Movement model:** Direct `linearVelocity` set each frame — instant response. If you want inertia/glide, switch to `AddForce` with drag.
- **Input:** Legacy `UnityEngine.Input` / `GetAxisRaw`. No Input Action assets.
- **Physics:** `gravityScale=0` on player and bullet. All future dynamic bodies (enemies, pickups) must also have `gravityScale=0`.
- **Sorting layers:** Not configured. Add before parallax or HUD.
- **Locale:** All comments are in Vietnamese.
- **No tests, no CI, no build pipeline.** Unity Editor Play is the only way to run.
