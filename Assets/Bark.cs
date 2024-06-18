using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using TMPro;

public class Bark : MonoBehaviour
{
    public TextMeshPro message;
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, 3f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetMessage(string text)
    {
        message.SetText(text);
    }
}
