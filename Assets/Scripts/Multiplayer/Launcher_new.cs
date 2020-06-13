using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using ExitGames.Client.Photon;
using System.Collections;
using System.Collections.Generic;
using System.IO;


public class Launcher_new : MonoBehaviourPunCallbacks, IConnectionCallbacks, IMatchmakingCallbacks, IOnEventCallback
{
    private GameObject localAvatar;

    public bool voiceDebug = true;

    PhotonView photonView;
    void Start()
    {
        Resources.LoadAll("ScriptableObjects");
        Debug.Log("[PUN] connecting to server");

        PhotonNetwork.AuthValues = new AuthenticationValues();

        
        PhotonNetwork.NickName = MasterManager.GameSettings.Nickname;

        PhotonNetwork.GameVersion = MasterManager.GameSettings.Gameversion;
        PhotonNetwork.ConnectUsingSettings();

    }

    public override void OnConnectedToMaster()
    {

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

        InstantiateLocalAvatar();
    }

    //local Avatar

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
 

            Debug.Log("[PUN] LocalAvatar instantiatiation triggered now waiting for OVRAvatar to initialize");

        }
        else
        {
            Debug.LogError("[PUN] Failed instantiate LocalAvatar, Failed to allocate a ViewId.");

            Destroy(localAvatar);
        }
    }

    void IOnEventCallback.OnEvent(EventData photonEvent)
    {

        

        if (photonEvent.Code == MasterManager.GameSettings.InstantiateVrAvatarEventCode)
        {
            Debug.Log("[PUN] Event " + photonEvent);

            InstantiateRemoteAvatar(photonEvent);
        }

    }

    //remote Avatar
    private void InstantiateRemoteAvatar(EventData photonEvent)
    {

        //sender 
        Player player = PhotonNetwork.CurrentRoom.Players[photonEvent.Sender];

        Debug.Log("[PUN] Instantiatate an avatar for user " + player.NickName + "\n with user ID " + player.UserId);

        GameObject remoteAvatar = Instantiate(Resources.Load("RemoteAvatarNoVoice")) as GameObject;

        remoteAvatar.name = player.NickName;

        PhotonView photonView = remoteAvatar.GetComponent<PhotonView>();

        Debug.Log("[PUN] photonView " + (photonView==null) );

        Debug.Log("[PUN] photonView " + ((int)photonEvent.CustomData));

        photonView.ViewID = (int)photonEvent.CustomData;

        //OvrAvatar ovrAvatar = remoteAvatar.GetComponent<OvrAvatar>();
       
        Debug.Log("[PUN] RemoteAvatar instantiated");
    

    }


}
