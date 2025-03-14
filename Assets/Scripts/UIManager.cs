using UnityEngine;

public enum EUIObject
{
    CROSSHAIR,
    RIFLE,
}

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private GameObject crossHairUI;
    [SerializeField] private GameObject rifleUI;
    [SerializeField] private GameObject bulletUI;

    public void SetActiveUI(EUIObject ui, bool isOn)
    {
        if (ui == EUIObject.CROSSHAIR)
        {
            crossHairUI.SetActive(isOn);
        }
        else if (ui == EUIObject.RIFLE)
        {
            rifleUI.SetActive(isOn);
        }
    }
}
