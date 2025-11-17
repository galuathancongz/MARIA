namespace Luzart
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class UITutorial : UIBase
    {
        public ScreenTutorial[] screenTutorials;

        private void Awake()
        {
            int length = screenTutorials.Length;
            for (int i = 0; i < length; i++)
            {
                screenTutorials[i].actionHide = Hide;
            }
        }
        protected override void Setup()
        {
            base.Setup();
            int length = screenTutorials.Length;
            for (int i = 0; i < length; i++)
            {
                screenTutorials[i].Setup();
            }
        }
        public override void Show(Action onHideDone)
        {
            base.Show(onHideDone);
        }
        private void DisableAllScreenTutorials()
        {
            int length = screenTutorials.Length;
            for (int i = 0; i < length; i++)
            {
                screenTutorials[i].gameObject.SetActive(false);
            }
        }
        public virtual void ShowScreenTutorial(int index, GameObject ob, Action actionClick)
        {
            Action _actionClick = () =>
            {
                actionClick?.Invoke();
            };
            DisableAllScreenTutorials();
            screenTutorials[index].gameObject.SetActive(true);
            screenTutorials[index].Show(_actionClick);
            screenTutorials[index].SpawnItem(ob);
        }
    }

}