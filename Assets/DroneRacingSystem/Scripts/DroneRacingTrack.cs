
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Kinix
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(-1)]
    public class DroneRacingTrack : UdonSharpBehaviour
    {
        [HideInInspector]
        public DroneRacingManager manager;

        [HideInInspector]
        public int finalCheckpointID;

        private DroneRacingCheckpoint[] checkpoints;
        private DroneBooster[] boosters;
        private DronePortal[] portals;

        [Header("The name your track will have when selected")]
        public string trackName;

        public void Start()
        {
            checkpoints = GetComponentsInChildren<DroneRacingCheckpoint>();
            foreach(DroneRacingCheckpoint c in checkpoints)
            {
                c.listener = this;
                if (c.checkpointID > finalCheckpointID) finalCheckpointID = c.checkpointID;
            }

            boosters = GetComponentsInChildren<DroneBooster>();
            foreach (DroneBooster b in boosters)
            {
                b.listener = this;
            }

            portals = GetComponentsInChildren<DronePortal>();
            foreach (DronePortal p in portals)
            {
                p.listener = this;
            }

            PrepareCheckpoints();
        }
        
        public void PrepareCheckpoints()
        {
            if (checkpoints == null) return;

            foreach (DroneRacingCheckpoint c in checkpoints)
            {
                c.nextpointVisual.SetActive(false);
            }

            if (checkpoints.Length == 0) return;

            checkpoints[0].nextpointVisual.SetActive(true);
        }

        public void CheckpointPassed(int id, DroneRacingCheckpoint check)
        {
            manager.CheckPointPass(id, check);
        }

        public void PlayBoost(Vector3 pos)
        {
            manager.PlayBoostSound(pos);
        }

        public void PlayPortal(Vector3 pos)
        {
            manager.PlayPortalSound(pos);
        }

        public void PlayCheckpoint(Vector3 pos)
        {       
            manager.PlayCheckpointSound(pos);
        }

        public void ShowNextPoint(int nextID)
        {
            foreach (DroneRacingCheckpoint c in checkpoints)
            {
                if (c.checkpointID == nextID)
                {
                    c.nextpointVisual.SetActive(true);
                    break;
                }
            }
        }
    }
}