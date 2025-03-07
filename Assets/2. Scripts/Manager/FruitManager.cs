using System.Collections.Generic;
using UnityEngine;

public class FruitManager : GenericSingleton<FruitManager>
{ 
    [SerializeField] private MatchFruit matchFruit;
    public List<FruitData> EntireFruitDataList;
    public List<FruitData> CurrentFruitDataList;
    [SerializeField] private List<GameObject> fruits;

    [Header("Fruits Position")]
    [SerializeField] private float _moveSpeed = 8.0f;
    private bool isShifting = false;
    private int columns = 3;
    private int rows = 6;
    private float startX = -1.6f;
    private float startY = -2.8f;
    private float reStartY = -2.8f + (5 * 1.4f); // 윗줄에 생성
    private float cellSizeX = 1.6f;
    private float cellSizeY = 1.4f;
    private float moveStep = 1.4f;
    private Dictionary<GameObject, Vector2> targetPositions;

    [Header("Fruit Upgrade")]
    [SerializeField] private GameObject[] fruitUpgradeSlots;

    public List<GameObject> GetFruitsInTouchableArea()
    {
        GameObject touchableArea = InputManager.Instance.TouchableArea;
        Collider2D touchableAreaCollider = touchableArea.GetComponent<Collider2D>();

        List<GameObject> fruitsToDespawn = new List<GameObject>();

        foreach (GameObject fruit in fruits)
        {
            if (touchableAreaCollider.OverlapPoint(fruit.transform.position))
            {
                fruitsToDespawn.Add(fruit);
            }
        }

        return fruitsToDespawn;
    }

    public float MoveSpeed
    {
        get { return _moveSpeed; }
        set
        {
            _moveSpeed = value;
        }
    }


    private void Update()
    {
        if (isShifting)
        {
            ShiftFruitsDown();
        }
    }

    #region 과일보드판 관련 메서드
    public void ResetAllFruit()
    {
        foreach (GameObject fruit in fruits)
        {
            Destroy(fruit);
        }
        fruits.Clear();
        targetPositions.Clear();

        // 새로운 과일 생성
        SpawnInitialFruits();
    }

    public void InitializeFruitData()
    {
        if (fruits == null)
        {
            fruits = new List<GameObject>();
        }

        if (targetPositions == null)
        {
            targetPositions = new Dictionary<GameObject, Vector2>();
        }

        for (int i = 0; i < fruitUpgradeSlots.Length; i++)
        {
            if (i < CurrentFruitDataList.Count) // 과일 데이터가 있을 경우
            {
                FruitUpgradeButton upgradeButton = fruitUpgradeSlots[i].GetComponent<FruitUpgradeButton>();
                if (upgradeButton != null)
                {
                    upgradeButton.SetData(CurrentFruitDataList[i]);
                }
            }
            else
            {
                // 빈 슬롯은 비활성화
                fruitUpgradeSlots[i].SetActive(false);
            }
        }
    }

    public void ResetFruitUpgrades()
    {
        CurrentFruitDataList.Clear();

        for (int i = 0; i < 5 && i < EntireFruitDataList.Count; i++)
        {
            FruitData fruitData = EntireFruitDataList[i];
            fruitData.ResetFruitUpgrade();
            CurrentFruitDataList.Add(fruitData);
        }
    }

    private void CopyFruitData(FruitData source, FruitData target)
    {
        target.fruitName = source.fruitName;
        target.fruitType = source.fruitType;
        target.fruitPrefab = source.fruitPrefab;
        target.fruitImage = source.fruitImage;
        target.initialFruitPrice = source.initialFruitPrice;
        target.fruitPrice = source.fruitPrice;
        target.increaseRate = source.increaseRate;
        target.upgradeCost = source.upgradeCost;
        target.fruitLevel = source.fruitLevel;
    }

    public void UpdateCurrentFruitDataList(int index, FruitData newFruitData)
    {
        int currentIndex = CurrentFruitDataList.FindIndex(data => data.fruitIndex == index);

        if (currentIndex != -1)
        {
            // CurrentFruitDataList에서 기존 데이터 교체
            CurrentFruitDataList[currentIndex] = newFruitData;

            // 해당 슬롯 UI 업데이트
            FruitUpgradeButton upgradeButton = fruitUpgradeSlots[currentIndex].GetComponent<FruitUpgradeButton>();
            if (upgradeButton != null)
            {
                upgradeButton.SetData(newFruitData);
            }
        }
        else
        {
            // CurrentFruitDataList에 없을 경우 처리
            if (index < fruitUpgradeSlots.Length)
            {
                CurrentFruitDataList.Add(newFruitData);

                // 해당 슬롯 활성화 및 데이터 설정
                fruitUpgradeSlots[index].SetActive(true);
                FruitUpgradeButton upgradeButton = fruitUpgradeSlots[index].GetComponent<FruitUpgradeButton>();
                if (upgradeButton != null)
                {
                    upgradeButton.SetData(newFruitData);
                }
            }
            else
            {
                Debug.LogWarning($"CurrentFruitDataList에서 유효하지 않은 인덱스입니다: {index}");
            }
        }
    }
    

    private void SpawnInitialFruits()
    {
        // Fruit들을 초기 위치에 생성
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                float posX = startX + col * cellSizeX;
                float posY = startY + row * cellSizeY;
                Vector3 position = new Vector3(posX, posY, 0);
                SpawnFruit(position);
            }
        }
    }

    public void SpawnFruit(Vector3 position)
    {
        int randomIndex = Random.Range(0, CurrentFruitDataList.Count);
        GameObject fruit = Instantiate(CurrentFruitDataList[randomIndex].fruitPrefab, position, Quaternion.identity);
        fruit.transform.SetParent(transform); // FruitManager의 자식으로 생성
        fruits.Add(fruit);
        targetPositions[fruit] = position; // Dictionary에 position 재정의
    }

    public void DespawnFruit(GameObject fruit)
    {
        fruits.Remove(fruit);
        targetPositions.Remove(fruit);
        Destroy(fruit);
    }

    // 선택 후 과일들을 아래로 움직이는 함수
    public void ShiftFruitsDown()
    {
        bool correctPosition = true;

        foreach (GameObject fruit in fruits)
        {
            Vector2 currentPosition = fruit.transform.position;
            Vector2 targetPosition = targetPositions[fruit];

            // 목표 위치로 이동
            Vector2 newPosition = Vector2.MoveTowards(currentPosition, targetPosition, _moveSpeed * Time.deltaTime); // 목표 위치로 이동

            fruit.transform.position = newPosition;

            if (newPosition != targetPosition)
            {
                correctPosition = false;
            }
        }

        if (correctPosition)
        {
            isShifting = false;
        }
    }

    private void UpdateFruitsPositions()
    {
        foreach (GameObject fruit in fruits)
        {
            Vector2 currentPosition = fruit.transform.position;
            if (currentPosition.y > startY)
            {
                targetPositions[fruit] = new Vector2(currentPosition.x, currentPosition.y - moveStep); // 목표 위치를 현재 위치에서 moveStep만큼 아래로 설정
            }
        }
        isShifting = true;
    }

    public void SpawnNewTopFruits()
    {
        for (int i = 0; i < columns; i++)
        {
            float posX = startX + i * cellSizeX;
            Vector2 position = new Vector2(posX, reStartY);
            SpawnFruit(position);
        }
    }

    public void DragFruit(GameObject fruit)
    {
        DespawnFruit(fruit);
        UpdateFruitsPositions();
        isShifting = true;
    }

    private void DespawnTouchableAreaFruits()
    {
        GameObject touchableArea = InputManager.Instance.TouchableArea;
        Collider2D touchableAreaCollider = touchableArea.GetComponent<Collider2D>();
        if (touchableAreaCollider == null)
        {
            Debug.LogError("TouchableArea에 Collider2D가 존재하지 않습니다!");
            return;
        }

        List<GameObject> fruitsToDespawn = new List<GameObject>();

        foreach (GameObject fruit in fruits)
        {
            if (touchableAreaCollider.OverlapPoint(fruit.transform.position))
            {
                fruitsToDespawn.Add(fruit);
            }
        }

        for (int i = fruitsToDespawn.Count - 1; i >= 0; i--)
        {
            DespawnFruit(fruitsToDespawn[i]);
        }
    }

    public FruitData GetFruitDataByType(FruitType fruitType)
    {
        foreach (FruitData fruitData in CurrentFruitDataList)
        {
            if (fruitData.fruitType == fruitType)
            {
                return fruitData;
            }
        }

        Debug.LogWarning($"해당 타입의 과일 데이터를 찾을 수 없습니다: {fruitType}");
        return null; // 해당 타입의 과일 데이터를 찾을 수 없는 경우 null 반환
    }
    #endregion

    #region 게임 메인 로직
    public void OnFruitSelected(GameObject selectedFruit)
    {
        if (selectedFruit == null)
        {
            Debug.LogWarning("선택된 과일이 null입니다. 입력이 제대로 전달되지 않았습니다.");
            return;
        }

        // NullReferenceException이 발생할 가능성이 있는 참조들 점검
        if (matchFruit == null)
        {
            Debug.LogError("matchFruit reference is not set!");
            return;
        }

        if (CurrentFruitDataList == null || CurrentFruitDataList.Count == 0)
        {
            Debug.LogError("CurrentFruitDataList가 null이거나 비어 있습니다!");
            return;
        }

        FruitType selectedFruitType = GetFruitType(selectedFruit);
        if (selectedFruitType == FruitType.None)
        {
            Debug.LogError($"선택된 과일의 타입을 찾을 수 없습니다: {selectedFruit.name}");
            return;
        }

        float selectedFruitY = selectedFruit.transform.position.y;
        List<GameObject> fruitsToDespawn = new List<GameObject>();

        matchFruit.CheckMatch(selectedFruitType);

        // 같은 위치의 과일들을 모두 제거
        foreach (GameObject fruit in fruits)
        {
            if (Mathf.Approximately(fruit.transform.position.y, selectedFruitY))
            {
                fruitsToDespawn.Add(fruit);
            }
        }

        foreach (GameObject fruit in fruitsToDespawn)
        {
            DespawnFruit(fruit);
        }

        UpdateFruitsPositions();
        isShifting = true;
        SpawnNewTopFruits();
    }

    private FruitType GetFruitType(GameObject fruit)
    {
        string fruitName = fruit.name.Replace("(Clone)", "").Trim();

        foreach (FruitData fruitData in CurrentFruitDataList)
        {
            if (fruitData.fruitPrefab != null && fruitData.fruitPrefab.name.Equals(fruitName))
            {
                return fruitData.fruitType;
            }
        }

        Debug.LogError("FruitType과 선택된 과일이 매치되지 않습니다: " + fruit.name);
        return FruitType.None; // 매칭되는 FruitType이 없는 경우 None 반환
    }

    // 과일의 현재 가격을 가져오는 메서드
    public int GetFruitPrice(FruitData fruitData)
    {
        return fruitData.fruitPrice;
    }
    #endregion   
}