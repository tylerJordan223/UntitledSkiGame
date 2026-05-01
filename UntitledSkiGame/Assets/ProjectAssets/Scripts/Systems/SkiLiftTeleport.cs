using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkiLiftTeleport : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private Rigidbody playerRb;

    [Header("Teleport Target")]
    [SerializeField] private Transform liftTopSpawnPoint;

    [Header("UI")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private TMP_Text liftMessageText;

    [Header("Timing")]
    [SerializeField] private float fadeInDuration = 1.25f;
    [SerializeField] private float messageHoldDuration = 1.5f;
    [SerializeField] private float fadeOutDuration = 1.0f;

    [Header("Interaction")]
    [SerializeField] private bool triggerAutomatically = true;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private bool playerInside;
    private bool isTeleporting;

    private void Start()
    {
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }

        if (liftMessageText != null)
        {
            Color textColor = liftMessageText.color;
            textColor.a = 0f;
            liftMessageText.color = textColor;
        }
    }

    private void Update()
    {
        if (!triggerAutomatically && playerInside && !isTeleporting && Input.GetKeyDown(interactKey))
        {
            StartCoroutine(LiftSequence());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isTeleporting)
            return;

        if (player != null && other.transform.root == player.root)
        {
            playerInside = true;

            if (triggerAutomatically)
            {
                StartCoroutine(LiftSequence());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (player != null && other.transform.root == player.root)
        {
            playerInside = false;
        }
    }

    private IEnumerator LiftSequence()
    {
        isTeleporting = true;

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.blocksRaycasts = true;
        }

        yield return StartCoroutine(FadeScreen(0f, 1f, fadeInDuration));
        SetMessageAlpha(1f);

        yield return new WaitForSeconds(messageHoldDuration);

        TeleportPlayer();

        yield return new WaitForSeconds(0.4f);

        SetMessageAlpha(0f);
        yield return StartCoroutine(FadeScreen(1f, 0f, fadeOutDuration));

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.blocksRaycasts = false;
        }

        isTeleporting = false;
    }

    private IEnumerator FadeScreen(float startAlpha, float endAlpha, float duration)
    {
        if (fadeCanvasGroup == null)
            yield break;

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            yield return null;
        }

        fadeCanvasGroup.alpha = endAlpha;
    }

    private void SetMessageAlpha(float alpha)
    {
        if (liftMessageText == null)
            return;

        Color textColor = liftMessageText.color;
        textColor.a = alpha;
        liftMessageText.color = textColor;
    }

    private void TeleportPlayer()
    {
        if (player == null || liftTopSpawnPoint == null)
            return;

        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;
        }

        player.position = liftTopSpawnPoint.position;
        player.rotation = liftTopSpawnPoint.rotation;
    }
}