using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PPConsole
{
    /// <summary>
    /// UI-based debug console using TextMeshPro.
    /// Press ~ (tilde) or F1 to toggle the console.
    /// </summary>
    public class ConsoleUI : MonoBehaviour
    {
        [Header("UI References - Assign in Inspector")]
        [Tooltip("The root panel GameObject that contains the entire console UI")]
        [SerializeField] private GameObject consolePanel;
        
        [Tooltip("The TMP InputField where users type commands")]
        [SerializeField] private TMP_InputField inputField;
        
        [Tooltip("The TextMeshProUGUI text component that displays console output")]
        [SerializeField] private TextMeshProUGUI outputText;
        
        [Tooltip("The ScrollRect component for scrolling the output")]
        [SerializeField] private ScrollRect scrollRect;

        [Header("Settings")]
        [SerializeField] private int maxLogEntries = 500;
        
        private List<string> _logEntries = new List<string>();
        private int _historyIndex = -1;
        private bool _isVisible = false;

        private void Awake()
        {
            // Subscribe to console log messages
            ConsoleManager.Instance.OnLogMessage += OnConsoleLog;

            // Set initial state
            SetConsoleVisible(false);

            // Set up input field callbacks
            if (inputField != null)
            {
                inputField.onSubmit.AddListener(OnSubmitCommand);
            }
        }

        private void OnDestroy()
        {
            // Check if instance exists without creating it
            if (ConsoleManager.Exists)
            {
                ConsoleManager.Instance.OnLogMessage -= OnConsoleLog;
            }

            if (inputField != null)
            {
                inputField.onSubmit.RemoveListener(OnSubmitCommand);
            }
        }

        private void Update()
        {
            // Toggle console visibility
            if (Input.GetKeyDown(KeyCode.BackQuote) || Input.GetKeyDown(KeyCode.F1))
            {
                ToggleConsole();
            }

            // Handle history navigation when console is visible
            if (_isVisible && inputField != null && inputField.isFocused)
            {
                HandleHistoryNavigation();
            }
        }

        private void ToggleConsole()
        {
            _isVisible = !_isVisible;
            SetConsoleVisible(_isVisible);
            _historyIndex = -1;

            if (_isVisible && inputField != null)
            {
                inputField.ActivateInputField();
                inputField.Select();
            }
        }

        private void SetConsoleVisible(bool visible)
        {
            if (consolePanel != null)
            {
                consolePanel.SetActive(visible);
            }
        }

        private void HandleHistoryNavigation()
        {
            var history = ConsoleManager.Instance.GetCommandHistory();
            
            if (history.Count == 0)
                return;

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (_historyIndex < history.Count - 1)
                {
                    _historyIndex++;
                    inputField.text = history[history.Count - 1 - _historyIndex];
                    inputField.MoveToEndOfLine(false, false);
                }
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (_historyIndex > 0)
                {
                    _historyIndex--;
                    inputField.text = history[history.Count - 1 - _historyIndex];
                    inputField.MoveToEndOfLine(false, false);
                }
                else if (_historyIndex == 0)
                {
                    _historyIndex = -1;
                    inputField.text = "";
                }
            }
        }

        private void OnSubmitCommand(string command)
        {
            if (!string.IsNullOrWhiteSpace(command))
            {
                ConsoleManager.Instance.ExecuteCommand(command);
                inputField.text = "";
                _historyIndex = -1;
            }

            // Re-activate the input field so user can type next command
            inputField.ActivateInputField();
            inputField.Select();
        }

        private void OnConsoleLog(string message, LogType type)
        {
            // Special handling for clear command
            if (type == LogType.Log && string.IsNullOrEmpty(message))
            {
                _logEntries.Clear();
                UpdateOutputText();
                return;
            }

            // Add color tags based on log type
            string coloredMessage = type switch
            {
                LogType.Error => $"<color=#FF5555>{message}</color>",
                LogType.Warning => $"<color=#FFFF55>{message}</color>",
                _ => message
            };

            _logEntries.Add(coloredMessage);
            
            if (_logEntries.Count > maxLogEntries)
            {
                _logEntries.RemoveAt(0);
            }

            UpdateOutputText();
        }

        private void UpdateOutputText()
        {
            if (outputText != null)
            {
                outputText.text = string.Join("\n", _logEntries);
                
                // Auto-scroll to bottom
                if (scrollRect != null)
                {
                    Canvas.ForceUpdateCanvases();
                    scrollRect.verticalNormalizedPosition = 0f;
                }
            }
        }
    }
}
