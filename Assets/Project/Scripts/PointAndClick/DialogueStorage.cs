using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Talking
{
    public string name;
    public string dialogue;
    public DialogueSound sound;
}

[System.Serializable]
public class DialogueStorage
{
    public List<Talking> dialogue;
    public float dialogueSpeed = 10f;
    public List<Talking> alternativeDialogue;
    public bool useAltDialogue = false;
}
