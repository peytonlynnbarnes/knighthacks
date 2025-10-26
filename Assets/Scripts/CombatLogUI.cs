using UnityEngine;
using TMPro;
using System.Collections;

public class CombatLogUI : MonoBehaviour
{
    public static CombatLogUI Instance;

    [Header("UI")]
    public TMP_Text logText; // assign in inspector

    private Coroutine messageRoutine;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// Displays a message immediately (replaces the old one)
    /// </summary>
    public void SetMessage(string message)
    {
        if (logText == null) return;
        logText.text = message;
    }

    /// <summary>
    /// Displays a message for a set time, replacing previous messages
    /// </summary>
    public void ShowMessage(string message, float duration = 2f)
    {
        if (messageRoutine != null)
            StopCoroutine(messageRoutine);

        messageRoutine = StartCoroutine(DisplayRoutine(message, duration));
    }

    private IEnumerator DisplayRoutine(string msg, float duration)
    {
        SetMessage(msg);
        yield return new WaitForSeconds(duration);
        SetMessage(""); // clear after duration
    }
}
