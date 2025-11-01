using System.Collections;
using TMPro;
using UnityEngine;

public class Loading_Text : MonoBehaviour
{
    private TMP_Text text;
    [SerializeField] private string[] messages;
    private float interval = 0.3f;

    private int currentIndex = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        text = GetComponent<TMP_Text>();

        if(messages.Length > 0 && text != null)
        {
            StartCoroutine(SwitchText());
        }
    }

    private IEnumerator SwitchText()
    {
        while (true)
        {
            text.text = messages[currentIndex];
            currentIndex = (currentIndex + 1) % messages.Length;
            yield return new WaitForSeconds(interval);
        }
    }
}
