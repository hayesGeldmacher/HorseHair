using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum CursorType
{
    Default,
    Item,
    NonEssentialItem,
    Environment,
    Goal,
    AnimatedItem,
}

public class CursorManager : MonoBehaviour
{
    [SerializeField] private List<CursorsScript> CursorsScriptsList;
    [SerializeField] private ItemBox itemBox;

    private Dictionary<CursorType, CursorsScript> _CursorsScriptsDict;
    private CursorsScript _CursorsScript;
    private int _currentFrame;
    private float _frameTimer;
    private int _frameCount;

    private void OnEnable()
    {
        EventClick.OnObjectHovered += HandleObjectHovered;
    }

    private void OnDisable()
    {
        EventClick.OnObjectHovered -= HandleObjectHovered;
    }

    private void Start()
    {
        _CursorsScriptsDict = new Dictionary<CursorType, CursorsScript>();
        foreach (CursorsScript anim in CursorsScriptsList)
        {
            _CursorsScriptsDict[anim.cursorType] = anim;
        }

        SetActiveCursorsScript(_CursorsScriptsDict[CursorType.Default]);
    }

    private void Update()
    {
        _frameTimer -= Time.deltaTime;
        if (_frameTimer <= 0f)
        {
            _frameTimer += _CursorsScript.frameRate;
            _currentFrame = (_currentFrame + 1) % _frameCount;
            Cursor.SetCursor(_CursorsScript.frames[_currentFrame], _CursorsScript.offset, CursorMode.Auto);
        }

        if (Input.GetKeyDown(KeyCode.T))
            SetActiveCursorsScript(_CursorsScriptsDict[CursorType.Environment]);
    }

    private void SetActiveCursorsScript(CursorsScript CursorsScript)
    {
        _CursorsScript = CursorsScript;
        _currentFrame = 0;
        _frameCount = CursorsScript.frames.Length;
        _frameTimer = CursorsScript.frameRate;     
    }

    private void HandleObjectHovered(ObjectType type, string name)
    {

        bool playHover = true;
        switch (type)
        {
            case ObjectType.Environment:
                SetActiveCursorsScript(_CursorsScriptsDict[CursorType.Environment]);
                itemBox.ShowTextbox(name);
                break;
            case ObjectType.Item:
                SetActiveCursorsScript(_CursorsScriptsDict[CursorType.Item]);
                itemBox.ShowTextbox(name);
                break;
            case ObjectType.NEI:
                SetActiveCursorsScript(_CursorsScriptsDict[CursorType.NonEssentialItem]);
                itemBox.ShowTextbox(name);
                break;
            case ObjectType.Goal:
                SetActiveCursorsScript(_CursorsScriptsDict[CursorType.Goal]);
                itemBox.ShowTextbox(name);
                break;
            case ObjectType.AI:
                SetActiveCursorsScript(_CursorsScriptsDict[CursorType.Item]);
                itemBox.ShowTextbox(name);
                break;
            case ObjectType.FPB:
                SetActiveCursorsScript(_CursorsScriptsDict[CursorType.Goal]);
                itemBox.ShowTextbox(name);
                break;
            default:
                SetActiveCursorsScript(_CursorsScriptsDict[CursorType.Default]);
                itemBox.HideTextbox();
                playHover = false;
                break;
        }
        if (playHover) { AudioManager.instance.PlayHoverSound(); } //plays a hover sfx when etnering an object hover -HG
    }
}
