using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Turn Settings")]
    public int maxRollsPerTurn = 3;
    public int currentRoll = 0;
    public bool canSpin = true;

    [Header("Chosen Items")]
    public List<string> chosenItems = new List<string>();

    [Header("Slot Icon Object Names")]
    public string[] slotIconNames = { "slot1_icon", "slot2_icon", "slot3_icon" };

    [Header("Sprites")]
    public Sprite defaultSprite;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        // ✅ Ensure everything starts fresh on scene load
        ResetSpinState();
    }

    public void AddChosenItem(string itemName, Sprite itemSprite)
    {
        if (!canSpin)
        {
            Debug.Log("⛔ Can't spin — waiting for next round!");
            return;
        }

        chosenItems.Add(itemName);
        currentRoll++;

        Debug.Log($"✅ Added {itemName} to chosen items ({currentRoll}/{maxRollsPerTurn})");
        UpdateSlotIcon(currentRoll - 1, itemSprite);

        // reached turn limit
        if (currentRoll >= maxRollsPerTurn)
        {
            canSpin = false;
            Debug.Log("🎯 All spins used! Processing combat...");

            if (HandleCombat.Instance != null)
                HandleCombat.Instance.ProcessTurn(chosenItems);
            else
                Debug.LogWarning("⚠️ No HandleCombat instance found in scene!");
        }
    }

    private void UpdateSlotIcon(int index, Sprite sprite)
    {
        if (index < 0 || index >= slotIconNames.Length) return;

        GameObject iconObj = GameObject.Find(slotIconNames[index]);
        if (iconObj != null)
        {
            Image img = iconObj.GetComponent<Image>();
            if (img != null)
                img.sprite = sprite;
        }
    }

    // 🧹 Clears slot icons + resets internal choices
    public void ClearItemSlots()
    {
        foreach (string name in slotIconNames)
        {
            GameObject iconObj = GameObject.Find(name);
            if (iconObj != null)
            {
                Image img = iconObj.GetComponent<Image>();
                if (img != null && defaultSprite != null)
                    img.sprite = defaultSprite;
            }
        }

        chosenItems.Clear();
        currentRoll = 0;
        canSpin = true; // ✅ ensures player can roll again next round

        Debug.Log("🧹 Slots cleared and player can spin again!");
    }

    public void StartNextRound()
    {
        ClearItemSlots();
        ResetSpinState();

        Debug.Log("🔁 Next round started!");

        // reset slot machine state
        SlotsButton slot = Object.FindFirstObjectByType<SlotsButton>();
        if (slot != null)
            slot.ResetRoll();
    }

    // ✅ Centralized spin reset logic
    public void ResetSpinState()
    {
        currentRoll = 0;
        canSpin = true;

        Debug.Log("🎯 Spin state reset — ready for next round!");
    }

    // 🚪 Game end logic
    public void EndGame(bool playerWon)
    {
        canSpin = false;
        Debug.Log(playerWon ? "🎉 Player Victory!" : "💀 Player Defeat!");

        // small delay before switching scenes
        StartCoroutine(LoadEndSceneDelayed(playerWon));
    }

    private IEnumerator LoadEndSceneDelayed(bool playerWon)
    {
        yield return new WaitForSeconds(2.5f);
        if (playerWon)
        {
            SceneManager.LoadScene(3);
        }
        else
        {
            SceneManager.LoadScene(2);
        }

        // 🧭 Switch to Game Over / Main Menu (index 0 or 2)
        
    }
}
