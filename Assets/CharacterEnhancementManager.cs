using UnityEngine;

public class CharacterEnhancementManager : MonoBehaviour
{
    public static CharacterData currentCharacter; // 강화할 캐릭터 데이터

    // 데이터가 제대로 설정되지 않았을 경우 경고 출력
    private void Start()
    {
        if (currentCharacter == null)
        {
            Debug.LogWarning("강화 데이터가 설정되지 않았습니다.");
        }
    }
}