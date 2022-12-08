using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;
using Photon.Realtime;
using Photon.Pun;
using FMOD.Studio;

public enum Event_Code
{
    DestroySpider,
    SpiderDestroyed,
    RespawnSpider,
    UpdateScore,

    PreGame,
    GameStarted,
    GameWon,
    GameRestarted,

    SoundTrigger,

    SyncTimer
}

public class ReceiveEvents : MonoBehaviour
{
    PhotonView PV;

    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    private void Start()
    {
        if (!PV.IsMine)
        {
            Destroy(gameObject);
        }
    }

    private void OnEvent(EventData _photonEvent)
    {
        byte eventCode = _photonEvent.Code;

        if (eventCode == (int)Event_Code.DestroySpider)
        {
            object[] data = (object[])_photonEvent.CustomData;

            // Check if the Event is send to me
            if (data[0] != null)
            {
                if (PhotonView.Find((int)data[0]).IsMine)
                {
                    EventSystemNew.RaiseEvent(Event_Type.SPIDER_DESTROY_CAMERA);

                    // Find PhotonView with ViewID Data
                    PhotonNetwork.Destroy(PhotonView.Find((int)data[0]).gameObject);

                    object[] content = new object[] { PV.ViewID };

                    RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };

                    PhotonNetwork.RaiseEvent((int)Event_Code.SpiderDestroyed, content, raiseEventOptions, SendOptions.SendReliable);

                    EventSystemNew<bool, bool>.RaiseEvent(Event_Type.SPIDER_DIED, true, (bool)data[1]);
                }
            }
        }

        if (eventCode == (int)Event_Code.SpiderDestroyed)
        {
            EventSystemNew<bool, bool>.RaiseEvent(Event_Type.SPIDER_DIED, false, false);
        }

        if (eventCode == (int)Event_Code.RespawnSpider)
        {
            object[] data = (object[])_photonEvent.CustomData;

            // Check if the Event is send to me
            if (PhotonView.Find((int)data[0]).IsMine)
            {
                EventSystemNew<bool>.RaiseEvent(Event_Type.SPIDER_RESPAWNED, true);
            }
            else
            {
                EventSystemNew<bool>.RaiseEvent(Event_Type.SPIDER_RESPAWNED, false);
            }
        }

        if (eventCode == (int)Event_Code.UpdateScore)
        {
            object[] data = (object[])_photonEvent.CustomData;

            Player correctPlayer = null;

            foreach (Player player in PhotonNetwork.PlayerList)
            {
                if (player.UserId == (string)data[0])
                {
                    correctPlayer = player;
                }
            }

            EventSystemNew<Player, int>.RaiseEvent(Event_Type.UPDATE_SCORE, correctPlayer, (int)data[1]);
        }

        if (eventCode == (int)Event_Code.GameStarted)
        {
            if (!GameManager.Instance.gameStarted)
            {
                EventSystemNew.RaiseEvent(Event_Type.GAME_STARTED);
            }
        }

        if (eventCode == (int)Event_Code.GameWon)
        {
            object[] data = (object[])_photonEvent.CustomData;

            EventSystemNew<string>.RaiseEvent(Event_Type.GAME_WON, (string)data[0]);
        }

        if (eventCode == (int)Event_Code.GameRestarted)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                object[] data = (object[])_photonEvent.CustomData;

                PhotonNetwork.LoadLevel((string)data[0]);
            }
        }

        if (eventCode == (int)Event_Code.SoundTrigger)
        {
            object[] data = (object[])_photonEvent.CustomData;

            int soundTypeInt = (int)data[0];

            if (PhotonView.Find((int)data[1]) != null)
            {
                EventSystemNew<Sound_Type, GameObject, bool>.RaiseEvent(Event_Type.TRIGGER_SOUND, (Sound_Type)soundTypeInt, PhotonView.Find((int)data[1]).gameObject, (bool)data[2]);
            }
        }

        if (eventCode == (int)Event_Code.SyncTimer)
        {
            object[] data = (object[])_photonEvent.CustomData;

            EventSystemNew<float, bool, bool>.RaiseEvent(Event_Type.SYNC_TIMER, (float)data[0], (bool)data[1], (bool)data[2]);
        }
    }
}
