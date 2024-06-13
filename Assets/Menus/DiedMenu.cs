using UnityEngine.SceneManagement;

public class DiedMenu : Menu {
    override public string[] Buttons { get => new string[] {"Continue", "See map", "Main menu"}; }

    override public void OnPressed(int num) {
        switch (num) {
            case 0: 
                GameManager.I.Restart();
                break;
            case 1: 
                GameManager.I.SeeMap();
                break;
            case 2: 
                SceneManager.LoadScene("Start", LoadSceneMode.Single);
                break;
        }
    }
}
