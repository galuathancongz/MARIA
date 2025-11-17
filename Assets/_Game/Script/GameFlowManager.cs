// File: GameFlowManager.cs
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    public List<Storyboard> Storyboards;
    private int _currentStoryboard;

    private void Start()
    {
        int length = Storyboards.Count;
        for (int i = 0; i < length; i++)
        {
            int index = i;
            Storyboard storyboard = Storyboards[index];
            storyboard.InitStoryBoard(index);
        }
        RunCurrentStoryboard();
    }

    private void RunCurrentStoryboard()
    {
        if (_currentStoryboard < 0 || _currentStoryboard >= Storyboards.Count)
        {
            Debug.Log("Đã hoàn thành tất cả storyboard");
            return;
        }

        var sb = Storyboards[_currentStoryboard];
        sb.gameObject.SetActive(true);
        sb.StartStoryboard(nextIndex =>
        {
            _currentStoryboard = _currentStoryboard + 1;
            RunCurrentStoryboard();
        });
    }
}