using UnityEngine;

[CreateAssetMenu(menuName = "SkillEffects/DebuffEffect")]
public class DebuffEffect : SkillEffect
{
    public float damageReductionPercentage = 20f;
    public float damageOverTimePercentage = 15f;
    public int duration = 3;

    public override void ApplyEffect(CharacterData target)
    {
        target.ApplyDebuff(damageReductionPercentage, damageOverTimePercentage, duration);
    }
}
