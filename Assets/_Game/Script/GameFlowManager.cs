// File: GameFlowManager.cs
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Luzart
{
    public class GameFlowManager : MonoBehaviour
    {
        public List<Storyboard> Storyboards;
        private List<Storyboard> _instantiatedStoryboards = new List<Storyboard>();
        public int _currentStoryboardIndex;

        private void Start()
        {
            int length = Storyboards.Count;
            for (int i = 0; i < length; i++)
            {
                int index = i;
                Storyboard storyboard = Storyboards[index];
                var sbInstance = Instantiate(storyboard, transform);
                sbInstance.InitStoryBoard(index);
                _instantiatedStoryboards.Add(sbInstance);
            }
            RunCurrentStoryboard();
        }

        private void RunCurrentStoryboard()
        {
            if (_currentStoryboardIndex < 0 || _currentStoryboardIndex >= Storyboards.Count)
            {
                Debug.Log("Đã hoàn thành tất cả storyboard");
                return;
            }

            var sb = _instantiatedStoryboards[_currentStoryboardIndex];
            sb.gameObject.SetActive(true);
            sb.StartStoryboard(nextIndex =>
            {
                _currentStoryboardIndex = _currentStoryboardIndex + 1;
                RunCurrentStoryboard();
            });
        }
    }
}
