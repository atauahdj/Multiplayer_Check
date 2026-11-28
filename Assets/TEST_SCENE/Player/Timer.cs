using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Timer : NetworkBehaviour
{
    TMP_Text timer_text;
    float time_second;
    int minutes;
    void Start()
    {
        timer_text = GameObject.FindGameObjectWithTag("Timer_Text").GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer) return;
        time_second += Time.deltaTime;
        if(time_second > 59)
        {
            minutes++;
            time_second = 0;
        }
        timer_text.text = (minutes < 10 ? $"0{minutes}:" : $"{minutes}:") + ((time_second < 9.5f ? $"0{Mathf.Round(time_second)}" : $"{Mathf.Round(time_second)}"));
    }
}
