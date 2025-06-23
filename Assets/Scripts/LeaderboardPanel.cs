using UnityEngine;

public class LeaderboardPanel : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnEnable()
    {
        GameManager.Instance.leaderboardManager.GetLeaderboard();
    }
}

