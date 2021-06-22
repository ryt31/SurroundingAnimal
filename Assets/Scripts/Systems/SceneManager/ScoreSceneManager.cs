using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;


public class ScoreSceneManager : MonoBehaviour
{
    GameManager GameManager;

    private int Score;
    [SerializeField] private Text ScoreText = null;

    void Start()
    {
        GameManager = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
        Score = CaucScore();
        ScoreText.text = Score.ToString();
    }

    int CaucScore() => GameManager.AnimalScore.Dog + GameManager.AnimalScore.Cat + GameManager.AnimalScore.Fox + GameManager.AnimalScore.Raccoon;

    public void LoadScore()
    {
        naichilab.RankingLoader.Instance.SendScoreAndShowRanking(Score, 0);
    }

    public void LoadTitleScene()
    {
        GameManager.ChangeScene(SceneType.Title);

    }
}
