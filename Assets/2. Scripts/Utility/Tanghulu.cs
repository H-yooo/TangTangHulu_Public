using System.Collections.Generic;
using UnityEngine;

public class Tanghulu : GenericSingleton<Tanghulu>
{
    [SerializeField] private Transform tanghuluStick; // 탕후루 꼬치의 위치 (-1.33f, 3.2f, 0)
    [SerializeField] private Transform tanghuluCup; // 탕후루 컵 위치, 과일이 꽂힐 꼬치 위치
    [SerializeField] private List<GameObject> availableFruits; // 사용 가능한 과일 데이터 리스트

    private List<GameObject> fruitsOnStick = new List<GameObject>(); // 꼬치에 꽂힌 과일 리스트
    [SerializeField] private int maxFruitCount = 3;
    private int currentFruitCount = 3; // 기본 최대 과일 개수
    [SerializeField] private GameObject Match3_Stick;
    [SerializeField] private GameObject Match4_Stick;
    [SerializeField] private GameObject Match5_Stick;

    public int MaxFruitCount
    {
        get => maxFruitCount;
        set => maxFruitCount = value;
    }

    public int CurrentFruitCount
    {
        get => currentFruitCount;
        set => currentFruitCount = value;
    }

    public IReadOnlyList<GameObject> GetFruitsOnStick()
    {
        return fruitsOnStick.AsReadOnly();
    }

    // Awake문 절대 지우지 말것...!
    protected override void Awake()
    {
    }

    public void ChangeGravityScale(float newGravityScale)
    {
        foreach (GameObject fruit in fruitsOnStick)
        {
            Rigidbody2D fruitRigidbody = fruit.GetComponent<Rigidbody2D>();
            if (fruitRigidbody != null)
            {
                fruitRigidbody.gravityScale = newGravityScale; // 중력 스케일 변경
            }
        }
    }

    // 과일을 꼬치에 추가하는 메서드
    public void AddFruitToTanghulu(FruitType matchedFruitType)
    {
        // FruitType에 맞는 과일 프리팹 찾기 (TanghuluStickFruit 컴포넌트 사용)
        GameObject matchedFruitPrefab = availableFruits.Find(fruit =>
            fruit.GetComponent<TanghuluStickFruit>() != null &&
            fruit.GetComponent<TanghuluStickFruit>().fruitType == matchedFruitType);

        if (matchedFruitPrefab == null)
        {
            Debug.LogWarning($"해당 FruitType의 과일 프리팹이 없습니다: {matchedFruitType}");
            return;
        }

        // 과일 프리팹을 Tanghulu에 추가
        GameObject newFruit = Instantiate(matchedFruitPrefab, tanghuluCup);

        float yOffset = 0.5f + (fruitsOnStick.Count * 0.3f); // TanghuluStick 위치 조정

        newFruit.transform.localPosition = new Vector3(0, yOffset, 0);

        // 리스트에 추가
        fruitsOnStick.Add(newFruit);
    }

    public GameObject GetCompletedTanghulu()
    {
        // Tanghulu를 대표하는 게임 오브젝트, 과일들이 자식으로 포함되어 있는 상태
        return this.gameObject;
    }

    // 탕후루 초기화 메서드
    public void ClearTanghulu()
    {
        foreach (GameObject fruit in fruitsOnStick)
        {
            Destroy(fruit);
        }
        fruitsOnStick.Clear(); // 리스트 초기화
    }

    public void AdjustMatch3()
    {
        Match3_Stick.SetActive(true);
        Match4_Stick.SetActive(false);
        Match5_Stick.SetActive(false);
        tanghuluStick.localPosition = new Vector3(-1.33f, 3.2f, 0);
        tanghuluCup.localPosition = new Vector3(0, -0.55f, 0);
        tanghuluStick.localScale = new Vector3(1f, 0.5f, 1f);
        tanghuluCup.localScale = new Vector3(1f, 2f, 1f);
        currentFruitCount = 3;
    }

    public void AdjustMatch4()
    {
        Match3_Stick.SetActive(false);
        Match4_Stick.SetActive(true);
        Match5_Stick.SetActive(false);
        tanghuluStick.localPosition = new Vector3(-1.33f, 3.2f, 0);
        tanghuluCup.localPosition = new Vector3(0, -0.9f, 0);
        tanghuluStick.localScale = new Vector3(1f, 0.5f, 1f);
        tanghuluCup.localScale = new Vector3(1f, 2f, 1f);
        currentFruitCount = 4;
    }

    public void AdjustMatch5()
    {
        Match3_Stick.SetActive(false);
        Match4_Stick.SetActive(false);
        Match5_Stick.SetActive(true);
        tanghuluStick.localPosition = new Vector3(-1.33f, 3.2f, 0);
        tanghuluCup.localPosition = new Vector3(0, -1.35f, 0);
        tanghuluStick.localScale = new Vector3(1f, 0.5f, 1f);
        tanghuluCup.localScale = new Vector3(1f, 2f, 1f);
        currentFruitCount = 5;
    }

    public void AdjustBossMatch()
    {
        Match3_Stick.SetActive(false);
        Match4_Stick.SetActive(false);
        Match5_Stick.SetActive(true);
        tanghuluStick.localPosition = new Vector3(0, 3.2f, 0);
        tanghuluStick.localScale = new Vector3(1f, 1f, 1f);
        tanghuluCup.localScale = new Vector3(2f, 2f, 1f);
        currentFruitCount = 3;
    }

    public void ChangeAdjustMatch()
    {
        switch (maxFruitCount)
        {
            case 3:
                AdjustMatch3();
                break;

            case 4:
                AdjustMatch4();
                break;

            case 5:
                AdjustMatch5();
                break;
        }
    }
}