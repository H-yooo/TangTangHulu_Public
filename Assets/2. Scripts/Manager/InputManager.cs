using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : GenericSingleton<InputManager>
{
    [SerializeField] private GameObject touchableArea; // 터치가능한 Area
    [SerializeField] private GameObject fruitEffectPrefab;

    public GameObject TouchableArea // touchableArea에 접근할 수 있는 프로퍼티
    {
        get { return touchableArea; }
    }

    public LayerMask fruitLayerMask;
    private Collider2D touchableAreaCollider;
    private float maxRaycastDistance = 5f; // Fruit 감지 불가 시 수정

    private Vector2 dragStartPosition;
    private Vector2 dragEndPosition;
    private float dragDistanceX;
    private bool isDragging = false;
    private float dragStartTime; // 드래그 시작 시간을 기록
    private float maxDragDuration = 1.0f; // 드래그 최대 유지 시간

    [SerializeField] private bool isInputBlocked = false;


    private void Start()
    {
        touchableAreaCollider = touchableArea.GetComponent<Collider2D>();

        if (touchableAreaCollider == null)
        {
            touchableAreaCollider = GetComponentInChildren<Collider2D>();
        }

        isInputBlocked = true;
    }

    private void Update()
    {
        if (isInputBlocked)
        {
            return;  // 입력이 차단된 경우 아무런 동작도 하지 않음
        }
        else
        {
            // 드래그 기능
            if (Input.GetMouseButtonDown(0))
            {
                dragStartPosition = Input.mousePosition;
                isDragging = true;
                dragStartTime = Time.time; // 드래그 시작 시간 기록
            }

            if (isDragging && Time.time - dragStartTime > maxDragDuration)
            {
                //드래그 취소
                isDragging = false;
            }

            if (Input.GetMouseButtonUp(0))
            {
                dragEndPosition = Input.mousePosition;

                if (isDragging)
                {
                    // 드래그 거리 계산
                    dragDistanceX = Mathf.Abs(Input.mousePosition.x - dragStartPosition.x);

                    if (dragDistanceX < 150)
                    {
                        HandleClick();
                    }
                    else
                    {
                        Vector2 dragDirection = dragEndPosition - dragStartPosition;
                        HandleDrag(dragDirection);
                    }
                }

                isDragging = false;
            }
        }
    }

    private void SpawnFruitEffect(Vector3 worldPosition, bool isSuccess)
    {
        GameObject effect = Instantiate(fruitEffectPrefab, worldPosition, Quaternion.identity);
        Animator anim = effect.GetComponentInChildren<Animator>();
        if (anim == null) return;

        if (isSuccess)
        {
            int randomEffect = Random.Range(0, 4); // 0:Red, 1:Blue, 2:Green, 3:Pink
            anim.SetInteger("EffectResult", randomEffect);
        }
        else
        {
            anim.SetInteger("EffectResult", 99); // 실패 이펙트
        }

        Destroy(effect, 1.2f); // 일정 시간 뒤 오브젝트 제거
    }

    private void HandleClick()
    {
        // 화면(Screen)에서 터치 위치(Input.mousePosition)를 월드 좌표(WorldPoint)로 변환
        Vector3 touchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        touchPosition.z = 0; // 2D 상에서의 위치 비교를 위해 z 좌표를 0으로 설정

        // 터치 위치가 지정된 영역 내에 있는지 확인
        if (IsWithinTouchableArea(touchPosition))
        {
            RaycastHit2D hit = Physics2D.Raycast(touchPosition, Vector2.zero, maxRaycastDistance, fruitLayerMask);
            if (hit.collider != null && hit.collider.gameObject.layer == LayerMask.NameToLayer("Fruit"))
            {
                GameObject selectedFruit = hit.collider.gameObject;

                Vector3 worldPos = selectedFruit.transform.position;
                bool isMatch = FruitManager.Instance.IsCorrectFruit(selectedFruit);
                SpawnFruitEffect(worldPos, isMatch); // 성공/실패 이펙트 실행


                FruitManager.Instance.OnFruitSelected(selectedFruit);
            }
        }
    }

    private void HandleDrag(Vector2 dragDirection)
    {
        SoundManager.Instance.PlaySFX("Swipe");

        float dragThreshold = 150f; // 드래그 거리 임계값

        if (Mathf.Abs(dragDirection.x) < dragThreshold)
        {
            // 드래그 거리가 너무 짧으면 취소
            return;
        }

        bool isDraggingRight = dragDirection.x > 0; // 오른쪽으로 드래그했는지 확인

        // touchableArea 내 과일 제거 로직
        List<GameObject> fruitsToDespawn = FruitManager.Instance.GetFruitsInTouchableArea();

        foreach (GameObject fruit in fruitsToDespawn)
        {
            SlideAndDestroy(fruit, isDraggingRight);
        }

        // 과일들이 다 제거되고 난 뒤 새로운 과일을 추가
        StartCoroutine(SpawnNewFruitsAfterDelay(0.2f));  // 지연 시간을 두고 새 과일 추가
    }

    private IEnumerator SpawnNewFruitsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);  // 지연 시간 대기

        // 새로운 과일을 맨 위에 추가
        FruitManager.Instance.SpawnNewTopFruits();
    }

    private void SlideAndDestroy(GameObject fruit, bool isDraggingRight)
    {
        // 이동할 방향 설정 (오른쪽 또는 왼쪽으로 밀려나게 함)
        Vector3 endPosition = fruit.transform.position + new Vector3(isDraggingRight ? 5f : -5f, 0, 0);
        float moveDuration = 0.1f;

        // 과일을 옆으로 밀어낸 후 사라지게 함
        LeanTween.move(fruit, endPosition, moveDuration).setOnComplete(() =>
        {
            FruitManager.Instance.DragFruit(fruit);
        });
    }

    // 터치 위치가 지정된 영역 내에 있는지 확인하는 함수
    private bool IsWithinTouchableArea(Vector3 position)
    {
        // OverlapPoint(Vector2 point): point가 콜라이더 영역 내에 있으면 true 반환 여기서 position은 터치한 위치
        return touchableAreaCollider != null && touchableAreaCollider.OverlapPoint(position);
    }

    public void BlockInput(bool block)
    {
        isInputBlocked = block;
    }

    public void LateBlockInput(float duration)
    {
        StartCoroutine(LateBlockInputCoroutine(duration));
    }

    public IEnumerator LateBlockInputCoroutine(float duration)
    {
        BlockInput(true); // 입력 차단
        yield return new WaitForSeconds(duration); // 지정된 시간 동안 대기
        BlockInput(false); // 입력 차단 해제
    }
}
