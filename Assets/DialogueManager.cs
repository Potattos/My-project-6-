using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    [Tooltip("대사를 표시할 TMP 텍스트 컴포넌트")]
    [SerializeField] private TMP_Text _dialogueText;

    [Tooltip("캐릭터 이름을 표시할 TMP 텍스트 컴포넌트")]
    [SerializeField] private TMP_Text _nameText;

    [Tooltip("대사 목록")]
    [SerializeField] private DialogueEntry[] _dialogues;

    [Tooltip("캐릭터 이미지 슬롯 (최대 3개: 왼쪽, 중앙, 오른쪽)")]
    [SerializeField] private Image[] _characterImages;

    [Tooltip("타이핑 속도 (초당 글자 수)")]
    [SerializeField] private float _typingSpeed = 0.05f;

    [Tooltip("다음에 로드할 씬의 이름")]
    [SerializeField] private string _nextSceneName;

    private int _currentIndex = 0;
    private Coroutine _typingCoroutine;

    private void Start()
    {
        DisplayDialogue();
    }

    public void OnClickNextDialogue()
    {
        _currentIndex++;

        if (_currentIndex < _dialogues.Length)
        {
            DisplayDialogue();
        }
        else
        {
            LoadNextScene();
        }
    }

    private void DisplayDialogue()
    {
        DialogueEntry currentDialogue = _dialogues[_currentIndex];

        // 캐릭터 이미지 및 이름 설정
        for (int i = 0; i < _characterImages.Length; i++)
        {
            if (i < currentDialogue.characterExpressions.Length)
            {
                // 이미지와 이름 설정
                _characterImages[i].sprite = currentDialogue.characterExpressions[i];
                _characterImages[i].gameObject.SetActive(currentDialogue.characterExpressions[i] != null);
            }
            else
            {
                // 슬롯 비활성화
                _characterImages[i].gameObject.SetActive(false);
            }
        }

        _nameText.text = currentDialogue.characterName;

        // 타이핑 효과 중지 및 시작
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
        }
        _typingCoroutine = StartCoroutine(TypeDialogue(currentDialogue.dialogue));
    }

    private IEnumerator TypeDialogue(string dialogue)
    {
        _dialogueText.text = "";

        foreach (char letter in dialogue)
        {
            _dialogueText.text += letter;
            yield return new WaitForSeconds(_typingSpeed);
        }

        _typingCoroutine = null;
    }

    private void LoadNextScene()
    {
        SceneManager.LoadScene(_nextSceneName);
    }
}

[System.Serializable]
public class DialogueEntry
{
    [Tooltip("캐릭터 이름")]
    public string characterName;

    [Tooltip("대사 내용")]
    [TextArea(3, 5)]
    public string dialogue;

    [Tooltip("캐릭터 표정 스프라이트 배열 (최대 3개: 왼쪽, 중앙, 오른쪽)")]
    public Sprite[] characterExpressions;
}
