using UnityEngine;
using UnityEngine.UI;

public class OPtionPanelCanvas : GenericSingleton<OPtionPanelCanvas>
{
    [SerializeField] private GameObject OptionPanel;
    [SerializeField] private Button ClosePanelBtn;

    private void Start()
    {
        ClosePanelBtn.onClick.AddListener(CloseOptionPanel);
    }

    public void CloseOptionPanel()
    {
        OptionPanel.SetActive(false);
    }
}
