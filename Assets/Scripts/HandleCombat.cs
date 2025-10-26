using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class HandleCombat : MonoBehaviour
{
    public static HandleCombat Instance;
    private int enemyTurnCount = 0;

    public AudioSource audioAttack;
    public AudioSource audioDefend;
    public AudioSource audioHeal;

    [Header("Player & Enemy Stats")]
    public int playerHealth = 100;
    public int playerShield = 0;
    public int enemyHealth = 100;
    public int enemyShield = 0;

    [Header("UI Elements (TMP)")]
    public TMP_Text playerHealthText;
    public TMP_Text enemyHealthText;
    public TMP_Text playerShieldText;
    public TMP_Text enemyShieldText;

    private float damageBuff = 1f;
    private float shieldBuff = 1f;
    private float waterBuff = 1f;
    private bool windDoublecast = false;
    private float vulnerable = 1f;
    private int enemyBurnStacks = 0;

    private bool isPlayerTurn = true;
    private bool gameEnded = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        UpdateAllUI();
    }

    public void ProcessTurn(List<string> chosenItems)
    {
        if (!isPlayerTurn || gameEnded) return;
        StartCoroutine(PlayerTurn(chosenItems));
    }

    private IEnumerator PlayerTurn(List<string> chosenItems)
    {
        ResetTurnModifiers();

        for (int i = 0; i < chosenItems.Count; i++)
        {
            if (gameEnded) yield break;

            int slot = i + 1;
            string raw = chosenItems[i].ToLower();

            string item = "none";
            if (raw.Contains("fire")) item = "fire";
            else if (raw.Contains("wind")) item = "wind";
            else if (raw.Contains("water")) item = "water";
            else if (raw.Contains("earth")) item = "earth";

            switch (item)
            {
                case "fire":
                    CombatLogUI.Instance?.ShowMessage($"Fire Slot {slot} activated!", 2.5f);
                    yield return new WaitForSeconds(1.2f);
                    FireEffect(slot);
                    break;

                case "water":
                    CombatLogUI.Instance?.ShowMessage($"Water Slot {slot} activated!", 2.5f);
                    yield return new WaitForSeconds(1.2f);
                    WaterEffect(slot);
                    break;

                case "wind":
                    CombatLogUI.Instance?.ShowMessage($"Wind Slot {slot} activated!", 2.5f);
                    yield return new WaitForSeconds(1.2f);
                    WindEffect(slot);
                    break;

                case "earth":
                    CombatLogUI.Instance?.ShowMessage($"Earth Slot {slot} activated!", 2.5f);
                    yield return new WaitForSeconds(1.2f);
                    EarthEffect(slot);
                    break;
            }

            UpdateAllUI();

            if (playerHealth <= 0 || enemyHealth <= 0)
            {
                CheckCombatResult();
                yield break;
            }

            yield return new WaitForSeconds(2.8f); // ⏳ longer pause between slot actions
        }

        isPlayerTurn = false;
        yield return new WaitForSeconds(1.5f);
        StartCoroutine(EnemyTurn());
    }

    private IEnumerator EnemyTurn()
    {
        if (gameEnded) yield break;

        CombatLogUI.Instance?.ShowMessage("Enemy turn begins...", 2f);
        yield return new WaitForSeconds(1f);

        if (enemyBurnStacks > 0)
        {
            int burnThisTurn = Mathf.Min(3, enemyBurnStacks);
            DealDamageToEnemy(burnThisTurn);
            enemyBurnStacks -= burnThisTurn;
            CombatLogUI.Instance?.ShowMessage($"Enemy takes {burnThisTurn} burn damage!", 2f);
            yield return new WaitForSeconds(1.8f);
        }

        if (enemyHealth <= 0 || playerHealth <= 0)
        {
            CheckCombatResult();
            yield break;
        }

        EnemyAttackPattern();
        UpdateAllUI();
        CheckCombatResult();

        if (!gameEnded && playerHealth > 0 && enemyHealth > 0)
        {
            yield return new WaitForSeconds(2.5f);
            CombatLogUI.Instance?.ShowMessage("Your turn!", 1.8f);
            isPlayerTurn = true;
            GameManager.Instance?.StartNextRound();
        }
    }

    private void EnemyAttackPattern()
    {
        enemyTurnCount++;
        float scale = Mathf.Min(1f + (enemyTurnCount - 1) * 0.3f, 3f);

        int roll = Random.Range(0, 100);
        if (enemyTurnCount > 6) roll += 15;

        if (roll < 40)
        {
            int damage = Mathf.RoundToInt(16 * scale);
            PlayMusic(1);
            TakeDamage(damage);
            CombatLogUI.Instance?.ShowMessage($"Enemy claws you for {damage} damage!", 2.5f);
        }
        else if (roll < 70)
        {
            int shieldGain = Mathf.RoundToInt(16 * scale);
            PlayMusic(2);
            enemyShield += shieldGain;
            CombatLogUI.Instance?.ShowMessage($"Enemy braces and gains {shieldGain} shield!", 2.5f);
        }
        else
        {
            int damage = Mathf.RoundToInt(8 * scale);
            int shieldGain = Mathf.RoundToInt(8 * scale);
            PlayMusic(1);
            TakeDamage(damage);
            enemyShield += shieldGain;
            CombatLogUI.Instance?.ShowMessage($"Enemy strikes: {damage} dmg, +{shieldGain} shield.", 2.5f);
        }
    }

    private void ResetTurnModifiers()
    {
        damageBuff = 1f;
        shieldBuff = 1f;
        waterBuff = 1f;
        windDoublecast = false;
        vulnerable = 1f;
    }

    private void FireEffect(int slot)
    {
        switch (slot)
        {
            case 1:
                DealDamageToEnemy(5);
                damageBuff = 1.5f;
                PlayMusic(1);
                CombatLogUI.Instance?.ShowMessage("You hit for 5 dmg (+50% next attack!)", 2.5f);
                break;

            case 2:
                int boosted2 = Mathf.RoundToInt(10 * damageBuff*vulnerable);
                DealDamageToEnemy(boosted2);
                TakeDamage(5);
                PlayMusic(1);
                enemyBurnStacks += 6;
                CombatLogUI.Instance?.ShowMessage($"You deal {boosted2} dmg, take 5 dmg, and apply 6 Burn!", 2.5f);
                break;

            case 3:
                int boosted3 = Mathf.RoundToInt(15 * damageBuff*vulnerable);
                int recoil = Mathf.RoundToInt(boosted3 * 0.5f);
                DealDamageToEnemy(boosted3);
                TakeDamage(recoil);
                PlayMusic(1);
                CombatLogUI.Instance?.ShowMessage($"Inferno! {boosted3} dmg, {recoil} recoil!", 2.8f);
                if(windDoublecast)
                {
                    CombatLogUI.Instance?.ShowMessage($"Doublecast activates and you go again!", 2.8f);
                    DealDamageToEnemy(boosted3);
                    TakeDamage(recoil);
                    PlayMusic(1);
                    CombatLogUI.Instance?.ShowMessage($"Inferno! {boosted3} dmg, {recoil} recoil!", 2.8f);
                }
                break;
        }
    }

    private void WaterEffect(int slot)
    {
        switch (slot)
        {
            case 1:
                HealPlayer(6);
                waterBuff = 1.2f;
                PlayMusic(3);
                CombatLogUI.Instance?.ShowMessage("You heal 6 HP (+20% future heals)", 2.5f);
                break;

            case 2:
                int heal = Mathf.RoundToInt(4 * waterBuff);
                int damage = Mathf.RoundToInt(4 * damageBuff*vulnerable);
                HealPlayer(heal);
                DealDamageToEnemy(damage);
                PlayMusic(3);
                CombatLogUI.Instance?.ShowMessage($"You heal {heal} and deal {damage} damage!", 2.8f);
                PlayMusic(1);
                break;

            case 3:
                int dmg = Mathf.RoundToInt(8 * waterBuff);
                int shield2 = Mathf.RoundToInt(8 * waterBuff);
                DealDamageToEnemy(dmg);
                GainShield(shield2);
                PlayMusic(1);
                CombatLogUI.Instance?.ShowMessage($"You hit for {dmg} and gain {shield2} shield!", 2.8f);
                PlayMusic(2);
                if(windDoublecast)
                {
                    CombatLogUI.Instance?.ShowMessage($"Doublecast activates and you go again!", 2.8f);
                    DealDamageToEnemy(dmg);
                    GainShield(shield2);
                    PlayMusic(1);
                    CombatLogUI.Instance?.ShowMessage($"You hit for {dmg} and gain {shield2} shield!", 2.8f);
                    PlayMusic(2);
                }
                break;
        }
    }

    private void WindEffect(int slot)
    {
        switch (slot)
        {
            case 1:
                DealDamageToEnemy(10);
                PlayMusic(1);
                CombatLogUI.Instance?.ShowMessage("You activate Doublecast for slot 3!", 2.5f);
                break;

            case 2:
                CombatLogUI.Instance?.ShowMessage($"You apply vulnerable!");
                vulnerable = 1.5f;
                break;

            case 3:
                int dmg3 = Mathf.RoundToInt(10 * damageBuff * vulnerable);
                DealDamageToEnemy(dmg3);
                PlayMusic(1);
                CombatLogUI.Instance?.ShowMessage($"You strike for {dmg3} damage and double evasion chance for this turn!", 2.5f);
                if(windDoublecast)
                {
                    PlayMusic(1);
                    DealDamageToEnemy(dmg3);
                    CombatLogUI.Instance?.ShowMessage($"You strike for {dmg3} damage and double evasion chance for this turn!", 2.5f);
                }
                break;
        }
    }

    private void EarthEffect(int slot)
    {
        switch (slot)
        {
            case 1:
                GainShield(5);
                shieldBuff = 1.5f;
                PlayMusic(2);
                CombatLogUI.Instance?.ShowMessage("You gain 5 shield (+50% next defense)", 2.5f);
                break;

            case 2:
                int shielded = Mathf.RoundToInt(10 * shieldBuff);
                GainShield(shielded);
                PlayMusic(2);
                CombatLogUI.Instance?.ShowMessage($"You gain {shielded} shield!", 2.5f);
                break;

            case 3:
                int converted = Mathf.Min(25, Mathf.RoundToInt(playerShield * 0.5f));
                DealDamageToEnemy(converted);
                playerShield -= converted;
                PlayMusic(1);
                CombatLogUI.Instance?.ShowMessage($"You convert shield into {converted} dmg!", 2.8f);
                if(windDoublecast)
                {
                    PlayMusic(1);
                    DealDamageToEnemy(converted);
                    CombatLogUI.Instance?.ShowMessage($"You strike with your converted shields for {converted} dmg!", 2.8f);
                }
                break;
        }
    }

    private void DealDamageToEnemy(int dmg)
    {
        if (enemyShield > 0)
        {
            int absorbed = Mathf.Min(enemyShield, dmg);
            enemyShield -= absorbed;
            dmg -= absorbed;
        }
        if (dmg > 0) enemyHealth -= dmg;
        enemyHealth = Mathf.Max(0, enemyHealth);
    }

    private void TakeDamage(int dmg)
    {
        if (playerShield > 0)
        {
            int absorbed = Mathf.Min(playerShield, dmg);
            playerShield -= absorbed;
            dmg -= absorbed;
        }
        if (dmg > 0) playerHealth -= dmg;
        playerHealth = Mathf.Max(0, playerHealth);
    }

    private void GainShield(int amount)
    {
        playerShield += amount;
        PlayMusic(2);
    }

    private void HealPlayer(int amount)
    {
        playerHealth = Mathf.Min(100, playerHealth + amount);
        PlayMusic(3);
    }

    private void UpdateAllUI()
    {
        if (playerHealthText) playerHealthText.text = $"{playerHealth}";
        if (enemyHealthText) enemyHealthText.text = $"{enemyHealth}";
        if (playerShieldText) playerShieldText.text = $"{playerShield}";
        if (enemyShieldText) enemyShieldText.text = $"{enemyShield}";
    }

    private void CheckCombatResult()
    {
        if (gameEnded) return;

        if (playerHealth <= 0 && enemyHealth <= 0)
        {
            CombatLogUI.Instance?.ShowMessage("It's a draw!", 3f);
            gameEnded = true;
            GameManager.Instance?.EndGame(false);
        }
        else if (playerHealth <= 0)
        {
            CombatLogUI.Instance?.ShowMessage("You were defeated!", 3f);
            gameEnded = true;
            GameManager.Instance?.EndGame(false);
        }
        else if (enemyHealth <= 0)
        {
            CombatLogUI.Instance?.ShowMessage("Enemy defeated! You win!", 3f);
            gameEnded = true;
            GameManager.Instance?.EndGame(true);
        }
    }

    public void PlayMusic(int type)
    {
        switch (type)
        {
            case 1: if (audioAttack) audioAttack.Play(); break;
            case 2: if (audioDefend) audioDefend.Play(); break;
            case 3: if (audioHeal) audioHeal.Play(); break;
        }
    }
}
