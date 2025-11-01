using UnityEngine;

public class TestBlock : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = new Ray(transform.position + new Vector3(0f,0.2f,0f), transform.forward);
        if (Physics.Raycast(ray, out var hit,0.5f))
        {
            Debug.Log(hit.transform.name);
        }
        Debug.DrawRay(transform.position, transform.forward * 0.5f, Color.red);
    }
}
