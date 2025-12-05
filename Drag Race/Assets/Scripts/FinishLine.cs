using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class FinishLine : MonoBehaviour
{
    public GameObject finishPanel;           
    public TextMeshProUGUI finishText;       
    public Button restartButton;
    public Button menuButton;
    public string menuSceneName;

    [Header("Timing")]
    public float menuDelay = 1.5f;            

    bool finished = false;                   
    Coroutine showPanelCoroutine = null;

    void Start()
    {
        if (finishPanel != null) finishPanel.SetActive(false);
        if (finishText != null) finishText.enabled = false;

        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(RestartLevel);
        }

        if (menuButton != null)
        {
            menuButton.onClick.RemoveAllListeners();
            menuButton.onClick.AddListener(GoToMenu);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (finished) return;

        // detect player by searching for ShipController in the collided object or its children
        var sc = other.GetComponentInChildren<ShipController>();
        if (sc != null)
        {
            finished = true;

           
            ShowFinishTextImmediate();

            // stop gas and freeze ship
            sc.SetGas(false);
            var rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.constraints = RigidbodyConstraints.FreezeAll;
            }

            // Start coroutine to show the menu panel after menuDelay seconds
            showPanelCoroutine = StartCoroutine(ShowPanelAfterDelay(menuDelay));
        }
    }

    private void ShowFinishTextImmediate()
    {
        if (finishText != null)
        {
            finishText.enabled = true;
            finishText.gameObject.SetActive(true);
            finishText.text = "FINISH!";
        }
    }

    private IEnumerator ShowPanelAfterDelay(float delay)
    {
        // Wait, then show panel (if assigned)
        yield return new WaitForSeconds(delay);

        if (finishPanel != null)
        {
            finishPanel.SetActive(true);
        }

        showPanelCoroutine = null;
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMenu()
    {
        if (string.IsNullOrEmpty(menuSceneName))
        {
            Debug.LogWarning("FinishLine.GoToMenu: menuSceneName is empty. Assign a valid scene name in the inspector or use SceneManager to load a scene index.");
            return;
        }

        SceneManager.LoadScene(menuSceneName);
    }

  
    public void CancelPendingPanel()
    {
        if (showPanelCoroutine != null)
        {
            StopCoroutine(showPanelCoroutine);
            showPanelCoroutine = null;
        }
    }
}
