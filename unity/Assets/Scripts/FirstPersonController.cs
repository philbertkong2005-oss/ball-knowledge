using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BallKnowledge.Greybox
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class FirstPersonController : MonoBehaviour
    {
        private const float MaxPitchDegrees = 89f;

        private static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;

        private CharacterController characterController;
        private Transform cameraTransform;
        private GreyboxSettings loadedSettings;
        private Vector3 standingCameraLocalPosition;
        private Vector3 standingControllerCenter;
        private Vector3 lastTrackedPosition;
        private float standingControllerHeight;
        private float crouchedControllerHeight;
        private float pitchDegrees;
        private float verticalVelocity;
        private bool hasTrackedPosition;

        public bool HasLoadedSettings
        {
            get { return loadedSettings != null; }
        }

        public GreyboxSettings LoadedSettings
        {
            get { return loadedSettings; }
        }

        public string LastErrorMessage { get; private set; } = string.Empty;

        public float CurrentHorizontalSpeedMetersPerSecond { get; private set; }

        public float TotalHorizontalDistanceMeters { get; private set; }

        public float TripHorizontalDistanceMeters { get; private set; }

        public float ElapsedSeconds
        {
            get { return Time.timeSinceLevelLoad; }
        }

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            cameraTransform = FindCameraTransform();

            if (characterController != null)
            {
                standingControllerHeight = characterController.height;
                standingControllerCenter = characterController.center;
            }

            if (cameraTransform != null)
            {
                standingCameraLocalPosition = cameraTransform.localPosition;
            }
        }

        private void Start()
        {
            if (!TryValidateSceneReferences())
            {
                DisableWithError(LastErrorMessage);
                return;
            }

            try
            {
                var loadedConfig = GreyboxConfigLoader.LoadFrom(Application.streamingAssetsPath);
                loadedSettings = loadedConfig.Greybox;
            }
            catch (Exception ex)
            {
                DisableWithError("Failed to load greybox config: " + ex.Message);
                return;
            }

            if (loadedSettings == null || loadedSettings.Player == null)
            {
                DisableWithError("Failed to load greybox config: player settings were missing.");
                return;
            }

            crouchedControllerHeight = Mathf.Max(characterController.radius * 2f, standingCameraLocalPosition.y);
            LockCursor();
        }

        private void Update()
        {
            if (!HasLoadedSettings)
            {
                return;
            }

            HandleCursorState();
            HandleLook();
            HandleMovement();
            TrackHorizontalMovement();
        }

        private void OnDisable()
        {
            UnlockCursor();
        }

        public void ResetTripDistance()
        {
            TripHorizontalDistanceMeters = 0f;
        }

        private bool TryValidateSceneReferences()
        {
            if (characterController == null)
            {
                LastErrorMessage = "FirstPersonController requires a CharacterController.";
                return false;
            }

            if (cameraTransform == null)
            {
                LastErrorMessage = "FirstPersonController requires a child camera transform.";
                return false;
            }

            return true;
        }

        private Transform FindCameraTransform()
        {
            var childCamera = GetComponentInChildren<Camera>();
            return childCamera == null ? null : childCamera.transform;
        }

        private void HandleCursorState()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                UnlockCursor();
                return;
            }

            var mouse = Mouse.current;
            if (mouse != null &&
                Cursor.lockState != CursorLockMode.Locked &&
                mouse.leftButton.wasPressedThisFrame)
            {
                LockCursor();
            }
        }

        private void HandleLook()
        {
            var mouse = Mouse.current;
            if (mouse == null || Cursor.lockState != CursorLockMode.Locked)
            {
                return;
            }

            var sensitivity = (float)loadedSettings.Player.MouseSensitivity;
            var delta = mouse.delta.ReadValue() * sensitivity;

            transform.Rotate(Vector3.up, delta.x, Space.Self);

            pitchDegrees = Mathf.Clamp(pitchDegrees - delta.y, -MaxPitchDegrees, MaxPitchDegrees);
            cameraTransform.localRotation = Quaternion.Euler(pitchDegrees, 0f, 0f);
        }

        private void HandleMovement()
        {
            var keyboard = Keyboard.current;
            var input = Vector2.zero;

            if (keyboard != null)
            {
                if (keyboard.aKey.isPressed)
                {
                    input.x -= 1f;
                }

                if (keyboard.dKey.isPressed)
                {
                    input.x += 1f;
                }

                if (keyboard.sKey.isPressed)
                {
                    input.y -= 1f;
                }

                if (keyboard.wKey.isPressed)
                {
                    input.y += 1f;
                }
            }

            if (input.sqrMagnitude > 1f)
            {
                input.Normalize();
            }

            UpdateCrouchPose(keyboard != null && keyboard.leftCtrlKey.isPressed);

            var moveSpeed = GetConfiguredMoveSpeed(keyboard);
            var horizontalVelocity =
                ((transform.right * input.x) + (transform.forward * input.y)) * moveSpeed;

            if (characterController.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = 0f;
            }

            verticalVelocity += Physics.gravity.y * Time.deltaTime;

            var frameVelocity = horizontalVelocity + (Vector3.up * verticalVelocity);
            characterController.Move(frameVelocity * Time.deltaTime);
        }

        private void UpdateCrouchPose(bool isCrouching)
        {
            characterController.height = isCrouching ? crouchedControllerHeight : standingControllerHeight;
            characterController.center = new Vector3(
                standingControllerCenter.x,
                characterController.height * 0.5f,
                standingControllerCenter.z);

            var cameraLocalPosition = standingCameraLocalPosition;
            if (isCrouching)
            {
                cameraLocalPosition.y = Mathf.Min(
                    standingCameraLocalPosition.y,
                    characterController.height - characterController.radius);
            }

            cameraTransform.localPosition = cameraLocalPosition;
        }

        private float GetConfiguredMoveSpeed(Keyboard keyboard)
        {
            var playerSettings = loadedSettings.Player;
            if (keyboard != null && keyboard.leftCtrlKey.isPressed)
            {
                return (float)playerSettings.CrouchSpeedMs;
            }

            if (keyboard != null && keyboard.leftShiftKey.isPressed)
            {
                return (float)playerSettings.RunSpeedMs;
            }

            return (float)playerSettings.WalkSpeedMs;
        }

        private void TrackHorizontalMovement()
        {
            var currentPosition = transform.position;
            if (!hasTrackedPosition)
            {
                lastTrackedPosition = currentPosition;
                hasTrackedPosition = true;
                CurrentHorizontalSpeedMetersPerSecond = 0f;
                return;
            }

            var horizontalDelta = currentPosition - lastTrackedPosition;
            horizontalDelta.y = 0f;

            var distance = horizontalDelta.magnitude;
            if (Time.deltaTime > 0f)
            {
                CurrentHorizontalSpeedMetersPerSecond = distance / Time.deltaTime;
            }
            else
            {
                CurrentHorizontalSpeedMetersPerSecond = 0f;
            }

            TotalHorizontalDistanceMeters += distance;
            TripHorizontalDistanceMeters += distance;
            lastTrackedPosition = currentPosition;
        }

        private void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void DisableWithError(string message)
        {
            LastErrorMessage = message;
            Debug.LogError(string.Format(
                InvariantCulture,
                "[BallKnowledge] FirstPersonController disabled: {0}",
                message));
            enabled = false;
            UnlockCursor();
        }
    }
}
