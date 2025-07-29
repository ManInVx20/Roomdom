using TMPro;
using UnityEngine;

namespace VinhLB
{
    public class GameOverScreen : UIScreen
    {
        [SerializeField]
        private TMP_Text _titleText;

        public void Initialize(bool won)
        {
            Initialize();

            _titleText.text = won ? "WIN" : "LOSE";
        }
    }
}