using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimeManager : GenericSingleton<TimeManager>
{
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private Image timeImage;
    [SerializeField] private GameObject timerPlusMinus;
    [SerializeField] private TextMeshProUGUI timerPlusMinusText;
    private Coroutine timerPlusMinusCoroutine;

    [SerializeField] private float stageTime;
    [SerializeField] private float currentTime = 40;  // 현재 라운드 시간 (기본값 30초)
    public float maxTime = 90;  // 라운드 최대 시간
    public float minTime = 40;   // 라운드 최소 시간

    private bool isTimerRunning = false;

    // 나중에 Upgrade로 병합
    private float successTime = 0.1f;
    private float failTime = 1f;

    public float GetCurrentTime()
    {
        return currentTime;
    }

    public bool IsTimerRunning
    {
        get { return isTimerRunning; }
    }

    public void SetCurrentTime(float newTime)
    {
        currentTime = Mathf.Clamp(newTime, minTime, maxTime);
        Debug.Log("현재 시간이 설정되었습니다: " + currentTime);
    }

    private void Start()
    {
        isTimerRunning = false;
    }

    private void Update()
    {
        if (isTimerRunning && stageTime > 0)
        {
            stageTime -= Time.deltaTime;
            UpdateTimeUI();
        }
        else if (stageTime <= 0 && isTimerRunning)
        {
            stageTime = 0;
            isTimerRunning = false;

            // 스테이지 종료 후 결과창 표시
            GameManager.Instance.ShowResults();
        }
    }

    public void StartTimer()
    {
        stageTime = currentTime;
        isTimerRunning = true;
        UpdateTimeUI();
    }

    public void ResetTimer()
    {
        stageTime = currentTime;
        UpdateTimeUI();
    }

    public void DelayStartTimer(float delay)
    {
        StartCoroutine(DelayTimerCoroutine(delay));
    }

    private IEnumerator DelayTimerCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartTimer();
    }

    private void UpdateTimeUI()
    {
        if (timeText != null)
        {
            timeText.text = Mathf.CeilToInt(stageTime).ToString();
        }

        if (timeImage != null)
        {
            timeImage.fillAmount = stageTime / currentTime;
        }
    }

    internal void IncreaseRoundTime(float time)
    {
        SetCurrentTime(currentTime + time);
    }

    internal void DecreaseRoundTime(float time)
    {
        SetCurrentTime(currentTime - time);
    }

    public void TimerPlusMinusUI(bool isSuccess)
    {
        if (isSuccess)
        {
            stageTime += successTime;
            timerPlusMinusText.color = Color.green; // 초록색
            timerPlusMinusText.text = $"+{successTime}초";
        }
        else
        {
            stageTime -= failTime;
            timerPlusMinusText.color = Color.red; // 빨간색
            timerPlusMinusText.text = $"-{failTime}초";
        }

        // 이미 실행 중인 코루틴이 있으면 중지
        if (timerPlusMinusCoroutine != null)
        {
            StopCoroutine(timerPlusMinusCoroutine);
            timerPlusMinus.SetActive(false);
        }

        // 새로운 코루틴 실행
        timerPlusMinusCoroutine = StartCoroutine(TimerPlusMinusCoroutine());
    }

    private IEnumerator TimerPlusMinusCoroutine()
    {
        timerPlusMinus.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        timerPlusMinus.SetActive(false);
        timerPlusMinusCoroutine = null; // 코루틴 상태 초기화
    }
}