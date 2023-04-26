using System;
using System.Collections.Generic;
using BMRP.Runtime.Core;
using BoschingMachine.Logic.Scripts.Player;
using TMPro;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class GameConsole : MonoBehaviour
{
    [SerializeField] private InputActionAsset playerAsset;

    private string output;

    private InputAction openAction = new InputAction(binding: "<Keyboard>/backquote");
    private InputAction escapeAction = new InputAction(binding: "<Keyboard>/escape");
    private bool active;
    private Canvas canvas;
    private TMP_Text display;
    private TMP_InputField input;
    private CursorLock.CursorReservation cursorReservation;

    private void Awake()
    {
        canvas = GetComponentInChildren<Canvas>();
        display = canvas.transform.DeepFind("Text").GetComponent<TMP_Text>();
        input = canvas.transform.DeepFind("Input").GetComponent<TMP_InputField>();
    }

    private void OnEnable()
    {
        openAction.Enable();
        escapeAction.Enable();

        openAction.performed += Toggle;
        escapeAction.performed += Close;

        input.onValueChanged.AddListener(ValueChanged);

        cursorReservation = new CursorLock.CursorReservation(CursorLockMode.None);
    }

    private void OnDisable()
    {
        escapeAction.performed -= Close;
        openAction.performed -= Toggle;
        openAction.Disable();
        escapeAction.Disable();

        input.onValueChanged.RemoveListener(ValueChanged);
    }

    private void Update()
    {
        if (!active) return;
        input.ActivateInputField();
    }

    private void Close(InputAction.CallbackContext obj) => SetActive(false);
    private void Toggle(InputAction.CallbackContext ctx) => SetActive(!active);

    private void Start()
    {
        SetActive(false);
    }

    private void BuildDisplay()
    {
        display.text = output;
    }

    public void SetActive(bool s)
    {
        active = s;
        canvas.gameObject.SetActive(s);
        if (s)
        {
            playerAsset.Disable();
            EventSystem.current.SetSelectedGameObject(input.gameObject);
            BuildDisplay();
            cursorReservation.Push();
        }
        else
        {
            playerAsset.Enable();
            cursorReservation.Pop();
        }
    }

    public void ValueChanged(string command)
    {
        if (string.IsNullOrEmpty(command)) return;
        if (command[^1] != '\n') return;
        command = command.Substring(0, command.Length - 1);
        if (ParseCommand(command))
        {
            SetActive(false);
        }
        input.text = "";
        BuildDisplay();
    }

    public static List<string> GetArgs(string command)
    {
        var args = new List<string>();
        args.Add("");

        for (var i = 0; i < command.Length; i++)
        {
            switch (command[i])
            {
                case ' ':
                    args.Add("");
                    continue;
                case '"':
                    i++;
                    while (command[i] != '"')
                    {
                        args[^1] += command[i++];
                    }
                    break;
            }

            args[^1] += command[i];
        }
        
        return args;
    }
    
    public bool ParseCommand(string command)
    {
        var args = GetArgs(command);
        if (args.Count == 0) return false;
        
        switch (args[0])
        {
            case "fov":
                PlayerCameraAnimator.SetFov(float.Parse(args[1]));
                output += $"Set FOV to {args[1]}\n";
                return true;
        }
        
        output += $"\"{command}\" is not a valid command.\n";
        return false;
    }
}