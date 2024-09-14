using UnityEngine.SceneManagement;

public class DiedMenu : Menu {
    override public string[] Buttons { get => new string[] {"Retry this cave", "View map", "Main menu"}; }

    override public void OnPressed(int num) {
        switch (num) {
            case 0: 
                GameManager.I.RestartWithSeed(CaveGrid.I.seed);
                break;
            case 1: 
                GameManager.I.SeeMap();
                break;
            case 2: 
                GameManager.I.MainMenu();
                break;
        }
    }
}
