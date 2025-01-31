using UnityEngine;

[CreateAssetMenu(menuName = "SkillEffects/HealEffect")]
public class HealEffect : SkillEffect
{
    public float healPercentage = 20f;

    public override void ApplyEffect(CharacterData target)
    {
        target.Heal(healPercentage);
    }
}
