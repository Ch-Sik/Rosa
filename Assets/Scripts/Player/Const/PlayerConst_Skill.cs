/// <summary>
/// 플레이어 스킬관련 여러가지 상수 모음
/// </summary>

public enum SkillCode
{
    // 확장 가능성 (ex. 2단 점프 언락)을 고려하여 MAGIC_ 접두사 사용.
    MAGIC_IVY,
    MAGIC_CUCUMBER,
    MAGIC_MYOSOTIS,
    MAGIC_CISTUS,
    MAGIC_WATERMELON,
    MAGIC_MUSHROOM
}

/// <summary> 설치 가능 지형별로 구분한 식물마법 타입 </summary>
public enum MagicCastType
{
    /// <summary> 지면에다만 설치 가능 (오이, 파리지옥, 수박 등) </summary>
    GROUND_ONLY,
    /// <summary> 벽에다만 설치 가능 (담쟁이 등) </summary>
    WALL_ONLY,
    /// <summary> 천장에다만 설치 가능 (현재 해당되는 스킬 없음) </summary>
    CEIL_ONLY,
    /// <summary> 지면/벽/천장 아무데나 설치 가능 </summary>
    EVERYWHERE,
}