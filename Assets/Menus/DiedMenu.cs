using UnityEngine.SceneManagement;

public class DiedMenu : Menu {
    override public string[] Buttons { get => new string[] {"Restart", "Retry map", "View map", "Main menu"}; }

    override public void OnPressed(int num) {
        switch (num) {
            case 0: 
                GameManager.I.Restart();
                break;
            case 1: 
                GameManager.I.RestartWithSeed(CaveGrid.I.seed);
                break;
            case 2: 
                GameManager.I.SeeMap();
                break;
            case 3: 
                SceneManager.LoadScene("Start", LoadSceneMode.Single);
                break;
        }
    }
}
