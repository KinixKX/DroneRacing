
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

namespace Kinix
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DroneRacingScore : UdonSharpBehaviour
    {
        public TextMeshProUGUI playerName;
        public TextMeshProUGUI playerTime;
        //public TextMeshProUGUI playerPlacement;
    }
}
