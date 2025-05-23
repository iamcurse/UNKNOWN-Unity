using System;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class CaptureTheFlag : NetworkBehaviour
{
    
    private readonly SyncVar<int> _team1Score = new();
    private readonly SyncVar<int> _team2Score = new();
    private readonly SyncVar<int> _winTeamID = new();
    
    // ReSharper disable once NotAccessedField.Local
    [ShowOnly][SerializeField] private int team1Score;
    // ReSharper disable once NotAccessedField.Local
    [ShowOnly][SerializeField] private int team2Score;
    private UIManager _uiManager;
    
    public int winScore = 5;
    
    [SerializeField] private Flag flag;

    

    private void Awake()
    {
        _uiManager = FindAnyObjectByType<UIManager>();
        _winTeamID.OnChange += OnGameFinished;
    }

    private void OnDestroy()
    {
        // Unsubscribe from the OnChange event to avoid memory leaks
        _winTeamID.OnChange -= OnGameFinished;
    }

    private void FixedUpdate()
    {
        team1Score = _team1Score.Value;
        team2Score = _team2Score.Value;
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddScore(int teamID)
    {
        switch (teamID)
        {
            case 1:
                _team1Score.Value++;
                break;
            case 2:
                _team2Score.Value++;
                break;
        }
        
        OnScoreChanged(_team1Score.Value, _team2Score.Value);
        CheckWinCondition();
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void CheckWinCondition()
    {
        if (_team1Score.Value >= winScore)
        {
            // Handle team 1 win
            
            _winTeamID.Value = 1;
        }
        else if (_team2Score.Value >= winScore)
        {
             // Handle team 2 win
            
            _winTeamID.Value = 2;
        }
    }
    
    [ObserversRpc]
    private void OnScoreChanged(int teamAScore, int teamBScore)
    {
        // Update UI
        _uiManager.UpdateScoreUI(teamAScore, teamBScore);
    }
    
    private void OnGameFinished(int previous ,int winTeamID, bool asServer)
    {
        if (asServer) return;
        switch (winTeamID)
        {
            // Logic after game finished
            case 1:
                Debug.Log("Red Team wins!");
                break;
            case 2:
                Debug.Log("Blue Team wins!");
                break;
        }

        // Reset the game or show end screen
        // Handle game finished logic
        Debug.Log("Game Finished!");
    }
}
