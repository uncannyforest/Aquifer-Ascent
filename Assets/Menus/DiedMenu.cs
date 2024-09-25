using UnityEngine.SceneManagement;

public class DiedMenu : Menu {
    override public string[] Buttons { get => new string[] {"Retry this cave", "View map", "Save this cave", "Main menu"}; }

    override public void OnPressed(int num) {
        switch (num) {
            case 0: 
                GameManager.I.RestartWithSeed(GameManager.I.initSeed);
                break;
            case 1: 
                GameManager.I.SeeMap();
                break;
            case 2: 
                GameManager.I.SwitchMenu(GameManager.Mode.SAVE);
                break;
            case 3: 
                GameManager.I.EnterFoyer();
                break;
        }
    }
}
