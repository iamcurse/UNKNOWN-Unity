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
    
    public int winScore = 5;
    
    [SerializeField] private Flag flag;

    private void Awake()
    {
        _winTeamID.OnChange += OnGameFinished;
    }

    private void OnDestroy()
    {
        _winTeamID.OnChange -= OnGameFinished;
    }

    private void Update()
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
        if (_team1Score.Value > winScore)
        {
            Debug.Log("Team 1 wins!");
            // Handle team 1 win
            
            _winTeamID.Value = 1;
        }
        else if (_team2Score.Value > winScore)
        {
            Debug.Log("Team 2 wins!");
            // Handle team 2 win
            
            _winTeamID.Value = 2;
        }
    }
    
    [ObserversRpc]
    private void OnScoreChanged(int team1, int team2)
    {
        // Update UI or show score animation
        Debug.Log($"Score Updated: Red = {team1}, Blue = {team2}");
    }
    
    private void OnGameFinished(int previous ,int winTeamID, bool asServer)
    {
        if (asServer) return;
        switch (winTeamID)
        {
            // Handle game finished logic
            case 1:
                Debug.Log("Team 1 wins!");
                break;
            case 2:
                Debug.Log("Team 2 wins!");
                break;
        }

        // Reset the game or show end screen
        // Handle game finished logic
        Debug.Log("Game Finished!");
    }
}
