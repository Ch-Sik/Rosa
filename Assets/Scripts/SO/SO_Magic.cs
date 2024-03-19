using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 더블 점프 등의 스킬들을 추가할 지 몰라서 Magic을 Skill의 하위 분류로 구분
[CreateAssetMenu(fileName = "Skill_0", menuName = "Skill/Magic")]
public class SO_Magic : SO_Skill
{
    public Sprite previewSprite;
    public MagicCastType castType;
    public float lifeTime;
    public float seedRechargeTime;
    public GameObject prefab;
}
