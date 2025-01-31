using UnityEngine;

public class CharacterEnhancement : MonoBehaviour
{
    private CharacterEnhancementDisplay display;

    void Start()
    {
        display = FindObjectOfType<CharacterEnhancementDisplay>();

        if (CharacterEnhancementManager.currentCharacter != null && display != null)
        {
            display.UpdateUI(CharacterEnhancementManager.currentCharacter);
        }
    }

    public void EnhanceCharacter()
    {
        CharacterData character = CharacterEnhancementManager.currentCharacter;

        if (character != null && character.enhancementLevel < character.maxEnhancementLevel)
        {
            // 강화 처리
            character.enhancementLevel++;

            // UI 업데이트
            display.UpdateUI(character);
        }
        else
        {
            Debug.LogWarning("캐릭터를 더 이상 강화할 수 없습니다.");
        }
    }
}
