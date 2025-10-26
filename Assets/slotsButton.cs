using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SlotsButton : MonoBehaviour
{
    [Header("Animation Settings")]
    public string spriteObjectName = "slots_spritesheet";
    public AudioSource audioSource; // assign in Inspector
    public string triggerName = "slotsActive";
    private Animator spriteAnimator;

    [Header("UI Image Object Names")]
    public string[] imageNames = { "item1", "item2", "item3" };

    [Header("Item Buttons")]
    public string[] buttonNames = { "item1_button", "item2_button", "item3_button" };

    [Header("Element Sprites")]
    public Sprite fireSprite;
    public Sprite windSprite;
    public Sprite waterSprite;
    public Sprite earthSprite;
    public Sprite defaultSprite;

    private Sprite[] elementSprites;
    private Sprite[] rolledSprites = new Sprite[3];

    [Header("Slot Settings")]
    [Range(0f, 1f)] public float jackpotChance = 0.2f;
    public float imageDelay = 0.4f;

    private int item_Time = 0; 
    private bool rolling = false; // renamed for clarity

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnSlotClick);

        GameObject spriteObj = GameObject.Find(spriteObjectName);
        if (spriteObj != null)
            spriteAnimator = spriteObj.GetComponent<Animator>();

        elementSprites = new Sprite[] { fireSprite, windSprite, waterSprite, earthSprite };
        SetItemButtonsInteractable(false);
    }

    void OnSlotClick()
    {
        // 🆕 check with GameManager
        if (!GameManager.Instance.canSpin)
        {
            Debug.Log("⛔ No spins left this round!");
            return;
        }

        if (rolling) return;
        rolling = true;
        item_Time = 0;

        Debug.Log($"🎬 Starting spin #{GameManager.Instance.currentRoll + 1}");

        if (spriteAnimator != null)
            spriteAnimator.SetTrigger(triggerName);

        // Reset visuals
        foreach (string name in imageNames)
        {
            GameObject obj = GameObject.Find(name);
            if (obj != null)
            {
                Image img = obj.GetComponent<Image>();
                if (img != null && defaultSprite != null)
                    img.sprite = defaultSprite;
            }
        }

        SetItemButtonsInteractable(false);
        StartCoroutine(UpdateImagesAfterDelay(imageDelay));
    }

    public void ResetRoll()
    {
        item_Time = 0;
        rolling = false;

        // Make sure the spin button becomes clickable again
        Button spinButton = GetComponent<Button>();
        if (spinButton != null)
            spinButton.interactable = true;

        Debug.Log("🎯 Slot roll reset — ready for next round!");
    }


    IEnumerator UpdateImagesAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        bool jackpot = Random.value < jackpotChance;

        if (jackpot)
        {
            Sprite jackpotSprite = elementSprites[Random.Range(0, elementSprites.Length)];
            for (int i = 0; i < 3; i++)
                rolledSprites[i] = jackpotSprite;
        }
        else
        {
            for (int i = 0; i < 3; i++)
                rolledSprites[i] = elementSprites[Random.Range(0, elementSprites.Length)];
        }

        // Apply new images
        for (int i = 0; i < imageNames.Length; i++)
        {
            GameObject imgObj = GameObject.Find(imageNames[i]);
            if (imgObj != null)
            {
                Image img = imgObj.GetComponent<Image>();
                if (img != null)
                    img.sprite = rolledSprites[i];
            }
        }

        item_Time = 1;
        rolling = false;

        // enable selection
        for (int i = 0; i < buttonNames.Length; i++)
        {
            int index = i;
            GameObject btnObj = GameObject.Find(buttonNames[i]);
            if (btnObj != null)
            {
                Button btn = btnObj.GetComponent<Button>();
                btn.interactable = true;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnItemSelected(index));
            }
        }
    }

  void OnItemSelected(int index)
{
    if (item_Time != 1)
    {
        Debug.LogWarning("⏳ Can't select yet!");
        return;
    }

    item_Time = 0;
    SetItemButtonsInteractable(false);

    Sprite chosen = rolledSprites[index];
    string chosenName = chosen != null ? chosen.name : "Unknown";

    // 🆕 Send both name and sprite to GameManager
    GameManager.Instance.AddChosenItem(chosenName, chosen);

    // 🧹 Reset ALL slot images to default
    for (int i = 0; i < imageNames.Length; i++)
    {
        GameObject imgObj = GameObject.Find(imageNames[i]);
        if (imgObj != null)
        {
            Image img = imgObj.GetComponent<Image>();
            if (img != null && defaultSprite != null)
                img.sprite = defaultSprite;
        }
    }

    Debug.Log($"🪄 Player selected {chosenName} (spin #{GameManager.Instance.currentRoll})");

    // Disable spin button if out of turns
    if (!GameManager.Instance.canSpin)
        GetComponent<Button>().interactable = false;
}


    void SetItemButtonsInteractable(bool state)
    {
        foreach (string btnName in buttonNames)
        {
            GameObject btnObj = GameObject.Find(btnName);
            if (btnObj != null)
            {
                Button btn = btnObj.GetComponent<Button>();
                btn.interactable = state;
            }
        }
    }

    public void PlayMusic()
    {
        if (audioSource == null)
        {
            Debug.LogWarning("⚠️ No AudioSource assigned!");
            return;
        }

        audioSource.Play(); // 🔊 start playing
        Debug.Log("🎵 Music started!");
    }
}
