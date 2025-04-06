
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

namespace Kinix
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DroneRacingSpectator : UdonSharpBehaviour
    {
        public DroneRacingManager manager;

        [Header("The text to display the currently spectated player's name")]
        public TextMeshProUGUI currentPlayerText;
        [Header("The text to display teh currently spectated player's drone speed")]
        public TextMeshProUGUI droneSpeedText;

        private VRCDroneApi trackedDrone;
        private int currentlyWatching;

        public GameObject trackingPoint;
        public GameObject spectatorScreen;
        public Camera spectatorCamera;
        private bool spectating;

        private bool fpvMode;
        private float positionOffset;
        private float rotationOffset;

        private float smoothingPosition = 0.06f;
        private float smoothingRotation = 0.25f;
        private Vector3 vel = Vector3.zero;

        private RaycastHit hit = new RaycastHit();

        private int maskedLayer;
        private int mask = 0;

        public void Start()
        {
            DisableSpectator();
        }

        public void NextPlayer()
        {
            currentlyWatching++;

            if (currentlyWatching > manager.playerSlots.Length - 1) currentlyWatching = 0;

            UpdatePlayerSlot(currentlyWatching);
        }

        public void PreviousPlayer()
        {
            currentlyWatching--;
            if (currentlyWatching < 0) currentlyWatching = manager.playerSlots.Length - 1;

            UpdatePlayerSlot(currentlyWatching);
        }

        public void UpdatePlayerSlot(int playerID)
        {
            if (manager.playerSlots[playerID] == null) return;
            if (!Utilities.IsValid(manager.playerSlots[playerID].gameObject)) return;

            VRCPlayerApi player = Networking.GetOwner(manager.playerSlots[playerID].gameObject);

            currentPlayerText.text = $"{player.displayName}";
            trackedDrone = player.GetDrone();

            spectating = true;
            spectatorScreen.SetActive(true);
        }

        public void FirstPersonView()
        {
            fpvMode = true;
            positionOffset = 0f;
            rotationOffset = 0f;
        }

        public void ThirdPersonView()
        {
            fpvMode = false;
            positionOffset = 1f;
            rotationOffset = 10f;
        }

        public void Update()
        {
            if (!spectating) return;
            if (!Utilities.IsValid(trackedDrone)) return;

            droneSpeedText.text = $"{trackedDrone.GetVelocity().magnitude.ToString("F1")}";

            trackingPoint.transform.SetLocalPositionAndRotation(trackedDrone.GetPosition(), trackedDrone.GetRotation());

            maskedLayer = 1 << mask;

            if (!fpvMode)
            {
                trackingPoint.transform.Translate(Vector3.up * positionOffset);
                trackingPoint.transform.Rotate(Vector3.right * rotationOffset);

                if (Physics.Raycast(trackingPoint.transform.position, Vector3.Normalize(trackingPoint.transform.rotation * Vector3.back), out hit, 1.25f, maskedLayer))
                {
                    trackingPoint.transform.position = hit.point;
                    trackingPoint.transform.Translate(Vector3.forward * 0.25f);
                }
                else
                {
                    trackingPoint.transform.Translate(Vector3.back * 1f);
                }
            }

            spectatorCamera.transform.position = Vector3.SmoothDamp(spectatorCamera.transform.position, trackingPoint.transform.position, ref vel, smoothingPosition);
            spectatorCamera.transform.rotation = Quaternion.Slerp(spectatorCamera.transform.rotation, trackingPoint.transform.rotation, smoothingRotation);


        }

        public void DisableSpectator()
        {
            spectating = false;
            spectatorScreen.SetActive(false);
            currentPlayerText.text = $"[ NOT SPECTATING ]";

        }
    }
}
