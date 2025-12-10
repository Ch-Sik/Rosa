using System;

//Struct 써도 되는데, 비교 함수랑 저장 편의를 위해 클래스 사용
[Serializable]
public class OptionSetting
{
    public int window;
    public int resolution;
    public float vol;
    public float bgm;
    public float sfx;

    public OptionSetting MakeCopy()
    {
        return new OptionSetting()
        {
            window = window,
            resolution = resolution,
            vol = vol,
            bgm = bgm,
            sfx = sfx
        };
    }

    public bool isEqual(OptionSetting another)
    {
        if (window != another.window ||
            resolution != another.resolution ||
            vol != another.resolution ||
            bgm != another.bgm ||
            sfx != another.sfx)
            return false;

        return true;
    }

    //생성자로 Default Option 생성
    public OptionSetting()
    {
        window = 1;
        resolution = 1;
        vol = 0.5f;
        bgm = 0.5f;
        sfx = 0.5f;
    }
}