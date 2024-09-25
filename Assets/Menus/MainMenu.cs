using UnityEngine.SceneManagement;

public class MainMenu : Menu {
    override public string[] Buttons { get => new string[] {"New cave", "Load cave"}; }

    override public void OnPressed(int num) {
        switch (num) {
            case 0: 
                GameManager.I.LeaveFoyer();
                GameManager.I.StartLevel();
                break;
            case 1: 
                GameManager.I.SwitchMenu(GameManager.Mode.LOAD);
                break;
        }
    }
}
