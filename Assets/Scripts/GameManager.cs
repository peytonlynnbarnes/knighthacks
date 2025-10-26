using UnityEngine;
using UnityEngine.UI;
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

    // Called from SlotsButton when a selection is made
    public void AddChosenItem(string itemName, Sprite itemSprite)
    {
        chosenItems.Add(itemName);
        currentRoll++;

        Debug.Log($"✅ Added {itemName} to chosen items ({currentRoll}/{maxRollsPerTurn})");

        UpdateSlotIcon(currentRoll - 1, itemSprite);

        // Check if all items chosen
        if (currentRoll >= maxRollsPerTurn)
        {
            canSpin = false;
            Debug.Log("🎯 All spins used! Processing combat...");

            // 🆕 Trigger combat handler
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

    public void StartNextRound()
    {
        currentRoll = 0;
        canSpin = true;
        chosenItems.Clear();

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

        Debug.Log("🔁 Next round started!");
    }
}
