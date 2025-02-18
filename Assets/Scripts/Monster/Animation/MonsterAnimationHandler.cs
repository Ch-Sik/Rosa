using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;

public enum AnimParameterType { BOOL, INT, FLOAT, TRIGGER }

/// <summary>
/// Animator를 사용하는 몬스터 애니메이션을 컨트롤하는 스크립트
/// </summary>
public class MonsterAnimationHandler : MonoBehaviour
{
    [Serializable]
    public class MonsterAnimParameter
    {
        public string blackboardKey;
        public AnimParameterType parameterType;
        public string animParameter;
    }


    [SerializeField]
    private Blackboard blackboard;
    [SerializeField]
    private Animator animator;

    [SerializeField]
    private List<MonsterAnimParameter> parameters;

    // Start is called before the first frame update
    void Start()
    {
        blackboard.OnBlackboardUpdated += BlackboardUpdateHandler;
        animator.keepAnimatorStateOnDisable = true;     // 애니메이터 비활성화되어도 State/Param 리셋되지 않도록 설정
    }
    
    void BlackboardUpdateHandler(string key, object value)
    {
        MonsterAnimParameter keyParamPair = parameters.Find(x => x.blackboardKey == key);
        if(keyParamPair == null)
        {
            return;
        }

        try
        {
            switch (keyParamPair.parameterType)
            {
                case AnimParameterType.BOOL:
                    animator.SetBool(key, (bool)value);
                    break;
                case AnimParameterType.INT:
                    animator.SetInteger(key, (int)value);
                    break;
                case AnimParameterType.FLOAT:
                    animator.SetFloat(key, (float)value);
                    break;
                case AnimParameterType.TRIGGER:
                    if ((bool)value == true)
                        animator.SetTrigger(key);
                    break;
            }
        }
        catch 
        {
            Debug.LogError("설정한 애니메이터 파라미터가 존재하지 않습니다! 이름 또는 타입을 확인하세요!", this);
        }
    }

    [Button]
    void ApplyToAnimController()
    {
        if(animator == null) return;

        // 애니메이터 컨트롤러 가져오기.
        // Animator에서 런타임 인스턴스의 참조만 제공해서 파일 수정하려면 이렇게 가져와야 함.
        var runtimeController = animator.runtimeAnimatorController;
        if(runtimeController == null)
        {
            Debug.LogErrorFormat("RuntimeAnimatorController must not be null.");
            return;
        }
        var controller = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(UnityEditor.AssetDatabase.GetAssetPath(runtimeController));
        if(controller == null)
        {
            Debug.LogErrorFormat("AnimatorController must not be null.");
            return;
        }
        Debug.Log(controller);

        foreach(var param in parameters)
        {
            // 기존에 같은 이름을 가진 파라미터가 있을 경우 넘김
            if(controller.parameters.Any((el) => { return el.name == param.animParameter; }))
            {
                Debug.Log($"기존에 같은 이름의 파라미터가 있음: {param.animParameter}");
                continue;
            }

            var newParam = new AnimatorControllerParameter();
            newParam.name = param.animParameter;
            switch(param.parameterType)
            {
                case AnimParameterType.TRIGGER:
                    newParam.type = AnimatorControllerParameterType.Trigger;
                break;
                case AnimParameterType.BOOL:
                    newParam.type = AnimatorControllerParameterType.Bool;
                break;
                case AnimParameterType.INT:
                    newParam.type = AnimatorControllerParameterType.Int;
                break;
                case AnimParameterType.FLOAT:
                    newParam.type = AnimatorControllerParameterType.Float;
                break;
            }
            controller.AddParameter(newParam);
            Debug.Log($"새 애니메이터 파라미터 추가\nname: {param.animParameter}\ntype: {param.parameterType}");
        }
    }
}
