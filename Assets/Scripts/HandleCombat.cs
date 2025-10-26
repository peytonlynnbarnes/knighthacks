using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class HandleCombat : MonoBehaviour
{
    public static HandleCombat Instance;

    [Header("Player & Enemy Stats")]
    public int playerHealth = 100;
    public int playerShield = 0;
    public int enemyHealth = 100;
    public int enemyShield = 0;

    [Header("UI Elements (TMP)")]
    public TMP_Text playerHealthText;
    public TMP_Text enemyHealthText;

    // modifiers for chaining effects
    private float damageBuff = 1f;
    private float shieldBuff = 1f;
    private float waterBuff = 1f;
    private bool windQuickcast = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        UpdateHealthUI();
    }

    public void ProcessTurn(List<string> chosenItems)
    {
        Debug.Log("⚔️ Processing combat turn...");
        damageBuff = 1f;
        shieldBuff = 1f;
        waterBuff = 1f;
        windQuickcast = false;

        for (int i = 0; i < chosenItems.Count; i++)
        {
            int slot = i + 1;
            string raw = chosenItems[i].ToLower();

            // normalize variants
            string item = "none";
            if (raw.Contains("fire")) item = "fire";
            else if (raw.Contains("wind") || raw.Contains("air")) item = "wind";
            else if (raw.Contains("water")) item = "water";
            else if (raw.Contains("earth")) item = "earth";

            switch (item)
            {
                case "fire":
                    FireEffect(slot);
                    break;

                case "water":
                    WaterEffect(slot);
                    break;

                case "wind":
                    WindEffect(slot);
                    break;

                case "earth":
                    EarthEffect(slot);
                    break;

                default:
                    Debug.Log("❓ Unknown element: " + raw);
                    break;
            }

            UpdateHealthUI();
            if (playerHealth <= 0 || enemyHealth <= 0) break;
        }

        CheckCombatResult();
        StartCoroutine(NextRoundDelay());
    }

    // ----------------------------
    // 🔥 FIRE
    // ----------------------------
    private void FireEffect(int slot)
    {
        switch (slot)
        {
            case 1:
                Debug.Log("🔥 Fire Slot 1: Deal 5 damage and boost next 2 attacks by 50%!");
                DealDamageToEnemy(5);
                damageBuff = 1.5f;
                break;

            case 2:
                int baseDmg2 = 16;
                int boostedDmg2 = Mathf.RoundToInt(baseDmg2 * damageBuff);
                DealDamageToEnemy(boostedDmg2);
                Debug.Log($"🔥 Fire Slot 2: Deal {boostedDmg2} damage and burn for 6 over time!");
                // Burn as delayed DoT
                StartCoroutine(ApplyBurn(3, 2));
                break;

            case 3:
                int baseDmg3 = 22;
                int boostedDmg3 = Mathf.RoundToInt(baseDmg3 * damageBuff);
                int recoil = 5;
                DealDamageToEnemy(boostedDmg3);
                playerHealth -= recoil;
                Debug.Log($"🔥 Fire Slot 3: Inferno Finisher! Deal {boostedDmg3} and take {recoil} recoil!");
                break;
        }
    }

    // ----------------------------
    // 💧 WATER
    // ----------------------------
    private void WaterEffect(int slot)
    {
        switch (slot)
        {
            case 1:
                Debug.Log("💧 Water Slot 1: Heal 6 HP, cleanse 1 debuff, and boost next 2 effects by 20%!");
                HealPlayer(6);
                waterBuff = 1.2f;
                break;

            case 2:
                int heal = Mathf.RoundToInt(18 * waterBuff);
                int shield = Mathf.RoundToInt(10 * waterBuff);
                HealPlayer(heal);
                GainShield(shield);
                Debug.Log($"💧 Water Slot 2: Heal {heal} HP and gain {shield} Shield.");
                break;

            case 3:
                int dmg = Mathf.RoundToInt(12 * waterBuff);
                int shield2 = Mathf.RoundToInt(8 * waterBuff);
                DealDamageToEnemy(dmg);
                GainShield(shield2);
                Debug.Log($"💧 Water Slot 3: Deal {dmg} damage and gain {shield2} Shield.");
                break;
        }
    }

    // ----------------------------
    // 🌪️ WIND
    // ----------------------------
    private void WindEffect(int slot)
    {
        switch (slot)
        {
            case 1:
                Debug.Log("🌪️ Wind Slot 1: Deal 10 damage. Next 2 slots have 20% less effect but 50% chance to echo 50%!");
                DealDamageToEnemy(10);
                windQuickcast = true;
                break;

            case 2:
                int baseDmg = 14;
                int actual = windQuickcast ? Mathf.RoundToInt(baseDmg * 0.8f) : baseDmg;
                DealDamageToEnemy(actual);
                GainShield(4);
                if (Random.value < 0.3f) // 30% double-cast
                {
                    DealDamageToEnemy(actual);
                    Debug.Log("🌪️ Wind double-cast triggered!");
                }
                break;

            case 3:
                int dmg3 = windQuickcast ? Mathf.RoundToInt(8 * 0.8f) : 8;
                DealDamageToEnemy(dmg3);
                Debug.Log($"🌪️ Wind Slot 3: Deal {dmg3} and apply Vulnerable (+25% dmg next turn)!");
                // Could add Vulnerable status to enemy here
                break;
        }
    }

    // ----------------------------
    // 🪨 EARTH
    // ----------------------------
    private void EarthEffect(int slot)
    {
        switch (slot)
        {
            case 1:
                Debug.Log("🪨 Earth Slot 1: Gain 14 Shield; next 2 Shield effects are 50% stronger!");
                GainShield(14);
                shieldBuff = 1.5f;
                break;

            case 2:
                int baseShield = Mathf.RoundToInt(20 * shieldBuff);
                GainShield(baseShield);
                Debug.Log($"🪨 Earth Slot 2: Gain {baseShield} Shield.");
                break;

            case 3:
                int converted = Mathf.Min(25, Mathf.RoundToInt(playerShield * 0.5f));
                DealDamageToEnemy(converted);
                playerShield -= converted;
                Debug.Log($"🪨 Earth Slot 3: Quake! Convert {converted * 2} Shield into {converted} damage.");
                break;
        }
    }

    // ----------------------------
    // Utility methods
    // ----------------------------
    private void DealDamageToEnemy(int dmg)
    {
        if (enemyShield > 0)
        {
            int absorbed = Mathf.Min(enemyShield, dmg);
            enemyShield -= absorbed;
            dmg -= absorbed;
        }

        if (dmg > 0)
            enemyHealth -= dmg;

        enemyHealth = Mathf.Max(0, enemyHealth);
    }

    private void GainShield(int amount)
    {
        playerShield += amount;
    }

    private void HealPlayer(int amount)
    {
        playerHealth = Mathf.Min(100, playerHealth + amount);
    }

    private IEnumerator ApplyBurn(int damagePerTick, int ticks)
    {
        for (int i = 0; i < ticks; i++)
        {
            yield return new WaitForSeconds(1f);
            DealDamageToEnemy(damagePerTick);
            Debug.Log($"🔥 Burn tick: Enemy takes {damagePerTick} damage (Tick {i + 1}/{ticks})");
            UpdateHealthUI();
        }
    }

    private void UpdateHealthUI()
    {
        if (playerHealthText != null)
            playerHealthText.text = $"{playerHealth} ({playerShield}🛡)";
        else
            Debug.LogWarning("⚠️ Player TMP text not assigned!");

        if (enemyHealthText != null)
            enemyHealthText.text = $"{enemyHealth} ({enemyShield}🛡)";
        else
            Debug.LogWarning("⚠️ Enemy TMP text not assigned!");
    }

    private void CheckCombatResult()
    {
        if (playerHealth <= 0 && enemyHealth <= 0)
            Debug.Log("💀 It's a draw!");
        else if (playerHealth <= 0)
            Debug.Log("❌ Player defeated!");
        else if (enemyHealth <= 0)
            Debug.Log("🏆 Enemy defeated!");
    }

    private IEnumerator NextRoundDelay()
    {
        yield return new WaitForSeconds(2f);
        GameManager.Instance.StartNextRound();
        UpdateHealthUI();
    }
}
