using UnityEngine;

public class TipTrigger : MonoBehaviour
{
    public GameObject tipText;
    public string message = "Hi";

    void Start()
    {
        if (tipText != null)
        {
            tipText.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && tipText != null)
        {
            tipText.SetActive(true);
            tipText.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = message;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && tipText != null)
        {
            tipText.SetActive(false);
        }
    }
}
