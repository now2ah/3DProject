using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class MainSpotLight : MonoBehaviour
{
    Animator _animator;

    private void Start()
    {
        _animator = GetComponent<Animator>();
    }
}
