# GuideLine to produce a Navmesh CSV starting from Crowd Data (Unity 2019.4.0)

1) open NavMeshTrajectoriesrecorder Scene 
![Unity View](https://github.com/dgiunchi/UnityCrowdVR/blob/master/GuidelineNavMesh/figure01_NavMeshScene.png?raw=true)


2) the objct that control the simulation is activateForAdamNavMesh ![Unity View](https://github.com/dgiunchi/UnityCrowdVR/blob/master/GuidelineNavMesh/figure02_Manager.png?raw=true) that contains a Component (in Inspector).

    2.a) The first input field is for the CSV data from which initial position are taken. It is a normal trajectory data from crowd simulation. 
    
    2.b) You can resize (button Below) the scene with the Scaling factor (for data_1 is 0.02), and to scale the Walls with height (used only for the Walls), Use resize into Editor Not in Play Mode. Scene Pefab is the provided environment.
    
    2.c) The NavMesh Prefab is NavMesh_Simulation
    
    2.d) Speed is the multiplication factor to the capturedeltatime. Increasing it, increase the speed of the simulation. If navmesh agent speed need to be modified, change it in prefab.
    
    
    NavMesh_Simulation prefab is a classic capsule object with a NavMeshAgent (refer to Unity documentation) ![Unity View](https://github.com/dgiunchi/UnityCrowdVR/blob/master/GuidelineNavMesh/figure03_NavMeshPrefab.png?raw=true)
    
    
    
3) When a different environment is created, you need to select the plane (and eventual other structures) and create the Navmesh going to Window->AI->Navigation, change the parameters in Navigation panel and bake it.


4) Press Unity play and record and wait until the CSV with all the timed-trajectories are saved into the file NavMesh.csv
