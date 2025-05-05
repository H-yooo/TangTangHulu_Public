using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NotificationCanvas : GenericSingleton<NotificationCanvas>
{
    [SerializeField] private GameObject notificationPanel1; // Notification Panel 오브젝트
    [SerializeField] private TextMeshProUGUI notificationText1; // 알림 텍스트
    [SerializeField] private GameObject notificationPanel2;
    [SerializeField] private TextMeshProUGUI notificationText2;
    [SerializeField] private Button notificationYesBTN;
    [SerializeField] private Button notificationNoBTN;

    private Action onConfirm;
    private Action onCancel;

    private void Start()
    {
        notificationYesBTN.onClick.AddListener(OnYesButtonClicked);
        notificationNoBTN.onClick.AddListener(OnNoButtonClicked);
    }

    // 알림을 표시
    public void ShowNotificationPanel1(string message)
    {
        notificationText1.text = message; // 알림 메시지 설정
        notificationPanel1.SetActive(true); // Notification Panel 활성화

        // 2초 후 자동으로 비활성화
        Invoke("HideNotification", 2.0f);
    }

    public void ShowNotificationPanel2(string message, Action confirmCallback, Action cancelCallback = null)
    {
        notificationText2.text = message;  // 메시지 설정
        onConfirm = confirmCallback;  // 확인 콜백 설정
        onCancel = cancelCallback;    // 취소 콜백 설정
        notificationPanel2.SetActive(true);  // 패널 활성화
    }

    // Yes / No 콜백
    private void OnYesButtonClicked()
    {
        SoundManager.Instance.PlaySFX("Click");
        onConfirm?.Invoke();  // 확인 콜백 호출
        CloseNotificationPanel2();
    }

    private void OnNoButtonClicked()
    {
        SoundManager.Instance.PlaySFX("Click");
        onCancel?.Invoke();  // 취소 콜백 호출
        CloseNotificationPanel2();
    }


    // 알림 숨김
    private void HideNotification()
    {
        notificationPanel1.SetActive(false);
    }

    // 클릭하면 비활성화
    public void OnNotificationClicked()
    {
        CancelInvoke("HideNotification"); // 자동 비활성화 취소
        HideNotification(); // 즉시 비활성화
    }

    private void CloseNotificationPanel2()
    {
        notificationPanel2.SetActive(false);  // 패널 비활성화
        onConfirm = null;  // 콜백 초기화
        onCancel = null;
    }
}
