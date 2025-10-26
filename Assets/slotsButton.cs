using UnityEngine;
using UnityEngine.UI;

public class SlotsButton : MonoBehaviour
{
    public string spriteObjectName = "slots_spritesheet"; // name of the sprite object in the Hierarchy
    public string triggerName = "slotsActive";           // must match the Animator trigger
    private Animator spriteAnimator;

    void Start()
    {
        // Automatically connect the button click
        GetComponent<Button>().onClick.AddListener(PlaySpriteAnimation);

        // Find the Animator on the sprite object by name
        GameObject spriteObj = GameObject.Find(spriteObjectName);
        if (spriteObj != null)
        {
            spriteAnimator = spriteObj.GetComponent<Animator>();
        }

        if (spriteAnimator == null)
        {
            Debug.LogWarning($"Could not find Animator on object '{spriteObjectName}'!");
        }
    }

    void PlaySpriteAnimation()
    {
        if (spriteAnimator != null)
        {
            spriteAnimator.SetTrigger(triggerName);
        }
    }
}
