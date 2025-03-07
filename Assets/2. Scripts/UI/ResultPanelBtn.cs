using UnityEngine;
using UnityEngine.UI;

public class ResultPanelBtn : MonoBehaviour
{
    [SerializeField] private Button reduceExpenditureBtn;
    [SerializeField] private Button shopBtn;

    private void Start()
    {
        reduceExpenditureBtn.onClick.AddListener(OnReduceExpenditureClicked);
        shopBtn.onClick.AddListener(OnVillageBtnClicked);
    }

    // 지출 감소 버튼 클릭 시 동작
    private void OnReduceExpenditureClicked()
    {
        GameManager.Instance.ReduceExpenditure();
        reduceExpenditureBtn.interactable = false; // 버튼 클릭 불가

        // 버튼 흐리게 처리 (alpha값 변경)
        ColorBlock colors = reduceExpenditureBtn.colors;
        colors.disabledColor = new Color(colors.normalColor.r, colors.normalColor.g, colors.normalColor.b, 0.5f);  // 50% 투명도
        reduceExpenditureBtn.colors = colors;
    }

    // 결산완료 버튼 클릭 시 동작
    private void OnVillageBtnClicked()
    {
        reduceExpenditureBtn.interactable = true;
        GameManager.Instance.CloseResultPanel();
        VillageManager.Instance.OpenVillage();
        GameManager.Instance.SaveGameData();
    }
}
