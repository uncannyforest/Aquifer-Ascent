using UnityEngine;
using UnityEngine.UI;

public class SaveMenu : Menu {
    public string copiedText = "Copied! Now save in any text editor for safekeeping";
    public InputField code;

    override public string[] Buttons { get => new string[] {"Copy to clipboard", "Back"}; }

    override public void OnPressed(int num) {
        switch (num) {
            case 0: 
                if (code.text == copiedText) break;
                GUIUtility.systemCopyBuffer = code.text;
                code.text = copiedText;
                break;
            case 1:
                GameManager.I.SwitchMenu(GameManager.Mode.DEAD_MENU);
                break;
        }
    }

    void OnEnable() {
        this.code.text = "" + GameManager.I.initSeed;
    }
}
