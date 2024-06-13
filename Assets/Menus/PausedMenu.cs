using UnityEngine.SceneManagement;

public class PausedMenu : Menu {
    override public string[] Buttons { get => new string[] {"Resume", "Forfeit"}; }

    override public void OnPressed(int num) {
        switch (num) {
            case 0: 
                GameManager.I.Unpause();
                break;
            case 1: 
                GameManager.I.Die();
                break;
        }
    }
}
