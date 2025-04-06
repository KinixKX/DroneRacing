
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Kinix
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DroneBooster : UdonSharpBehaviour
    {
        [Header("The speed direction is determined by the orientation of the ring")]
        public float boostSpeed;
        [Header("The particles that will play when passing by this booster ring.")]
        public ParticleSystem boostVisual;
        [HideInInspector]
        public DroneRacingTrack listener;

        public override void OnDroneTriggerEnter(VRCDroneApi drone)
        {
            if (!drone.GetPlayer().isLocal) return;

            Vector3 finalVel = drone.GetVelocity();
            Vector3 padForce = boostSpeed * transform.up;

            finalVel.y += padForce.y;
            finalVel.x += padForce.x;
            finalVel.z += padForce.z;

            drone.SetVelocity(finalVel);

            boostVisual.Play();
            listener.PlayBoost(gameObject.transform.position);
        }
    }
}
