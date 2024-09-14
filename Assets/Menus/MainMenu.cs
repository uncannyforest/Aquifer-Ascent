using UnityEngine.SceneManagement;

public class MainMenu : Menu {
    override public string[] Buttons { get => new string[] {"New cave"}; }

    override public void OnPressed(int num) {
        switch (num) {
            case 0: 
                GameManager.I.CloseMainMenu();
                GameManager.I.StartLevel();
                break;
        }
    }
}
