# Project Context — Angry Girls

## Game summary

- **Название:** Angry Girls.
- **Жанр по текущему коду и GDD:** физический slingshot/launch puzzle с пошаговой фазой атак, коллекцией персонажей, миссиями и RPG-подобной мета-прогрессией.
- **Core gameplay loop (подтверждено кодом + GDD):** загрузка миссии/сцены → настройка стадии и спавн → игрок запускает доступного персонажа → персонажи и боты выполняют Alternate Phase → проверка победы/поражения/следующей стадии → выдача награды → возврат к подготовке миссии.
- **Основные боевые фазы:** `GameStartPhase`, `StageSetupPhase`, `LaunchPhase`, `AlternatePhase`, `StageCompletePhase`, `VictoryPhase`, `DefeatPhase` (`GamePhaseFlowController.cs`).
- **Мета-игра в коде:** персонажи и состав команды, миссии и сложности, кредиты, инвентарь, магазин, экипировка, настройки, сохранение/загрузка.
- **Target platforms:** в GDD заявлены Windows PC и Android, WebGL как возможная платформа. Код содержит платформенные профили Windows/WebGL/Android/iOS/Default и ветвление ввода desktop/mobile. Реальная поддержка платформ и успешная сборка не подтверждены без `ProjectSettings`, пакетов и проверки сборок.
- **Текущая стадия разработки:** прототип/активная разработка с несколькими реализованными игровыми системами и незавершёнными/временными участками. В дампе есть `TODO`, временная загрузка оружия из `Resources`, закомментированные участки и заглушки. Это не подтверждение готовности к релизу.
- **Контентное ограничение по сообщению владельца проекта:** игра может иметь боевые/романтические мотивы, но без adult-контента и секса с монстрами. Это авторское ограничение проекта, а не вывод из C#-дампа.

## Technical baseline

- **Unity Editor version:** Unity 6.6 — указано владельцем проекта. Точный формат версии/patch и версия Editor из `ProjectVersion.txt` не проверены.
- **Render pipeline:** неизвестно; нет `ProjectSettings/GraphicsSettings.asset`, URP/HDRP package manifests или материалов.
- **Input handling:** используется legacy `UnityEngine.Input` (`Input.GetMouseButton`, `Input.touchCount`, `Input.GetTouch`, `Input.GetAxis`) через `InputManager`, `MouseInput`, `TouchInput`, `VirtualInput`. Наличие/отсутствие пакета New Input System не подтверждено. Не называть проект «Both» без `manifest.json` и настроек Active Input Handling.
- **Main UI system:** uGUI (`UnityEngine.UI.Button`, `UnityEngine.UI`, UI-компоненты `UI_*`). TextMeshPro используется для всплывающих цифр урона по GDD/коду. UI Toolkit не подтверждён.
- **Main scripting architecture:** namespace `Angry_Girls`; `MonoBehaviour`-менеджеры с доступом через singleton-подобные `CoreManager.Instance` и `GameplayCoreManager.Instance`; игровые контроллеры, фазы, сервисы, менеджеры данных; `ScriptableObject` как конфигурация; Addressables для части ассетов; пулы объектов; UniTask для async; DOTween для анимаций.
- **Assembly definitions:** неизвестно — файлы `.asmdef` не приложены.
- **Подтверждённые/видимые зависимости в коде:** `Cysharp.Threading.Tasks` (UniTask), `DG.Tweening` (DOTween), `UnityEngine.AddressableAssets` / Addressables, `UnityEngine.ResourceManagement.AsyncOperations`, `UnityEngine.VFX`, `UnityEngine.UI`. Версии пакетов неизвестны. Версия TextMeshPro и наличие прочих пакетов требуют проверки manifest.
- **Пути из дампа:** исходники собраны из `D:\Study\Unity\Angry_Girls\Assets\AngryGirls\Scripts&SO\...`. Это путь исходной машины, не гарантированный путь в другой рабочей копии.

## Source-of-truth documents

- **Design document:** `AngryGirls_DesignVision_ProductionEdition(1).docx` — исходный GDD; он содержит утверждения, не все из которых совпадают с кодом. Использовать исправленную редакцию `AngryGirls_DesignVision_ProductionEdition_Revised.docx` вместе с оговорками о непроверенных сценах/Inspector.
- **Technical architecture:** `Architecture.md` (этот комплект).
- **Coding conventions:** отдельный документ не предоставлен. Пока следовать существующему стилю namespace `Angry_Girls`, PascalCase для типов/публичных членов, `_camelCase` для приватных сериализуемых полей; не превращать это в обязательное правило там, где существующий код расходится.
- **Test plan:** отдельный документ и тестовые сборки не приложены. Автоматические EditMode/PlayMode тесты не подтверждены.

## Main systems

| System | Location / entry points | Responsibilities | Dependencies |
|---|---|---|---|
| Bootstrap / initialization | `CoreSystems/Core/Bootstrap.cs`, `CoreManager.cs`, `GameStateManager.cs` | Создание/инициализация глобальных менеджеров, загрузка `MainMenuScene`, New Game / Continue flow | Core components в сцене, repositories/templates, `NavigationManager` |
| Gameplay composition | `Gameplay/GameplayCoreManager.cs` | Находит или добавляет gameplay managers и вызывает `Initialize()` | Компоненты дочерних объектов сцены |
| Battle phase flow | `Gameplay/GamePhaseFlowController.cs` | Переключает фазы боя и запускает phase objects | `StageManager`, `LaunchManager`, `TurnManager`, `GameLogic`, `GameplayCharactersManager` |
| Player / input | `Gameplay/InputManager.cs`, `Inputs.cs`, `VirtualInput.cs`, `LaunchManager.cs`, `CharacterLauncher.cs` | Mouse/touch abstraction, aiming/launch flow | Legacy Input API, launcher, camera, launch-path UI |
| Characters | `Character/CControl.cs`, `Character/CharacterHealth.cs`, `Character/ChracterMovement.cs`, `Character/AnimationProcessor.cs`, `Character/CharacterSettings.cs` | Состояние юнита, здоровье, движение, анимации, способности | Rigidbody/Collider/Animator, character profile/settings, interaction events |
| Gameplay / stages | `Gameplay/StageManager.cs`, `Gameplay/GameLogic.cs`, `Gameplay/GameplayCharactersManager.cs`, `Gameplay/TurnManager.cs`, `Gameplay/TurnOrderService.cs` | Спавн стадий, команды, очередь ходов, Alternate Phase, результат боя | `GameplayCoreManager`, сценовые данные и префабы |
| Projectile / interactions | `Gameplay/Projectile.cs`, `ProjectileManager.cs`, `InteractionManager.cs`, `DeathZone.cs` | Снаряды, коллизии/триггеры, урон, подбираемые объекты, death zones | Physics, layers, projectile configs, character callbacks |
| Abilities | `Character/AttackAbility.cs`, `AttackAbilityManager.cs`, `FireballAttack.cs`, `HeadspinAttack.cs`, `SwordSpinAttack.cs`, `UppercutAttack.cs`, `AttackAbilityData.cs` | Выбор типа способности и варианты Launch/Alternate | `Resources/AttackAbilitiesTemplates`, projectile/VFX/audio/animation data |
| UI / navigation | `UI/UI_UIManager.cs`, `UI_UIScreen.cs`, `UI_*`, `NavigationManager.cs` | Main menu, mission preparation, gameplay UI, shop, team, settings, result and loading screens | uGUI, managers, scene names/enums |
| Save/load & meta | `CoreSystems/Core/SaveLoadManager.cs`, `Repository.cs`, `CharactersManager.cs`, `MissionsManager.cs`, `CreditsManager.cs`, `InventoryManager.cs`, `ShopManager.cs`, `SettingsManager.cs` | DTO-based serialization, managers reinit from save/template, player progression | Repository persistence, Addressables for saved asset IDs |
| Assets / pools / VFX | `AddressableAssetManager.cs`, `PoolManager.cs`, `PoolObjectLoader.cs`, `VFXManager.cs`, factories | Asset handles/cache, object reuse, effect spawn | Addressables, Resources, factory configuration |
| Audio | `AudioManager.cs`, `AudioClipData.cs`, `AudioClipLibrary.cs`, `LevelSettings.cs`, `LevelMusicController.cs` | SFX/music, categories, fades, settings | AudioSources, clip libraries, UniTask |
| Platform settings | `PlatformSettingsCatalog.cs`, `SettingsManager.cs`, `FrameSettings.cs`, `PhysicsSettings.cs` | Platform defaults and configurable camera/audio/game settings | ScriptableObject profiles; actual project settings unknown |

> Пути выше — пути относительно каталога скриптов по именам из дампа. Точная папочная структура подтверждается только частично заголовками файлов дампа.

## Constraints

- **Must not change:** не менять без согласования основную идентичность игры; не удалять/переименовывать сериализуемые поля, enum values, `SceneType`, `CharacterType`, `AttackType`, asset GUIDs или публичные API без проверки сцен, префабов, ScriptableObjects и save migration.
- **Compatibility requirements:** Unity 6.6 по словам владельца; конкретные версии пакетов и целевые build targets проверить в `Packages/manifest.json`, `ProjectSettings/ProjectVersion.txt` и Build Profiles.
- **Naming conventions:** namespace `Angry_Girls`; основные типы/публичные API PascalCase; приватные поля часто `_camelCase`;
- **Asset conventions:** конфигурация персонажей/предметов/миссий — ScriptableObjects; часть ресурсов грузится через Addressables, abilities templates — через `Resources.LoadAll("AttackAbilitiesTemplates")`, временное оружие — через `Resources.Load("DefinetlyNotAWeapon")`. Не предполагать, что все ассеты Addressables.

## Known issues / current work

Подтверждено по тексту дампа (это аудит исходников, не запуск игры):
- `CControl.InitializeCharacter()` содержит временную загрузку и создание оружия `Resources.Load("DefinetlyNotAWeapon")` с комментарием `TODO: TEMP`; нужно заменить/формализовать до production.
- `BoxColliderUpdater` содержит условие сравнения `SqrMagnitude(...) > 0.00001f || ... < 0.00001f`, логически почти всегда истинное; также в GDD указано, что реализацию динамических коллайдеров стоит переделать.
- `SaveLoadManager` вызывает `Repository.SaveState()` / `Repository.LoadState()`, но фактический формат и физическое место хранения нужно подтвердить по `Repository.cs`; утверждение GDD «PlayerPrefs + JSON» нельзя считать проверенным без этого.
- `InventoryData.UpdateFromSaveAsync()` в показанном коде восстанавливает item по ID, но не восстанавливает сохранённые quantity/equipped/upgrade-поля, несмотря на наличие похожих полей в DTO — вероятный дефект сохранения инвентаря.
- `CharactersData` и другие save DTO требуют проверки обратной совместимости после изменений данных.
- `GamePhaseFlowController`/`TurnManager` используют ожидания завершения хода; проверьте наличие таймаутов и поведение при застрявшей анимации/удалённом юните. GDD прямо говорит, что таймаут Alternate Phase не реализован.
- `AttackAbilityManager` грузит шаблоны из `Resources/AttackAbilitiesTemplates`; если папка/ассеты отсутствуют или данные не настроены, ability может быть `null`.
- В `GameplayCoreManager` и `CoreManager` менеджеры создаются/инициализируются во время lifecycle Unity; сцены должны содержать ожидаемые ссылки и конфигурацию. Проверить двойную инициализацию и ссылки в Inspector.
- В коде есть закомментированные участки, `TODO`, временные решения и сообщения логов с повреждённой кодировкой в отдельных местах дампа.
- Файлы сцен, префабов, ScriptableObjects, `.meta`, `Packages/manifest.json`, `ProjectSettings`, Build Profiles, asmdef и тестов не предоставлены; поэтому ссылки, GUID, package versions, рендер-пайплайн и успешные сборки не проверены.

## Last verified build/test state

- **Build:** 
- **Tests:** 
- **Verification basis:** 

## Agent rule

1. Если факт не подтверждён кодом или предоставленным артефактом, маркируй его как неизвестный/гипотезу.
2. Отличай «реализовано в коде», «описано в GDD», «планируется» и «проверено запуском».
3. Перед изменением сериализуемых типов, enum, сценовых entry points или save DTO проверь потребителей и обратную совместимость.
4. Не считай GDD источником истины для деталей реализации, если исходники показывают другое; укажи расхождение и обнови документацию.
5. Не утверждай, что проект компилируется/работает, пока он не открыт и не проверен в Unity.
