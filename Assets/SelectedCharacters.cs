using System.Collections.Generic;
using UnityEngine;

public static class SelectedCharacters
{
    public static List<CharacterData> selectedCharacters = new List<CharacterData>();

    public static void ToggleCharacterSelection(CharacterData character)
    {
        if (selectedCharacters.Contains(character))
            selectedCharacters.Remove(character);
        else if (selectedCharacters.Count < 3)
            selectedCharacters.Add(character);
    }

    public static void ResetSelections()
    {
        selectedCharacters.Clear();
    }

    public static void UpdateCharacterData(CharacterData updatedCharacter)
    {
        for (int i = 0; i < selectedCharacters.Count; i++)
        {
            if (selectedCharacters[i].characterName == updatedCharacter.characterName)
            {
                selectedCharacters[i] = updatedCharacter;
                break;
            }
        }
    }
}