using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BallKnowledge.Greybox
{
    [RequireComponent(typeof(FirstPersonController))]
    public sealed class ScaleReadoutHud : MonoBehaviour
    {
        private static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;

        private FirstPersonController controller;
        private GUIStyle labelStyle;
        private GUIStyle shadowStyle;
        private Texture2D backgroundTexture;

        private void Awake()
        {
            controller = GetComponent<FirstPersonController>();
            backgroundTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            backgroundTexture.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.75f));
            backgroundTexture.Apply();
            backgroundTexture.hideFlags = HideFlags.HideAndDontSave;
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || controller == null)
            {
                return;
            }

            if (keyboard.rKey.wasPressedThisFrame)
            {
                controller.ResetTripDistance();
            }
        }

        private void OnGUI()
        {
            EnsureStyles();

            var boxRect = new Rect(12f, 12f, 470f, 162f);
            GUI.DrawTexture(boxRect, backgroundTexture, ScaleMode.StretchToFill);

            if (controller == null)
            {
                DrawLine(boxRect.x + 12f, boxRect.y + 12f, "Scale HUD unavailable: missing FirstPersonController.");
                return;
            }

            if (!controller.HasLoadedSettings)
            {
                var error = string.IsNullOrEmpty(controller.LastErrorMessage)
                    ? "Waiting for greybox config."
                    : controller.LastErrorMessage;
                DrawLine(boxRect.x + 12f, boxRect.y + 12f, "Controller unavailable: " + error);
                return;
            }

            var elapsed = TimeSpan.FromSeconds(controller.ElapsedSeconds);
            var realMinutesPerGameDay = controller.LoadedSettings.Clock == null
                ? 0d
                : controller.LoadedSettings.Clock.RealMinutesPerGameDay;
            var gameHoursElapsed = realMinutesPerGameDay > 0d
                ? (controller.ElapsedSeconds / (float)(realMinutesPerGameDay * 60d)) * 24f
                : 0f;

            var lines = new[]
            {
                string.Format(
                    InvariantCulture,
                    "Speed:        {0:0.00} m/s",
                    controller.CurrentHorizontalSpeedMetersPerSecond),
                string.Format(
                    InvariantCulture,
                    "Distance:     {0:0.0} m",
                    controller.TotalHorizontalDistanceMeters),
                string.Format(
                    InvariantCulture,
                    "This trip:    {0:0.0} m",
                    controller.TripHorizontalDistanceMeters),
                string.Format(
                    InvariantCulture,
                    "Elapsed:      {0:mm\\:ss}",
                    elapsed),
                string.Format(
                    InvariantCulture,
                    "Game time:    {0:0.0} h",
                    gameHoursElapsed),
                "Controls:     WASD move / Shift sprint / Ctrl crouch / R reset trip / Esc free cursor",
            };

            var y = boxRect.y + 12f;
            for (var i = 0; i < lines.Length; i++)
            {
                DrawLine(boxRect.x + 12f, y, lines[i]);
                y += 24f;
            }
        }

        private void OnDestroy()
        {
            if (backgroundTexture != null)
            {
                Destroy(backgroundTexture);
            }
        }

        private void EnsureStyles()
        {
            if (labelStyle != null && shadowStyle != null)
            {
                return;
            }

            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 16;
            labelStyle.normal.textColor = Color.white;
            labelStyle.alignment = TextAnchor.UpperLeft;

            shadowStyle = new GUIStyle(labelStyle);
            shadowStyle.normal.textColor = Color.black;
        }

        private void DrawLine(float x, float y, string text)
        {
            GUI.Label(new Rect(x + 1f, y + 1f, 520f, 24f), text, shadowStyle);
            GUI.Label(new Rect(x, y, 520f, 24f), text, labelStyle);
        }
    }
}
