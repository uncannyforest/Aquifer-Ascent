using UnityEngine;
using UnityEngine.UI;

public class LoadMenu : Menu {
    public InputField code;

    override public string[] Buttons { get => new string[] {"Paste from clipboard", "Load", "Back"}; }

    override public void OnPressed(int num) {
        switch (num) {
            case 0: 
                code.text = GUIUtility.systemCopyBuffer;
                break;
            case 1: 
                TryLoad();
                break;
            case 2:
                GameManager.I.SwitchMenu(GameManager.Mode.FOYER);
                break;
        }
    }

    private void TryLoad() {
        bool parsed = int.TryParse(code.text, out int seed);
        if (parsed) {
            GameManager.I.LeaveFoyer();
            GameManager.I.StartLevel(seed);
        } else {
            code.transform.Find("Placeholder").GetComponent<Text>().text = "Code must be a number with no dot, try again";
            code.text = "";
        }
    }
}
