using System;
using Object = UnityEngine.Object;

//!注意，由于配置文件中记录了枚举数值，如果要添加新类型，只能在后边添加，请不要中途插入
public enum eAssetType
{
    //!注意，由于配置文件中记录了枚举数值，如果要添加新类型，只能在后边添加，请不要中途插入
    Config,
    Scene,

    UIPrefab,
    Atlas,
    //!注意，由于配置文件中记录了枚举数值，如果要添加新类型，只能在后边添加，请不要中途插入
    Character_Hero,
    Equip,
    FightObject,
    Effect,

    Audio,
    //!注意，由于配置文件中记录了枚举数值，如果要添加新类型，只能在后边添加，请不要中途插入
    FightSkill,
    FightCamera,
    FightEffectSet,

    //场景预设
    Level,

    //裸的大图片，不一定与UI有关，用作UITexture，卡牌，背景等等
    BigTex,

    // AI配置
    AiConfig,

    Character_Enemy,

    TextureTavern,
    TextureBigItem,

    HeadIcons,
    ChildView,
    Newbie,


}

public delegate void AssetLoaded(IAsset asset);

public interface IAsset : IDisposable
{
    string AssetName { get; set; }
    string BundlePathKey { get; set; }

    eAssetType AssetType { get; set; }
    Type ValueType { get; set; }

    bool IsDone { get; set; }
    Object Value { get; set; }

    Object Instantiate();

    //是否常驻于内存，在DisposeAll时，是否被释放
    bool IsStay { get; set; }

    event AssetLoaded OnAssetLoaded;
}
