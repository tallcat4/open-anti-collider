/*
 * This script is inspired by:
 * Repository: AvatarColliderDetector
 * URL: https://github.com/5Solkun/AvatarColliderDetector
 * Author: 5Sori
 * License: MIT
 */

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class OpenAntiCollider : UdonSharpBehaviour
{
    [Header("General Settings")]
    [SerializeField] private float penaltyThreshold = 100.0f;
    [SerializeField] private float scoreDecayRate = 20.0f;

    [Header("Speed Hack Detection")]
    [SerializeField] private bool enableSpeedHackDetection = true;
    [SerializeField] private float speedHackWeight = 150.0f;
    [SerializeField] private float maxWalkSpeed = 7.0f;

    [Header("Fly Hack Detection")]
    [SerializeField] private bool enableFlyHackDetection = true;
    [SerializeField] private float flyHackWeight = 50.0f;
    [SerializeField] private float fallVelocityThreshold = -0.5f;

    [Header("Fake Ground Detection")]
    [SerializeField] private bool enableFakeGroundDetection = true;
    [SerializeField] private float fakeGroundWeight = 150.0f;
    [SerializeField] private float groundCheckInterval = 0.33f;
    [SerializeField] private float sphereCastRadius = 0.25f;
    [SerializeField] private float sphereCastOriginHeight = 0.3f;
    [SerializeField] private float sphereCastMaxDistance = 0.4f;
    [SerializeField] private LayerMask groundLayers = 1 | (1 << 11);

    [Header("Collider Spam Detection")]
    [SerializeField] private bool enableColliderSpamDetection = false;
    [SerializeField] private float colliderSpamWeight = 100.0f;
    [SerializeField] private float scanRadius = 3.0f;
    [SerializeField] private LayerMask colliderDetectionLayers = (1 << 10);
    [SerializeField] private float colliderCheckInterval = 0.2f;

    [Header("Penalty Actions")]
    [SerializeField] private Transform teleportTarget;
    [SerializeField] private GameObject[] objectsToActivate;
    [SerializeField] private bool hideObjectsOnSafe = true;
    [SerializeField] private UdonSharpBehaviour customScript;
    [SerializeField] private string customEventName;

    [Header("Whitelist")]
    [SerializeField] private string[] whiteList = { "5Sori" };

    private VRCPlayerApi localPlayer;

    private float currentScore = 0f;
    private float colliderCheckTimer = 0f;
    private float groundCheckTimer = 0f;
    private bool isTooManyColliders = false;
    private bool isFakeGround = false;

    void Start()
    {
        localPlayer = Networking.LocalPlayer;
        
        if (hideObjectsOnSafe)
        {
            SetObjectsActive(false);
        }
    }

    void Update()
    {
        if (localPlayer == null || IsWhitelisted()) return;

        float deltaTime = Time.deltaTime;
        ApplyScoreDecay(deltaTime);

        if (HandlePenaltyState()) return;

        float scoreAddition = CalculateAnomalyScore(deltaTime);
        currentScore += scoreAddition;

        Debug.Log($"[OpenAntiCollider] Score: {currentScore:F2} / {penaltyThreshold:F2} ({(currentScore / penaltyThreshold * 100):F1}%)");
    }

    private void ApplyScoreDecay(float deltaTime)
    {
        if (currentScore > 0)
        {
            currentScore = Mathf.Max(0, currentScore - scoreDecayRate * deltaTime);
        }
    }

    private bool HandlePenaltyState()
    {
        if (currentScore >= penaltyThreshold)
        {
            ExecutePenalty();
            //Reset score after penalty execution
            currentScore = 0f;
            return true;
        }
        
        return false;
    }

    private float CalculateAnomalyScore(float deltaTime)
    {
        float score = 0f;
        Vector3 velocity = localPlayer.GetVelocity();
        bool isGrounded = localPlayer.IsPlayerGrounded();

        if (enableSpeedHackDetection)
            score += CheckSpeedAnomaly(velocity, deltaTime);
        if (enableFlyHackDetection)
            score += CheckFlyAnomaly(velocity, isGrounded, deltaTime);
        if (enableFakeGroundDetection)
            score += CheckGroundAnomaly(isGrounded, deltaTime);
        if (enableColliderSpamDetection)
            score += CheckColliderAnomaly(deltaTime);

        return score;
    }

    private float CheckSpeedAnomaly(Vector3 velocity, float deltaTime)
    {
        Vector3 horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);
        if (horizontalVelocity.magnitude > maxWalkSpeed)
        {
            Debug.Log($"[OpenAntiCollider] Speed Detected (Speed: {horizontalVelocity.magnitude:F2})");
            return speedHackWeight * deltaTime;
        }
        return 0f;
    }

    private float CheckFlyAnomaly(Vector3 velocity, bool isGrounded, float deltaTime)
    {
        if (!isGrounded && velocity.y > fallVelocityThreshold)
        {
            Debug.Log($"[OpenAntiCollider] Fly Detected! (Velocity.Y: {velocity.y:F2})");
            return flyHackWeight * deltaTime;
        }
        return 0f;
    }

    private float CheckGroundAnomaly(bool isGrounded, float deltaTime)
    {
        groundCheckTimer += deltaTime;
        
        if (groundCheckTimer > groundCheckInterval)
        {
            groundCheckTimer = 0f;
            isFakeGround = isGrounded && CheckFakeGround();
            if (isFakeGround)
            {
                Debug.Log("[OpenAntiCollider] Fake Ground Detected!");
            }
        }

        return isFakeGround ? fakeGroundWeight * deltaTime : 0f;
    }

    private float CheckColliderAnomaly(float deltaTime)
    {
        colliderCheckTimer += deltaTime;
        
        if (colliderCheckTimer > colliderCheckInterval)
        {
            colliderCheckTimer = 0f;
            isTooManyColliders = CheckExcessiveColliders();
        }

        return isTooManyColliders ? colliderSpamWeight * deltaTime : 0f;
    }

    private bool CheckFakeGround()
    {
        Vector3 origin = localPlayer.GetPosition() + (Vector3.up * sphereCastOriginHeight);
        RaycastHit hit;
        
        bool hitWorld = Physics.SphereCast(
            origin, 
            sphereCastRadius, 
            Vector3.down, 
            out hit, 
            sphereCastMaxDistance, 
            groundLayers
        );

        return !hitWorld;
    }

    private bool CheckExcessiveColliders()
    {
        Vector3 scanPosition = localPlayer.GetPosition();
        Collider[] hitColliders = Physics.OverlapSphere(scanPosition, scanRadius, colliderDetectionLayers);
        
        return hitColliders.Length > 1;
    }

    private bool IsWhitelisted()
    {
        string playerName = localPlayer.displayName;
        
        foreach (string name in whiteList)
        {
            if (playerName == name) return true;
        }
        
        return false;
    }

    public void ExecutePenalty()
    {
        if (teleportTarget != null)
        {
            localPlayer.SetVelocity(Vector3.zero);
            localPlayer.TeleportTo(teleportTarget.position, teleportTarget.rotation, VRC_SceneDescriptor.SpawnOrientation.AlignPlayerWithSpawnPoint);
        }

        SetObjectsActive(true);

        if (customScript != null && !string.IsNullOrEmpty(customEventName))
        {
            customScript.SendCustomEvent(customEventName);
        }
    }

    private void SetObjectsActive(bool isActive)
    {
        if (objectsToActivate == null) return;
        
        foreach (GameObject obj in objectsToActivate)
        {
            if (obj != null)
            {
                obj.SetActive(isActive);
            }
        }
    }
}
