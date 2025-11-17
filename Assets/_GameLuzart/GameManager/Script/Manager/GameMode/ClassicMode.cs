namespace Luzart
{
    public class ClassicMode : BaseMode
    {
        private UIGameplay uIGameplay;
    
        private int time = 20;
        private int timeDefault = 150;
        private bool isInit { get; set; } = false;
    
        public override void StartLevel(int level)
        {
            base.StartLevel(level);
        }
        
        protected override void OnWinGame()
        {
            base.OnWinGame();
            DataManager.Instance.GameData.level++;
            DataManager.Instance.SaveGameData();
        }
    
        protected override void OnLoseGame()
        {
            base.OnLoseGame();
        }
    }
}
