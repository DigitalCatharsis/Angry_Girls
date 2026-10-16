# Architecture Passport — Angry Girls


## 1. Project structure

Из заголовков дампа видны следующие каталоги/группы:

- `Assets/AngryGirls/Scripts&SO/Character/` — контроллеры персонажа, здоровье, движение, анимация, способности, subcomponents.
- `Assets/AngryGirls/Scripts&SO/CoreSystems/Core/` — `Bootstrap`, `CoreManager`, `GameStateManager`, `SaveLoadManager`.
- `Assets/AngryGirls/Scripts&SO/CoreSystems/CatalogsRepositoriesDataTemplates/` — repositories, каталоги, `DefaultSaveTemplate`, настройки предметов/миссий.
- `Assets/AngryGirls/Scripts&SO/Gameplay/` — боевые менеджеры, фазы, ввод, запуск, стадии, очередь ходов, взаимодействия.
- `Assets/AngryGirls/Scripts&SO/UI/` — экраны и UI-панели.
- `Assets/AngryGirls/Scripts&SO/!Unsorted/` — например `PlatformSettingsCatalog.cs`.
- Editor scripts присутствуют, например `DynamicFactoryExtentionEditor.cs`, `FactoryExtentionEditor.cs`, `ItemSettingsEditor.cs`, `LayerAdderEditor.cs`, `LayerChangerEditor.cs`, `LookAtPointEditor.cs`, `ShowOnlyDrawer.cs`; их точные compile boundaries без asmdef не подтверждены.
- Runtime scenes, prefabs, ScriptableObjects и tests: не входят в предоставленный дамп, расположение неизвестно.
- Third-party code: не идентифицирован как исходники в дампе; зависимости вызываются через namespaces пакетов.

## 2. Architecture overview

```text
Bootstrap scene
  └─ Bootstrap.Start()
      ├─ проверяет CoreManager.Instance
      ├─ создаёт GameStateManager при отсутствии
      ├─ await CoreManager.InitializeSystemsAsync()
      └─ LoadSceneAsync("MainMenuScene")

Persistent core
  └─ CoreManager (DontDestroyOnLoad)
      ├─ AddressableAssetManager / repositories
      ├─ CharactersManager, MissionsManager
      ├─ CreditsManager, InventoryManager, ShopManager
      ├─ SaveLoadManager, SettingsManager, AudioManager
      ├─ PoolManager / PoolObjectLoader / VFXManager
      └─ PauseControl

Gameplay scene
  └─ GameplayCoreManager.OnEnable()
      └─ InitializeCoreSystems()
          ├─ InputManager, CameraManager, GameLogic
          ├─ GamePhaseFlowController
          ├─ GameplayCharactersManager, StageManager
          ├─ LaunchManager, TurnManager, HealthBarManager
          ├─ TurnOrderService, InteractionManager
          ├─ ProjectileManager, AttackAbilityManager
          └─ OnInitialized → GamePhaseFlowController.LateInitialize()
                               → GameStartPhase
```

Порядок — из явных вызовов кода; фактический порядок Unity callbacks и корректность ссылок требуют scene validation.

### Dependency flow

- `CoreManager.Instance` — точка доступа к мета-системам и persistent services.
- `GameplayCoreManager.Instance` — точка доступа к системам текущей gameplay-сцены.
- `CControl` публикует события (`UnitGotHit`, `UnitGotKilled`, `UnitHasBeenLaunched`, `UnitPerformedAttack`, `UnitHasPerformedLanding` и др.), на которые подписываются анимационный процессор и игровые системы.
- `GamePhaseFlowController` управляет `IGamePhase`-объектами: `GameStartGamePhase`, `StageSetupGamePhase`, `LaunchGamePhase`, `AlternateGamePhase`, `StageCompleteGamePhase`, `VictoryGamePhase`, `DefeatGamePhase`.
- `StageManager` создаёт launcher и врагов, вызывает очистку сцены и при переходе стадии просит `GameplayCharactersManager` переинициализировать выживших.
- `LaunchManager` использует `InputManager`, `CameraManager`, `StageManager`, `GameLogic`, `GameplayCharactersManager`; конфигурация `_launchesBeforeFirstAlternate` по умолчанию равна 2.
- `TurnManager` строит/выполняет очередь альтернативных атак; `TurnOrderService` делегирует построение порядка `GameplayCharactersManager`.
- `AttackAbilityManager` загружает `AttackAbilityData` через `Resources.LoadAll<AttackAbilityData>("AttackAbilitiesTemplates")` и связывает `AttackType` с конкретными реализациями.
- `NavigationManager` загружает сцены по `SceneType.ToString()`; имена сцен в Build Settings/Build Profiles должны совпадать с enum names.

## 3. Core systems inventory

| System | Actual classes/files | Public API / responsibilities | Dependencies | Lifecycle |
|---|---|---|---|---|
| Bootstrap | `Bootstrap.cs` | `Start()`, вызывает core init и грузит `MainMenuScene` | `CoreManager`, `GameStateManager`, SceneManager | Входная bootstrap-сцена |
| Persistent core | `CoreManager.cs` | `Instance`, `InitializeSystemsAsync()`; инициализация менеджеров | serialized repositories/templates, child components, Addressables | Singleton; `DontDestroyOnLoad(gameObject)` в `Awake` |
| Player/meta state | `GameStateManager.cs` | `NewGame()`, load/continue flow, tutorial session flag | Core managers, `NavigationManager` | Обычный C# singleton |
| Gameplay composition | `GameplayCoreManager.cs` | `InitializeCoreSystems()`, system accessors | components in children/current scene | Инициализируется в `OnEnable`; не помечен DontDestroyOnLoad |
| Battle flow | `GamePhaseFlowController.cs` | `SwitchState`, `CurrentGamePhaseState`, `OnPhaseChanged` | stage, launch, turn, game logic | Создаёт объекты фаз при `Initialize` |
| Stage spawning | `StageManager.cs` | `SetupCurrentStage()`, `ProceedToNextStage()`, `HasNextStage()` | stage data, character manager, launcher prefab | Состояние локально gameplay-сцены |
| Launch/input | `LaunchManager.cs`, `InputManager.cs`, `Inputs.cs`, `CharacterLauncher.cs` | candidate selection, launch state, mouse/touch abstraction | Unity legacy Input, camera, trajectory UI | Gameplay lifecycle |
| Character runtime | `CControl.cs`, `CharacterHealth.cs`, `ChracterMovement.cs`, `AnimationProcessor.cs`, `SubComponentsController.cs` | health, movement, animation, event lifecycle | Animator, Rigidbody, Collider, settings/profile | На персонаже/его child components |
| Turn order | `TurnManager.cs`, `TurnOrderService.cs`, `GameplayCharactersManager.cs` | builds and executes Alternate Phase turn order | launched players, alive bots, launch manager | Gameplay phase |
| Interaction/physics | `InteractionManager.cs`, `Projectile.cs`, `ProjectileManager.cs`, `DeathZone.cs` | collision/trigger routing, projectile damage, pick-ups/death zones | Physics layers, collider setup, projectile config | Gameplay scene |
| Abilities | `AttackAbility.cs`, `AttackAbilityManager.cs`, `AttackAbilityData.cs`, `FireballAttack.cs`, `HeadspinAttack.cs`, `SwordSpinAttack.cs`, `UppercutAttack.cs` | Launch/Alternate attack behaviors | `Resources/AttackAbilitiesTemplates`, VFX/projectile/audio data | Loaded during gameplay system initialization |
| Meta progression | `CharactersManager.cs`, `MissionsManager.cs`, `CreditsManager.cs`, `InventoryManager.cs`, `ShopManager.cs` | character roster, mission progress, credits, inventory, assortment | ScriptableObject catalogs/templates, repositories, Addressables | Core lifetime |
| Persistence | `SaveLoadManager.cs`, `ISaveReinitManager.cs`, `ISaveData.cs`, `Repository.cs` | register managers, collect DTOs, load and save state | concrete managers, repository backend | Core lifetime; actual storage backend inspect `Repository.cs` in project |
| Asset loading | `AddressableAssetManager.cs`, `ItemSettingsRepository.cs`, `MissionsRepository.cs`, `CharacterSettingsCatalog.cs` | async load/cache/repository lookup | Addressables, ScriptableObjects | Core lifetime / reference-counted handles |
| Pooling/VFX | `PoolManager.cs`, `PoolObjectLoader.cs`, `PoolObject.cs`, `VFXManager.cs`, factories | reuse pooled units/effects/UI objects | factories, prefabs, VFX assets | Core services plus scene cleanup |
| Audio | `AudioManager.cs`, `AudioClipData.cs`, `AudioClipLibrary.cs`, `LevelSettings.cs`, `LevelMusicController.cs` | SFX/music, fades, clip variation/settings | AudioSources, libraries, UniTask | Core audio manager + scene-local music controller |
| UI | `UI_UIManager.cs`, `UI_UIScreen.cs`, `UI_MainMenuScreen.cs`, `UI_MissionPreparationScreen.cs`, `UI_GameplayScreen.cs`, `GameResultUI.cs`, `PauseMenu.cs` | screens, mission preparation, HUD, results, settings | uGUI, managers, scene objects | UI manager/screen initialization; serialized references required |

## 4. Data and assets

- **Configuration source:** `CharacterSettings`, `ItemSettings`, `Mission`/`MissionData`/`MissionRewardData`, `LevelSettings`, `PlatformSettingsCatalog`, `DefaultSaveTemplate` and other ScriptableObjects.
- **Character configuration:** `CharacterSettings` stores `UnitType`, `CharacterType`, `AttackType`, portrait reference, base stats, movement/animation data, dynamic collider data and death-animation option.
- **Item configuration:** `ItemSettings` stores item name, rarity, type, price and stat bonuses; item definitions are accessed via `ItemSettingsRepository`.
- **Mission configuration:** `MissionsRepository` stores `Mission[]`; each mission has Easy/Normal/Hard `MissionData` and Addressable sprite references.
- **Ability data:** `AttackAbilityManager` loads `AttackAbilityData` from `Resources/AttackAbilitiesTemplates`; ensure this path exists and contains valid Launch/Alternate data.
- **Addressables:** `AddressableAssetManager` uses `AssetReference`/GUID and tracked async handles; verify Addressables Groups and GUIDs in the full project.
- **Resources:** code also explicitly uses `Resources.LoadAll` for abilities and a temporary `Resources.Load("DefinetlyNotAWeapon")` in `CControl.InitializeCharacter()`.
- **Prefab ownership / scene ownership:** cannot be established from C# dump. Inspect references in Inspector and all scenes before moving or renaming assets.
- **Asset naming/path conventions:** project-specific but not fully documented; avoid broad asset renames.
- **Localization data:** 
- **Save DTOs:** separate serializable save classes exist for characters, missions, credits, shop and inventory. Verify that every field is actually copied in both directions; `InventoryData.UpdateFromSaveAsync()` currently appears to restore item settings by ID but not all `quantity`, `IsEquipped`, or `UpgradeLevel` values from the DTO.

## 5. Unity-specific decisions

- **Unity version:** Unity 6.6 per project owner;
- **Render pipeline/shaders:** 
- **Input:** current implementation uses legacy `UnityEngine.Input`; mouse on non-mobile, touch on mobile. Do not migrate to Input System without checking project settings and all input code.
- **UI:** uGUI. UI Toolkit not evidenced.
- **Addressables/asset loading:** Addressables for typed sprite/ScriptableObject references and GUID-based lookup; Resources for ability templates and temporary weapon.
- **Async/task conventions:** UniTask (`Cysharp.Threading.Tasks`) used for async operations; DOTween (`DG.Tweening`) for animation/tweening. `async void` occurs in `Bootstrap.Start`; observe exception handling and cancellation on scene unload.
- **Serialization/save format:** `SaveLoadManager` registers manager DTO delegates and calls `Repository.SaveState()` / `Repository.LoadState()`. The exact disk/player-pref backend and JSON format must be verified in `Repository.cs`; do not repeat the old GDD claim “PlayerPrefs + JSON” as a fact until verified.
- **Lifecycle hazards:** managers initialize in `Awake`/`OnEnable`/`Start` paths and rely on singleton availability and scene references. Validate ordering in an actual player build.
- **Scene loading:** `NavigationManager.NavigateToScene()` loads `sceneType.ToString()` and toggles a loading screen. Ensure all referenced scene names are included in Build Profiles.
- **Platform profiles:** `TargetPlatform` includes Windows, WebGL, Android, iOS, Default; runtime selection code has compile-time platform branches. This is configuration support, not proof of release readiness.

## 6. Testing and verification

- **EditMode tests:** 
- **PlayMode tests:** 
- **Build validation:** 
- **Required manual checks:**
  1. Open project in Unity 6.6; resolve package/import/compiler errors.
  2. Verify Bootstrap scene contains exactly one valid `CoreManager` with all required serialized references.
  3. Confirm `MainMenuScene`, `MissionPreparation`, and all `SceneType` mission scenes are in Build Profiles and scene names match enum names.
  4. Verify gameplay scene has configured `GameplayCoreManager`, stage data, launcher/enemy prefabs, camera, colliders/layers, UI and all ScriptableObject references.
  5. Verify `Resources/AttackAbilitiesTemplates` contains correctly configured `AttackAbilityData` for every required `AttackType` and both usage modes.
  6. Run New Game, save/load/continue, mission completion, reward grant, item purchase, team selection, inventory and equipment persistence.
  7. Test mouse and touch separately, camera controls, launch cancellation, ability windows, death zones, stage transitions, all-dead states and stuck animation recovery.
  8. Build each intended target platform separately; WebGL/mobile compatibility cannot be inferred from conditional code alone.
- **Known test limitations:**

## 7. Change impact checklist

Before changing a system:
1. Identify direct callers, event subscribers and consumers.
2. Search serialized references in scenes, prefabs and ScriptableObjects.
3. Check runtime lifecycle and initialization order.
4. Check platform/package compatibility in `manifest.json` and Project Settings.
5. Check tests and build targets.
6. Preserve enum numeric values, serialized field names, Addressable GUIDs and save DTO compatibility unless a migration is planned.
7. Verify Resources paths and Addressables keys.
8. Check event subscription/unsubscription and async cancellation across scene changes.
9. Run relevant EditMode/PlayMode tests and at least one real build.

## 8. Evidence and confidence

- **High confidence (direct source evidence):** class names, method names, enum values, code-level dependencies, use of legacy Input, use of UniTask/DOTween/Addressables/VFX, explicit Resources paths, manager registration and the six battle phase names.
- **Medium confidence (source plus inference):** conceptual division into persistent core and scene-local gameplay systems; exact runtime behavior depends on scene composition and serialized references.
- **Unverified:** render pipeline, package versions, asmdefs, exact Unity patch version, build targets, actual save backend, scene contents, prefab references, ScriptableObject values, current compilation status, performance, test coverage.
