using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Battle/EnemyData")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public Sprite enemySprite;
    public int maxHealth;
    
    [Header("Skills")]
    public int[] skillDamage = new int[3];  // Damage for each skill
    public string[] skillNames = new string[3];  // Names of the skills
    public int ultimatePointsNeeded = 3;  // Points needed for ultimate
}