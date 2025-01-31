using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelectionSceneManager : MonoBehaviour
{
    // 선택된 캐릭터 데이터를 강화 씬으로 전달
    public void LoadEnhancementSceneForCharacter(CharacterData character)
    {
        CharacterEnhancementManager.currentCharacter = character;
        SceneManager.LoadScene("EnhancementScene");
    }
}
