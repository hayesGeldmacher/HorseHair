using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    [SerializeField] private List<CursorAnimation> cursorAnimationsList;

    private Dictionary<CursorType, CursorAnimation> _cursorAnimationsDict;
    private CursorAnimation _cursorAnimation;
    private int _currentFrame;
    private float _frameTimer;
    private int _frameCount;

    public enum CursorType
    {
        Default,
        Item,
        NonEssentialItem,
        Environment,
    }

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
        _cursorAnimationsDict = new Dictionary<CursorType, CursorAnimation>();
        foreach (CursorAnimation anim in cursorAnimationsList)
        {
            _cursorAnimationsDict[anim.cursorType] = anim;
        }

        SetActiveCursorAnimation(_cursorAnimationsDict[CursorType.Default]);
    }

    private void Update()
    {
        _frameTimer -= Time.deltaTime;
        if (_frameTimer <= 0f)
        {
            _frameTimer += _cursorAnimation.frameRate;
            _currentFrame = (_currentFrame + 1) % _frameCount;
            Cursor.SetCursor(_cursorAnimation.frames[_currentFrame], _cursorAnimation.offset, CursorMode.Auto);
        }

        if (Input.GetKeyDown(KeyCode.T))
            SetActiveCursorAnimation(_cursorAnimationsDict[CursorType.Environment]);
    }

    private void SetActiveCursorAnimation(CursorAnimation cursorAnimation)
    {
        _cursorAnimation = cursorAnimation;
        _currentFrame = 0;
        _frameCount = cursorAnimation.frames.Length;
        _frameTimer = cursorAnimation.frameRate;     
    }

    private void HandleObjectHovered(ObjectType type)
    {
        switch (type)
        {
            case ObjectType.Environment:
                SetActiveCursorAnimation(_cursorAnimationsDict[CursorType.Environment]);
                break;
            case ObjectType.Item:
                SetActiveCursorAnimation(_cursorAnimationsDict[CursorType.Item]);
                break;
            case ObjectType.NEI:
                SetActiveCursorAnimation(_cursorAnimationsDict[CursorType.NonEssentialItem]);
                break;
            default:
                SetActiveCursorAnimation(_cursorAnimationsDict[CursorType.Default]);
                break;
        }
    }

    [System.Serializable]
    public class CursorAnimation
    {
        public CursorType cursorType;
        public Texture2D[] frames;
        public float frameRate;
        public Vector2 offset;
    }
}
