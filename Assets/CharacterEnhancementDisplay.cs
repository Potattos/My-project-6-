using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterEnhancementDisplay : MonoBehaviour
{
    [Header("UI Elements")]
    public Image characterImage; 
    public TextMeshProUGUI characterNameText;
    public TextMeshProUGUI characterLevelText;

    [Header("Skill UI Elements")]
    public Image skill1Image;
    public Image skill2Image;
    public Image skill3Image;
    public TextMeshProUGUI skill1FixedDescription;
    public TextMeshProUGUI skill2FixedDescription;
    public TextMeshProUGUI skill3FixedDescription;
    public TextMeshProUGUI skill1LevelDescription;
    public TextMeshProUGUI skill2LevelDescription;
    public TextMeshProUGUI skill3LevelDescription;
    public TextMeshProUGUI characterHealthText; // 체력 정보 추가

    [Header("Level Descriptions")]
    public TextMeshProUGUI[] levelDescriptionTexts;

    [Header("Text Colors")]
    public Color defaultColor = Color.white;
    public Color enhancedColor = Color.green;

    void Start()
    {
        if (CharacterEnhancementManager.currentCharacter != null)
        {
            UpdateUI(CharacterEnhancementManager.currentCharacter);
        }
        else
        {
            Debug.LogError("강화할 캐릭터 데이터가 없습니다.");
        }
    }

    public void UpdateUI(CharacterData character)
    {
        // 캐릭터 기본 정보 업데이트
        if (characterImage != null)
            characterImage.sprite = character.characterImage;

        characterNameText.text = character.characterName;
        characterLevelText.text = "Level: " + character.enhancementLevel;

        // 스킬 이미지 업데이트
        skill1Image.sprite = character.skill1Image;
        skill2Image.sprite = character.skill2Image;
        skill3Image.sprite = character.skill3Image;

        // 고정된 스킬 설명 업데이트
        skill1FixedDescription.text = character.skill1Description;
        skill2FixedDescription.text = character.skill2Description;
        skill3FixedDescription.text = character.skill3Description;

        // 레벨에 따라 변화하는 스킬 설명 업데이트
        int level = character.enhancementLevel;
        skill1LevelDescription.text = character.skill1Descriptions[level - 1];
        skill2LevelDescription.text = character.skill2Descriptions[level - 1];
        skill3LevelDescription.text = character.skill3Descriptions[level - 1];
        // 체력 정보 업데이트
        int maxHealth = character.GetMaxHealth();
        characterHealthText.text = $"Max Health: {maxHealth}";

        // 레벨 설명 색상 업데이트
        UpdateLevelDescriptionColors(character.enhancementLevel, character.levelDescriptions);
    }

    private void UpdateLevelDescriptionColors(int enhancementLevel, string[] levelDescriptions)
    {
        for (int i = 0; i < levelDescriptionTexts.Length; i++)
        {
            if (i < levelDescriptions.Length)
            {
                levelDescriptionTexts[i].text = levelDescriptions[i];
                levelDescriptionTexts[i].color = (i < enhancementLevel) ? enhancedColor : defaultColor;
            }
            else
            {
                levelDescriptionTexts[i].text = "";
            }
        }
    }
}
