using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    
    [SerializeField] private TMP_Text scoreText;
    public Image flagImage;
    
    private CaptureTheFlag _captureTheFlag;
    
    private void Awake()
    {
        // Initialize UI elements if needed
        if (scoreText == null)
            Debug.LogError("Score Text is not assigned in the inspector.");
        if (flagImage == null)
            Debug.LogError("Flag Image is not assigned in the inspector.");
        
        _captureTheFlag = FindAnyObjectByType<CaptureTheFlag>();
        
        UpdateScoreUI(0, 0);
    }
    
    // Update Score UI
    public void UpdateScoreUI(int team1Score, int team2Score)
    {
        // Update the score UI elements here
        Debug.Log($"Score Updated: Red = {team1Score}, Blue = {team2Score}");

        scoreText.text = $"<color=red>{team1Score}</color> I <color=blue>{team2Score}</color>";
    }
}
