using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

public class InputWindow : MonoBehaviour
{
    private static InputWindow instance;
    
    private Button submitBtn;
    private Button cancelBtn;
    private TextMeshProUGUI titleText;
    private TMP_InputField inputField;

    private void Awake() {
        instance = this;

        submitBtn = transform.Find("Submit").GetComponent<Button>();
        cancelBtn = transform.Find("Cancel").GetComponent<Button>();
        titleText = transform.Find("TitleText").GetComponent<TextMeshProUGUI>();
        inputField = transform.Find("Input").GetComponent<TMP_InputField>();

        Hide();
    }

    private void Show(string titleString, string inputString, string validCharacters, int characterLimit, Action onCancel, Action<string> onSubmit)
    {
        gameObject.SetActive(true);
        transform.SetAsLastSibling();

        titleText.text = titleString;

        inputField.characterLimit = characterLimit;
        inputField.onValidateInput = (string text, int charIndex, char addedChar) => {
            return ValidateChar(validCharacters, addedChar);
        };

        inputField.text = inputString;
        inputField.Select();

        submitBtn.onClick.AddListener(() => {
            Hide();
            onSubmit(inputField.text);
        });

        cancelBtn.onClick.AddListener(() => {
            Hide();
            onCancel();
        });
    }

    private char ValidateChar(string validCharacters, char addedChar) {
        if (validCharacters.IndexOf(addedChar) != -1) {
            // Valid
            return addedChar;
        } else {
            // Invalid
            return '\0';
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public static void ShowString_Static(string titleString, string inputString, string validCharacters, int characterLimit, Action onCancel, Action<string> onSubmit) {
        instance.Show(titleString, inputString, validCharacters, characterLimit, onCancel, onSubmit);
    }

    public static void ShowInt_Static(string titleString, int defaultInt, Action onCancel, Action<int> onSubmit) {
        instance.Show(titleString, defaultInt.ToString(), "0123456789-", 20, onCancel, 
            (string inputText) => {
                // Try to Parse input string
                if (int.TryParse(inputText, out int _i)) {
                    onSubmit(_i);
                } else {
                    onSubmit(defaultInt);
                }
            }
        );
    }
}
