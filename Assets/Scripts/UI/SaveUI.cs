using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveUI : MonoBehaviour
{
    [SerializeField] private float uiShowDuration = 1f;
    [SerializeField] private Animator anim;

    private void Start()
    {
        gameObject.SetActive(false);
    }
    
    public void ShowSaveUI()
    {
        gameObject.SetActive(true);
        StartCoroutine(CoSaveUI());
    }

    IEnumerator CoSaveUI()
    {
        yield return new WaitForSeconds(uiShowDuration);
        anim.SetTrigger("Out");
        yield return new WaitForSeconds(0.33f);
        gameObject.SetActive(false);
    }
}
