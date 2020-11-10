# GuideLine to produce a Navmesh CSV starting from Crowd Data (Unity 2019.4.0)

1) open Demo_Original Scene (created referencing data1.csv)
![Unity View](https://github.com/dgiunchi/UnityCrowdVR/blob/master/GuidelineAnimation/figure01_AnimationScene.png?raw=true)


2) the objct that control the simulation is SimulationManagerAdam ![Unity View](https://github.com/dgiunchi/UnityCrowdVR/blob/master/GuidelineAnimation/figure02_Manager.png?raw=true) that contains a Component (in Inspector).

    2.a) The first input field is for the CSV data from which initial position are taken. It is a normal trajectory data from crowd simulation, that can be showed  with DrawTrajectories or Played with PlayCSV.
    
    2.b) You can resize (button Below) the scene with the Scaling factor (for data_1 is 0.02), and to scale the Walls with height (used only for the Walls), Use resize into Editor Not in Play Mode. Scene Prefab is the provided environment. You can draw also the trajectories an the bouns of the area. o it not in Play Mode. 
    ![Unity View (https://github.com/dgiunchi/UnityCrowdVR/blob/master/GuidelineAnimation/figure03_Trajectories.png?raw=true)
    
    2.c) The Prefab is ScenePrefab that contains the structure for that specific scene (remember that the scene needs to be compatible with CSV)
    
    2.d) Single play Inex is the index of the person you want to play alone (after a recording session that can contains more than one person)
    
    2.e) NavMesh_Simulation prefab is a classic capsule object with a NavMeshAgent (refer to Unity documentation) ![Unity View](https://github.com/dgiunchi/UnityCrowdVR/blob/master/GuidelineAnimation/figure03_NavMeshPrefab.png?raw=true)
    
    2.f) Simulation-Animation section contains a prefab for the record, an a list of prefabs for the play, and an additional prefab for rigid avatars. ![Unity View](https://github.com/dgiunchi/UnityCrowdVR/blob/master/GuidelineAnimation/figure04_Prefabs.png?raw=true)
    
    While Play Prefabs are Rigged Models that move their joints accoding to our logic, Record Prefab is the one that create the animation during a Record Session.

3) Record Session

Click Play in Unity and after having selected a csv and a skeleton record prefab, click Record. The system firstly load all in memory, and then launches the simulation with all the model walking following the assigned trajectory and creating the animation.

It can take a while to see skeleton and Adam model appearing on the screen. ![Unity View](https://github.com/dgiunchi/UnityCrowdVR/blob/master/GuidelineAnimation/figure05_Record.png?raw=true)

Wait Until Serialization process ends (it will be displayed on the Console). A file ".dat" will be created.

4) Simulation now can be played. Play session needs both the CSV used for record and the dat file created.

5) Try to deploy in Oculus.

