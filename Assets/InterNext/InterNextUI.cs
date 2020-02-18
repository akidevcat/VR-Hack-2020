using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InterNextUI : MonoBehaviour
{
    public InterNextCore INCore;
    public List<Button> Buttons;
    public Scrollbar Sbar;
    public Button NextPage;
    public Button PrevPage;
    public Text InfoText;
    private string[] SplittedInfoText;
    private int page = 0;
    private int elementsPage = 0;

    private string IntArrToStr(int[] arr)
    {
        string s = "";
        for (int i = 0; i < arr.Length; i++)
        {
            s += arr[i];
            if (i < arr.Length - 1)
                s += ", ";
        }
        return s;
    }

    public void Initialize()
    {
        SplittedInfoText = INCore.LoadedInfo.SplitPages();
        UpdatePage();
    }

    public void UpdatePage()
    {
        InfoText.text = SplittedInfoText[page];
    }

    public void ShowNextPage()
    {
        if (page < SplittedInfoText.Length - 1)
            page++;
        UpdatePage();
    }

    public void ShowPrevPage()
    {
        if (page > 0)
            page--;
        UpdatePage();
    }

    public void ShowNextElementsPage()
    {
        if (elementsPage < INCore.LoadedDevice.details_groups.Count - 1)
            elementsPage++;
        OnSbarUpdate();
    }

    public void ShowPrevElementsPage()
    {
        if (elementsPage > 0)
            elementsPage--;
        OnSbarUpdate();
    }

    public void OnSbarUpdate()
    {
        //int firstIndex = (int)(Sbar.value * (INCore.LoadedDevice.details_groups.Count - 1));
        int firstIndex = elementsPage;
        for (int i = firstIndex; i < firstIndex + 5; i++)
        {
            if (i >= INCore.LoadedDevice.details_groups.Count + 1)
            {
                Buttons[i - firstIndex].gameObject.SetActive(false);
            } else
            {
                Buttons[i - firstIndex].gameObject.SetActive(true);
                if (i > 0)
                {
                    var dg = INCore.LoadedDevice.details_groups[i - 1];
                    Buttons[i - firstIndex].transform.GetChild(0).GetComponent<Text>().text = dg.name;
                    Buttons[i - firstIndex].transform.GetChild(1).GetComponent<Text>().text = $"Show Details [{IntArrToStr(dg.details_id)}] {(dg.hide_others ? "Only" : "")}";
                    Buttons[i - firstIndex].onClick.RemoveAllListeners();
                    Buttons[i - firstIndex].onClick.AddListener(delegate() { INCore.ShowGroup(dg.details_id, dg.hide_others); });
                }
                else
                {
                    Buttons[i - firstIndex].transform.GetChild(0).GetComponent<Text>().text = "All";
                    Buttons[i - firstIndex].transform.GetChild(1).GetComponent<Text>().text = "Show All Details";
                    Buttons[i - firstIndex].onClick.RemoveAllListeners();
                    Buttons[i - firstIndex].onClick.AddListener(delegate () { INCore.ShowAll(); });
                }
            }
        }
    }
}
