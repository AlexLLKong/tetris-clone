using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    private int score = 0;
    [SerializeField]
    private int level = 1;
    private int lines = 0;
    private int nextLevel = 10;
    private int highScore = 0;
    [SerializeField]
    private TextMeshProUGUI scoreDisplay;
    [SerializeField]
    private TextMeshProUGUI levelDisplay;
    [SerializeField]
    private TextMeshProUGUI linesDisplay;
    [SerializeField]
    private TextMeshProUGUI highScoreDisplay;
    public Action<int> clearedLine;
    public Action softDrop;
    public Action<int> hardDrop;
    void Start()
    {
        updateScore();
        loadHighScore();
    }
    private void OnEnable()
    {
        clearedLine += onClearedLine;
        softDrop += onSoftDrop;
        hardDrop += onHardDrop;
    }
    private void OnDisable()
    {
        clearedLine -= onClearedLine;
        softDrop -= onSoftDrop;
        hardDrop -= onHardDrop;
    }
    private void onClearedLine(int numlines)
    {
        if (numlines == 1)
            score += 100 * level;
        else if (numlines == 2)
            score += 300 * level;
        else if (numlines == 3)
            score += 500 * level;
        else
            score += 800 * level;
        lines += numlines;
        checkForLevelUp();
        updateLines();
        updateScore();
    }
    private void onSoftDrop()
    {
        score++;
        updateScore();
    }
    private void onHardDrop(int numDropped)
    {
        score += 2 * numDropped;
        updateScore();
    }
    private void updateScore()
    {
        scoreDisplay.SetText(score.ToString());
        if (score >= highScore)
        {
            highScore = score;
            highScoreDisplay.SetText(highScore.ToString());
        }
    }
    private void updateLines()
    {
        linesDisplay.SetText(lines.ToString());
    }
    private void updateLevel()
    {
    levelDisplay.SetText(level.ToString());
    }
    private void checkForLevelUp()
    {
        if (lines >= nextLevel)
        {
            level++;
            nextLevel += 10;
            updateLevel();
        }
    }
    public int getLevel()
    {
        return level;
    }
    public int getScore()
    {
        return score;
    }
    public int getHighScore()
    {
        return highScore;
    }
    public void loadHighScore()
    {
        if(PlayerPrefs.HasKey("highScore"))
        {
            highScore = PlayerPrefs.GetInt("highScore");
        }
        highScoreDisplay.SetText(highScore.ToString());
    }
}
