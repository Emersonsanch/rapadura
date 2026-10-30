using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.Mounts
{
    /// <summary>
    /// One mount species (Docs/Mounts.md §12.1), "data as asset" like <c>ItemDefinition</c>/<c>ShopDefinition</c>,
    /// loaded from <c>Resources/Mounts</c> by <see cref="MountCatalog"/>, which falls back to the code-built starters
    /// when no asset exists (so the bake is optional). Every number the speed/damage rules need is here; the rules
    /// themselves are <see cref="MountRules"/> (pure, shared with a future server).
    /// <para>Deviation from §12.1: the whistle is referenced by item id (<see cref="WhistleItemId"/>) instead of an
    /// <c>ItemDefinition</c> reference, and the visual recipe is an enum of the code recipes.</para>
    /// </summary>
    [CreateAssetMenu(fileName = "NewMount", menuName = "Rapadura/Mounts/Mount Definition", order = 0)]
    public class MountDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _mountId = "mount_new";
        [SerializeField] private string _displayNameKey = "mount.new.name";
        [SerializeField] private string _descriptionKey = "mount.new.desc";
        [SerializeField] private string _loreKey = "mount.new.lore";
        [SerializeField] private MountType _type = MountType.Terrestre;
        [SerializeField] private ItemRarity _rarity = ItemRarity.Common;
        [SerializeField] private MountSize _size = MountSize.Medio;
        [SerializeField] private BiomeType _regionTag = BiomeType.Forest;
        [SerializeField] private string _sourceHintKey = "mount.new.source";

        [Header("Attributes (level 1)")]
        [Tooltip("× PlayerMotor run speed (5.5 m/s). Never grows with level.")]
        [SerializeField, Range(MountRules.MinSpeedMultiplier, MountRules.MaxSpeedMultiplier)] private float _speedMultiplier = 1.4f;
        [SerializeField, Min(1f)] private float _gallopMultiplier = MountRules.GallopMultiplier;
        [Tooltip("Defense PercentAdditive on the rider's BuffController (source \"mount\"), 0..0.15.")]
        [SerializeField, Range(0f, MountRules.MaxDefensePercent)] private float _defensePercent;
        [SerializeField, Min(1f)] private float _baseHealth = 100f;
        [SerializeField, Min(1f)] private float _baseStamina = 100f;
        [SerializeField, Range(0f, 1f)] private float _damageShareToMount = MountRules.DefaultDamageShare;
        [SerializeField] private bool _isWarMount;
        [SerializeField] private MountAbilityDefinition _ability;

        [Header("Care and cousins (M2)")]
        [SerializeField] private MountDiet _diet = MountDiet.Herbivora;
        [SerializeField] private bool _hasFavorite;
        [SerializeField] private CharacterId _favoriteCharacter;
        [SerializeField] private string _favoriteLineKey = string.Empty;
        [SerializeField] private MountTradeRule _tradeRule = MountTradeRule.Livre;

        [Header("Shop")]
        [SerializeField] private string _whistleItemId = "item_whistle_new";
        [SerializeField, Min(0)] private int _shopPrice;

        [Header("Visual")]
        [SerializeField] private MountVisualRecipe _visualRecipe = MountVisualRecipe.Pony;
        [Tooltip("Height (m) of the saddle above the ground: where the rider's hips sit.")]
        [SerializeField, Min(0.3f)] private float _seatHeight = 0.95f;
        [Tooltip("Saddle position along the body (m, +forward) relative to the mount pivot.")]
        [SerializeField] private float _seatForward;

        [Header("Sound (SfxEventMap cue ids)")]
        [SerializeField] private string _sfxCallKey = MountAudio.Whistle;
        [SerializeField] private string _sfxStepKey = MountAudio.HoofStep;
        [SerializeField] private string _sfxAbilityKey = MountAudio.AbilityReady;

        public string MountId => _mountId;
        public string DisplayNameKey => _displayNameKey;
        public string DescriptionKey => _descriptionKey;
        public string LoreKey => _loreKey;
        public MountType Type => _type;
        public ItemRarity Rarity => _rarity;
        public MountSize Size => _size;
        public BiomeType RegionTag => _regionTag;
        public string SourceHintKey => _sourceHintKey;
        public float SpeedMultiplier => _speedMultiplier;
        public float GallopMultiplier => _gallopMultiplier;
        public float DefensePercent => _defensePercent;
        public float BaseHealth => _baseHealth;
        public float BaseStamina => _baseStamina;
        public float DamageShareToMount => _damageShareToMount;
        public bool IsWarMount => _isWarMount;
        public MountAbilityDefinition Ability => _ability;
        public MountDiet Diet => _diet;
        public bool HasFavorite => _hasFavorite;
        public CharacterId FavoriteCharacter => _favoriteCharacter;
        public string FavoriteLineKey => _favoriteLineKey;
        public MountTradeRule TradeRule => _tradeRule;
        public string WhistleItemId => _whistleItemId;
        public int ShopPrice => _shopPrice;
        public MountVisualRecipe VisualRecipe => _visualRecipe;
        public float SeatHeight => _seatHeight;
        public float SeatForward => _seatForward;
        public string SfxCallKey => _sfxCallKey;
        public string SfxStepKey => _sfxStepKey;
        public string SfxAbilityKey => _sfxAbilityKey;

        /// <summary>Budget points of §2.4 (each species must hit its rarity's budget exactly).</summary>
        public float BudgetPoints => MountRules.BudgetPoints(_speedMultiplier, _defensePercent, _baseHealth, _baseStamina);

        /// <summary>Builds a definition from a <see cref="MountSpec"/> (the code catalog, the editor baker).</summary>
        public void Configure(MountSpec spec, MountAbilityDefinition ability)
        {
            _mountId = spec.MountId;
            _displayNameKey = "mount." + spec.MountId + ".name";
            _descriptionKey = "mount." + spec.MountId + ".desc";
            _loreKey = "mount." + spec.MountId + ".lore";
            _sourceHintKey = "mount." + spec.MountId + ".source";
            _type = spec.Type;
            _rarity = spec.Rarity;
            _size = spec.Size;
            _regionTag = spec.Region;
            _speedMultiplier = spec.Speed;
            _gallopMultiplier = MountRules.GallopMultiplier;
            _defensePercent = spec.Defense;
            _baseHealth = spec.Health;
            _baseStamina = spec.Stamina;
            _damageShareToMount = MountRules.DefaultDamageShare;
            _isWarMount = spec.IsWarMount;
            _ability = ability;
            _diet = spec.Diet;
            _hasFavorite = spec.Favorite.HasValue;
            _favoriteCharacter = spec.Favorite ?? default;
            _favoriteLineKey = spec.Favorite.HasValue ? "mount." + spec.MountId + ".favorite_line" : string.Empty;
            _tradeRule = spec.Rarity >= ItemRarity.Epic ? MountTradeRule.Vinculada : spec.Rarity == ItemRarity.Rare ? MountTradeRule.UmaVez : MountTradeRule.Livre;
            _whistleItemId = spec.WhistleItemId;
            _shopPrice = spec.ShopPrice;
            _visualRecipe = spec.Recipe;
            _seatHeight = spec.SeatHeight;
            _seatForward = spec.SeatForward;
            _sfxCallKey = MountAudio.Whistle;
            _sfxStepKey = MountAudio.HoofStep;
            _sfxAbilityKey = MountAudio.AbilityReady;
        }

        /// <summary>Code-built definition (lives in memory only).</summary>
        public static MountDefinition CreateRuntime(MountSpec spec, MountAbilityDefinition ability)
        {
            var definition = CreateInstance<MountDefinition>();
            definition.name = spec.MountId;
            definition.hideFlags = HideFlags.DontUnloadUnusedAsset;
            definition.Configure(spec, ability);
            return definition;
        }
    }
}
