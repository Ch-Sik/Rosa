using System;
using UnityEngine;

[Serializable]
public class CharacterEmotion
{
    public CommunicationTarget target;
    public string Name;
    public Sprite Normal;
    public Sprite Happy1;
    public Sprite Happy2;
    public Sprite Embarrassed1;
    public Sprite Embarrassed2;
    public Sprite Serious1;
    public Sprite Serious2;
    public Sprite Sad;
    public Sprite Sigh;
    public Sprite Mad;
    public Sprite Special;

    public Sprite GetEmotionImage(Emotion emotion)
    {
        Sprite ret = null;
        switch (emotion)
        {
            case Emotion.Normal: ret = Normal; break;
            case Emotion.Happy1: ret = Happy1; break;
            case Emotion.Happy2: ret = Happy2; break;
            case Emotion.Embarrassed1: ret = Embarrassed1; break;
            case Emotion.Embarrassed2: ret = Embarrassed2; break;
            case Emotion.Serious1: ret = Serious1; break;
            case Emotion.Serious2: ret = Serious2; break;
            case Emotion.Sad: ret = Sad; break;
            case Emotion.Sigh: ret = Sigh; break;
            case Emotion.Mad: ret = Mad; break;
            case Emotion.Special: ret = Special; break;
            default: return Normal;
        }
        Debug.Assert(ret != null, "CharacterEmotion: 해당 표정 스프라이트가 지정되어있지 않음");
        return ret;
    }

    public CharacterEmotion DeepCopy()
    {
        return new CharacterEmotion()
        {
            Name = this.Name,
            target = this.target,
            Normal = this.Normal,
            Happy1 = this.Happy1,
            Happy2 = this.Happy2,
            Embarrassed1 = this.Embarrassed1,
            Embarrassed2 = this.Embarrassed2,
            Serious1 = this.Serious1,
            Serious2 = this.Serious2,
            Sad = this.Sad,
            Sigh = this.Sigh,
            Mad = this.Mad,
            Special = this.Special,
        };
    }
}
