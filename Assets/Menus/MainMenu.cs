using UnityEngine.SceneManagement;

public class MainMenu : Menu {
    override public string[] Buttons { get => new string[] {"Start"}; }

    override public void OnPressed(int num) {
        switch (num) {
            case 0: 
                SceneManager.LoadScene("PermanentElements", LoadSceneMode.Single);
                break;
        }
    }
}
