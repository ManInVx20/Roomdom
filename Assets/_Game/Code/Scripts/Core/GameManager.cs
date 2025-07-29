using VinhLB.Utilities;

namespace VinhLB
{
    public class GameManager : PersistentMonoSingleton<GameManager>
    {
        public enum GameState
        {
            InGame,
            GameOver,
        }

        public GameState CurrentGameState { get; set; }

        public bool IsInGameState => CurrentGameState == GameState.InGame;
    }
}