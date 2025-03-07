using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerManager : GenericSingleton<CustomerManager>
{
    [SerializeField] private MatchFruit matchFruit;
    [SerializeField] private Fever fever;
    private Customer currentCustomer;
    
    [SerializeField] private int favorability = 10;  // 손님의 초기 호감도
    [SerializeField] private int correctMatches; // 맞춘 과일 수
    [SerializeField] private float storeRating = 0; // 가게평점

    public int dayFavorability = 0;
    [SerializeField] private int dayGuests = 0;
    [SerializeField] private int totalGuests;

    [SerializeField] private List<GameObject> customerPrefabs; // 손님 프리팹 리스트
    [SerializeField] private Transform customerGroup; // 손님이 생성될 부모 오브젝트
    [SerializeField] private float waitingPositionY = 6.5f; // 대기 중인 손님 위치
    [SerializeField] private float spawnPositionY = 6.8f; // 손님이 처음 생성되는 위치

    private Queue<GameObject> waitingCustomers = new Queue<GameObject>();

    public float GetSpawnPositionY()
    {
        return spawnPositionY;
    }

    // waitingPositionY의 값을 외부에서 접근할 수 있는 메서드
    public float GetWaitingPositionY()
    {
        return waitingPositionY;
    }

    public void DecreaseFavorability()
    {
        favorability -= 3;
        if (favorability < 0) favorability = 0;
    }

    public void IncreaseCorrectMatches()
    {
        correctMatches++;
    }

    // 새로운 손님이 들어왔을 때
    public void AddGuest()
    {
        dayFavorability += favorability;
        dayGuests++;
        favorability = 10; // 호감도를 초기화
    }

    // 과일을 모두 맞췄을 때 골드 지급
    public void RewardGoldForMatches(int fruitTotalPrice)
    {
        // 호감도에 따른 보상 감소 로직
        float favorabilityMultiplier = Mathf.Max(0, favorability / 10f);  // 호감도에 비례한 보상 감소 (10 = 100%, 0 = 0%)
        int reward = Mathf.CeilToInt(fruitTotalPrice * favorabilityMultiplier);

        if (fever.isFeverModeActive)
        {
            GoldManager.Instance.IncreaseGold(reward * 2);
        }
        else
        {
            GoldManager.Instance.IncreaseGold(reward);
        }


        Debug.Log("현재 손님의 호감도: " + favorability);
        Debug.Log("골드 보상: " + reward);
    }

    public void CalculateStoreRating()
    {
        if (dayFavorability == 0)
        {
            storeRating = 0;
        }
        else
        {
            storeRating = (float)dayFavorability / dayGuests;
        }
    }

    public float GetStoreRating()
    {
        CalculateStoreRating();
        return storeRating;
    }

    public void DayCustomerReset()
    {
        totalGuests += dayGuests;
        dayGuests = 0;
        dayFavorability = 0;
        favorability = 10; // 호감도 초기화
    }

    // 손님 생성 메서드
    public void SpawnCustomer(float yPosition)
    {
        // 새 손님 생성 전 대기열 확인 (대기열에서 중복 생성 방지)
        if (yPosition == waitingPositionY && waitingCustomers.Count > 0)
        {
            return;
        }

        // 손님 프리팹 중 랜덤으로 선택하여 생성
        int randomIndex = Random.Range(0, customerPrefabs.Count);
        GameObject newCustomer = Instantiate(customerPrefabs[randomIndex], customerGroup);
        Customer customerScript = newCustomer.GetComponent<Customer>();

        // Fever 모드 상태 적용
        customerScript.CustomerFeverMode(fever.isFeverModeActive);

        // 초기 위치 설정
        customerScript.transform.position = new Vector3(customerScript.transform.position.x, yPosition, customerScript.transform.position.z);

        // 대기 상태일 경우 Idle 설정 및 큐에 추가
        if (yPosition == waitingPositionY)
        {
            customerScript.SetIdleState(); // Idle 상태 설정
            waitingCustomers.Enqueue(newCustomer); // 대기 중인 손님으로 큐에 추가
        }
        else
        {
            currentCustomer = customerScript;
            customerScript.MoveCustomerToPosition(); // 첫 번째 손님은 바로 이동
        }
    }

    public IEnumerator DelaySpawnCustomer(float delay)
    {
        matchFruit.HideFruitToMatch();
        yield return new WaitForSeconds(delay);
        SpawnCustomer(spawnPositionY);
    }

    public void OnCustomerArrived(Customer customer)
    {
        Debug.Log("손님 도착 완료");

        // 손님 도착 후 과일 맞추기 시작
        matchFruit.ShowFruitToMatch(); // 과일 맞추기 패널 활성화

        // 대기 중인 다음 손님 생성
        SpawnCustomer(waitingPositionY);
    }

    public void OnCustomerExited(Customer customer)
    {
        Tanghulu.Instance.ClearTanghulu(); // Tanghulu 초기화

        // 대기 중인 손님 이동 시작
        if (waitingCustomers.Count > 0)
        {
            GameObject nextCustomerObject = waitingCustomers.Dequeue();
            Customer nextCustomer = nextCustomerObject.GetComponent<Customer>();
            currentCustomer = nextCustomer;

            // Fever 모드 상태 적용
            currentCustomer.CustomerFeverMode(fever.isFeverModeActive);
            
            nextCustomer.MoveCustomerToPosition(); // 대기 중인 손님 목표 위치로 이동
        }

        // 나간 손님 삭제
        Destroy(customer.gameObject);
    }

    public void ClearAllCustomers()
    {
        // 대기 손님 삭제
        while (waitingCustomers.Count > 0)
        {
            GameObject customer = waitingCustomers.Dequeue();
            if (customer != null)
            {
                Destroy(customer);
            }
        }

        // 현재 손님 삭제
        if (currentCustomer != null)
        {
            Destroy(currentCustomer.gameObject);
            currentCustomer = null;
        }

        // CustomerGroup에 있는 모든 자식 오브젝트 제거
        foreach (Transform child in customerGroup)
        {
            Destroy(child.gameObject);
        }

        waitingCustomers.Clear();
    }

    // 모든 과일을 맞춘 후 손님이 만족 여부에 따라 애니메이션 재생
    public void CustomerSatisfaction()
    {
        bool isSatisfy = (favorability == 10);
        currentCustomer.PlaySatisfactionAnimation(isSatisfy);
    }

    // 현재 손님을 반환하는 메서드
    public Customer GetCurrentCustomer()
    {
        return currentCustomer;
    }

    public void SetCustomerFever(bool isActive)
    {
        // 현재 손님에 적용
        if (currentCustomer != null)
        {
            currentCustomer.CustomerFeverMode(isActive);
        }

        // 대기 중인 손님들에게도 적용
        foreach (GameObject customerObject in waitingCustomers)
        {
            Customer waitingCustomer = customerObject.GetComponent<Customer>();
            if (waitingCustomer != null)
            {
                waitingCustomer.CustomerFeverMode(isActive);
            }
        }
    }
}