using UnityEngine;

public enum LR { LEFT, RIGHT }
public enum AudioType { BGM, SFX, UI }
public enum UiState
{
    IN_GAME,    // 별도의 메뉴가 열리지 않은 경우
    DIALOG,     // 대화 UI가 열린 경우
    MENU        // 인벤토리, 지도, 상점 등의 UI가 열린 경우
}


public static class EnumExtensions
{
    // LR
    public static bool isLEFT(this LR dir)
    {
        return dir == LR.LEFT;
    }
    public static bool isRIGHT(this LR dir)
    {
        return dir == LR.RIGHT;
    }
    public static LR opposite(this LR dir)
    {
        if (dir == LR.LEFT)
            return LR.RIGHT;
        else
            return LR.LEFT;
    }

    public static Vector2 toVector2(this LR dir)
    {
        return dir == LR.LEFT ? Vector2.left : Vector2.right;
    }

    public static float toFloat(this LR dir)
    {
        return dir == LR.LEFT ? -1f : 1f;
    }

    public static LR toLR(this Vector3 v)
    {
        return v.x < 0 ? LR.LEFT : LR.RIGHT;
    }

    public static LR toLR(this Vector2 v)
    {
        return v.x < 0 ? LR.LEFT : LR.RIGHT;
    }
}