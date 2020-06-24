using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using ExitGames.Client.Photon;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class Launcher : MonoBehaviourPunCallbacks, IConnectionCallbacks, IMatchmakingCallbacks, IOnEventCallback
{
    private GameObject localAvatar;

    private GameObject localObserver;

    public GameObject rig;

    public GameObject loading;

    public bool observer = false;

    public bool voiceDebug = true;

    public AvatarBehaviourRecorder avatarRecorder;

    PhotonView photonView;
    void Start()
    {
        Resources.LoadAll("ScriptableObjects");
        Debug.Log("[PUN] connecting to server");

        PhotonNetwork.AuthValues = new AuthenticationValues();

        if (observer) {

            PhotonNetwork.NickName ="Observer";
            PhotonNetwork.AuthValues.UserId = "1";
        }
        else
        {
            PhotonNetwork.NickName = MasterManager.GameSettings.Nickname;           
         
        }
     
        PhotonNetwork.GameVersion = MasterManager.GameSettings.Gameversion;
        PhotonNetwork.ConnectUsingSettings();

    }

    public override void OnConnectedToMaster() {

        Debug.Log("[PUN] connected to server");

        Debug.Log("[PUN] connected with Nickname: " + PhotonNetwork.LocalPlayer.NickName + "\n UserID: " + PhotonNetwork.LocalPlayer.UserId);

        Debug.Log("[PUN] joining room " + MasterManager.GameSettings.RoomName);

        RoomOptions options = new RoomOptions();
        options.PublishUserId = true;
        PhotonNetwork.JoinOrCreateRoom(MasterManager.GameSettings.RoomName, options, TypedLobby.Default);
    }

    void IMatchmakingCallbacks.OnJoinedRoom()
    {
        Debug.Log("[PUN] joined room " + PhotonNetwork.CurrentRoom);

        StartCoroutine(waitForSdkManagerInstantiated());
    }

    private IEnumerator waitForSdkManagerInstantiated()
    {

        //get audiosource from the localavatar       
        while (OvrAvatarSDKManager.Instance == null)
        {
            yield return new WaitForSeconds(0.1f);
        }

        if (observer) ObserverInstantiation();
        else InstantiateLocalAvatar();
    }


    void IOnEventCallback.OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == MasterManager.GameSettings.InstantiateVrAvatarEventCode)
        {
            InstantiateRemoteAvatar(photonEvent);
        }
        else if (photonEvent.Code == MasterManager.GameSettings.InstantiateObserverEventCode) {

            InstantiateRemoteObserver(photonEvent);
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("[PUN] disconnected from server because of " + cause.ToString());
    }


    //AVATAR
    //local avatar
    void InstantiateLocalAvatar()
    {

        Debug.Log("[PUN] instantiate LocalAvatar");

        GameObject OVRPlayerController = GameObject.Find("OVRPlayerController");
        photonView = OVRPlayerController.AddComponent<PhotonView>();//Add a photonview to the OVR player controller 
        PhotonTransformView photonTransformView = OVRPlayerController.AddComponent<PhotonTransformView>();//Add a photonTransformView to the OVR player controller 
        photonTransformView.m_SynchronizeRotation = false;
        photonView.ObservedComponents = new List<Component>();
        photonView.ObservedComponents.Add(photonTransformView);
        photonView.Synchronization = ViewSynchronization.UnreliableOnChange; // set observeoption to unreliableonchange

        //instantiate the local avatr
        GameObject TrackingSpace = GameObject.Find("TrackingSpace");
        localAvatar = Instantiate(Resources.Load("LocalAvatar"), TrackingSpace.transform.position, TrackingSpace.transform.rotation, TrackingSpace.transform) as GameObject;
        PhotonAvatarView photonAvatrView = localAvatar.GetComponent<PhotonAvatarView>();
        photonAvatrView.photonView = photonView;
        photonAvatrView.ovrAvatar = localAvatar.GetComponent<OvrAvatar>();
        photonView.ObservedComponents.Add(photonAvatrView);


        if (PhotonNetwork.AllocateViewID(photonView))
        {

            RaiseEventOptions raiseEventOptions = new RaiseEventOptions
            {
                CachingOption = EventCaching.AddToRoomCache,
                Receivers = ReceiverGroup.Others
            };

            PhotonNetwork.RaiseEvent(MasterManager.GameSettings.InstantiateVrAvatarEventCode, photonView.ViewID, raiseEventOptions, SendOptions.SendReliable);

            OvrAvatar ovrAvatar = localAvatar.GetComponent<OvrAvatar>();
            ovrAvatar.oculusUserID = MasterManager.GameSettings.UserID;

            Debug.Log("[PUN] LocalAvatar instantiatiation triggered now waiting for OVRAvatar to initialize");

            OvrAvatar.LocalAvatarInstantiated += LocalAvatarInstantiated;
        }
        else
        {
            Debug.LogError("[PUN] Failed instantiate LocalAvatar, Failed to allocate a ViewId.");

            Destroy(localAvatar);
        }
    }

    private void LocalAvatarInstantiated() {

        StartCoroutine(PhotonVoiceInstantiationForLocalAvatar());

        //inputsManager.Instance.localAvatar= localAvatar;
    }
   
    private IEnumerator PhotonVoiceInstantiationForLocalAvatar()
    {
       
        Debug.Log("[PUN] OVRAvatar completed instantiation of LocalAvatar now we setup voice by adding Speaker,Recorder,VoiceView ");

        //get audiosource from the localavatar       
        while (localAvatar.GetComponentInChildren<AudioSource>() == null)
        {
            yield return new WaitForSeconds(0.1f);
        }
        AudioSource audioSource = localAvatar.GetComponentInChildren<AudioSource>();

        //////get the ovr 
        while (audioSource.gameObject.GetComponent<OVRLipSyncContext>() == null)
        {
            yield return new WaitForSeconds(0.1f);
        }
        OVRLipSyncContext LipSyncContext = audioSource.gameObject.GetComponent<OVRLipSyncContext>();
        LipSyncContext.audioSource = audioSource;
        if (voiceDebug) LipSyncContext.audioLoopback = true; // Don't use mic.
        else LipSyncContext.audioLoopback = false;
        LipSyncContext.skipAudioSource = false;

        ////add speaker to the element which holds the audio source 
        Speaker speaker = audioSource.gameObject.AddComponent<Speaker>();

        ////add recorder to the element that has the photonView
        Recorder recorder = photonView.gameObject.AddComponent<Recorder>();
        recorder.DebugEchoMode = true;

        ////add Photonvoice view to the local avatar
        PhotonVoiceView voiceView = photonView.gameObject.AddComponent<PhotonVoiceView>();
        voiceView.RecorderInUse = recorder;
        voiceView.SpeakerInUse = speaker;
        voiceView.SetupDebugSpeaker = true;

        ////start transmission 
        yield return voiceView.RecorderInUse.TransmitEnabled = true;
        voiceView.RecorderInUse.StartRecording();

    }

    //remote Avatar
    private void InstantiateRemoteAvatar(EventData photonEvent)
    {

        //sender 
        Player player = PhotonNetwork.CurrentRoom.Players[photonEvent.Sender];

        Debug.Log("[PUN] Instantiatate an avatar for user " + player.NickName + "\n with user ID " + player.UserId);

        GameObject remoteAvatar = Instantiate(Resources.Load("RemoteAvatar")) as GameObject;

        remoteAvatar.name = player.NickName;

        PhotonView photonView = remoteAvatar.GetComponent<PhotonView>();
        photonView.ViewID = (int)photonEvent.CustomData;

        OvrAvatar ovrAvatar = remoteAvatar.GetComponent<OvrAvatar>();
        ovrAvatar.oculusUserID = player.UserId;

        Debug.Log("[PUN] RemoteAvatar instantiated");

        OvrAvatar.RemoteAvatarInstantiated += OvrAvatar_RemoteAvatarInstantiated;

        //PhotonVoiceView pvv = remoteAvatar.GetComponent<PhotonVoiceView>();
      
    }

    private GameObject OvrAvatar_RemoteAvatarInstantiated(GameObject rA)
    {

        if(inputsManager.Instance.remoteAvatar == rA || inputsManager.Instance.localAvatar == rA)
            return rA;

        if (inputsManager.Instance.remoteAvatar == null) { 
            inputsManager.Instance.remoteAvatar = rA;
            Debug.Log("[PUN] inputsManager associate avatar" + rA.name+" to remoteAvatar");
        }else if (inputsManager.Instance.localAvatar == null) { 
            inputsManager.Instance.localAvatar = rA;
            Debug.Log("[PUN] inputsManager associate avatar" + rA.name + " to localAvatar");
            Debug.Log("[PUN] RemoteAvatar instantiated");
        }
            
        else 
            Debug.LogError("inputs manager cannot register any avatar");

        return rA;
    }


    //OBSERVER
    //local Observer
    void ObserverInstantiation()
    {
        Debug.Log("[PUN] instantiate Local Observer");
     
        //instantiate the local avatar      
        localObserver = Instantiate(Resources.Load("ObserverCamera")) as GameObject;
        photonView = localObserver.GetComponent<PhotonView>();

        //enable observer camera
        localObserver.tag = "MainCamera";


        if (PhotonNetwork.AllocateViewID(photonView))
        {
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions
            {
                CachingOption = EventCaching.AddToRoomCache,
                Receivers = ReceiverGroup.Others
            };

            PhotonNetwork.RaiseEvent(MasterManager.GameSettings.InstantiateObserverEventCode, photonView.ViewID, raiseEventOptions, SendOptions.SendReliable);

            Debug.Log("[PUN] Local Observer instantiated");

            //enablePhotonVoice()
            StartCoroutine(PhotonVoiceInstantiationForLocalObserver());

        }
        else
        {
            Debug.LogError("[PUN] Failed instantiate Local Observer, Failed to allocate a ViewId.");

            Destroy(localObserver);
        }

        //destroy the player controller 
        Destroy(GameObject.Find("OVRPlayerController"));

        //enable observer recorder
        avatarRecorder.enabled = true;

        //create a folder for saving the data
        DataFolderCreation();
    }

    private IEnumerator PhotonVoiceInstantiationForLocalObserver()
    {
        
        Debug.Log("[PUN] setup voice for observer by adding Speaker,Recorder,VoiceView ");

        //add audio source
        AudioSource audioSource = localObserver.GetComponent<AudioSource>();

        ////add speaker to the element which holds the audio source 
        Speaker speaker = audioSource.gameObject.AddComponent<Speaker>();

        ////add recorder to the element that has the photonView
        Recorder recorder = localObserver.gameObject.AddComponent<Recorder>();
        recorder.DebugEchoMode = false;

        ////add Photonvoice view to the local avatar
        PhotonVoiceView voiceView = localObserver.gameObject.AddComponent<PhotonVoiceView>();
        voiceView.RecorderInUse = recorder;
        voiceView.SpeakerInUse = speaker;
        voiceView.SetupDebugSpeaker = true;

        ////start transmission 
        yield return voiceView.RecorderInUse.TransmitEnabled = true;
        voiceView.RecorderInUse.StartRecording();


    }

    //Remote observer 
    private void InstantiateRemoteObserver(EventData photonEvent)
    {

        //sender 
        Player player = PhotonNetwork.CurrentRoom.Players[photonEvent.Sender];

        Debug.Log("[PUN] Instantiatate Observer for user " + player.NickName + "\n with user ID " + player.UserId);

        GameObject remoteObserver = Instantiate(Resources.Load("RemoteObserver")) as GameObject;

        PhotonView photonView = remoteObserver.GetComponent<PhotonView>();
        photonView.ViewID = (int)photonEvent.CustomData;

        Debug.Log("[PUN] Remote Observer Instantiated");

    }

    private void DataFolderCreation() {

        string path = Application.dataPath + "\\" + MasterManager.GameSettings.DataFolder + "\\";

        if(!Directory.Exists(path)) Directory.CreateDirectory(path);

    }
}
