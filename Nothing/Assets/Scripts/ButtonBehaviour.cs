using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonBehaviour : MonoBehaviour
{
    public Image buttonBkgImage;

    public List<ButtonBehaviour> allButtons;

    public Color selectColor;

    public ItemType itemButton;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Unselect()
    {
        buttonBkgImage.color = Color.white;
    }

    public void ClickOnButton()
    {
        foreach (ButtonBehaviour but in allButtons)
        {
            but.Unselect();
        }
        buttonBkgImage.color = selectColor;

        GameEngineBehaviour.instance.SetNextSpawnItemType(itemButton);
    }
}
