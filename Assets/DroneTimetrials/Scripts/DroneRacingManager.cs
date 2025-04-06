
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Data;
using TMPro;
using System;

namespace Kinix
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(-2)]
    public class DroneRacingManager : UdonSharpBehaviour
    {
        [HideInInspector]
        public DroneRacingSlot[] playerSlots;
        [HideInInspector]
        public DroneRacingSlot localSlot;

        [Header("The text component where the current time will be shown to the player")]
        public TextMeshProUGUI timeText;
        public GameObject anchorToDrone;
        [Header("The text component where the current track name will be shown to the player")]
        public TextMeshProUGUI trackNameText;
        private float racingTime;
        private bool racing;
        private int currentIndex;

        public AudioSource sfxLocalPlayer;
        public AudioSource sfxGlobalPlayer;
        [Header("Sound effect played when flying through a boost ring")]
        public AudioClip boostSFX;
        [Header("Sound effect played when flying through a checkpoint ring")]
        public AudioClip checkpointSFX;
        [Header("Sound effect played when flying through a portal ring")]
        public AudioClip portalSFX;
        [Header("Sound effect played when starting a race")]
        public AudioClip raceStartSFX;
        [Header("Sound effect played when finishing a race")]
        public AudioClip raceFinishSFX;
        [Header("Sound effect played when someone gets a new Personal Best")]
        public AudioClip newPBSFX;

        [HideInInspector]
        public int selectedTrack;
        [Header("List of each Race Track object. Put any you made in here")]
        public DroneRacingTrack[] racingTracks;

        private GameObject[] scoreboardList = new GameObject[0];
        [Header("The clonable object to display the highscores on the board")]
        public GameObject scoreboardPrefab;
        [Header("The object where the highscore objects will be parented to")]
        public GameObject scoreboardParent;

        private VRCPlayerApi localPlayer;
        private TimeSpan displayedHrs;

        public void Start()
        {
            localPlayer = Networking.LocalPlayer;

            playerSlots = new DroneRacingSlot[0];

            foreach (DroneRacingTrack t in racingTracks)
            {
                t.manager = this;
            }

            if(racingTracks.Length > 0) ChangeTrack(0);

            TimeDisplay(false);
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            SendCustomEventDelayedSeconds(nameof(RemoveInactivePlayers), 1f);
        }

        public void HideTracks()
        {
            selectedTrack = -1;

            for (int i = 0; i < racingTracks.Length; i++)
            {
                racingTracks[i].gameObject.SetActive(selectedTrack == i);
            }

            racing = false;
            currentIndex = 0;
            racingTime = 0;

            trackNameText.text = "Race Tracks disabled";

            TimeDisplay(false);
        }

        public void NextTrack()
        {
            ChangeTrack(selectedTrack + 1);
        }
        public void PreviousTrack()
        {
            ChangeTrack(selectedTrack - 1);
        }

        public void ChangeTrack(int trackNumber)
        {
            if (trackNumber > racingTracks.Length - 1) trackNumber = 0;
            else if (trackNumber < 0) trackNumber = racingTracks.Length - 1;

            selectedTrack = trackNumber;

            for (int i = 0; i < racingTracks.Length; i++)
            {
                racingTracks[i].gameObject.SetActive(selectedTrack == i);
            }

            UpdateHighscoreList();

            trackNameText.text = racingTracks[selectedTrack].trackName;

            racingTracks[selectedTrack].PrepareCheckpoints();

            racing = false;
            currentIndex = 0;
            racingTime = 0;
        }
        public void UpdateHighscoreList()
        {
            if (selectedTrack == -1) return;

            Debug.Log($"Updating the leaderboard with players from the list");
            Debug.Log($"Player list size is {playerSlots.Length}");

            if (scoreboardList.Length != 0) ClearScoreboard();
            if (playerSlots.Length == 0) return;

            scoreboardList = new GameObject[playerSlots.Length];

            GameObject g = null;

            for (int i = 0; i < scoreboardList.Length; i++)             
            {
                if (playerSlots[i] == null) continue;
                if (!Utilities.IsValid(playerSlots[i].gameObject)) continue;

                g = Instantiate(scoreboardPrefab, scoreboardParent.transform);
                g.SetActive(true);

                scoreboardList[i] = g;

                DroneRacingScore drs = g.GetComponent<DroneRacingScore>();

                drs.playerName.text = Networking.GetOwner(playerSlots[i].gameObject).displayName;

                if (selectedTrack > playerSlots[i].trackTimes.Length - 1) return;

                TimeSpan hrs = TimeSpan.FromSeconds(playerSlots[i].trackTimes[selectedTrack]);
                drs.playerTime.text = $"{hrs.Minutes}:" + (hrs.Seconds > 9 ? $"{hrs.Seconds}" : "0" + hrs.Seconds.ToString() + $".{hrs.Milliseconds}");
            }
        }

        public void Update()
        {
            UITracking();

            if (!racing) return;

            racingTime += Time.deltaTime;

            displayedHrs = TimeSpan.FromSeconds(racingTime);
            timeText.text = $"{displayedHrs.Minutes}:" + (displayedHrs.Seconds > 9 ? $"{displayedHrs.Seconds}" : "0" + displayedHrs.Seconds.ToString() + $".{displayedHrs.Milliseconds}");

        }

        public void UITracking()
        {
            if (!localPlayer.GetDrone().IsDeployed()) return;

            VRCDroneApi drone = localPlayer.GetDrone();

            anchorToDrone.transform.position = drone.GetPosition();
            anchorToDrone.transform.rotation = drone.GetRotation();
            anchorToDrone.transform.Translate(Vector3.forward * 1.5f);
            anchorToDrone.transform.Translate(Vector3.down * .5f);
        }

        public void RaceStart()
        {
            racingTime = 0;
            racing = true;
            PlayRaceStart();
            currentIndex = 0;

            Debug.Log($"Race starting on track {selectedTrack}");

            TimeDisplay(true);
        }
        public void RaceEnd()
        {
            racing = false;
            PlayRaceEnd();
            currentIndex = -1;

            if (localSlot == null) return;

            Debug.Log($"Race finished on track {selectedTrack}. Saving...");

            localSlot.SaveTime(racingTime);
        }

        public void TimeDisplay(bool state)
        {
            timeText.gameObject.SetActive(state);
        }

        public void CheckPointPass(int index, DroneRacingCheckpoint checkpoint)
        {
            if (selectedTrack == -1) return;

            if (index == 0)
            {
                RaceStart();

                checkpoint.checkpointVisual.Play();
                checkpoint.nextpointVisual.SetActive(false);

                racingTracks[selectedTrack].ShowNextPoint(index + 1);
            }

            if (index == currentIndex + 1)
            {
                currentIndex = index;

                if (currentIndex == racingTracks[selectedTrack].finalCheckpointID)
                {
                    RaceEnd();
                    racingTracks[selectedTrack].ShowNextPoint(0);
                }
                else
                {
                    PlayCheckpointSound(checkpoint.transform.position);
                    racingTracks[selectedTrack].ShowNextPoint(index + 1);
                }

                checkpoint.nextpointVisual.SetActive(false);
                checkpoint.checkpointVisual.Play();
            }
        }

        public void PlayRaceStart()
        {
            sfxLocalPlayer.transform.position = transform.position;
            sfxLocalPlayer.PlayOneShot(raceStartSFX);
        }
        public void PlayRaceEnd()
        {
            sfxLocalPlayer.transform.position = transform.position;
            sfxLocalPlayer.PlayOneShot(raceFinishSFX);
        }
        public void PlayCheckpointSound(Vector3 position)
        {
            sfxLocalPlayer.transform.position = position;
            sfxLocalPlayer.PlayOneShot(checkpointSFX);
        }
        public void PlayPortalSound(Vector3 position)
        {
            sfxLocalPlayer.transform.position = position;
            sfxLocalPlayer.PlayOneShot(portalSFX);
        }
        public void PlayBoostSound(Vector3 position)
        {
            sfxLocalPlayer.transform.position = position;
            sfxLocalPlayer.PlayOneShot(boostSFX);
        }
        public void PlayNewPBSound(Vector3 position)
        {
            sfxGlobalPlayer.transform.position = position;
            sfxGlobalPlayer.PlayOneShot(newPBSFX);
        }

        public void SetPlayerSlot(DroneRacingSlot local)
        {
            localSlot = local;
        }

        public void AddActivePlayer(DroneRacingSlot slot)
        {
            DroneRacingSlot[] temp = new DroneRacingSlot[playerSlots.Length];

            for(int i = 0; i < playerSlots.Length; i++)
            {
                temp[i] = playerSlots[i];
            }

            playerSlots = new DroneRacingSlot[temp.Length + 1];

            Debug.Log($"Adding slot. New array size is {playerSlots.Length}");

            for (int i = 0; i < temp.Length; i++)
            {
                playerSlots[i] = temp[i];
            }

            playerSlots[playerSlots.Length - 1] = slot;

            UpdateHighscoreList();
        }
        public void RemoveInactivePlayers()
        {
            int newSize = 0;

            DroneRacingSlot[] temp = new DroneRacingSlot[playerSlots.Length];

            for (int i = 0; i < playerSlots.Length; i++)
            {
                temp[i] = playerSlots[i];
                if (playerSlots[i] != null) newSize++;
            }

            playerSlots = new DroneRacingSlot[newSize];
            int currentPos = 0;

            for (int i = 0; i < temp.Length; i++)
            {
                if (temp[i] != null)
                {
                    playerSlots[currentPos] = temp[i];
                    currentPos++;
                }
            }

            UpdateHighscoreList();
        }

        public void ClearScoreboard()
        {
            foreach (GameObject g in scoreboardList) Destroy(g);
        }
    }
}