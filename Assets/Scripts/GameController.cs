using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // برای استفاده از DOTween

public class GameController : MonoBehaviour
{
    [Header("Status")]
    // Points
    [SerializeField] private float playerPoints;
    [SerializeField] private float opponentPoints;
    // Hands
    [SerializeField] private int playerHandTypeId; //  0:Rock   1:Paper   2:Scissors
    [SerializeField] private int opponentHandTypeId;

    [Header("Options")]
    [SerializeField] private float playerHandPosX;
    [SerializeField] private float opponentHandPosX;
    [SerializeField] private float[] playerHandPosY;
    [SerializeField] private float[] opponentHandPosY;
    [SerializeField] private int targetPoint = 5;

    [Header("Access")]
    public Sprite[] handSprites;
    public SpriteRenderer playerHand;
    public SpriteRenderer opponentHand;
    public CanvasGroup startUI;
    public CanvasGroup gameUI;
    public CanvasGroup resultUI;
    public CanvasGroup actionButtonsUI;
    public Image rockButtonImage;
    public Image paperButtonImage;
    public Image scissorsButtonImage;
    public Text statusTextUI;
    public Text playerPointsTextUI;
    public Text opponentPointsTextUI;
    public Text resultTitleTextUI;
    public Text resultMessageTextUI;

    [Header("Color")]
    public Color activeButtonColor;
    public Color deActiveButtonColor;
    
    // other
    private float statusTextPosY_default;
    private ScoreBoard _scoreBoard;

    private void Awake()
    {
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        _scoreBoard = FindFirstObjectByType<ScoreBoard>();
        
        statusTextPosY_default = statusTextUI.transform.position.y;
        
        SetUI(startUI, true, true);
        SetUI(gameUI, false, true);
        SetUI(resultUI, false, true);
        
        SetUI(actionButtonsUI, false, true);

        SetHandPosY(playerHand.transform, playerHandPosY[0], true);
        SetHandPosY(opponentHand.transform, opponentHandPosY[0], true);

        SetPosX(playerHand.transform, playerHandPosX);
        SetPosX(opponentHand.transform, opponentHandPosX);
        
        UpdatePointsUI();
        ResetTimer();
    }

    public void Resign()
    {
        StartCoroutine(GetResult(true));
    }

    public void StartTurn()
    {
        playerPoints = 0;
        opponentPoints = 0;
        UpdatePointsUI();
        
        SetUI(startUI, false);
        SetUI(gameUI, true);
        SetUI(resultUI, false);

        ResetTurn();
    }

    void ResetTurn()
    {
        playerHandTypeId = 0;
        opponentHandTypeId = 0;
        UpdateHandSprites();
        
        SetUI(actionButtonsUI, true);
        
        SetHandPosY(playerHand.transform, playerHandPosY[1]);
        SetHandPosY(opponentHand.transform, opponentHandPosY[1]);
        
        UpdateActionButtons();
        StartCoroutine(StartTimer());
    }

    public void SetPlayerHandType(int typeId)
    {
        playerHandTypeId = typeId;
        UpdateActionButtons();
    }

    IEnumerator GetResult(bool resign = false)
    {
        StopCoroutine(StartTimer());
        SetUI(actionButtonsUI, false, false, 0.01f);
        
        SetHandPosY(playerHand.transform, playerHandPosY[2]);
        SetHandPosY(opponentHand.transform, opponentHandPosY[2]);

        if (!resign)
        {
            opponentHandTypeId = UnityEngine.Random.Range(0, 3);
            UpdateHandSprites();

            SetPosY(statusTextUI.transform, statusTextPosY_default - 125);

            switch (DetermineWinner())
            {
                case "win":
                {
                    statusTextUI.text = "<color=yellow>You won</color>";
                    playerPoints += 1;
                }
                    break;

                case "lose":
                {
                    statusTextUI.text = "<color=red>You lose</color>";
                    opponentPoints += 1;
                }
                    break;

                case "equal":
                {
                    statusTextUI.text = "<color=white>Equal</color>";
                }
                    break;
            }

            UpdatePointsUI();
            yield return new WaitForSeconds(2);
        }
        
        if (playerPoints < targetPoint && opponentPoints < targetPoint && !resign)
            ResetTurn();
        else
        {
            string status = playerPoints >= targetPoint ? "win" : "lose";
            _scoreBoard.data.gameStatus.Add(resign ? "lose" : status);
            _scoreBoard.SaveGame();
            
            resultTitleTextUI.text = status == "win" && !resign ? "You won" : "You lose";
            resultMessageTextUI.text = status == "win" && !resign ? "congratulations !" : "";
            
            SetUI(resultUI, true);
        }
    }
    
    private string DetermineWinner()
    {
        if (playerHandTypeId == opponentHandTypeId)
            return "equal";

        if ((playerHandTypeId == 0 && opponentHandTypeId == 2) || 
            (playerHandTypeId == 2 && opponentHandTypeId == 1) || 
            (playerHandTypeId == 1 && opponentHandTypeId == 0))
        {
            return "win";
        }
        else
        {
            return "lose";
        }
    }

    private void ResetTimer()
    {
        statusTextUI.text = "";
    }

    private IEnumerator StartTimer()
    {
        SetPosY(statusTextUI.transform, statusTextPosY_default);
        statusTextUI.text = "";
        yield return new WaitForSeconds(3);
        int current = 3;

        while (current > 0)
        {
            statusTextUI.text = current == 3 ? "Rock" : (current == 2 ? "Paper" : "Scissors");
            yield return new WaitForSeconds(1);
            current -= 1;
        }

        statusTextUI.text = "";
        StartCoroutine(GetResult());
    }

    private void SetHandPosY(Transform target, float posY, bool force = false)
    {
        if (force)
            target.position = new Vector2(target.position.x, posY);
        else
            StartCoroutine(MoveHand(target, posY));
    }

    private IEnumerator MoveHand(Transform target, float posY)
    {
        while (Mathf.Abs(target.position.y - posY) > 0.1f)
        {
            target.position = new Vector2(target.position.x, Mathf.MoveTowards(target.position.y, posY, 0.01f));
            yield return null;
        }
    }

    public static void SetUI(CanvasGroup ui, bool active, bool force = false, float duration = 0.5f)
    {
        if (force)
        {
            ui.alpha = active ? 1 : 0;
            ui.gameObject.SetActive(active);
            return;
        }
        
        if (active)
            ui.gameObject.SetActive(true);
        
        ui.DOFade(active ? 1 : 0, duration).OnComplete(() => ui.gameObject.SetActive(active));
    }

    private void SetPosX(Transform target, float posX)
    {
        target.position = new Vector2(posX, target.position.y);
    }
    
    private void SetPosY(Transform target, float posY)
    {
        target.position = new Vector2(target.position.x, posY);
    }

    private void SetImageColor(Image image, Color targetColor)
    {
        image.DOColor(targetColor, 0.5f);
    }

    private void SetScale(Transform target, float targetScale)
    {
        target.DOScale(targetScale, 0.5f);
    }

    void UpdateHandSprites()
    {
        playerHand.sprite = handSprites[playerHandTypeId];
        opponentHand.sprite = handSprites[opponentHandTypeId];
    }

    void UpdateActionButtons()
    {
        SetImageColor(rockButtonImage, playerHandTypeId == 0 ? activeButtonColor : deActiveButtonColor);
        SetImageColor(paperButtonImage, playerHandTypeId == 1 ? activeButtonColor : deActiveButtonColor);
        SetImageColor(scissorsButtonImage, playerHandTypeId == 2 ? activeButtonColor : deActiveButtonColor);

        float normalScale = 0.4f;
        float activeScale = 0.55f;
        
        SetScale(rockButtonImage.transform, playerHandTypeId == 0 ? activeScale : normalScale);
        SetScale(paperButtonImage.transform, playerHandTypeId == 1 ? activeScale : normalScale);
        SetScale(scissorsButtonImage.transform, playerHandTypeId == 2 ? activeScale : normalScale);
    }

    void UpdatePointsUI()
    {
        playerPointsTextUI.text = playerPoints.ToString();
        opponentPointsTextUI.text = opponentPoints.ToString();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}