
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Kinix
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DroneRacingCheckpoint : UdonSharpBehaviour
    {
        [Header("Indicates in which order the player must travel through these.")]
        public int checkpointID;
        [Header("The particles that will play when passing by this checkpoint ring.")]
        public ParticleSystem checkpointVisual;
        [Header("This object will enable when it's the next checkpoint to move to.")]
        public GameObject nextpointVisual;
        [HideInInspector]
        public DroneRacingTrack listener;

        public override void OnDroneTriggerEnter(VRCDroneApi drone)
        {
            if (!drone.GetPlayer().isLocal) return;

            listener.CheckpointPassed(checkpointID, this);
        }
    }

}