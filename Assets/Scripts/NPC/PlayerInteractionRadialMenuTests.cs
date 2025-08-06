using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayerInteractionRadialMenuTests
{
    private class TestNPCInteractionUIManager : NPCInteractionUIManager
    {
        public new void Awake() { instance = this; }
    }

    private class TestDialogueManager : DialogueManager
    {
        public new void Awake() { instance = this; }
    }

    private class TestRadialMenuUI : RadialMenuUI
    {
        public new void Awake() { instance = this; }
    }

    [UnityTest]
    public IEnumerator RadialMenuDoesNotOpenWhenTradeUIActive()
    {
        var player = new GameObject("Player");
        var cam = player.AddComponent<Camera>();
        var interaction = player.AddComponent<PlayerInteraction>();
        interaction.playerCamera = cam;
        interaction.interactionDistance = 5f;

        var npc = new GameObject("NPC");
        npc.AddComponent<BoxCollider>();
        var npcController = npc.AddComponent<NPCController>();
        npcController.enabled = false;
        npc.transform.position = player.transform.position + cam.transform.forward * 2f;

        var radialObj = new GameObject("RadialMenu");
        var radialMenu = radialObj.AddComponent<TestRadialMenuUI>();
        radialMenu.menuRoot = new GameObject("MenuRoot");
        radialMenu.menuRoot.SetActive(false);

        var tradeMgrObj = new GameObject("TradeMgr");
        var tradeMgr = tradeMgrObj.AddComponent<TestNPCInteractionUIManager>();
        tradeMgr.npcInteractionUI = new GameObject("TradeUI");
        tradeMgr.npcInteractionUI.SetActive(true);

        typeof(PlayerInteraction).GetField("lastInteractableObject", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(interaction, npc);

        interaction.interactPressed = true;
        interaction.Update();

        Assert.IsFalse(radialMenu.IsOpen);
        yield return null;
    }

    [UnityTest]
    public IEnumerator RadialMenuDoesNotOpenWhenDialogueActive()
    {
        var player = new GameObject("Player");
        var cam = player.AddComponent<Camera>();
        var interaction = player.AddComponent<PlayerInteraction>();
        interaction.playerCamera = cam;
        interaction.interactionDistance = 5f;

        var npc = new GameObject("NPC");
        npc.AddComponent<BoxCollider>();
        var npcController = npc.AddComponent<NPCController>();
        npcController.enabled = false;
        npc.transform.position = player.transform.position + cam.transform.forward * 2f;

        var radialObj = new GameObject("RadialMenu");
        var radialMenu = radialObj.AddComponent<TestRadialMenuUI>();
        radialMenu.menuRoot = new GameObject("MenuRoot");
        radialMenu.menuRoot.SetActive(false);

        var dialogueMgrObj = new GameObject("DialogueMgr");
        var dialogueMgr = dialogueMgrObj.AddComponent<TestDialogueManager>();
        dialogueMgr.dialoguePanel = new GameObject("DialoguePanel");
        dialogueMgr.dialoguePanel.SetActive(true);

        typeof(PlayerInteraction).GetField("lastInteractableObject", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(interaction, npc);

        interaction.interactPressed = true;
        interaction.Update();

        Assert.IsFalse(radialMenu.IsOpen);
        yield return null;
    }
}