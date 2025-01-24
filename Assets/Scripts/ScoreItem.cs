using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreItem : MonoBehaviour
{
    [Header("General")]
    public bool active;
    public string status;
    public int num;
    
    [Header("Access")]
    public CanvasGroup ui;
    public Text statusTextUI;
    
    void Start()
    {
        UpdateAlpha();
    }

    void UpdateAlpha()
    {
        ui.alpha = active ? 1 : 0;
    }

    public void UpdateUI()
    {
        UpdateAlpha();

        statusTextUI.text = $"<color=grey>{num} .</color>  {status}";
        statusTextUI.color = status == "win" ? Color.green : Color.red;
    }
}
