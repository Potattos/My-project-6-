using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterSelectionButton : MonoBehaviour
{
    public CharacterData characterData;
    public TextMeshProUGUI characterNameText;
    public TextMeshProUGUI characterLevelText;
    public TextMeshProUGUI selectionStateText; // 선택 상태 표시 텍스트
    public Image characterImage; // 캐릭터 이미지를 표시할 UI Image

    public Color selectedColor = Color.green; // 선택된 상태의 버튼 색상
    public Color unselectedColor = Color.white; // 선택되지 않은 상태의 버튼 색상

    private Button button;
    private Image buttonBackground;

    void Start()
    {
        button = GetComponent<Button>();
        buttonBackground = GetComponent<Image>();

        if (button == null || buttonBackground == null)
        {
            Debug.LogError("Required components missing on " + gameObject.name);
            return;
        }

        button.onClick.AddListener(ToggleSelection);
        Debug.Log($"Button initialized for character: {characterData?.characterName}");
        UpdateButtonState();
        UpdateCharacterInfo();
    }

    void ToggleSelection()
    {
        Debug.Log($"Button clicked for character: {characterData?.characterName}");

        SelectedCharacters.ToggleCharacterSelection(characterData);

        // 가장 최근 선택된 캐릭터 가져오기
        CharacterData selectedCharacter = SelectedCharacters.selectedCharacters.Count > 0
            ? SelectedCharacters.selectedCharacters[SelectedCharacters.selectedCharacters.Count - 1]
            : null;

        // UI 업데이트
        var display = FindObjectOfType<CharacterInfoDisplay>();
        if (display != null)
        {
            display.UpdateCharacterInfo(selectedCharacter);
        }

        UpdateButtonState();
    }


    void UpdateButtonState()
    {
        bool isSelected = SelectedCharacters.selectedCharacters.Contains(characterData);
        buttonBackground.color = isSelected ? selectedColor : unselectedColor; // 색상 변경

        if (selectionStateText != null)
        {
            selectionStateText.text = isSelected ? "선택됨" : ""; // 선택 상태에 따라 텍스트 변경
        }

        Debug.Log($"Button state updated - Character: {characterData?.characterName}, Selected: {isSelected}");
    }

    void UpdateCharacterInfo()
    {
        if (characterNameText != null && characterData != null)
        {
            characterNameText.text = characterData.characterName;
        }

        if (characterLevelText != null && characterData != null)
        {
            characterLevelText.text = "Lv. " + characterData.enhancementLevel;
        }

        if (characterImage != null && characterData != null)
        {
            characterImage.sprite = characterData.characterImage;
            characterImage.enabled = true; // 이미지 활성화
        }
    }

    
}

