using System.Collections.Generic;
using UnityEngine;

public class CharacterSelectionManager : MonoBehaviour
{
    public static CharacterSelectionManager Instance { get; private set; }
    
    [Header("Selected Characters")]
    public List<CharacterData> selectedCharacters = new List<CharacterData>();
    public int maxSelectableCharacters = 3;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Update()
    {
        // Sync the manager's list with the static list
        selectedCharacters = SelectedCharacters.selectedCharacters;
    }

    public void SelectCharacter(CharacterData character)
    {
        SelectedCharacters.ToggleCharacterSelection(character);
    }

    public void DeselectCharacter(CharacterData character)
    {
        if (SelectedCharacters.selectedCharacters.Contains(character))
        {
            SelectedCharacters.selectedCharacters.Remove(character);
        }
    }

    public CharacterData[] GetSelectedCharacters()
    {
        return SelectedCharacters.selectedCharacters.ToArray();
    }

    public void SetSelectedCharacters(List<CharacterData> characters)
    {
        SelectedCharacters.selectedCharacters = characters;
    }
}