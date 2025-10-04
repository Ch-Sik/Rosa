using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawnTree : MonoBehaviour
{
    [SerializeField] private int maxItemCount = 1;
    [SerializeField] private GameObject itemPrefab;

    [SerializeField] private Transform[] spawnPositions;

    [Title("Shake vfx 관련")]
    [SerializeField] private Transform shakeEffectTarget;
    [SerializeField] private float shakeDuration;
    [SerializeField] private float shakePower;
    [SerializeField] private int shakeCount;

    private int curItemCount;
    private GameObject itemInstance = null;

    private void Start()
    {
        if (itemPrefab == null)
            Debug.LogError("Item Prefab not set");
        if (spawnPositions == null || spawnPositions.Length == 0)
            Debug.LogError("Item spawn position not set");
        curItemCount = maxItemCount;
    }

    public void OnInteraction()
    {
        DoShakeEffect();

        if (curItemCount == 0) return;
        if (itemInstance != null) return;

        Vector3 randomPosition = spawnPositions[Random.Range(0, spawnPositions.Length)].position;
        itemInstance = Instantiate(itemPrefab, randomPosition, Quaternion.identity);

        curItemCount--;
    }

    private void DoShakeEffect()
    {
        // 기존에 진행하던 트윈이 있다면 처리
        shakeEffectTarget.DOComplete();
        shakeEffectTarget.DOShakePosition(shakeDuration, shakePower, shakeCount);
    }
}
