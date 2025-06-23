using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardEntryUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI nicknameText;
    [SerializeField] private TextMeshProUGUI scoreText;

    public void SetData(int rank, string nickname, int score)
    {
        if (rankText != null)
            rankText.text = "#" + rank.ToString();
        if (nicknameText != null)
            nicknameText.text = nickname;
        if (scoreText != null)
            scoreText.text = score.ToString();
    }

    public void Reset()
    {
        if (rankText != null)
            rankText.text = "-";
        if (nicknameText != null)
            nicknameText.text = "-";
        if (scoreText != null)
            scoreText.text = "-";
    }

    void Awake() { }

    void Update() { }
}