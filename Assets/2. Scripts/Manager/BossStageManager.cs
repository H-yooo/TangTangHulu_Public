using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossStageManager : GenericSingleton<BossStageManager>
{
    [SerializeField] MatchFruit matchFruit;
    [SerializeField] private GameObject bossStagePanel;
    [SerializeField] private GameObject bossStageResultPanel;
    [SerializeField] private GameObject subCamera;
    public Button bossStageUpgradeBtn;
    public Button bossStageShopBtn;

    [SerializeField] private TextMeshProUGUI[] topRankTextDisplays; // 1~5등 전광판 UI 텍스트 배열
    [SerializeField] private TextMeshProUGUI playerRankTextDisplay; // 플레이어 등수와 개수를 표시하는 텍스트

    private float spawnThreshold = 25f;
    private float spawnInterval = 10f;
    private Coroutine heightCheckCoroutine;

    public int playerRank;
    public int playerTanghuluCount = 0; // 플레이어의 탕후루 개수
    private int[] competitorCounts; // 경쟁자들의 탕후루 개수를 저장하는 배열
    private int[] competitorGoals; // 경쟁자들의 목표 탕후루 개수를 저장
    private float[] competitorIntervals; // 경쟁자들의 숫자 증가 주기를 저장

    private int lowerMin, lowerMax, middleMin, middleMax, upperMin, upperMax;

    public bool isBossStage = false;

    // 보스 스테이지 초기화 메서드
    public void StartBossStage(int index)
    {
        isBossStage = true;

        SetTanghuluRanges(index);
        InitializeCompetitorCounts();
        
        if (bossStagePanel == null)
        {
            bossStagePanel = GameObject.Find("BossStagePanel");
        }
        bossStagePanel.SetActive(true);
        
        if (subCamera == null)
        {
            subCamera = GameObject.FindWithTag("SubCamera");
        }
        subCamera.SetActive(true);

        // 카메라 높이 체크 시작
        StartSubCameraHeightCheck();

        ReinitializeTopRankTextDisplays();
        ReinitializePlayerRankTextDisplay();
        StartCoroutine(UpdateCompetitorCounts());

        // Tanghulu 스크립트 보스스테이지 설정
        Tanghulu.Instance.AdjustBossMatch();

        matchFruit.ShowFruitToMatch();
        VillageManager.Instance.OnOpenShop();

        SoundManager.Instance.PlayBGM("BossStage");
    }

    private void StartSubCameraHeightCheck()
    {
        // 카메라 높이 체크 코루틴 시작
        if (heightCheckCoroutine != null)
        {
            StopCoroutine(heightCheckCoroutine);
        }

        heightCheckCoroutine = StartCoroutine(CheckSubCameraHeight());
    }

    private IEnumerator CheckSubCameraHeight()
    {
        while (subCamera.activeSelf) // 카메라가 활성 상태일 때만 실행
        {
            float currentCameraHeight = subCamera.transform.position.y;

            if (currentCameraHeight >= spawnThreshold)
            {
                // 반복 이미지 생성
                StageManager.Instance.SpawnRepeatedImage();

                // 다음 스폰 높이 갱신
                spawnThreshold += spawnInterval;
            }

            yield return new WaitForSeconds(1.0f); // 주기적 체크
        }
    }

    private void SetTanghuluRanges(int index)
    {
        switch (index)
        {
            case 0:
                lowerMin = 1; lowerMax = 10;
                middleMin = 5; middleMax = 15;
                upperMin = 15; upperMax = 20;
                break;
            case 1:
                lowerMin = 5; lowerMax = 15;
                middleMin = 10; middleMax = 20;
                upperMin = 20; upperMax = 25;
                break;
            case 2:
                lowerMin = 10; lowerMax = 20;
                middleMin = 15; middleMax = 25;
                upperMin = 25; upperMax = 30;
                break;
            case 3:
                lowerMin = 15; lowerMax = 25;
                middleMin = 20; middleMax = 30;
                upperMin = 30; upperMax = 40;
                break;
            case 4:
                lowerMin = 20; lowerMax = 30;
                middleMin = 30; middleMax = 40;
                upperMin = 40; upperMax = 50;
                break;
            default:
                Debug.LogWarning($"Unhandled day: {index}");
                break;
        }
    }

    // 모든 경쟁자와 플레이어의 탕후루 개수를 0으로 초기화
    private void InitializeCompetitorCounts()
    {
        playerTanghuluCount = 0;
        competitorCounts = new int[9];
        competitorGoals = new int[9];
        competitorIntervals = new float[9];

        for (int i = 0; i < competitorCounts.Length; i++)
        {
            // 목표 숫자 설정
            if (i < 3)
                competitorGoals[i] = Random.Range(lowerMin, lowerMax + 1);
            else if (i < 6)
                competitorGoals[i] = Random.Range(middleMin, middleMax + 1);
            else
                competitorGoals[i] = Random.Range(upperMin, upperMax + 1);

            // 랜덤 증가 주기 설정
            competitorIntervals[i] = Random.Range(1.0f, 4.0f);

            // 초기 값 설정
            competitorCounts[i] = 0;
        }

        UpdateTopRanks();
    }

    // 경쟁자 탕후루 개수를 실시간으로 업데이트하는 메서드
    private IEnumerator UpdateCompetitorCounts()
    {
        yield return new WaitForSeconds(2.0f);

        List<Coroutine> competitorCoroutines = new List<Coroutine>();

        // 경쟁자별로 개별 코루틴 시작
        for (int i = 0; i < competitorCounts.Length; i++)
        {
            competitorCoroutines.Add(StartCoroutine(UpdateSingleCompetitorCount(i)));
        }

        // 플레이어 탕후루 개수 업데이트는 계속 진행
        while (TimeManager.Instance.GetCurrentTime() > 0)
        {
            UpdateTopRanks(); // 전체 순위 업데이트
            yield return null; // 한 프레임 대기
        }

        // 라운드 종료 후 모든 경쟁자 코루틴 중지
        foreach (var coroutine in competitorCoroutines)
        {
            StopCoroutine(coroutine);
        }
    }

    private IEnumerator UpdateSingleCompetitorCount(int index)
    {
        while (competitorCounts[index] < competitorGoals[index])
        {
            while (!TimeManager.Instance.IsTimerRunning)
            {
                yield return null; // 타이머가 실행 중이 아닐 경우 대기
            }

            yield return new WaitForSeconds(competitorIntervals[index]); // 경쟁자별 주기 적용

            // 타이머가 실행 중일 때만 카운트 증가
            if (TimeManager.Instance.IsTimerRunning)
            {
                competitorCounts[index]++;
            }
        }
    }

    public void AddTanghuluCount()
    {
        playerTanghuluCount++;
        UpdateTopRanks();

        if (playerTanghuluCount % 6 == 0)
        {
            TimeManager.Instance.TimerPlusMinusUI(true);
        }
    }

    // 상위 1~5등과 플레이어 순위를 UI에 업데이트하는 메서드
    private void UpdateTopRanks()
    {
        // 경쟁자와 플레이어의 결과 리스트를 합쳐서 정렬
        List<(string name, int count)> allCounts = new List<(string, int)>();
        for (int i = 0; i < competitorCounts.Length; i++)
        {
            allCounts.Add(($"Competitor {i + 1}", competitorCounts[i]));
        }
        allCounts.Add(("Player", playerTanghuluCount));

        // 탕후루 개수로 내림차순 정렬
        allCounts.Sort((a, b) => b.count.CompareTo(a.count));

        // 상위 1~5등을 UI에 업데이트
        for (int i = 0; i < topRankTextDisplays.Length && i < allCounts.Count; i++)
        {
            // 플레이어가 상위 1~5위 안에 있을 경우 빨간색으로 표시
            if (allCounts[i].name == "Player")
            {
                topRankTextDisplays[i].color = Color.red; // 글씨 색상을 빨간색으로 설정
            }
            else
            {
                topRankTextDisplays[i].color = Color.black; // 다른 경쟁자는 기본 색상 (예: 검정색)으로 설정
            }

            // 텍스트 업데이트
            topRankTextDisplays[i].text = $"{i + 1}등: {allCounts[i].count}개";
        }

        // 플레이어의 현재 등수와 개수 표시
        playerRank = allCounts.FindIndex(entry => entry.name == "Player") + 1;
        playerRankTextDisplay.text = $"My: {playerTanghuluCount}개";
    }

    private void ReinitializeTopRankTextDisplays()
    {
        // topRankTextDisplays가 null이거나 요소가 없을 때 초기화 시도
        if (topRankTextDisplays == null || topRankTextDisplays.Length == 0)
        {
            if (bossStagePanel != null)
            {
                // bossStagePanel의 자식들 중에서 경쟁자 순위 텍스트들을 찾기
                topRankTextDisplays = new TextMeshProUGUI[5];
                for (int i = 0; i < 5; i++)
                {
                    string elementName = $"BossStageCompetitor{i + 1}_Text"; // 경쟁자 텍스트 이름 생성
                    Transform childTransform = bossStagePanel.transform.Find(elementName);
                    if (childTransform != null)
                    {
                        topRankTextDisplays[i] = childTransform.GetComponent<TextMeshProUGUI>();
                    }
                }
            }
        }
    }

    private void ReinitializePlayerRankTextDisplay()
    {
        // playerRankTextDisplay가 null일 때 초기화 시도
        if (playerRankTextDisplay == null)
        {
            if (bossStagePanel != null)
            {
                // 플레이어 순위 텍스트 찾기
                Transform playerTextTransform = bossStagePanel.transform.Find("BossStagePlayer_Text");
                if (playerTextTransform != null)
                {
                    playerRankTextDisplay = playerTextTransform.GetComponent<TextMeshProUGUI>();
                }
            }
        }
    }

    public void SubCameraMoveUp()
    {
        if (Tanghulu.Instance != null && Tanghulu.Instance.GetFruitsOnStick().Count > 0)
        {
            // 마지막으로 추가된 과일 위치를 목표 위치로 설정
            GameObject lastFruit = Tanghulu.Instance.GetFruitsOnStick()[Tanghulu.Instance.GetFruitsOnStick().Count - 1];
            Vector3 targetPosition = lastFruit.transform.position;


            targetPosition.z = subCamera.transform.position.z;
            StartCoroutine(MoveCameraToTarget(targetPosition));
        }
    }

    private IEnumerator MoveCameraToTarget(Vector3 targetPosition)
    {
        while (Vector3.Distance(subCamera.transform.position, targetPosition) > 0.01f)
        {
            subCamera.transform.position = Vector3.Lerp(subCamera.transform.position, targetPosition, Time.deltaTime * 2f);
            yield return null;
        }
    }

    private IEnumerator DelaySubCameraOff(float time)
    {
        yield return new WaitForSeconds(time);
        subCamera.transform.position = new Vector3(0, 3.6f, -10);
        subCamera.SetActive(false);
    }

    // 보스 스테이지 종료 시 사용
    public void EndBossStage()
    {
        StopCoroutine(UpdateCompetitorCounts());

        if (heightCheckCoroutine != null)
        {
            StopCoroutine(heightCheckCoroutine);
        }

        bossStagePanel.SetActive(false);
        DelaySubCameraOff(1.0f);
        bossStageResultPanel.SetActive(false);
        Tanghulu.Instance.ChangeAdjustMatch();

        isBossStage = false;
    }
}
