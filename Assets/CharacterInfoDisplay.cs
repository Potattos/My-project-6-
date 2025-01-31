using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterInfoDisplay : MonoBehaviour
{
    // 캐릭터 정보 표시
    public TextMeshProUGUI characterNameText;
    public TextMeshProUGUI characterLevelText;
    public TextMeshProUGUI healthText;

    // 스킬 이미지 및 버튼
    public Image skill1Image;
    public Image skill2Image;
    public Image skill3Image;

    public Button skill1Button;
    public Button skill2Button;
    public Button skill3Button;

    // 스킬 설명 표시
    public TextMeshProUGUI skillDescriptionText;
    public TextMeshProUGUI skillDescriptionsText;

    // 모든 UI 그룹
    public GameObject uiGroup;

    private CharacterData currentCharacterData;

    public void UpdateCharacterInfo(CharacterData characterData)
    {
        if (characterData == null)
        {
            ClearCharacterInfo();
            SetUIActive(false); // UI 비활성화
            return;
        }

        currentCharacterData = characterData;

        SetUIActive(true); // UI 활성화

        // 캐릭터 기본 정보 업데이트
        if (characterNameText != null)
            characterNameText.text = characterData.characterName;

        if (characterLevelText != null)
            characterLevelText.text = "Lv. " + characterData.enhancementLevel;

        if (healthText != null)
            healthText.text = "Health: " + characterData.GetMaxHealth();

        // 스킬 이미지 업데이트
        if (skill1Image != null)
            skill1Image.sprite = characterData.skill1Image;

        if (skill2Image != null)
            skill2Image.sprite = characterData.skill2Image;

        if (skill3Image != null)
            skill3Image.sprite = characterData.skill3Image;

        // 버튼 이벤트 설정
        if (skill1Button != null)
        {
            skill1Button.onClick.RemoveAllListeners();
            skill1Button.onClick.AddListener(() => DisplaySkillInfo(1));
        }

        if (skill2Button != null)
        {
            skill2Button.onClick.RemoveAllListeners();
            skill2Button.onClick.AddListener(() => DisplaySkillInfo(2));
        }

        if (skill3Button != null)
        {
            skill3Button.onClick.RemoveAllListeners();
            skill3Button.onClick.AddListener(() => DisplaySkillInfo(3));
        }

        // 초기화
        ClearSkillInfo();
    }

    public void DisplaySkillInfo(int skillNumber)
    {
        if (currentCharacterData == null)
        {
            ClearSkillInfo();
            return;
        }

        string description = "";
        string levelDescription = "";

        int currentLevel = currentCharacterData.enhancementLevel;

        switch (skillNumber)
        {
            case 1:
                description = currentCharacterData.skill1Description;
                levelDescription = GetLevelDescription(currentCharacterData.skill1Descriptions, currentLevel);
                break;
            case 2:
                description = currentCharacterData.skill2Description;
                levelDescription = GetLevelDescription(currentCharacterData.skill2Descriptions, currentLevel);
                break;
            case 3:
                description = currentCharacterData.skill3Description;
                levelDescription = GetLevelDescription(currentCharacterData.skill3Descriptions, currentLevel);
                break;
        }

        // 스킬 설명 업데이트
        if (skillDescriptionText != null)
            skillDescriptionText.text = description;

        if (skillDescriptionsText != null)
            skillDescriptionsText.text = levelDescription;
    }

    private string GetLevelDescription(string[] descriptions, int level)
    {
        if (descriptions == null || level <= 0 || level > descriptions.Length)
            return "";

        return descriptions[level - 1]; // 현재 레벨에 해당하는 설명 반환
    }

    private void ClearCharacterInfo()
    {
        currentCharacterData = null;

        if (characterNameText != null)
            characterNameText.text = "";

        if (characterLevelText != null)
            characterLevelText.text = "";

        if (healthText != null)
            healthText.text = "";

        if (skill1Image != null)
            skill1Image.sprite = null;

        if (skill2Image != null)
            skill2Image.sprite = null;

        if (skill3Image != null)
            skill3Image.sprite = null;

        ClearSkillInfo();
    }

    private void ClearSkillInfo()
    {
        if (skillDescriptionText != null)
            skillDescriptionText.text = "";

        if (skillDescriptionsText != null)
            skillDescriptionsText.text = "";
    }

    private void SetUIActive(bool isActive)
    {
        if (uiGroup != null)
            uiGroup.SetActive(isActive);
    }
}
