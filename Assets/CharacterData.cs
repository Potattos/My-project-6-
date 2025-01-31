using UnityEngine;
[System.Serializable]
public class CharacterData : ScriptableObject
{
    public string characterName;
    public Sprite characterImage;

    [Header("Animation")]
    public RuntimeAnimatorController animatorController; // 애니메이션 컨트롤러 추가
    [Header("Animation Sprites")]
   
    [Header("Animation Prefab")]
    public GameObject animationPrefab; // 애니메이션에 사용될 프리팹
    [Header("Health and Stats")]
    
    public int maxHealth; // 기본 최대 체력
    public int currentHealth; // 현재 체력
    public int[] healthEnhancements; // 레벨별 체력 증가 값
    public int defense; // 기본 방어력 추가

    [Header("Skill Description")]
    public string skill1Description;
    public string skill2Description;
    public string skill3Description;

    [Header("Enhancement Levels")]
    public int enhancementLevel; // 현재 강화 레벨
    public int maxEnhancementLevel = 5; // 최대 강화 레벨

    [Header("Skill Information")]
    public int skill1BaseDamage;
    public int skill2BaseDamage;
    public int skill3BaseDamage;

    [Header("Character Descriptions")]
    public string[] levelDescriptions; // 레벨별 캐릭터 설명 (총 5개)

    [Header("Skill Images")]
    public Sprite skill1Image;
    public Sprite skill2Image;
    public Sprite skill3Image;

    [Header("Skill Descriptions")]
    public string[] skill1Descriptions; // 스킬 1 레벨별 설명
    public string[] skill2Descriptions; // 스킬 2 레벨별 설명
    public string[] skill3Descriptions; // 스킬 3 레벨별 설명

    public int skill1Cost = 2; // 스킬 1 포인트 소모량
    public int skill2Cost = 2; // 스킬 2 포인트 소모량
    public int skill3Cost = 3; // 스킬 3 포인트 소모량

    public float damageMultiplier = 1.0f; // 기본값 1.0 (피해 증가 없음)


    [Header("Skill Enhancements")]
    public SkillEnhancementData[] skillEnhancements; // 스킬 강화 데이터

    [Header("Skill Effects")]
    public SkillEffect[] skill1Effects;
    public SkillEffect[] skill2Effects;
    public SkillEffect[] skill3Effects;

    public int ultimatePoints = 0;
    public const int MaxUltimatePoints = 4;

    public int GetMaxHealth()
    {
        int extraHealth = (enhancementLevel > 0 && enhancementLevel <= healthEnhancements.Length) 
            ? healthEnhancements[enhancementLevel - 1] 
            : 0;

        return maxHealth + extraHealth;
    }

    public int GetSkillCost(int skillNumber)
    {
        return skillNumber switch
        {
            1 => skill1Cost,
            2 => skill2Cost,
            3 => skill3Cost,
            _ => 0
        };
    }

    public int CalculateSkillDamage(int skillNumber)
    {
        int baseDamage = skillNumber switch
        {
            1 => skill1BaseDamage,
            2 => skill2BaseDamage,
            3 => skill3BaseDamage,
            _ => 0
        };

        if (enhancementLevel > 0 && skillEnhancements != null && enhancementLevel <= skillEnhancements.Length)
        {
            var enhancement = skillEnhancements[enhancementLevel - 1];
            int bonusDamage = skillNumber switch
            {
                1 => enhancement.skill1Increase,
                2 => enhancement.skill2Increase,
                3 => enhancement.skill3Increase,
                _ => 0
            };

            return baseDamage + bonusDamage;
        }

        return baseDamage;
    }

    public int CalculateDamageTaken(int incomingDamage)
    {
        int reducedDamage = Mathf.Max(incomingDamage - defense, 1); // 최소 피해는 1
        return reducedDamage;
    }

    public void ApplyDebuff(float damageIncreasePercentage, float damageOverTime, int duration)
    {
        Debug.Log($"{characterName}: Received debuff - Incoming damage increased by {damageIncreasePercentage}% for {duration} turns.");

        // 디버프 로직 수정 (피해 증가 배율 적용)
        damageMultiplier *= 1 + (damageIncreasePercentage / 100f);

        // DOT(초당 데미지) 로직 추가 가능
}

    public void Heal(float percentage)
    {
        int healAmount = Mathf.RoundToInt(maxHealth * (percentage / 100f));
        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth); // 체력을 최대 체력을 초과하지 않도록 제한
        Debug.Log($"{characterName} healed by {healAmount}. Current Health: {currentHealth}/{maxHealth}");
    }

    public void ApplyBuff(float damageIncrease, int duration)
    {
        Debug.Log($"{characterName}: Received buff - Damage Increase: {damageIncrease}% for {duration} turns.");
        // 버프 적용 로직
    }

    // 기존 ExecuteSkill: CharacterData 대상
    public void ExecuteSkill(int skillNumber, CharacterData target)
    {
        SkillEffect[] effects = skillNumber switch
        {
            1 => skill1Effects,
            2 => skill2Effects,
            3 => skill3Effects,
            _ => null
        };

        if (effects != null)
        {
            foreach (var effect in effects)
            {
                effect.ApplyEffect(target);
            }
        }
    }

    // 새로운 ExecuteSkill: EnemyData 대상
    public void ExecuteSkill(int skillNumber, EnemyData target)
    {
        Debug.Log($"{characterName} used skill {skillNumber} on {target.enemyName}.");

        // 스킬 효과 적용 예시
        int damage = skillNumber switch
        {
            1 => skill1BaseDamage,
            2 => skill2BaseDamage,
            3 => skill3BaseDamage,
            _ => 0
        };

        target.maxHealth = Mathf.Max(target.maxHealth - damage, 0);
        Debug.Log($"{target.enemyName} takes {damage} damage! Remaining health: {target.maxHealth}");
    }
}

[System.Serializable]
public class SkillEnhancementData
{
    public int skill1Increase;
    public int skill2Increase;
    public int skill3Increase;
}
