using System;
using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour
{
    public GameObject doorObject;

    GameObject _activator = null;

    Animator _animator;
    bool _isNear = false;

    bool _isOpened = false;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        InputManager.Instance.OnPickUpInput += _OnInteraction;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {
            _activator = other.gameObject;

            _isNear = true;
        }
    }

    void _OnInteraction(object sender, EventArgs e)
    {
        if (_isNear)
        {
            _Interaction(_activator.transform);
        }
    }


    void _Interaction(Transform activator)
    {
        Vector3 activatorDirection = activator.forward;
        Vector3 doorDirection = transform.right;

        Debug.Log(Vector3.Dot(activatorDirection, doorDirection));

        if (!_isOpened)
        {
            if (Vector3.Dot(activatorDirection, doorDirection) < 0)
            {
                _animator.SetTrigger("OpenForward");
                _isOpened = true;
            }
            else if (Vector3.Dot(activatorDirection, doorDirection) > 0)
            {
                _animator.SetTrigger("OpenBackward");
                _isOpened = true;
            }
        }
        else
        {
            _animator.SetTrigger("Close");
            _isOpened = false;
        }

        
    }

    //IEnumerator OpenDoorInCoroutine()
    //{
    //    float goalDirection = -90f;

    //    while(doorObject.transform.rotation.eulerAngles.y > goalDirection)
    //    {
    //        doorObject.transform.Rotate(0f, 0f, -30f * Time.deltaTime);
    //        yield return null;
    //    }
    //}
}
