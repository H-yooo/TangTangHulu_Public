using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Fever : MonoBehaviour
{
    [SerializeField] private Slider feverGaugeSlider;
    [SerializeField] private GameObject feverStartImage;
    [SerializeField] private GameObject fruitZone;
    private Animator fruitZoneAnimator;

    private float feverDuration = 15f; // Fever 모드 유지 시간
    private float _gaugeIncrease = 1.0f; // Fever 게이지 획득량 (0.1배수)
    private float _defaultGaugeDecreaseRate = 1.5f; // 기본 감소 속도
    private float gaugeDecreaseRate;

    public bool isFeverModeActive = false; // Fever 모드 활성 상태

    private Image fillImage; // 슬라이더 Fill Area의 이미지
    private float colorChangeSpeed = 3f; // 색상 변화 속도

    public float GaugeIncrease
    {
        get { return _gaugeIncrease; }
        set
        {
            _gaugeIncrease = value;
        }
    }

    public float DefaultGaugeDecreaseRate
    {
        get { return _defaultGaugeDecreaseRate; }
        set
        {
            _defaultGaugeDecreaseRate = value;
        }
    }

    private void Start()
    {
        fillImage = feverGaugeSlider.fillRect.GetComponent<Image>();
        fruitZoneAnimator = fruitZone.GetComponent<Animator>();
    }

    private void Update()
    {
        if(true)
        {
            if (feverGaugeSlider.value > 0)
            {
                feverGaugeSlider.value -= gaugeDecreaseRate * Time.deltaTime * 0.01f;
                feverGaugeSlider.value = Mathf.Max(feverGaugeSlider.value, 0); // 0보다 낮아지지 않게 보정
            }
        }
        else
        {
            // 업그레이드 획득 시 적용
        }

        if(!isFeverModeActive)
        {
            return;
        }
        else
        {
            UpdateRainbowColor();
        }
    }

    public void InitializeFeverGuageDecrease()
    {
        isFeverModeActive = false;
        feverGaugeSlider.value = 0f;
        gaugeDecreaseRate = _defaultGaugeDecreaseRate;
        ResetSliderColor();

        if (fruitZoneAnimator != null)
        {
            fruitZoneAnimator.SetBool("isFeverMode", false);
        }
    }

    public void IncreaseFeverGauge()
    {
        if (isFeverModeActive) return;

        feverGaugeSlider.value += _gaugeIncrease * 0.1f;
        feverGaugeSlider.value = Mathf.Min(feverGaugeSlider.value, feverGaugeSlider.maxValue);
    }

    public void StartFeverMode()
    {
        if (isFeverModeActive) return; // 이미 Fever 모드 중이면 중복 발동 방지

        isFeverModeActive = true;

        if (fruitZoneAnimator != null)
        {
            fruitZoneAnimator.SetBool("isFeverMode", true);
        }

        //Fever 이미지 출력
        StartCoroutine(FeverAnimationCoroutine());

        // Fever 모드 관련 효과
        CustomerManager.Instance.SetCustomerFever(true); // 골드 획득량 1.5배 증가
        FruitManager.Instance.MoveSpeed = 14.0f; // 탕후루 꽂히는 속도 1.5배 증가(중력조정)

        // 감소 속도 계산: 게이지를 duration 동안 0으로 만들도록 설정
        gaugeDecreaseRate = (feverGaugeSlider.value * 100) / feverDuration;

        StartCoroutine(FeverModeCoroutine());
    }

    private IEnumerator FeverModeCoroutine()
    {
        while (feverGaugeSlider.value > 0)
        {
            yield return null; // 게이지 끝날 때까지 대기
        }

        EndFeverMode();
    }

    private void EndFeverMode()
    {
        isFeverModeActive = false;

        if (fruitZoneAnimator != null)
        {
            fruitZoneAnimator.SetBool("isFeverMode", false);
        }

        // Fever 종료 후 원상복구
        InitializeFeverGuageDecrease();
        CustomerManager.Instance.SetCustomerFever(false);
        FruitManager.Instance.MoveSpeed = 8.0f;

        feverGaugeSlider.value = 0;
    }

    public bool IsFeverGaugeFull()
    {
        return feverGaugeSlider.value >= feverGaugeSlider.maxValue;
    }

    private void UpdateRainbowColor()
    {
        float hue = Mathf.PingPong(Time.time * colorChangeSpeed, 1f); // 시간에 따라 색상 변화
        Color rainbowColor = Color.HSVToRGB(hue, 1f, 1f); // HSV 공간에서 무지개색 생성
        fillImage.color = rainbowColor;
    }

    private void ResetSliderColor()
    {
        fillImage.color = Color.green;
    }

    private IEnumerator FeverAnimationCoroutine()
    {
        feverStartImage.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        feverStartImage.SetActive(false);
    }
}
