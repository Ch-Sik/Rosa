using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateOtherInteractiveObject : MonoBehaviour
{
    [SerializeField] InteractiveObject interactiveObject;

    public void Activate()
    {
        interactiveObject.canInteract = true;
    }
}
