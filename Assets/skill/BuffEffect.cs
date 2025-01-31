using UnityEngine;

[CreateAssetMenu(menuName = "SkillEffects/BuffEffect")]
public class BuffEffect : SkillEffect
{
    public float damageIncreasePercentage = 20f;
    public int duration = 3;

    public override void ApplyEffect(CharacterData target)
    {
        target.ApplyBuff(damageIncreasePercentage, duration);
    }
}

