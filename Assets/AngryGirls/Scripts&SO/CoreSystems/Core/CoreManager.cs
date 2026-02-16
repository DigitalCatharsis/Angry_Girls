using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Angry_Girls
{
    [Serializable]
    public class CoreManager : MonoBehaviour
    {
        public static CoreManager Instance { get; private set; }

        [SerializeField] private ItemSettingsRepository _itemSettingsRepository;
        [SerializeField] private ShopSettings _shopSettings;
        public ItemSettingsRepository ItemSettingsRepository => _itemSettingsRepository;

        public PoolManager PoolManager { get; private set; }
        public PoolObjectLoader PoolObjectLoader { get; private set; }
        public HashManager HashManager { get; private set; }
        public AudioManager AudioManager { get; private set; }
        public VFXManager VFXManager { get; private set; }
        public PauseControl PauseControl { get; private set; }
        public SaveLoadManager SaveLoadManager { get; private set; }
        public CreditsManager CreditsManager { get; private set; }
        public MissionsManager MissionsManager { get; private set; }
        public SettingsManager SettingsManager { get; private set; }
        public CharactersManager CharactersManager { get; private set; }
        public ShopManager ShopManager { get; private set; }
        public AddressableAssetManager AddressableAssetManager { get; private set; }
        public InventoryManager InventoryManager { get; private set; }

        [Header("Default Save Template")]
        [SerializeField] private DefaultSaveTemplate _defaultSaveTemplate;
        public DefaultSaveTemplate DefaultSaveTemplate => _defaultSaveTemplate;


        [SerializeField] private CharacterSettingsCatalogSO _characterSettingsCatalogSO;
        public CharacterSettingsCatalogSO CharacterSettingsCatalogSO => _characterSettingsCatalogSO;

        public async UniTask InitializeSystemsAsync()
        {
            try
            {
                AddressableAssetManager = new AddressableAssetManager();

                HashManager = new HashManager();

                PoolManager = new PoolManager();
                CreditsManager = new CreditsManager();
                await ItemSettingsRepository.InitializeAsync(AddressableAssetManager);

                MissionsManager = GetComponentInChildren<MissionsManager>();
                this.MissionsManager.Initialize(_defaultSaveTemplate);

                CharactersManager = GetComponentInChildren<CharactersManager>();

                InventoryManager = new();
                ShopManager = new();
                ShopManager.Initialize(ItemSettingsRepository, MissionsManager, InventoryManager, _shopSettings);

                SettingsManager = new SettingsManager();
                PauseControl = new PauseControl();

                AudioManager = gameObject.GetComponentInChildren<AudioManager>();
                AudioManager.Init();

                PoolObjectLoader = gameObject.AddComponent<PoolObjectLoader>();
                VFXManager = gameObject.AddComponent<VFXManager>();
                VFXManager.Init();

                SaveLoadManager = new SaveLoadManager();
                RegisterManagerForSaveLoader();

                SettingsManager.Init();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                throw ex;
            }
        }

        private void RegisterManagerForSaveLoader()
        {
            SaveLoadManager.RegisterManager<CharactersManager, CharactersSaveData>(
                CharactersManager,
                mgr => mgr.ConvertDataForSave(),
                (mgr, data) => mgr.ReinitDataFromSaveAsync(data)
);

            SaveLoadManager.RegisterManager<CreditsManager, CreditsSaveData>(
                CreditsManager,
                mgr => mgr.ConvertDataForSave(),
                (mgr, data) => mgr.ReinitDataFromSaveAsync(data)
            );

            SaveLoadManager.RegisterManager<MissionsManager, MissionsSaveData>(
                MissionsManager,
                mgr => mgr.ConvertDataForSave(),
                (mgr, data) => mgr.ReinitDataFromSaveAsync(data)
            );

            SaveLoadManager.RegisterManager<ShopManager, ShopSaveData>(
                ShopManager,
                mgr => mgr.ConvertDataForSave(),
                (mgr, data) => mgr.ReinitDataFromSaveAsync(data)
            );

            SaveLoadManager.RegisterManager<InventoryManager, InventorySaveData>(
                InventoryManager,
                mgr => mgr.ConvertDataForSave(),
                (mgr, data) => mgr.ReinitDataFromSaveAsync(data)
            );

            SaveLoadManager.RegisterManager<SettingsManager, SettingsData>(
                SettingsManager,
                mgr => mgr.GetSettings(),
                (mgr, data) => { mgr.SetupSettings(data); return UniTask.CompletedTask; }
            );
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}