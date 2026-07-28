using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BallKnowledge.Greybox.Editor
{
    public static class GreyboxSceneBuilder
    {
        // These hard-coded values are scene scaffolding for the scale test only, per the task brief.
        private const float GroundSizeMeters = 700f;
        private const float MarkerSpacingMeters = 100f;
        private const float EyeHeightMeters = 1.7f;
        private const float PlayerInsetMeters = 5f;
        private const float GroundHalfExtent = GroundSizeMeters * 0.5f;

        private static readonly Color GroundColor = new Color(0.45f, 0.45f, 0.45f, 1f);
        private static readonly Color EdgeMarkerColor = new Color(0.95f, 0.8f, 0.2f, 1f);
        private static readonly Color FarCornerColor = new Color(0.9f, 0.25f, 0.2f, 1f);

        [MenuItem("Ball Knowledge/Build Scale Test Scene")]
        public static void BuildScaleTestScene()
        {
            var builtNames = new List<string>();
            var disabledCameras = new List<string>();

            var existingRoot = GameObject.Find("BallKnowledgeScaleTestScene");
            if (existingRoot != null)
            {
                Object.DestroyImmediate(existingRoot);
            }

            var root = new GameObject("BallKnowledgeScaleTestScene");

            CreateGround(root.transform);
            builtNames.Add("700m x 700m ground");

            var edgeMarkerCount = CreateEdgeMarkers(root.transform);
            builtNames.Add(edgeMarkerCount.ToString(CultureInfo.InvariantCulture) + " edge markers");

            CreateFarCornerMarker(root.transform);
            builtNames.Add("far-corner marker");

            var player = CreatePlayerRig(root.transform);
            builtNames.Add("player rig");

            DisableOtherCameras(player, disabledCameras);

            if (!HasDirectionalLight())
            {
                CreateDirectionalLight();
                builtNames.Add("directional light");
            }
            else
            {
                builtNames.Add("existing directional light kept");
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Selection.activeGameObject = player;

            Debug.Log(
                "[BallKnowledge] Built scale test scene: " +
                string.Join(", ", builtNames) +
                ". Disabled cameras: " +
                (disabledCameras.Count > 0 ? string.Join(", ", disabledCameras) : "none") +
                ".");
        }

        private static void CreateGround(Transform parent)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "ScaleTestGround";
            ground.transform.SetParent(parent, false);
            ground.transform.localScale = new Vector3(GroundSizeMeters / 10f, 1f, GroundSizeMeters / 10f);
            ApplyMaterialColor(ground.GetComponent<Renderer>(), "ScaleTestGroundMat", GroundColor);
        }

        private static int CreateEdgeMarkers(Transform parent)
        {
            var count = 0;
            var startCorner = GetStartCorner();

            for (var distance = MarkerSpacingMeters; distance <= GroundSizeMeters; distance += MarkerSpacingMeters)
            {
                var southEdgePosition = startCorner + new Vector3(distance, 0f, 0f);
                CreateMarker(
                    parent,
                    "ScaleMarker_South_" + distance.ToString("0", CultureInfo.InvariantCulture),
                    southEdgePosition,
                    distance.ToString("0", CultureInfo.InvariantCulture) + "m",
                    EdgeMarkerColor,
                    Quaternion.Euler(0f, 0f, 0f));
                count++;

                var westEdgePosition = startCorner + new Vector3(0f, 0f, distance);
                CreateMarker(
                    parent,
                    "ScaleMarker_West_" + distance.ToString("0", CultureInfo.InvariantCulture),
                    westEdgePosition,
                    distance.ToString("0", CultureInfo.InvariantCulture) + "m",
                    EdgeMarkerColor,
                    Quaternion.Euler(0f, 90f, 0f));
                count++;
            }

            return count;
        }

        private static void CreateFarCornerMarker(Transform parent)
        {
            CreateMarker(
                parent,
                "ScaleMarker_FarCorner",
                new Vector3(GroundHalfExtent, 0f, GroundHalfExtent),
                "Diagonal End",
                FarCornerColor,
                Quaternion.Euler(0f, 225f, 0f),
                new Vector3(4f, 20f, 4f));
        }

        private static GameObject CreatePlayerRig(Transform parent)
        {
            var player = new GameObject("Player");
            player.transform.SetParent(parent, false);

            var controller = player.AddComponent<CharacterController>();
            controller.center = new Vector3(0f, controller.height * 0.5f, 0f);
            player.transform.position = GetStartCorner() + new Vector3(PlayerInsetMeters, controller.height * 0.5f, PlayerInsetMeters);

            player.AddComponent<FirstPersonController>();
            player.AddComponent<ScaleReadoutHud>();

            var cameraObject = new GameObject("PlayerCamera");
            cameraObject.transform.SetParent(player.transform, false);
            cameraObject.transform.localPosition = new Vector3(0f, EyeHeightMeters, 0f);
            cameraObject.transform.localRotation = Quaternion.identity;
            cameraObject.tag = "MainCamera";

            cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();

            return player;
        }

        private static void DisableOtherCameras(GameObject player, List<string> disabledCameras)
        {
            var playerCamera = player.GetComponentInChildren<Camera>();
            var cameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
            for (var i = 0; i < cameras.Length; i++)
            {
                var camera = cameras[i];
                if (camera == null || camera == playerCamera)
                {
                    continue;
                }

                if (!camera.enabled)
                {
                    continue;
                }

                camera.enabled = false;

                var listener = camera.GetComponent<AudioListener>();
                if (listener != null)
                {
                    listener.enabled = false;
                }

                disabledCameras.Add(camera.gameObject.name);
            }
        }

        private static bool HasDirectionalLight()
        {
            var lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            for (var i = 0; i < lights.Length; i++)
            {
                if (lights[i] != null && lights[i].type == LightType.Directional)
                {
                    return true;
                }
            }

            return false;
        }

        private static void CreateDirectionalLight()
        {
            var lightObject = new GameObject("Directional Light");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private static void CreateMarker(
            Transform parent,
            string name,
            Vector3 position,
            string label,
            Color color,
            Quaternion labelRotation)
        {
            CreateMarker(parent, name, position, label, color, labelRotation, new Vector3(2f, 12f, 2f));
        }

        private static void CreateMarker(
            Transform parent,
            string name,
            Vector3 position,
            string label,
            Color color,
            Quaternion labelRotation,
            Vector3 postScale)
        {
            var marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = name;
            marker.transform.SetParent(parent, false);
            marker.transform.position = position + new Vector3(0f, postScale.y * 0.5f, 0f);
            marker.transform.localScale = postScale;
            ApplyMaterialColor(marker.GetComponent<Renderer>(), name + "_Mat", color);

            var labelObject = new GameObject("Label");
            labelObject.transform.SetParent(marker.transform, false);
            labelObject.transform.localPosition = new Vector3(0f, (postScale.y * 0.5f) + 2f, 0f);
            labelObject.transform.localRotation = labelRotation;

            var textMesh = labelObject.AddComponent<TextMesh>();
            textMesh.text = label;
            textMesh.fontSize = 48;
            textMesh.characterSize = 0.35f;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.color = Color.white;
        }

        private static void ApplyMaterialColor(Renderer renderer, string assetName, Color color)
        {
            if (renderer == null)
            {
                return;
            }

            var template = renderer.sharedMaterial;
            var material = template != null ? new Material(template) : new Material(Shader.Find("Standard"));
            material.name = assetName;
            material.color = color;
            renderer.sharedMaterial = material;
        }

        private static Vector3 GetStartCorner()
        {
            return new Vector3(-GroundHalfExtent, 0f, -GroundHalfExtent);
        }
    }
}
