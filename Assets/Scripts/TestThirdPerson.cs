using System.Runtime.CompilerServices;
using UnityEngine;
public class TestThirdPerson : MonoBehaviour
{
    private Animator _animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (_animator.GetBool("Sitting"))
            {
                _animator.SetBool("Sitting", false);
            }
            else
            {
                _animator.SetBool("Sitting", true);
            }
        }
    }
}
