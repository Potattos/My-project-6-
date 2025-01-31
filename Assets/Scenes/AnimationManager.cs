using UnityEngine;
using System.Collections.Generic;

public class AnimationManager : MonoBehaviour
{
    [Header("빈 오브젝트에 있는 Animator 리스트")]
    public List<Animator> animators = new List<Animator>(); // 인스펙터에서 할당

    private Dictionary<int, Animator> characterAnimators = new Dictionary<int, Animator>();

    // 캐릭터 데이터를 받아 애니메이션 컨트롤러 할당
    public void AssignCharacterAnimators(List<CharacterData> selectedCharacters)
    {
        for (int i = 0; i < animators.Count; i++)
        {
            if (i < selectedCharacters.Count)
            {
                GameObject animationInstance = Instantiate(selectedCharacters[i].animationPrefab, animators[i].transform);
                animators[i].runtimeAnimatorController = selectedCharacters[i].animatorController;
                characterAnimators[i] = animators[i]; // 딕셔너리에 저장

                PlayIdleAnimation(i); // 기본적으로 Idle 애니메이션 실행
            }
            else
            {
                animators[i].runtimeAnimatorController = null; // 선택되지 않은 경우 초기화
            }
        }
    }

    // Idle 애니메이션 실행
    public void PlayIdleAnimation(int characterIndex)
    {
        if (characterAnimators.ContainsKey(characterIndex))
        {
            characterAnimators[characterIndex].SetTrigger("Idle");
        }
    }

    // 스킬 애니메이션 실행
    public void PlaySkillAnimation(int characterIndex, int skillIndex)
    {
        if (characterAnimators.ContainsKey(characterIndex))
        {
            string triggerName = $"Skill{skillIndex}";
            characterAnimators[characterIndex].SetTrigger(triggerName);
        }
    }

    // 모든 애니메이션을 Idle 상태로 초기화 (전투 시작 시 호출)
    public void ResetAnimations()
    {
        foreach (var animator in characterAnimators.Values)
        {
            animator.SetTrigger("Idle");
        }
    }
}