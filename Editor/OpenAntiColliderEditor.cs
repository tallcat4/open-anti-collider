/*
 * This script is inspired by:
 * Repository: AvatarColliderDetector
 * URL: https://github.com/5Solkun/AvatarColliderDetector
 * Author: 5Sori
 * License: MIT
 */

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(OpenAntiCollider))]
public class OpenAntiColliderEditor : Editor
{
    // General Settings
    SerializedProperty penaltyThreshold;
    SerializedProperty scoreDecayRate;

    // Speed Hack Detection
    SerializedProperty enableSpeedHackDetection;
    SerializedProperty speedHackWeight;
    SerializedProperty maxWalkSpeed;

    // Fly Hack Detection
    SerializedProperty enableFlyHackDetection;
    SerializedProperty flyHackWeight;
    SerializedProperty fallVelocityThreshold;

    // Fake Ground Detection
    SerializedProperty enableFakeGroundDetection;
    SerializedProperty fakeGroundWeight;
    SerializedProperty groundCheckInterval;
    SerializedProperty sphereCastRadius;
    SerializedProperty sphereCastOriginHeight;
    SerializedProperty sphereCastMaxDistance;
    SerializedProperty groundLayers;

    // Collider Spam Detection
    SerializedProperty enableColliderSpamDetection;
    SerializedProperty colliderSpamWeight;
    SerializedProperty scanRadius;
    SerializedProperty playerLocalLayer;
    SerializedProperty colliderCheckInterval;

    // Penalty Actions
    SerializedProperty teleportTarget;
    SerializedProperty objectsToActivate;
    SerializedProperty hideObjectsOnSafe;
    SerializedProperty customScript;
    SerializedProperty customEventName;

    // Whitelist
    SerializedProperty whiteList;

    private void OnEnable()
    {
        // General Settings
        penaltyThreshold = serializedObject.FindProperty("penaltyThreshold");
        scoreDecayRate = serializedObject.FindProperty("scoreDecayRate");

        // Speed Hack Detection
        enableSpeedHackDetection = serializedObject.FindProperty("enableSpeedHackDetection");
        speedHackWeight = serializedObject.FindProperty("speedHackWeight");
        maxWalkSpeed = serializedObject.FindProperty("maxWalkSpeed");

        // Fly Hack Detection
        enableFlyHackDetection = serializedObject.FindProperty("enableFlyHackDetection");
        flyHackWeight = serializedObject.FindProperty("flyHackWeight");
        fallVelocityThreshold = serializedObject.FindProperty("fallVelocityThreshold");

        // Fake Ground Detection
        enableFakeGroundDetection = serializedObject.FindProperty("enableFakeGroundDetection");
        fakeGroundWeight = serializedObject.FindProperty("fakeGroundWeight");
        groundCheckInterval = serializedObject.FindProperty("groundCheckInterval");
        sphereCastRadius = serializedObject.FindProperty("sphereCastRadius");
        sphereCastOriginHeight = serializedObject.FindProperty("sphereCastOriginHeight");
        sphereCastMaxDistance = serializedObject.FindProperty("sphereCastMaxDistance");
        groundLayers = serializedObject.FindProperty("groundLayers");

        // Collider Spam Detection
        enableColliderSpamDetection = serializedObject.FindProperty("enableColliderSpamDetection");
        colliderSpamWeight = serializedObject.FindProperty("colliderSpamWeight");
        scanRadius = serializedObject.FindProperty("scanRadius");
        playerLocalLayer = serializedObject.FindProperty("playerLocalLayer");
        colliderCheckInterval = serializedObject.FindProperty("colliderCheckInterval");

        // Penalty Actions
        teleportTarget = serializedObject.FindProperty("teleportTarget");
        objectsToActivate = serializedObject.FindProperty("objectsToActivate");
        hideObjectsOnSafe = serializedObject.FindProperty("hideObjectsOnSafe");
        customScript = serializedObject.FindProperty("customScript");
        customEventName = serializedObject.FindProperty("customEventName");

        // Whitelist
        whiteList = serializedObject.FindProperty("whiteList");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // General Settings
        EditorGUILayout.LabelField("General Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(penaltyThreshold);
        EditorGUILayout.PropertyField(scoreDecayRate);
        EditorGUILayout.Space(10);

        // Speed Hack Detection
        DrawDetectionSection(
            "Speed Hack Detection",
            enableSpeedHackDetection,
            () => {
                EditorGUILayout.PropertyField(speedHackWeight, new GUIContent("Weight (Score/Second)"));
                EditorGUILayout.PropertyField(maxWalkSpeed);
            }
        );

        // Fly Hack Detection
        DrawDetectionSection(
            "Fly Hack Detection",
            enableFlyHackDetection,
            () => {
                EditorGUILayout.PropertyField(flyHackWeight, new GUIContent("Weight (Score/Second)"));
                EditorGUILayout.PropertyField(fallVelocityThreshold);
            }
        );

        // Fake Ground Detection
        DrawDetectionSection(
            "Fake Ground Detection",
            enableFakeGroundDetection,
            () => {
                EditorGUILayout.PropertyField(fakeGroundWeight, new GUIContent("Weight (Score/Second)"));
                EditorGUILayout.PropertyField(groundCheckInterval);
                EditorGUILayout.PropertyField(sphereCastRadius);
                EditorGUILayout.PropertyField(sphereCastOriginHeight);
                EditorGUILayout.PropertyField(sphereCastMaxDistance);
                EditorGUILayout.PropertyField(groundLayers);
            }
        );

        // Collider Spam Detection (Layer10)
        DrawDetectionSection(
            "Collider Spam Detection (Layer10)",
            enableColliderSpamDetection,
            () => {
                EditorGUILayout.HelpBox("This detection is aggressive and may cause false positives.", MessageType.Warning);
                EditorGUILayout.PropertyField(colliderSpamWeight, new GUIContent("Weight (Score/Second)"));
                EditorGUILayout.PropertyField(scanRadius);
                EditorGUILayout.PropertyField(playerLocalLayer);
                EditorGUILayout.PropertyField(colliderCheckInterval);
            }
        );

        // Penalty Actions
        EditorGUILayout.LabelField("Penalty Actions", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(teleportTarget);
        EditorGUILayout.PropertyField(objectsToActivate);
        EditorGUILayout.PropertyField(hideObjectsOnSafe);
        EditorGUILayout.PropertyField(customScript);
        EditorGUILayout.PropertyField(customEventName);
        EditorGUILayout.Space(10);

        // Whitelist
        EditorGUILayout.LabelField("Whitelist", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(whiteList);

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawDetectionSection(string title, SerializedProperty enableProperty, System.Action drawProperties)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(enableProperty, new GUIContent(title));
        EditorGUILayout.EndHorizontal();

        EditorGUI.BeginDisabledGroup(!enableProperty.boolValue);
        EditorGUI.indentLevel++;
        drawProperties();
        EditorGUI.indentLevel--;
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
    }
}
#endif
