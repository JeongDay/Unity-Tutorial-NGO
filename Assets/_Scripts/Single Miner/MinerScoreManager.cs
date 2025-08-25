using TMPro;
using UnityEngine;

public class MinerScoreManager : MonoBehaviour
{
    private int score;
    [SerializeField] private TextMeshProUGUI scoreUI;

    void Start()
    {
        scoreUI.text = $"Mineral Count : {score}";
    }
    
    public void AddScore()
    {
        score++;
        scoreUI.text = $"Mineral Count : {score}";
    }
}