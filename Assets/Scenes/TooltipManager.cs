using UnityEngine;
using TMPro;

public class TooltipManager : MonoBehaviour
{
    public GameObject tooltip; // 툴팁 UI
    public TextMeshProUGUI tooltipText; // 툴팁 텍스트

    public void ShowTooltip(CharacterData characterData, int skillNumber)
    {
        if (characterData == null || skillNumber < 1 || skillNumber > 3)
        {
            tooltip.SetActive(false);
            return;
        }

        // 스킬 설명 가져오기
        string skillDescription = skillNumber switch
        {
            1 => characterData.skill1Descriptions[characterData.enhancementLevel - 1],
            2 => characterData.skill2Descriptions[characterData.enhancementLevel - 1],
            3 => characterData.skill3Descriptions[characterData.enhancementLevel - 1],
            _ => "Unknown Skill"
        };

        // 툴팁 내용 설정
        tooltipText.text = $"{characterData.characterName}\nLevel {characterData.enhancementLevel}:\n{skillDescription}";
        tooltip.SetActive(true);
    }

    public void HideTooltip()
    {
        tooltip.SetActive(false);
    }
}
