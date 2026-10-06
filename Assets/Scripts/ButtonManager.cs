using TMPro;
using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    public GameObject[] views;

    public GameObject skinObject;
    public MeshRenderer skin;
    public Material skinOpaque;
    public Material skin75;
    public Material skin50;
    public Material skin25;
    public TextMeshProUGUI skinText;

    private Material[] skinMaterials;
    private int skinState = 0;

    void Start()
    {
        skinMaterials = new[] { skinOpaque, skin25, skin50, skin75 };
        ApplySkinState();
        ShowView(1);
    }

    public void ShowView(int view_num)
    {
        int index = view_num - 1;
        for (int i = 0; i < views.Length; i++)
            views[i].SetActive(i == index);
    }

    public void NextTransparency()
    {
        skinState = (skinState + 1) % 5;
        ApplySkinState();
    }

    private void ApplySkinState()
    {
        bool visible = skinState < 4;
        if (visible) { 
            skinObject.SetActive(true);
            skin.material = skinMaterials[skinState];
        } 
        else {
            skinObject.SetActive(false);
        }
        skinText.text = $"{skinState * 25}%";
    }

}
