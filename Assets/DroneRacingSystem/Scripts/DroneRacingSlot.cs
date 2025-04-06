
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Kinix
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    [DefaultExecutionOrder(1)]
    public class DroneRacingSlot : UdonSharpBehaviour
    {
        [HideInInspector]
        [UdonSynced]
        public float[] trackTimes;   
        public DroneRacingManager manager;
        [Header("Particle system to play when getting a new Personal Best")]
        public ParticleSystem newPBEffect;
        private bool addedToList;

        public void Start()
        {
            trackTimes = new float[manager.racingTracks.Length];
        }

        public override void OnDeserialization()
        {
            if (!addedToList)
            {
                manager.AddActivePlayer(this);
                addedToList = true;
            }

            manager.UpdateHighscoreList();
        }

        public override void OnPlayerRestored(VRCPlayerApi player)
        {
            if (Networking.IsOwner(gameObject) && player.isLocal)
            {
                manager.SetPlayerSlot(this);
                Debug.Log("Im the owner of this slot");
            }

            if (!addedToList)
            {
                manager.AddActivePlayer(this);
                addedToList = true;
            }
        }

        public void SaveTime(float time)
        {
            if (manager.selectedTrack == -1) return;

            if (trackTimes[manager.selectedTrack] == 0 || time < trackTimes[manager.selectedTrack])
            {
                Debug.Log($"New highscore!");
                if (trackTimes[manager.selectedTrack] != 0) SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All,nameof(DisplayNewPB));

                trackTimes[manager.selectedTrack] = time;
                RequestSerialization();
            }

            manager.UpdateHighscoreList();
        }

        public void DisplayNewPB()
        {
            newPBEffect.transform.position = Networking.GetOwner(gameObject).GetPosition();
            newPBEffect.Play();

            manager.PlayNewPBSound(newPBEffect.transform.position);
        }
    }
}