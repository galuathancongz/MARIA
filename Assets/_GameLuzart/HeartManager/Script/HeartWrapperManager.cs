using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Luzart
{
    public partial class HeartManager 
    {
        private void StartFuncionWrapper()
        {
            //EventDispatcher.Register<LevelLoseEvent>(OnLoseLevel);
            //EventDispatcher.Register<LevelReplayEvent>(OnReplayLevel);
        }
        private void DestroyFuncionWrapper()
        {
            //EventDispatcher.UnRegister<LevelLoseEvent>(OnLoseLevel);
            //EventDispatcher.UnRegister<LevelReplayEvent>(OnReplayLevel);
        }

        //private void OnLoseLevel(LevelLoseEvent data)
        //{
        //    if(EStateHeart == EStateHeart.Infinite)
        //    {
        //        return;
        //    }
        //    AddHeartNone(-1);
        //}
        //private void OnReplayLevel(LevelReplayEvent data)
        //{
        //    if (EStateHeart == EStateHeart.Infinite)
        //    {
        //        return;
        //    }
        //    AddHeartNone(-1);
        //}



    }
}

