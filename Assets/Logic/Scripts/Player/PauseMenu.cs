using System.Collections;
using BoschingMachine.Logic.Scripts.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace BoschingMachine.Logic.Scripts.Player
{
    public class PauseMenu : MonoBehaviour
    {
        private const float EaseTime = 0.2f;

        private CanvasGroup group;
        private float percent;

        private bool isPaused;
        private bool easing;

        private CursorLock.CursorReservation cursorReservation;

        private InputAction pauseAction;

        private void Awake()
        {
            group = GetComponentInChildren<CanvasGroup>();

            pauseAction = new InputAction(binding: "<Keyboard>/escape");

            var buttons = GetComponentsInChildren<Button>();
            SetupButton(buttons, 0, "Resume", () => SetPause(false));
            SetupButton(buttons, 1, "Reload Scene", ReloadScene);
            SetupButton(buttons, 2, "Quit", Quit);
            
            UpdateVisuals();
        }

        private void OnEnable()
        {
            cursorReservation = new CursorLock.CursorReservation(CursorLockMode.None);

            pauseAction.performed += TogglePause;
            pauseAction.Enable();
        }

        private void OnDisable()
        {
            SetPause(false);
            
            pauseAction.performed -= TogglePause;
            pauseAction.Disable();
        }

        private void TogglePause(InputAction.CallbackContext ctx) => TogglePause();
        public void TogglePause() => SetPause(!isPaused);

        public void SetPause(bool isPaused)
        {
            if (isPaused)
            {
                this.isPaused = true;
                Time.timeScale = 0.0f;
                cursorReservation.Push();
            }
            else
            {
                this.isPaused = false;
                Time.timeScale = 1.0f;
                cursorReservation.Pop();
            }

            group.interactable = isPaused;
            group.blocksRaycasts = isPaused;

            EaseMenu();
        }

        private void EaseMenu()
        {
            if (easing) return;
            if (!isActiveAndEnabled) return;
            StartCoroutine(EaseRoutine());
        }

        private IEnumerator EaseRoutine()
        {
            easing = true;

            float t;
            do
            {
                t = isPaused ? 1.0f : 0.0f;
                percent += (t - percent) * Time.unscaledDeltaTime / EaseTime;
                percent = Mathf.Clamp01(percent);

                UpdateVisuals();

                yield return null;
            } while (Mathf.Abs(t - percent) > 0.01f);

            percent = t;
            UpdateVisuals();
            easing = false;
        }

        private void UpdateVisuals()
        {
            group.alpha = Curves.Smootherstep(percent);
        }

        public void ReloadScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void SetupButton(Button[] buttons, int i, string name, UnityAction callback)
        {
            if (i < 0) return;
            if (i >= buttons.Length) return;

            var button = buttons[i];
            button.name = name;
            button.onClick.AddListener(callback);
            
            var text = button.GetComponentInChildren<TMP_Text>();
            if (text) text.text = name;
        }
    }
}