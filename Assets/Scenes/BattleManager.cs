using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems; 

public class BattleManager : MonoBehaviour

{
    [Header("Player UI")]
    public Image[] playerImages;
    public Button[] attackButtons;
    public Button endTurnButton;
    public Button cancelButton;
    public TextMeshProUGUI skillPointsText;


    [Header("Animation Manager")]
    public AnimationManager animationManager; // AnimationManager 참조

    [Header("Player Health Bars")]
    public Image[] playerHealthBars; // 플레이어 체력바 이미지 배열
    public Image[] enemyHealthBars;  // 적 체력바 이미지 배열

    [Header("Notification Settings")]
    public GameObject notificationPanel; // Horizontal Layout Group이 설정된 패널
    public GameObject notificationPrefab; // 메시지 프리팹
    
    [Header("Extra UI")]
    public GameObject extraPanel;
    public Button togglePanelButton;

    [Header("Enemy UI")]
    public Image[] enemyImages;
    public Button[] enemyButtons;

    [Header("Battle Result UI")]
    public GameObject victoryPanel;
    public GameObject defeatPanel;

    

    [Header("Game Data")]
    public int maxSkillPointsPerTurn = 10;
    public EnemyData[] enemyDataList;  // List of all possible enemies

    [Header("Skill History UI")]
    public Transform skillHistoryPanel; // 스킬 히스토리를 표시할 패널
    public GameObject skillHistoryPrefab; // 스킬 히스토리 이미지 프리팹

    private List<GameObject> skillHistoryImages = new List<GameObject>(); // 히스토리 이미지 목록

    [Header("Enemy Button Colors")]
    public Color defaultEnemyButtonColor = Color.white;
    public Color selectedEnemyButtonColor = Color.yellow;

    [Header("Player Health UI")]
    public TextMeshProUGUI[] playerHealthTexts; // 각 캐릭터의 체력을 표시할 TMP
    public TextMeshProUGUI[] enemyHealthTexts;  // 각 적의 체력을 표시할 TMP

    [Header("Attack Limits")]
    public int maxAttacksPerTurn = 6; // 턴당 최대 공격 횟수
    private int currentAttackCount = 0; // 현재 턴에서 실행한 공격 횟수

    [Header("Tooltip UI")]
    public GameObject tooltipPrefab; // 툴팁 패널 프리팹
    private Dictionary<Button, GameObject> activeTooltips = new Dictionary<Button, GameObject>(); // 활성화된 툴팁 목록



    
    private const int MAX_ACTIVE_ENEMIES = 2;  // Maximum enemies on field
    private List<EnemyData> activeEnemyData;  // Currently active enemies
    private List<int> enemyCurrentHealth;
    private List<int> enemyUltimatePoints;
    private int nextEnemyIndex;  // Index of next enemy to spawn

    private Queue<SkillAction> skillQueue = new Queue<SkillAction>();
    private bool isExecutingSkills = false;

    private int sharedSkillPoints;
    private List<CharacterData> playerCharacters;
    private int selectedCharacterIndex = -1;
    private int selectedEnemyIndex = -1;
    
    private Dictionary<int, int> skillCosts = new Dictionary<int, int>();
    private bool isProcessingTurn = false;
    private bool isPlayerTurn = true;
    private bool isBattleOver = false;
    

    private void Start()
    {
        // 툴팁 초기 비활성화

        if (victoryPanel) victoryPanel.SetActive(false);
        if (defeatPanel) defeatPanel.SetActive(false);

        InitializeBattle();

        if (togglePanelButton != null && extraPanel != null)
        {
            togglePanelButton.onClick.AddListener(ToggleExtraPanel);
        }


    }




    private void ToggleExtraPanel()
    {
        if (extraPanel != null)
        {
            extraPanel.SetActive(!extraPanel.activeSelf);
        }
    }



    private void ApplyDamage(int attackerIndex, int targetIndex, int damage)
    {
        if (targetIndex < 0 || targetIndex >= playerCharacters.Count) return;

        var target = playerCharacters[targetIndex];
        if (target == null || target.currentHealth <= 0) return;

        Debug.Log($"[Damage Calculation] Target: {target.characterName}, Incoming Damage: {damage}, Defense: {target.defense}");

        // 방어력을 고려한 피해 계산
        int finalDamage = target.CalculateDamageTaken(damage);

        Debug.Log($"[Damage Calculation] Final Damage: {finalDamage}");

        // 체력 감소
        target.currentHealth = Mathf.Max(target.currentHealth - finalDamage, 0);

        Debug.Log($"{target.characterName} took {finalDamage} damage! Remaining HP: {target.currentHealth}");

        // 체력바 및 UI 갱신
        UpdateAllHealthBars();

        // 캐릭터가 패배했는지 확인
        if (target.currentHealth <= 0)
        {
            Debug.Log($"{target.characterName} has been defeated!");
            playerCharacters[targetIndex] = null;
        }
    }








    private void InitializeBattle()
    {
        // Initialize lists
        activeEnemyData = new List<EnemyData>();
        enemyCurrentHealth = new List<int>();
        enemyUltimatePoints = new List<int>();
        
        
        // Set up initial enemies
        nextEnemyIndex = MAX_ACTIVE_ENEMIES;  // Start checking from index 2
        for (int i = 0; i < MAX_ACTIVE_ENEMIES; i++)
        {
            if (i < enemyDataList.Length)
            {
                SpawnEnemy(i, i);
            }
        }

        // Initialize player characters
        playerCharacters = new List<CharacterData>(CharacterSelectionManager.Instance.selectedCharacters);
        foreach (var character in playerCharacters)
        {
            if (character != null)
            {
                character.currentHealth = character.maxHealth;
            }
        }


        playerCharacters = new List<CharacterData>(CharacterSelectionManager.Instance.selectedCharacters);
        foreach (var character in playerCharacters)
        {
            if (character != null)
            {
                character.currentHealth = character.maxHealth;
                character.ultimatePoints = 0; // 궁극기 포인트 초기화
            }
        }
        // Reset battle state
        sharedSkillPoints = Mathf.Min(sharedSkillPoints + 10, maxSkillPointsPerTurn);
        selectedCharacterIndex = -1;
        selectedEnemyIndex = -1;
        isPlayerTurn = true;
        isBattleOver = false;
        isProcessingTurn = false;
        isExecutingSkills = false;
        skillQueue.Clear();
        skillCosts.Clear();

        // Enable all necessary buttons at start
        if (endTurnButton != null) endTurnButton.interactable = true;
        if (cancelButton != null) cancelButton.interactable = true;
        
        // Setup all UI elements and buttons
        SetupBattleUI();
        SetupButtons();
        SetupPlayerImageButtons();
        SetupEnemyButtons();
        DisableAllAttackButtons();
        UpdateAllHealthBars();
        
        // Update initial skill points display
        if (skillPointsText != null)
        {
            skillPointsText.text = $"Skill Points: {sharedSkillPoints}";
        }

        if (animationManager != null)
        {
            animationManager.AssignCharacterAnimators(playerCharacters);
        }

        currentAttackCount = 0;
        
    }

    private void SpawnEnemy(int position, int enemyDataIndex)
    {
        if (enemyDataIndex >= enemyDataList.Length) return;

        activeEnemyData.Add(enemyDataList[enemyDataIndex]);
        enemyCurrentHealth.Add(enemyDataList[enemyDataIndex].maxHealth);
        enemyUltimatePoints.Add(0);

    // Setup UI
        enemyImages[position].sprite = enemyDataList[enemyDataIndex].enemySprite; // Null 참조 가능성
        enemyImages[position].gameObject.SetActive(true);
        enemyButtons[position].gameObject.SetActive(true);
    }


    private void UpdateAllHealthBars()
    {
        // 플레이어 체력 텍스트 업데이트
        for (int i = 0; i < playerCharacters.Count; i++)
        {
            if (playerCharacters[i] != null)
            {
                playerHealthTexts[i].text = $"HP: {playerCharacters[i].currentHealth}/{playerCharacters[i].maxHealth}";
            }
            else
            {
                playerHealthTexts[i].text = "Defeated";
            }
        }

        // 적 체력 텍스트 업데이트
        for (int i = 0; i < activeEnemyData.Count; i++)
        {
            enemyHealthTexts[i].text = $"HP: {enemyCurrentHealth[i]}/{activeEnemyData[i].maxHealth}";
        }

        for (int i = 0; i < playerCharacters.Count; i++)
        {
            if (playerCharacters[i] != null)
            {
                playerHealthTexts[i].text = $"HP: {playerCharacters[i].currentHealth}/{playerCharacters[i].maxHealth}";
            }
            else
            {
                playerHealthTexts[i].text = "Defeated";
            }
        }

        for (int i = 0; i < activeEnemyData.Count; i++)
        {
            enemyHealthTexts[i].text = $"HP: {enemyCurrentHealth[i]}/{activeEnemyData[i].maxHealth}";
        }

        // 체력바 업데이트
        UpdateHealthBars();
    }



    private void EnemyTurn()
    {
        if (isBattleOver) return;

        StartCoroutine(ExecuteEnemyTurns());
    }
   


    private IEnumerator ExecuteEnemyTurns()
    {
        for (int i = 0; i < activeEnemyData.Count; i++)
        {
            if (enemyCurrentHealth[i] <= 0) continue;

            // Check if ultimate is ready
            if (enemyUltimatePoints[i] >= activeEnemyData[i].ultimatePointsNeeded)
            {
                // Use ultimate skill
                ExecuteEnemySkill(i, 2);  // Skill index 2 is ultimate
                enemyUltimatePoints[i] = 0;
            }
            else
            {
                // Use regular skill
                int skillChoice = Random.Range(0, 2);  // Choose between first two skills
                ExecuteEnemySkill(i, skillChoice);
                enemyUltimatePoints[i]++;
            }

            
            yield return new WaitForSeconds(1f);
        }

        if (!isBattleOver)
        {
                StartPlayerTurn(); // 플레이어 턴 시작 메서드 호출
        }
    }

    private void ExecuteEnemySkill(int enemyIndex, int skillIndex)
    {
        var targetIndex = Random.Range(0, playerCharacters.Count);
        var target = playerCharacters[targetIndex];

        if (target != null)
        {
            int baseDamage = activeEnemyData[enemyIndex].skillDamage[skillIndex];

            // 방어력을 고려한 최종 데미지 계산
            int finalDamage = target.CalculateDamageTaken(baseDamage);

            // 체력 감소
            target.currentHealth = Mathf.Max(target.currentHealth - finalDamage, 0);

            Debug.Log($"Enemy {enemyIndex} used {activeEnemyData[enemyIndex].skillNames[skillIndex]} " +
                    $"on {target.characterName} for {finalDamage} damage! (Base Damage: {baseDamage}, Defense: {target.defense})");

            if (target.currentHealth <= 0)
            {
                Debug.Log($"{target.characterName} has been defeated!");
                playerCharacters[targetIndex] = null;
            }

            UpdateAllHealthBars();
            CheckBattleEnd();
        }
    }


// BattleManager.cs의 ExecuteSkill 메서드를 수정
    private void ExecuteSkill(int skillNumber, int targetIndex)
    {
        if (isBattleOver) return;

        // 1. 인덱스와 캐릭터 확인을 위한 디버그 로그 추가
        int characterIndex = (skillNumber - 1) / 3;
        int characterSkillNumber = (skillNumber - 1) % 3 + 1;
        Debug.Log($"Executing skill {characterSkillNumber} from character {characterIndex} targeting enemy {targetIndex}");

        if (characterIndex >= playerCharacters.Count || playerCharacters[characterIndex] == null)
        {
            Debug.LogError("Invalid character index or null character!");
            return;
        }

        var currentCharacter = playerCharacters[characterIndex];
        Debug.Log($"Character {currentCharacter.characterName} found. Calculating damage...");

        // 2. 스킬 애니메이션 실행
        if (animationManager != null)
        {
            animationManager.PlaySkillAnimation(characterIndex, characterSkillNumber);
        }

        // 3. 데미지 계산 과정 상세 로깅
        int baseDamage = characterSkillNumber switch
        {
            1 => currentCharacter.skill1BaseDamage,
            2 => currentCharacter.skill2BaseDamage,
            3 => currentCharacter.skill3BaseDamage,
            _ => 0
        };
        Debug.Log($"Base damage: {baseDamage}");

        // 4. 강화 레벨에 따른 추가 데미지 계산
        int bonusDamage = 0;
        if (currentCharacter.enhancementLevel > 0 && 
            currentCharacter.skillEnhancements != null && 
            currentCharacter.enhancementLevel <= currentCharacter.skillEnhancements.Length)
        {
            var enhancement = currentCharacter.skillEnhancements[currentCharacter.enhancementLevel - 1];
            bonusDamage = characterSkillNumber switch
            {
                1 => enhancement.skill1Increase,
                2 => enhancement.skill2Increase,
                3 => enhancement.skill3Increase,
                _ => 0
            };
        }
        Debug.Log($"Bonus damage from enhancements: {bonusDamage}");

        // 5. 최종 데미지 계산
        int totalDamage = baseDamage + bonusDamage;
        totalDamage = Mathf.RoundToInt(totalDamage * currentCharacter.damageMultiplier);
        Debug.Log($"Total calculated damage: {totalDamage}");

        // 6. 적 체력 확인 및 데미지 적용
        if (targetIndex >= 0 && targetIndex < enemyCurrentHealth.Count)
        {
            Debug.Log($"Enemy current health before damage: {enemyCurrentHealth[targetIndex]}");
            
            // 데미지 적용
            int previousHealth = enemyCurrentHealth[targetIndex];
            enemyCurrentHealth[targetIndex] = Mathf.Max(enemyCurrentHealth[targetIndex] - totalDamage, 0);
            int actualDamageDealt = previousHealth - enemyCurrentHealth[targetIndex];
            
            Debug.Log($"Damage dealt: {actualDamageDealt}");
            Debug.Log($"Enemy health after damage: {enemyCurrentHealth[targetIndex]}");
            
            // 피해 입었다는 것을 UI로 표시
            ShowNotification($"{currentCharacter.characterName}의 공격! {actualDamageDealt} 데미지!");

            // 적이 패배했는지 확인
            if (enemyCurrentHealth[targetIndex] <= 0)
            {
                Debug.Log($"Enemy {targetIndex} defeated!");
                TryReplaceDefeatedEnemy(targetIndex);
            }
        }
        else
        {
            Debug.LogError($"Invalid target index: {targetIndex}. Valid range: 0 to {enemyCurrentHealth.Count - 1}");
        }

        // 7. 스킬 효과 적용
        SkillEffect[] effects = characterSkillNumber switch
        {
            1 => currentCharacter.skill1Effects,
            2 => currentCharacter.skill2Effects,
            3 => currentCharacter.skill3Effects,
            _ => null
        };

        if (effects != null)
        {
            foreach (var effect in effects)
            {
                if (effect is HealEffect)
                {
                    foreach (var character in playerCharacters)
                    {
                        if (character != null)
                        {
                            effect.ApplyEffect(character);
                        }
                    }
                }
            }
        }

        // 8. UI 업데이트
        UpdateAllHealthBars();
        CheckBattleEnd();
    }

// CharacterData.cs에서 ExecuteSkill 메서드 제거
// EnemyData 타겟 버전의 ExecuteSkill은 더 이상 필요하지 않음

    private void StartPlayerTurn()
    {
        isPlayerTurn = true;
        currentAttackCount = 0; // 턴 시작 시 공격 횟수 초기화
        sharedSkillPoints = sharedSkillPoints + 10; // 남은 포인트에 추가
        if (sharedSkillPoints > maxSkillPointsPerTurn) 
        

        // 갱신된 스킬 포인트 표시
        if (skillPointsText != null)
        {
            skillPointsText.text = $"Skill Points: {sharedSkillPoints}";
        }

        // 모든 버튼 숨김
        DisableAllAttackButtons();

        // 턴 넘기기 버튼 및 스킬 취소 버튼 활성화
        endTurnButton.interactable = true;
        cancelButton.interactable = true;

        Debug.Log("Player turn started!");
    }


    private void EnableAllAttackButtons()
    {
        for (int i = 0; i < playerCharacters.Count; i++)
        {
            if (playerCharacters[i] != null)
            {
                EnableCharacterAttackButtons(i);
            }
            else
            {
                DisableCharacterAttackButtons(i); // 캐릭터가 없으면 비활성화
            }
        }
    }


    private void TryReplaceDefeatedEnemy(int position)
    {
        if (nextEnemyIndex < enemyDataList.Length)
        {
            // Remove defeated enemy
            activeEnemyData.RemoveAt(position);
            enemyCurrentHealth.RemoveAt(position);
            enemyUltimatePoints.RemoveAt(position);

            // Spawn new enemy
            SpawnEnemy(position, nextEnemyIndex);
            nextEnemyIndex++;
            
            Debug.Log($"Enemy at position {position} replaced with new enemy!");
        }
    }

    private void SetupBattleUI()
    {
        for (int i = 0; i < playerImages.Length; i++)
        {
            if (i < playerCharacters.Count && playerCharacters[i] != null)
            {
                playerImages[i].sprite = playerCharacters[i].characterImage;
                playerHealthTexts[i].text = $"HP: {playerCharacters[i].currentHealth}/{playerCharacters[i].maxHealth}";
                playerImages[i].gameObject.SetActive(true);
            }
            else
            {
                playerImages[i].gameObject.SetActive(false);
                playerHealthTexts[i].text = "";
            }
        }

        for (int i = 0; i < enemyHealthTexts.Length; i++)
        {
            if (i < activeEnemyData.Count)
            {
                enemyHealthTexts[i].text = $"HP: {enemyCurrentHealth[i]}/{activeEnemyData[i].maxHealth}";
            }
            else
            {
                enemyHealthTexts[i].text = "";
            }
        }
    }


    private void SetupButtons()
    {
        for (int i = 0; i < attackButtons.Length; i++)
        {
            int skillNumber = i + 1;
            attackButtons[i].onClick.RemoveAllListeners();
            attackButtons[i].onClick.AddListener(() => OnSkillButtonClicked(skillNumber));
            attackButtons[i].interactable = true;

            // 스킬 이미지 설정
            int characterIndex = i / 3; // 캐릭터당 3개의 스킬 버튼
            int skillIndex = i % 3 + 1; // 1, 2, 3 번째 스킬

            if (characterIndex < playerCharacters.Count && playerCharacters[characterIndex] != null)
            {
                var character = playerCharacters[characterIndex];
                var skillImage = skillIndex switch
                {
                    1 => character.skill1Image,
                    2 => character.skill2Image,
                    3 => character.skill3Image,
                    _ => null
                };

                if (skillImage != null)
                {
                    attackButtons[i].GetComponent<Image>().sprite = skillImage;
                    attackButtons[i].gameObject.SetActive(true); // 버튼 활성화
                }
                else
                {
                    attackButtons[i].gameObject.SetActive(false); // 스킬 이미지가 없으면 버튼 비활성화
                }
            }
            else
            {
                attackButtons[i].gameObject.SetActive(false); // 캐릭터가 없으면 버튼 비활성화
            }

            // 툴팁 이벤트 추가
            AddTooltipEvents(attackButtons[i], skillNumber);
        }

        // 턴 종료 버튼 설정
        endTurnButton.onClick.RemoveAllListeners();
        endTurnButton.onClick.AddListener(OnEndTurnButtonClicked);

        // 취소 버튼 설정
        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(OnCancelButtonClicked);
    }


    private void AddTooltipEvents(Button button, int skillNumber)
    {
        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>() ?? button.gameObject.AddComponent<EventTrigger>();
        trigger.triggers.Clear(); // 기존 트리거 제거

        // PointerEnter 이벤트
        EventTrigger.Entry pointerEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        pointerEnter.callback.AddListener((eventData) => ShowTooltip(button, skillNumber));
        trigger.triggers.Add(pointerEnter);

        // PointerExit 이벤트
        EventTrigger.Entry pointerExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        pointerExit.callback.AddListener((eventData) => {
            if (activeTooltips.ContainsKey(button))
            {
                HideTooltip(button);
            }
        });
        trigger.triggers.Add(pointerExit);
    }

    private void ShowTooltip(Button button, int skillNumber)
    {
        // 이미 툴팁이 존재하면 반환
        if (activeTooltips.ContainsKey(button)) return;

        // 프리팹을 Canvas 바로 아래에 인스턴스화
        GameObject tooltip = Instantiate(tooltipPrefab, button.transform.root);
        tooltip.transform.SetAsLastSibling(); // 최상위에 표시되도록 설정

        // 텍스트 설정
        TextMeshProUGUI mainText = tooltip.transform.Find("MainText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI detailedText = tooltip.transform.Find("DetailedText").GetComponent<TextMeshProUGUI>();

        int characterIndex = (skillNumber - 1) / 3;
        int characterSkillNumber = (skillNumber - 1) % 3 + 1;

        if (characterIndex < playerCharacters.Count && playerCharacters[characterIndex] != null)
        {
            var character = playerCharacters[characterIndex];
            mainText.text = characterSkillNumber switch
            {
                1 => character.skill1Description,
                2 => character.skill2Description,
                3 => character.skill3Description,
                _ => "Unknown Skill"
            };

            detailedText.text = characterSkillNumber switch
            {
                1 => character.skill1Descriptions[character.enhancementLevel - 1],
                2 => character.skill2Descriptions[character.enhancementLevel - 1],
                3 => character.skill3Descriptions[character.enhancementLevel - 1],
                _ => "No details available"
            };
        }

        // 툴팁 위치 계산
        RectTransform tooltipRect = tooltip.GetComponent<RectTransform>();
        RectTransform buttonRect = button.GetComponent<RectTransform>();

        // 버튼의 월드 좌표를 캔버스 좌표로 변환
        Canvas canvas = button.GetComponentInParent<Canvas>();
        Vector2 buttonPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.GetComponent<RectTransform>(),
            RectTransformUtility.WorldToScreenPoint(null, button.transform.position),
            null,
            out buttonPos
        );

        // 툴팁 크기 가져오기
        Vector2 tooltipSize = tooltipRect.sizeDelta;
        
        // 스크린 경계 체크를 위한 캔버스 크기
        Vector2 canvasSize = canvas.GetComponent<RectTransform>().sizeDelta;

        // 기본적으로 버튼의 오른쪽에 배치
        float xPos = buttonPos.x + buttonRect.sizeDelta.x + 10; // 10은 간격
        float yPos = buttonPos.y;

        // 오른쪽 경계를 벗어나는 경우 왼쪽에 배치
        if (xPos + tooltipSize.x > canvasSize.x / 2)
        {
            xPos = buttonPos.x - tooltipSize.x - 10;
        }

        // 위아래 경계 체크
        if (yPos + tooltipSize.y / 2 > canvasSize.y / 2)
        {
            yPos = canvasSize.y / 2 - tooltipSize.y / 2;
        }
        else if (yPos - tooltipSize.y / 2 < -canvasSize.y / 2)
        {
            yPos = -canvasSize.y / 2 + tooltipSize.y / 2;
        }

        tooltipRect.anchoredPosition = new Vector2(xPos, yPos);

        // 활성 툴팁 목록에 추가
        activeTooltips[button] = tooltip;
    }




    private void HideTooltip(Button button)
    {
        // 버튼에 해당하는 툴팁 제거
        if (activeTooltips.TryGetValue(button, out GameObject tooltip))
        {
            Destroy(tooltip); // 동적으로 생성된 툴팁 제거
            activeTooltips.Remove(button); // 활성 툴팁 목록에서 제거
        }
    }

    private void ResetButtonListeners()
    {
        // 턴 종료 버튼 리스너 초기화
        endTurnButton.onClick.RemoveAllListeners();
        endTurnButton.onClick.AddListener(OnEndTurnButtonClicked);

        // 취소 버튼 리스너 초기화
        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(OnCancelButtonClicked);
    }

    private void OnCharacterSelected(int index)
    {
        if (!isPlayerTurn || isExecutingSkills || isProcessingTurn) return;

        // 동일 캐릭터를 두 번 클릭하면 스킬 버튼 숨김
        if (selectedCharacterIndex == index)
        {
            DisableAllAttackButtons();
            selectedCharacterIndex = -1;
            return;
        }

        // 이전 캐릭터의 버튼 숨기기
        if (selectedCharacterIndex != -1)
        {
            DisableCharacterAttackButtons(selectedCharacterIndex);
        }

        // 선택된 캐릭터의 스킬 버튼 활성화
        selectedCharacterIndex = index;
        EnableCharacterAttackButtons(index);
    }


    private void OnEnemySelected(int index)
    {
        if (!isPlayerTurn || isExecutingSkills || isProcessingTurn) return;
        if (enemyCurrentHealth[index] <= 0) return;

        // 이전 선택된 버튼의 색상 초기화
        if (selectedEnemyIndex != -1)
        {
            enemyButtons[selectedEnemyIndex].GetComponent<Image>().color = defaultEnemyButtonColor;
        }

        // 새로 선택된 버튼의 색상 변경
        enemyButtons[index].GetComponent<Image>().color = selectedEnemyButtonColor;
        selectedEnemyIndex = index;

        Debug.Log($"Enemy {index} selected.");
    }

    private void DisableCharacterAttackButtons(int characterIndex)
    {
        int startButtonIndex = characterIndex * 3;
        for (int i = 0; i < 3; i++)
        {
            int buttonIndex = startButtonIndex + i;
            if (buttonIndex < attackButtons.Length)
            {
                attackButtons[buttonIndex].gameObject.SetActive(false);
            }
        }
    }

    private void OnSkillButtonClicked(int skillNumber)
    {
        // 스킬 실행 불가 상태 확인
        if (!isPlayerTurn || isExecutingSkills || isProcessingTurn)
        {
            Debug.Log("Cannot execute skill at this time!");
            ShowNotification("Cannot execute skill at this time!"); // 알림 메시지 추가
            return;
        }

        // 최대 공격 횟수 초과 확인
        if (currentAttackCount >= maxAttacksPerTurn)
        {
            Debug.Log("You have reached the maximum number of attacks this turn!");
            ShowNotification("You have reached the maximum number of attacks this turn!"); // 알림 메시지 추가
            return;
        }

        // 적 선택 여부 확인
        if (selectedEnemyIndex == -1)
        {
            Debug.Log("Please select an enemy target first!");
            ShowNotification("Please select an enemy target first!"); // 알림 메시지 추가
            return;
        }

        // 캐릭터와 스킬 정보 확인
        int characterIndex = (skillNumber - 1) / 3;
        int characterSkillNumber = (skillNumber - 1) % 3 + 1;

        

        if (characterIndex >= playerCharacters.Count || playerCharacters[characterIndex] == null)
        {
            Debug.Log("Invalid character index or character not found!");
            ShowNotification("Invalid character or character not found!"); // 알림 메시지 추가
            return;
        }

        var currentCharacter = playerCharacters[characterIndex];
        int skillCost = currentCharacter.GetSkillCost(characterSkillNumber);

        // 스킬 포인트 부족 확인
        if (sharedSkillPoints < skillCost)
        {
            Debug.Log($"Not enough skill points to use Skill {characterSkillNumber}!");
            ShowNotification($"Not enough skill points to use Skill {characterSkillNumber}!"); // 알림 메시지 추가
            return;
        }

        // 궁극기 포인트 부족 확인 (궁극기 스킬만 해당)
        if (characterSkillNumber == 3 && currentCharacter.ultimatePoints < CharacterData.MaxUltimatePoints)
        {
            Debug.Log("Not enough ultimate points to use the ultimate skill!");
            ShowNotification("Not enough ultimate points to use the ultimate skill!"); // 알림 메시지 추가
            return;
        }

        // 스킬 실행: 스킬 포인트 차감 및 큐에 추가
        sharedSkillPoints -= skillCost;
        skillPointsText.text = $"Skill Points: {sharedSkillPoints}";
        skillQueue.Enqueue(new SkillAction
        {
            CharacterIndex = characterIndex,
            SkillNumber = characterSkillNumber,
            TargetIndex = selectedEnemyIndex
        });

        Debug.Log($"Skill {characterSkillNumber} from {currentCharacter.characterName} queued against enemy {selectedEnemyIndex}");

        // 궁극기 포인트 증가 (궁극기가 아닌 경우만)
        if (characterSkillNumber == 1 || characterSkillNumber == 2)
        {
            currentCharacter.ultimatePoints = Mathf.Min(currentCharacter.ultimatePoints + 1, CharacterData.MaxUltimatePoints);
        }

        // 궁극기 버튼 상태 업데이트
        UpdateUltimateSkillButton(characterIndex);

        // 스킬 히스토리에 추가
        AddSkillToHistory(currentCharacter, characterSkillNumber);

        // 궁극기 사용 처리
        if (characterSkillNumber == 3 && currentCharacter.ultimatePoints == CharacterData.MaxUltimatePoints)
        {
            currentCharacter.ultimatePoints = 0;
            DisableUltimateSkillText(characterIndex);
        }

        // 공격 횟수 증가
        currentAttackCount++;

        // 성공적으로 스킬이 사용되었음을 알리는 로그
        Debug.Log("Skill executed successfully!");
    }





    // 스킬 히스토리에 이미지 추가
    private void AddSkillToHistory(CharacterData character, int skillNumber)
    {
        if (skillHistoryImages.Count >= 8) return; // 최대 8개 제한

        var skillImage = skillNumber switch
        {
            1 => character.skill1Image,
            2 => character.skill2Image,
            3 => character.skill3Image,
            _ => null
        };

        if (skillImage == null) return;

        GameObject newHistoryImage = Instantiate(skillHistoryPrefab, skillHistoryPanel);
        newHistoryImage.GetComponent<Image>().sprite = skillImage;
        skillHistoryImages.Add(newHistoryImage);
    }




    private void ClearSkillHistory()
    {
        foreach (var image in skillHistoryImages)
        {
            Destroy(image);
        }
        skillHistoryImages.Clear();
    }


    // Returns the minimum skill cost for a character
    private int GetMinimumSkillCost(int characterIndex)
    {
        var character = playerCharacters[characterIndex];
        if (character == null) return int.MaxValue;

        int minCost = int.MaxValue;
        for (int i = 1; i <= 3; i++) // Assume 3 skills per character
        {
            int cost = character.GetSkillCost(i);
            if (cost < minCost) minCost = cost;
        }
        return minCost;
    }


    private class SkillAction
    {
        public int CharacterIndex { get; set; }
        public int SkillNumber { get; set; }
        public int TargetIndex { get; set; }
    }

    private void OnEndTurnButtonClicked()
    {
        if (!isPlayerTurn || isExecutingSkills || isProcessingTurn) return;

        Debug.Log("End turn button clicked. Executing queued skills...");
        StartCoroutine(ExecuteQueuedSkills());

        ClearSkillHistory();
    }

        private void SetupEnemyButtons()
        {
            for (int i = 0; i < enemyButtons.Length; i++)
            {
                int enemyIndex = i;
                enemyButtons[i].onClick.RemoveAllListeners();
                enemyButtons[i].onClick.AddListener(() => OnEnemySelected(enemyIndex));
                enemyButtons[i].interactable = true;
            }
        }

    private void EnableCharacterAttackButtons(int characterIndex)
    {
        int startButtonIndex = characterIndex * 3;
        for (int i = 0; i < 3; i++)
        {
            int buttonIndex = startButtonIndex + i;
            if (buttonIndex < attackButtons.Length)
            {
                var button = attackButtons[buttonIndex];
                button.gameObject.SetActive(true);

                // 궁극기 버튼(3번)은 추가 조건 체크
                if (i == 2) // 3번 스킬
                {
                    button.interactable = sharedSkillPoints >= 4 && 
                                        playerCharacters[characterIndex].ultimatePoints >= CharacterData.MaxUltimatePoints;
                }
                else
                {
                    button.interactable = true; // 일반 스킬은 항상 활성화
                }
            }
        }
    }

    private void UpdateSkillPointsUI()
    {
        skillPointsText.text = $"Skill Points: {sharedSkillPoints}";

        // 각 캐릭터의 궁극기 버튼 상태 업데이트
        for (int i = 0; i < playerCharacters.Count; i++)
        {
            if (playerCharacters[i] != null)
            {
                UpdateUltimateSkillButton(i);
            }
        }
    }


    private void OnCancelButtonClicked()
    {
        if (!isPlayerTurn || isExecutingSkills || isProcessingTurn) return;

        // 사용된 스킬 포인트 복원
        foreach (var action in skillQueue)
        {
            int characterIndex = action.CharacterIndex;
            int skillCost = playerCharacters[characterIndex].GetSkillCost(action.SkillNumber);
            sharedSkillPoints += skillCost;
        }

        // 스킬 큐 초기화
        skillQueue.Clear();
        skillPointsText.text = $"Skill Points: {sharedSkillPoints}";

        // 모든 캐릭터의 궁극기 포인트 초기화
        foreach (var character in playerCharacters)
        {
            if (character != null)
            {
                character.ultimatePoints = 0; // 궁극기 포인트 초기화
                DisableUltimateSkillText(playerCharacters.IndexOf(character)); // 궁극기 버튼 비활성화
            }
        }

        // 스킬 히스토리 초기화
        ClearSkillHistory();

        selectedCharacterIndex = -1;
        selectedEnemyIndex = -1;

        DisableAllAttackButtons();
        
        currentAttackCount =0;

        DisableAllAttackButtons();

        // 적 버튼 색상 초기화
        foreach (var button in enemyButtons)
        {
            button.GetComponent<Image>().color = defaultEnemyButtonColor;
        }

        Debug.Log("All queued skills and ultimate points reset. Skill points restored.");
    }




    private void CheckBattleEnd()
    {
        if (isBattleOver) return;

        bool allEnemiesDefeated = activeEnemyData.Count == 0 || 
            enemyCurrentHealth.TrueForAll(health => health <= 0) && nextEnemyIndex >= enemyDataList.Length;

        if (allEnemiesDefeated)
        {
            isBattleOver = true;
            StartCoroutine(ShowBattleResult(true));
            return;
        }

        bool allPlayersDefeated = playerCharacters.TrueForAll(character => character == null);
        if (allPlayersDefeated)
        {
            isBattleOver = true;
            StartCoroutine(ShowBattleResult(false));
        }
    }

    private IEnumerator ShowBattleResult(bool isVictory)
    {
        yield return new WaitForSeconds(1f);
        DisableAllAttackButtons();
        endTurnButton.interactable = false;
        cancelButton.interactable = false;
        
        if (isVictory) victoryPanel?.SetActive(true);
        else defeatPanel?.SetActive(true);

        
    }
    private void SetupPlayerImageButtons()
    {
        for (int i = 0; i < playerImages.Length; i++)
        {
            int index = i; // Capture the index for the lambda
            if (i < playerCharacters.Count && playerCharacters[i] != null)
            {
                // Add click listener to player images
                Button imageButton = playerImages[i].GetComponent<Button>();
                if (imageButton == null)
                {
                    imageButton = playerImages[i].gameObject.AddComponent<Button>();
                }
                imageButton.onClick.RemoveAllListeners();
                imageButton.onClick.AddListener(() => OnCharacterSelected(index));
            }
        }
    }
        private void DisableAllAttackButtons()
    {
        foreach (var button in attackButtons)
        {
            button.gameObject.SetActive(false); // 버튼 비활성화
        }
    }

    private IEnumerator ExecuteQueuedSkills()
    {
        isExecutingSkills = true;
        DisableAllAttackButtons();
        endTurnButton.interactable = false;
        cancelButton.interactable = false;

        while (skillQueue.Count > 0)
        {
            var action = skillQueue.Dequeue();

            // 스킬 애니메이션 실행
            if (animationManager != null)
            {
                animationManager.PlaySkillAnimation(action.CharacterIndex, action.SkillNumber);
            }

            // 스킬 실행
            ExecuteSkill(action.CharacterIndex * 3 + action.SkillNumber, action.TargetIndex);

            yield return new WaitForSeconds(1f); // 애니메이션 실행 후 대기

            if (isBattleOver) break;
        }

        isExecutingSkills = false;

        if (!isBattleOver)
        {
            isPlayerTurn = false;
            EnemyTurn();
        }
    }

    private void SetUIActiveState(bool isActive)
    {
        // 버튼 비활성화
        foreach (var button in attackButtons)
        {
            button.gameObject.SetActive(isActive);
        }

        endTurnButton.gameObject.SetActive(isActive);
        cancelButton.gameObject.SetActive(isActive);

        // 스킬 히스토리 패널 비활성화
        if (skillHistoryPanel != null)
        {
            skillHistoryPanel.gameObject.SetActive(isActive);
        }
    }

    private void UpdateUltimatePoints(int characterIndex, int skillNumber)
    {
        var character = playerCharacters[characterIndex];

        // Increment ultimate points if not the ultimate skill
        if (skillNumber != 3)
        {
            character.ultimatePoints = Mathf.Min(character.ultimatePoints + 1, CharacterData.MaxUltimatePoints);
        }

        // Update ultimate skill button state
        int ultimateSkillButtonIndex = characterIndex * 3 + 2; // 궁극기 버튼은 세 번째 버튼
        if (ultimateSkillButtonIndex < attackButtons.Length)
        {
            var ultimateButton = attackButtons[ultimateSkillButtonIndex];
            var buttonText = ultimateButton.GetComponentInChildren<TextMeshProUGUI>();

            if (character.ultimatePoints >= CharacterData.MaxUltimatePoints)
            {
                buttonText.text = ""; // 텍스트를 비워 궁극기 사용 가능 표시
                ultimateButton.interactable = sharedSkillPoints >= 4; // 포인트 조건 체크
            }
            else
            {
                int remaining = CharacterData.MaxUltimatePoints - character.ultimatePoints;
                buttonText.text = $"궁극기까지 {remaining}회";
                ultimateButton.interactable = false; // 궁극기 버튼 비활성화
            }
        }

        // 궁극기 사용 시 포인트 초기화
        if (skillNumber == 3 && character.ultimatePoints == CharacterData.MaxUltimatePoints)
        {
            character.ultimatePoints = 0;
            if (ultimateSkillButtonIndex < attackButtons.Length)
            {
                var ultimateButton = attackButtons[ultimateSkillButtonIndex];
                var buttonText = ultimateButton.GetComponentInChildren<TextMeshProUGUI>();
                buttonText.text = $"궁극기까지 {CharacterData.MaxUltimatePoints}회";
                ultimateButton.interactable = false; // 초기화 후 버튼 비활성화
            }
        }
    }


    private void UpdateUltimateSkillButton(int characterIndex)
    {
        int startButtonIndex = characterIndex * 3;
        int ultimateSkillButtonIndex = startButtonIndex + 2; // 궁극기 버튼은 세 번째 버튼

        if (ultimateSkillButtonIndex < attackButtons.Length)
        {
            var character = playerCharacters[characterIndex];
            var button = attackButtons[ultimateSkillButtonIndex];
            var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();

            // 궁극기 포인트와 스킬 포인트 조건을 모두 만족해야 활성화
            if (character.ultimatePoints >= CharacterData.MaxUltimatePoints && sharedSkillPoints >= 4)
            {
                buttonText.text = ""; // 텍스트를 비워 궁극기 사용 가능 표시
                button.interactable = true;
            }
            else
            {
                int remaining = CharacterData.MaxUltimatePoints - character.ultimatePoints;
                buttonText.text = $"궁극기까지 {remaining}회";
                button.interactable = false; // 조건 만족 못하면 비활성화
            }
        }
    }
    private IEnumerator SmoothFill(Image healthBar, float targetFillAmount)
    {
        float currentFillAmount = healthBar.fillAmount;
        float duration = 0.5f; // 애니메이션 지속 시간
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            healthBar.fillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, elapsedTime / duration);
            yield return null;
        }

        healthBar.fillAmount = targetFillAmount; // 최종 값 보정
    }

    private void UpdateHealthBars()
    {
        // 플레이어 체력바 업데이트
        for (int i = 0; i < playerHealthBars.Length; i++)
        {
            if (i < playerCharacters.Count && playerCharacters[i] != null)
            {
                float healthPercentage = (float)playerCharacters[i].currentHealth / playerCharacters[i].maxHealth;
                if (playerHealthBars[i].gameObject.activeSelf)
                {
                    StartCoroutine(SmoothFill(playerHealthBars[i], healthPercentage));
                }
                playerHealthBars[i].gameObject.SetActive(healthPercentage > 0);
            }
            else
            {
                playerHealthBars[i].gameObject.SetActive(false); // 캐릭터가 없거나 체력이 0 이하일 경우 비활성화
            }
        }

        // 적 체력바 업데이트
        for (int i = 0; i < enemyHealthBars.Length; i++)
        {
            if (i < activeEnemyData.Count)
            {
                float healthPercentage = (float)enemyCurrentHealth[i] / activeEnemyData[i].maxHealth;
                if (enemyHealthBars[i].gameObject.activeSelf)
                {
                    StartCoroutine(SmoothFill(enemyHealthBars[i], healthPercentage));
                }
                enemyHealthBars[i].gameObject.SetActive(healthPercentage > 0);
            }
            else
            {
                enemyHealthBars[i].gameObject.SetActive(false); // 적 데이터가 없을 경우 비활성화
            }
        }
    }

// 알림을 관리하기 위한 딕셔너리
    private Dictionary<string, Coroutine> activeNotifications = new Dictionary<string, Coroutine>();

    private void ShowNotification(string message)
    {
        if (notificationPrefab == null || notificationPanel == null)
        {
            Debug.LogWarning("Notification Prefab or Panel is not assigned!");
            return;
        }

        // 같은 메시지가 이미 표시 중인지 확인
        if (activeNotifications.ContainsKey(message))
        {
            // 기존 알림 시간 연장
            StopCoroutine(activeNotifications[message]);
            activeNotifications[message] = StartCoroutine(RemoveNotificationAfterDelay(
                notificationPanel.transform.Find(message)?.gameObject, message, 1.5f));
            return;
        }

        // 새로운 알림 생성
        GameObject notificationInstance = Instantiate(notificationPrefab, notificationPanel.transform);
        notificationInstance.transform.SetParent(notificationPanel.transform, false);

        // 텍스트 설정
        TextMeshProUGUI notificationText = notificationInstance.GetComponentInChildren<TextMeshProUGUI>();
        if (notificationText != null)
        {
            notificationText.text = message;
            notificationInstance.name = message; // 알림의 이름을 메시지로 설정
        }
        else
        {
            Debug.LogWarning("Notification Prefab does not have a TextMeshProUGUI component!");
        }

        // 알림 제거 코루틴 실행 및 딕셔너리에 추가
        Coroutine removeCoroutine = StartCoroutine(RemoveNotificationAfterDelay(notificationInstance, message, 1.5f));
        activeNotifications.Add(message, removeCoroutine);
    }



    private IEnumerator RemoveNotificationAfterDelay(GameObject notification, string message, float delay)
    {
        yield return new WaitForSeconds(delay);

        // 알림 제거 및 딕셔너리에서 삭제
        Destroy(notification);
        activeNotifications.Remove(message);
    }



    private bool CheckAndShowNotifications(int skillCost, int skillNumber, int characterIndex)
    {
        var character = playerCharacters[characterIndex];

        // 궁극기 포인트 부족
        if (skillNumber == 3 && character.ultimatePoints < CharacterData.MaxUltimatePoints)
        {
            ShowNotification("Not enough ultimate points!");
            return false;
        }

        // 최대 스킬 수 초과
        if (currentAttackCount >= maxAttacksPerTurn)
        {
            ShowNotification("Maximum skill usage reached!");
            return false;
        }

        
        if (sharedSkillPoints < skillCost)
        {
            ShowNotification("Not enough skill points!");
            return false;
        }

        return true;
    }


    private void DisableUltimateSkillText(int characterIndex)
    {
        int startButtonIndex = characterIndex * 3;
        int ultimateSkillButtonIndex = startButtonIndex + 2; 

        if (ultimateSkillButtonIndex < attackButtons.Length)
        {
            var button = attackButtons[ultimateSkillButtonIndex];
            var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();

            
            buttonText.text = $"궁극기까지 {CharacterData.MaxUltimatePoints}회";
            button.interactable = false;
        }
    }

}