
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Kinix
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DronePortal : UdonSharpBehaviour
    {
        [Header("The exit point of this portal. These are 1 way only")]
        public GameObject portalTarget;
        [Header("The particles that will play when passing by this portal ring.")]
        public ParticleSystem portalVisual;
        [HideInInspector]
        public DroneRacingTrack listener;

        public override void OnDroneTriggerEnter(VRCDroneApi drone)
        {
            if (!drone.GetPlayer().isLocal) return;


            Vector3 exitVel = drone.GetVelocity().magnitude * portalTarget.transform.forward;
            Quaternion exitRot = Quaternion.LookRotation(portalTarget.transform.forward,Vector3.up);
              
            drone.TeleportTo(portalTarget.transform.position, exitRot);
            drone.SetVelocity(exitVel);

            portalVisual.Play();
            listener.PlayPortal(gameObject.transform.position);
        }
    }
}