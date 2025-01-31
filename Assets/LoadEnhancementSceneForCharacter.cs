using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadEnhancementSceneForCharacter : MonoBehaviour
{
    [Tooltip("강화 씬의 이름을 입력하세요.")]
    public string enhancementSceneName; // 인스펙터에서 씬 이름을 설정

    public void LoadEnhancementScene(CharacterData character)
    {
        if (string.IsNullOrEmpty(enhancementSceneName))
        {
            Debug.LogError("강화 씬 이름이 설정되지 않았습니다!");
            return;
        }

        CharacterEnhancementManager.currentCharacter = character; // 선택된 캐릭터 데이터 저장
        SceneManager.LoadScene(enhancementSceneName); // 설정된 씬 이름으로 이동
    }
}
