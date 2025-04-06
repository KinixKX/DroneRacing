# Drone Racing
Free VRChat world prefab to create courses for VRC+ Drones with Persistence integration.

## Installation

1. Download the lastest unitypackage from the releases page https://github.com/KinixKX/DroneRacing/releases
2. Ensure you are using VRChat Worlds SDK 3.8.0 or higher in your unity project
3. Add and install the unitypackage downloaded to your project.
4. Find the main prefab in `DroneRacingSystem > Prefabs > DroneRacingSystem` and drag it onto your scene

### Setup on Scene

If you wish to adjust the main panel with the game instructions, ensure you are only modifying the `GameCanvas` and
`SpectatorCanvas` objects. You can scale, rotate and move them anywhere in your scene.

![image](https://github.com/user-attachments/assets/fecf10fb-190f-4421-8e23-97c24c74918b)

### Creating your own courses

To create your own courses in the world, you must drag over a `RaceTrack` from the prefabs folder.
In there you will place any and every element of your course including the checkpoints for the course
decoration meshes, and anything else that needs to enable with the course being selected.

It is important that **EVERY** checkpoint, start point, finish point, booster and portal are all located 
inside the RaceTrack object with the script in it. Failing to do this will result in unexpected behaviours and the system breaking.
Objects/Prefabs for courses can be found in `DroneRacingSystem > Prefabs`

![image](https://github.com/user-attachments/assets/cb49d258-3475-498c-b2c9-5d0f39e0a656)

In order to define the layout and order of the track, each checkpoint comes with an ID in their script. This helps
the course determine the order in which to fly your drone through. The start and finish points are also a checkpoint,
but the `TrackStartPoint` has to have the ID 0 and the `TrackFinishPoint` has to have the last possible ID. **It's important you do not miss
a checkpoint ID, otherwise the track will be impossible to finish**.

* You may rotate, scale and move the **main** object of the prefab for course building.
* Avoid touching anything inside the prefabs themselves. (Except for Portals)
* Portals entry and exit may be moved indepedantly from the root prefab object.
* Avoid rotating the `TrackPortalExit` on the X and/or Z axis.
* Booster gates give the drones a boost in speed in the direction the Booster is aiming at.

Lastly, once you have finished with your courses you must add them to the list in the `DR_Manager` object.
You can add as many as you want, don't forget to name them!

![image](https://github.com/user-attachments/assets/d92db98f-70f4-4da4-bee7-9263ba57e787) ![image](https://github.com/user-attachments/assets/640755a7-5283-4d1c-9aed-0a9d91f56b72)


### Notes

Persistence will automatically save each player's highscore, but beware that **if you rearrange the courses in the list
the player's score won't properly offset with it.**

If you wish to check out a demo of the system, you can do so by checking out this world
https://vrchat.com/home/world/wrld_9bc167e2-deb7-48aa-9577-500c1592478e/info

## Support

If you need help with setting up the system, please feel free to reach out to me in Discord 'kinixkx' or in Bluesky 'kinix.bsky.social'
And if you want to consider donating, join my Patreon for as little as $2 ! https://www.patreon.com/Kinix
I prefer to work in my own worlds, but sometimes prefabs are fun to work with.

