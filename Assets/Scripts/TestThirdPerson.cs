using System.Runtime.CompilerServices;
using UnityEngine;
public class TestThirdPerson : MonoBehaviour
{
    private Animator _animator;

    public Transform sitpoint;

    public Transform standpoint;

    private CharacterController characterController;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _animator = GetComponent<Animator>();

        characterController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (_animator.GetBool("Sitting"))
            {
                transform.position = standpoint.position;
                _animator.SetBool("Sitting", false);
                characterController.enabled = true;
            }
            else
            {
                _animator.SetBool("Sitting", true);
                characterController.enabled = false;
                transform.position = sitpoint.position;
                transform.rotation = sitpoint.rotation;
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            _animator.SetTrigger("Final");
        }
    }
}
