//using TMPro;
//using UnityEngine;
//using UnityEngine.UI;

//public class DialogueManager : MonoBehaviour
//{
//    public static DialogueManager instance;

//    [Header("UI References")]
//    public GameObject dialoguePanel;
//    public TextMeshProUGUI dialogueText;
//    public Transform responsesParent;
//    public Button responseButtonPrefab;

//    private DialogueLine currentLine;

//    private void Awake()
//    {
//        if (instance == null)
//            instance = this;
//        else
//            Destroy(gameObject);

//        dialoguePanel.SetActive(false);
//    }

//    public void StartDialogue(DialogueLine startingLine)
//    {
//        if (startingLine == null)
//            return;

//        currentLine = startingLine;
//        ShowCurrentLine();
//        dialoguePanel.SetActive(true);
//        if (InputFreezeManager.instance != null)
//        {
//            InputFreezeManager.instance.FreezePlayerAndCursor();
//        }
//    }

//    private void EndDialogue()
//    {
//        dialoguePanel.SetActive(false);
//        foreach (Transform child in responsesParent)
//            Destroy(child.gameObject);

//        if (InputFreezeManager.instance != null)
//        {
//            InputFreezeManager.instance.UnfreezePlayerAndCursor();
//        }
//    }

//    private void ShowCurrentLine()
//    {
//        dialogueText.text = currentLine.lineText;

//        foreach (Transform child in responsesParent)
//            Destroy(child.gameObject);

//        if (currentLine.responses == null || currentLine.responses.Length == 0)
//        {
//            Button endButton = Instantiate(responseButtonPrefab, responsesParent);
//            endButton.GetComponentInChildren<TextMeshProUGUI>().text = "Close";
//            endButton.onClick.AddListener(EndDialogue);
//            return;
//        }

//        foreach (DialogueResponse response in currentLine.responses)
//        {
//            DialogueResponse localResponse = response;
//            Button button = Instantiate(responseButtonPrefab, responsesParent);
//            button.GetComponentInChildren<TextMeshProUGUI>().text = localResponse.responseText;
//            button.onClick.AddListener(() => OnResponseSelected(localResponse));
//        }
//    }

//    private void OnResponseSelected(DialogueResponse response)
//    {
//        if (response.nextLine == null)
//        {
//            EndDialogue();
//        }
//        else
//        {
//            currentLine = response.nextLine;
//            ShowCurrentLine();
//        }
//    }
//}