﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogPanelController : WindowContentController {

    public event System.Action OnConfirmAction = delegate { };
    public override event Action<ICloseableWindowContents> OnCloseWindowHandler = delegate { };

    [SerializeField]
    private Text characterNameText;

    [SerializeField]
    private Text nodeText;

    /*
    [SerializeField]
    private Button nextButton;
    */

    [SerializeField]
    private Text buttonText;

    [SerializeField]
    private GameObject viewQuestButton;

    [SerializeField]
    private GameObject acceptQuestButton;

    [SerializeField]
    private GameObject continueButton;

    [SerializeField]
    int dialogFontSize = 30;

    private Interactable interactable;

    private Dialog dialog;

    private Quest quest;

    public Dialog MyDialog { get => dialog; set => dialog = value; }
    public Interactable MyInteractable { get => interactable; set => interactable = value; }
    public Quest MyQuest { get => quest; set => quest = value; }

    private int dialogIndex = 0;

    public void Setup(Quest quest, Interactable interactable) {
        ClearSettings();
        MyQuest = quest;
        MyInteractable = interactable;
        MyDialog = SystemDialogManager.MyInstance.GetResource(quest.MyName);
        PopupWindowManager.MyInstance.dialogWindow.OpenWindow();
    }

    public void Setup(string dialogName, Interactable interactable) {
        ClearSettings();
        MyInteractable = interactable;
        MyDialog = SystemDialogManager.MyInstance.GetResource(dialogName);
        PopupWindowManager.MyInstance.dialogWindow.OpenWindow();
    }

    public void ClearSettings() {
        dialogIndex = 0;
        MyInteractable = null;
        MyQuest = null;
        MyDialog = null;
    }

    public void CancelAction() {
        //Debug.Log("NewGameMenuController.CancelAction()");
        PopupWindowManager.MyInstance.dialogWindow.CloseWindow();
    }

    public void ConfirmAction() {
        //Debug.Log("NewGameMenuController.ConfirmAction(): dialogIndex: " + dialogIndex + "; DialogNode Count: " + MyDialog.MyDialogNodes.Count);
        dialogIndex++;
        if (dialogIndex >= MyDialog.MyDialogNodes.Count) {
            if (quest == null) {
                // no one is currently subscribed so safe to set turnedIn at bottom because nothing here depends on it being set yet
                OnConfirmAction();
                PopupWindowManager.MyInstance.dialogWindow.CloseWindow();
            } else {
                if (!quest.TurnedIn) {
                    DisplayQuestText();
                    continueButton.SetActive(false);
                    viewQuestButton.SetActive(true);
                    acceptQuestButton.SetActive(true);
                } else {
                    Debug.Log("NewGameMenuController.ConfirmAction(): dialogIndex: " + dialogIndex + "; DialogNode Count: " + MyDialog.MyDialogNodes.Count + "; TRIED TO DISPLAY ALREADY TURNED IN QUEST!");
                    PopupWindowManager.MyInstance.dialogWindow.CloseWindow();
                }
            }
            // TESTING SETTING THIS LAST SO THE DIALOG WINDOW IS DETECTED AS CLOSED, AND OTHER WINDOWS DON'T GET BLOCKED FROM POPPING.
            MyDialog.TurnedIn = true;
        } else {
            DisplayNodeText();
        }
    }

    public void DisplayQuestText() {
        //Debug.Log("DialogPanelController.DisplayQuestText()");
        if (MyQuest != null) {
            nodeText.text = MyQuest.GetObjectiveDescription();
        }
    }

    public void ViewQuest() {
        QuestGiverUI.MyInstance.MyInteractable = interactable;
        PopupWindowManager.MyInstance.questGiverWindow.OpenWindow();
        QuestGiverUI.MyInstance.ShowDescription(MyQuest);
        PopupWindowManager.MyInstance.dialogWindow.CloseWindow();
    }

    public void AcceptQuest() {
        //Debug.Log("DialogPanelController.AcceptQuest()");
        // TESTING CLOSE THIS FIRST SO OTHER WINDOWS AREN'T BLOCKED FROM POPPING
        PopupWindowManager.MyInstance.dialogWindow.CloseWindow();

        QuestLog.MyInstance.AcceptQuest(MyQuest.MyName);
        //interactable.CheckForInteractableObjectives(MyQuest.MyName);
    }

    public void DisplayNodeText() {
        if (dialogIndex > MyDialog.MyDialogNodes.Count + 1) {
            //Debug.Log("Past last node index.  will not display");
            return;
        }
        if (characterNameText != null) {
            characterNameText.text = MyInteractable.GetComponent<INamePlateUnit>().MyDisplayName;
        }
        if (nodeText != null) {
            nodeText.text = string.Format("<size={0}>{1}</size>", dialogFontSize, MyDialog.MyDialogNodes[dialogIndex].MyDescription);
            CombatLogUI.MyInstance.WriteChatMessage(MyDialog.MyDialogNodes[dialogIndex].MyDescription);
        }
        if (buttonText != null) {
            buttonText.text = MyDialog.MyDialogNodes[dialogIndex].MyNextOption;
            //Debug.Log("DialogPanelController.OnOpenWindow(): ButtonText is not null, rebuilding layout");
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        } else {
            Debug.Log("DialogPanelController.OnOpenWindow(): ButtonText is null!!");
        }
    }

    public override void OnOpenWindow() {
        //Debug.Log("DialogPanelController.OnOpenWindow()");
        base.OnOpenWindow();

        // these go first or they will squish the text of the continue button out of place
        viewQuestButton.SetActive(false);
        acceptQuestButton.SetActive(false);
        continueButton.SetActive(true);
        dialogIndex = 0;
        PopupWindowManager.MyInstance.dialogWindow.SetWindowTitle(interactable.MyName);

        // this one last because it does a layout rebuild
        DisplayNodeText();
    }

    public override void OnCloseWindow() {
        base.OnCloseWindow();
        OnCloseWindowHandler(this);
        // TESTING, allow window to close before taking actions, to prevent blocking other windows from opening since dialog blocks every other interactable
        /*
        dialogIndex = 0;
        MyInteractable = null;
        MyQuest = null;
        MyDialog = null;
        */
        viewQuestButton.SetActive(false);
        acceptQuestButton.SetActive(false);
        continueButton.SetActive(false);
    }


}
