using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingPlatformFactory : MonoBehaviour
{
    [SerializeField] GameObject prefab;
    [SerializeField] float startDelay = 0f;
    [SerializeField] float spawnInterval = 3f;
    [SerializeField] GameObject preview;

    private Sequence _spawnSequence;
    
    // Start is called before the first frame update
    void Start()
    {
        Destroy(preview);
        _spawnSequence = DOTween.Sequence().AppendInterval(startDelay).AppendCallback(
            () => {
            _spawnSequence = DOTween.Sequence()
                .AppendCallback(() =>
                {
                    Instantiate(prefab, transform.position, Quaternion.identity);
                })
                .AppendInterval(spawnInterval)
                .SetLoops(-1);
            }
        );
    }

    void OnDestroy()
    {
        if(_spawnSequence != null)
            _spawnSequence.Kill();
    }
}
