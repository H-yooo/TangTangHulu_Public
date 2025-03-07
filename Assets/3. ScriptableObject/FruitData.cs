using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "FruitData", menuName = "FruitDataSO/FruitData")]
public class FruitData : ScriptableObject
{
    public string fruitName;
    public int fruitIndex;
    public FruitType fruitType;
    public GameObject fruitPrefab;
    public Image fruitImage;
    public int fruitLevel;
    public int initialFruitPrice;
    public int fruitPrice;
    public int increaseRate;
    public int upgradeCost;

    public void ResetFruitUpgrade()
    {
        // 과일 별 가격 초기화
        fruitPrice = initialFruitPrice;
        fruitLevel = 0;
    }
}