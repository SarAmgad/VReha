using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{
    private float time;

    // Start is called before the first frame update
    void Start()
    {
        time = 0;
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
    }

    public void SaveTime(string key)
    {
        PlayerPrefs.DeleteKey(key);
        PlayerPrefs.SetFloat(key, time);
    }
}
