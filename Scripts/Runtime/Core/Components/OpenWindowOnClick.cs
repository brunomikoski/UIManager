using System;
using UnityEngine;
using UnityEngine.UI;

namespace BrunoMikoski.UIManager
{
    public class OpenWindowOnClick : MonoBehaviour
    {
        [SerializeField]
        private Button button;

        [SerializeField] 
        private WindowID targetWindow;

        private void Reset()
        {
            button = GetComponent<Button>();
            
        }

        private void Awake()
        {
            button.onClick.AddListener(OnClick);
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(OnClick);
        }

        private void OnClick()
        {
            targetWindow.Open();
        }
    }
}