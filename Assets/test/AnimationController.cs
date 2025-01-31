using UnityEngine;

public class AnimationController : MonoBehaviour
{
    public Animator animator; // Animator를 연결할 변수
    private string currentAnimation = "Idle"; // 현재 애니메이션 상태

    void Start()
    {
        if (animator == null)
        {
            Debug.LogError("Animator가 연결되지 않았습니다.");
        }
    }

    public void PlayAnimation(string animationName)
    {
        if (currentAnimation == animationName) return;

        animator.Play(animationName); // 애니메이션 재생
        currentAnimation = animationName;

        // 특정 애니메이션 재생 후 대기 애니메이션으로 돌아가기
        Invoke(nameof(ResetToIdle), animator.GetCurrentAnimatorStateInfo(0).length);
    }

    private void ResetToIdle()
    {
        animator.Play("Idle");
        currentAnimation = "Idle";
    }

    // 버튼 입력 처리
    public void OnButton1Click()
    {
        PlayAnimation("Animation1");
    }

    public void OnButton2Click()
    {
        PlayAnimation("Animation2");
    }

    public void OnButton3Click()
    {
        PlayAnimation("Animation3");
    }
}
