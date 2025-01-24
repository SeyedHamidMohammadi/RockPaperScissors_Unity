using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreBoard : MonoBehaviour
{
    [Header("Access")]
    public CanvasGroup ui;
    
    [Header("List")]
    public ScrollRect scroll;
    public Transform content;
    public List<ScoreItem> items;
    public ScoreItem itemPrefab;

    [Header("Data")] 
    public Data data;
    
    [System.Serializable]
    public class Data
    {
        public List<string> gameStatus = new List<string>();
    }
    
    void Start()
    {
        GameController.SetUI(ui, false, true);
        
        // Load
        string json = PlayerPrefs.GetString("Data");

        if (PlayerPrefs.HasKey("Data"))
            data = JsonUtility.FromJson<Data>(json);
        else
            data.gameStatus.Clear();

        SaveGame();
    }

    public void OpenScoreBoard()
    {
        SetupList();
        GameController.SetUI(ui, true);
    }
    
    public void CloseScoreBoard()
    {
        GameController.SetUI(ui, false);
    }
    
    public void SaveGame()
    {
        Debug.Log("<color=green> Data: Save </color>");
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("Data", json);
        PlayerPrefs.Save();
    }
    
    void SetupList()
    {
        DeActiveAll();
        
        for (int i = 0; i < data.gameStatus.Count; i++)
            AddItem(i+1, data.gameStatus[i]);
        
        for (int i = 0; i < items.Count; i++)
        {
            if (!items[i].active)
                items[i].transform.SetSiblingIndex(items.Count-1);
        }
    }
    
    #region List
    void ScrollToTop()
    {
        Canvas.ForceUpdateCanvases();
        scroll.verticalNormalizedPosition = 1f;
    }

    void DeActiveAll()
    {
        for (int i = 0; i < items.Count; i++)
        {
            items[i].active = false;
            items[i].UpdateUI();
        }
    }

    public void AddItem(int num, string status)
    {
        ScoreItem item = null;
        
        for(int i=0; i < items.Count; i++)
        {
            if (!items[i].active)
            {
                item = items[i];
                break;
            }
        }

        if (item == null)
        {
            item = Instantiate(itemPrefab, content);
            items.Add(item);
        }

        item.active = true;
        item.num = num;
        item.status = status;
        item.UpdateUI();

        item.transform.SetSiblingIndex(items.Count-1);

        Debug.Log($"<color=yellow>Score Item Added: {item.num} </color>");
        ScrollToTop();
    }
    #endregion
}
