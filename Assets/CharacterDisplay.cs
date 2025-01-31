using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterDisplay : MonoBehaviour
{
    public Image[] characterImages; // 캐릭터 이미지를 표시할 이미지 배열 (3개)
    public TextMeshProUGUI[] characterNameTexts; // 캐릭터 이름을 표시할 텍스트 배열 (3개)
    public TextMeshProUGUI[] characterLevelTexts; // 캐릭터 레벨을 표시할 텍스트 배열 (3개)
    public TextMeshProUGUI[] characterHealthTexts; // 캐릭터 체력을 표시할 텍스트 배열 (3개)
    public Image[] skillImages; // 스킬 이미지를 표시할 이미지 배열 (9개: 캐릭터당 3개)

    void Start()
    {
        DisplaySelectedCharacters();
    }

    void DisplaySelectedCharacters()
    {
        for (int i = 0; i < characterImages.Length; i++)
        {
            if (i < SelectedCharacters.selectedCharacters.Count)
            {
                // 선택된 캐릭터 정보 표시
                CharacterData character = SelectedCharacters.selectedCharacters[i];
                characterImages[i].sprite = character.characterImage;

                EnableTMP(characterNameTexts[i], character.characterName);
                EnableTMP(characterLevelTexts[i], "Lv. " + character.enhancementLevel);
                EnableTMP(characterHealthTexts[i], $"HP: {character.currentHealth}/{character.GetMaxHealth()}");

                // 스킬 이미지 표시
                int skillStartIndex = i * 3;
                skillImages[skillStartIndex].sprite = character.skill1Image;
                skillImages[skillStartIndex + 1].sprite = character.skill2Image;
                skillImages[skillStartIndex + 2].sprite = character.skill3Image;
            }
            else
            {
                // 선택되지 않은 슬롯 초기화
                DisableTMP(characterNameTexts[i]);
                DisableTMP(characterLevelTexts[i]);
                DisableTMP(characterHealthTexts[i]);

                int skillStartIndex = i * 3;
                skillImages[skillStartIndex].sprite = null;
                skillImages[skillStartIndex + 1].sprite = null;
                skillImages[skillStartIndex + 2].sprite = null;
            }
        }
    }

    // TextMeshProUGUI를 활성화하고 텍스트를 설정하는 헬퍼 함수
    void EnableTMP(TextMeshProUGUI tmp, string text)
    {
        if (tmp != null)
        {
            tmp.text = text;
            tmp.gameObject.SetActive(true); // 활성화
        }
    }

    // TextMeshProUGUI를 비활성화하는 헬퍼 함수
    void DisableTMP(TextMeshProUGUI tmp)
    {
        if (tmp != null)
        {
            tmp.text = ""; // 텍스트 초기화
            tmp.gameObject.SetActive(false); // 비활성화
        }
    }
}
