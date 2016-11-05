/********************************************************************
	created:	2014/09/05
	created:	5:9:2014   20:37
	file base:	ClientGlobalDefines
	file ext:	cs
	author:		jazzyzhu
	
	purpose:	This file is used to contain all kinds of definitions used between files.
*********************************************************************/

using System;
using UnityEngine;


public enum ModelDirection
{
    LeftDirection = -1,
    None = 0,
    RightDirection = 1,
}

public enum eCharStatus
{
    Idle = 0,                    // 状态触发AIAction
    AnyStatus = 10,              // AI逻辑 临时无关紧要的状态  
    Move = 20,                   // AI逻辑
    MoveToTarget = 30,           // 状态触发AIAction
    MoveToPos = 40,              // 状态触发AIAction


    SmallHurt = 100,            // 状态触发AIAction
    Hurt = 110,                 // 状态触发AIAction

    NormalAtkSkill = 200,       // AI逻辑
    MeleeAtkSkill = 210,        // AI逻辑
    CastSkill = 220,            // AI逻辑
    CastButtonSkill = 230,      // AI逻辑

    FrozenAction = 300,         // 状态触发AIAction  冰冻，禁止行动
    ForceIdle = 310,            // 状态触发AIAction  强制idle
    Dead = 320,                 // 状态触发AIAction
    Summon = 400,               // 状态触发AIAction  被召唤单位出生技能
}

/// <summary>
/// kantxu(徐健):
/// 0--接受任何受击
/// 1--只接受大受击
/// 2--不接受任何受击
/// </summary>
public enum eMonsterBodyType
{
    SmallBody = 0,
    MiddleBody = 1,
    BigBody = 2,
}

public enum eMoveType
{
    None,
    MoveForward,
    MoveUp,
    MoveDown,
}


// kantxu(徐健):1=自己；2=己方单位；3=敌方单位；4=场景技能特效(一个特效)，目标是全部友军；5= 场景技能特效(一个特效)，目标是全部敌人；
// 6=目标是全部友军,每个友军一个特效；7=目标是全部敌人,每个敌人一个特效；8=我方血量最少1个；9=我方血量最少3个；10=敌方血量最少1个；11=敌方血量最少3个
public enum SkillTargetType
{
    // 没有目标
    TARGET_NONE,

    // 自己
    UNIT_SELF = 1,

    // 己方单位
    UNIT_FRIEND = 2,

    // 敌方单位
    UNIT_ENEMY = 3,

    // 己方阵营（共享特效）
    SELF_ALL_SHARE_EFFECT = 4,

    // 敌方阵营（共享特效）
    ENEMY_ALL_SHARE_EFFECT = 5,

    // 己方阵营
    SELF_ALL_EVERY_EFFECT = 6,

    // 敌方阵营
    ENEMY_ALL_EVERY_EFFECT = 7,

    // 技能目标
    SKILL_TARGET = 8,

    // 己方阵营血量最小的1个单位
    SELF_MIN_HP_1 = 9,

    // 己方阵营血量最小3个单位
    SELF_MIN_HP_3 = 10,

    // 敌方阵营血量最小的1个单位
    ENEMY_MIN_HP_1 = 11,

    // 敌方阵营血量最小3个单位
    ENEMY_MIN_HP_3 = 12,

    // 自身召唤物
    SUMMON_MONSTERS = 13,

    // 自己和自身召唤物
    SELF_SUMMON_MONSTERS = 14,

    // 友方全体远程
    SELF_ALL_RANGE = 15,

    // 友方全体近战
    SELF_ALL_MELEE = 16,
}


/// <summary>
/// 使用道具时的目标选择策略
/// 当影响人数不为1时，除了指定单位外，其他单位选择的策略。
/// 1=血量最低的1个单位优先，2=血量最高的1个单位优先，3=血量最低的2个单位优先，4=血量最高的2个单位优先。在优先之后的流程，则为己方全体单位随机。
/// </summary>
public enum ItemSelectPolicy
{
    LowestHp1 = 1,
    HighestHp1 = 2,
    LowestHp2 = 3,           //
    HighestHp2 = 4,         //
}

public enum RangeType
{
    Melee = 1,
    Range = 2,
    Heal = 3,
}


public enum AreaType
{
    Circle = 1, // 圆形状
    Square = 2, // 长条形状
    Single = 3, // 单个     
    EnemyArea = 4, // 敌方区域
    FriendArea = 5, // 我方区域
    AllArea = 6, // 所有区域
}

// 技能目标选择策略
public enum SelectTargetPolicy
{
    Warrior = 1, //! 战士
    Priest = 2, //! 牧师
    Mage = 3, //! 法师
    Knight = 4, //! 骑士
    Archer = 5, //! 弓手
    Melee = 6, //! 战士，骑士
    Range = 7, //! 弓手，法师
}

public struct MapPosition
{
    public static readonly MapPosition Zero = new MapPosition(0, 0);

    public static readonly MapPosition Max = new MapPosition(int.MaxValue, int.MaxValue);

    MapPosition(int x, int y)
    {
        PosX = x;
        PosY = y;
    }

    public int PosX;
    public int PosY;

    public static MapPosition operator +(MapPosition lVar, MapPosition rVal)
    {
        MapPosition grid = new MapPosition();
        grid.PosX = lVar.PosX + rVal.PosX;
        grid.PosY = lVar.PosY + rVal.PosY;

        return grid;
    }

    public static MapPosition operator -(MapPosition lVar, MapPosition rVal)
    {
        MapPosition grid = new MapPosition();
        grid.PosX = lVar.PosX - rVal.PosX;
        grid.PosY = lVar.PosY - rVal.PosY;

        return grid;
    }

    public static bool operator ==(MapPosition first, MapPosition second)
    {
        if (first.PosX == second.PosX
            && first.PosY == second.PosY)
        {
            return true;
        }

        return false;
    }

    public static bool operator !=(MapPosition first, MapPosition second)
    {
        if (first.PosX != second.PosX
            || first.PosY != second.PosY)
        {
            return true;
        }

        return false;
    }

    public bool Equals(MapPosition other)
    {
        return PosX == other.PosX && PosY == other.PosY;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        return obj is MapPosition && Equals((MapPosition)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (PosX * 397) ^ PosY;
        }
    }
}

public class ClientGlobalDefines
{
    public const float MapCellWidth = 0.5f;

    public const float HalfMapCellWidth = 0.25f;




    public const float MaxAttackRange = 12.0f;

    public const float MinAttackRange = 0.01f;

    public static readonly Vector3 InvisiblePosition = new Vector3(-999999, -999999, -999999);

    public static float StandardWidth = 960;
    public static float StandardHeight = 640;

    public const string ColorEnd = "[-]";

    public static readonly System.DateTime StartDateTime = new System.DateTime(1970, 1, 1, 0, 0, 0);


    public const ushort MaxMapGridRow = 3;
    public const ushort MaxMapGridColumn = 6;

    // 怪物预警时间
    public const float SkillEarlyWarningTime = 3f;

    // 角色默认半径
    public const float DefaultColliderRadius = 1.5f;

    public const string CharacterBoneBody = "Bip001 Spine1";
    public const string CharacterBoneLeftHand = "Bip001 L Hand";
    public const string CharacterBoneRightHand = "Bip001 R Hand";
    public const string CharacterBoneWeaponLeftHandTag = "weaponTag02";
    public const string CharacterBoneWeaponRightHandTag = "weaponTag";
    public const string CharacterBoneWingTag = "wingTag";

    public const string ShadowChildName = "shadow";
    public const string ModelChildName = "model";


    public const int MaxEventCountPerWave = 1000;

    public const float SecondsPerFrame = (1.0f/30.0f);

    public const float ServerPosUnitRatio = 100.0f;

    public const float ToonOutline = 0.5f;

    public static readonly Color ToonOutColor = new Color(0.0f, 0, 0, 0.78f);

    public const int CartoonShadingLod = 1000;

    public const int BaseShadingLod = 500;

    public const string DontUseColorKey = "NO_COLOR";
    public const string UseColorKey = "USE_COLOR";
    public const string GrayColorKey = "GRAY_COLOR";

    // 战斗默认的游戏速度
    public const float DefaultGameSpeed = 1f;

    public const string UseItemLimitString = "家园等级达到{0}级，解锁战斗中使用道具。";

    public enum ServerPlayerState
    {
        Live = 1,
        Dead = 2,
    }

    public const float GridLogicLength = 2.0f;


    //默认英雄头像
    public const string DefaultHeroIconPic = "jierui_1_h";

    //默认英雄插画
    public const string DefaultHeroBigPic = "";

    //默认装备icon
    public const string DefaultEquipIconPic = "4011011";

    //默认道具icon
    public const string DefalutItemIconPic = "4050001";

    // 异步时最多等待时间，单位是毫秒
    public const int MaxAsyncWaitTime = 10000;

    public const int BackupHeroBornEffectId = 7710002;

    public const int MaxTryMergeCount = 1;

    // 暴击，攻速相关的常量，后面做成配置
    public const float ConstantA = 2000;
    public const float ConstantB = 300;
    public const float ConstantC = 1;
    public const float ConstantD = 1;

    public const float FactorA = 220;
    public const float FactorB = 16;
    public const float FactorC = 1;
}
