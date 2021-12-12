// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;
using UnityEngine.UI;
using Microsoft.MixedReality.WebRTC.Unity;
using System.Collections.Generic;
using io.agora.webrtc;

public class UIController : MonoBehaviour
{
    public AgoraWebRTC AgoraWebRTC;
    public GameObject PrefabMedia;

    private List<GameObject> Peers = new List<GameObject>();

    private void OnEnable()
    {
        AgoraWebRTC.OnUserOnline.AddListener(OnUserJoin);
        AgoraWebRTC.OnUserOffline.AddListener(OnUserLeave);
    }

    private void OnDisable()
    {
        AgoraWebRTC.OnUserOnline.RemoveListener(OnUserJoin);
        AgoraWebRTC.OnUserOffline.RemoveListener(OnUserLeave);
    }

    void OnUserJoin(string uid)
    {
        foreach (var _peer in Peers)
        {
            var _agora = _peer.GetComponentInChildren<AgoraWebRTC>();
            if (_agora.LocalPeerId == uid)
                return;
            if (_agora.RemotePeerId == uid)
                return;
        }

        GameObject peer = (GameObject)Instantiate(PrefabMedia, transform.position, transform.rotation);

        var agora = peer.GetComponentInChildren<AgoraWebRTC>();

        agora.AutoLogErrors = AgoraWebRTC.AutoLogErrors;
        agora.AppID = AgoraWebRTC.AppID;
        agora.Token = AgoraWebRTC.Token;
        agora.ChannelName = AgoraWebRTC.ChannelName;
        agora.LocalPeerId = "0";
        agora.RemotePeerId = uid;
        agora.Main = false;
        agora.ActionPublish(false);

        peer.transform.SetParent(transform);

        Peers.Add(peer);

        var videoTf = peer.transform.Find("VideoPlayer").transform;
        peer.transform.Translate(videoTf.lossyScale.x * Peers.Count + videoTf.localPosition.x, 0, 0);

    }

    void OnUserLeave(string uid)
    {
        foreach (var peer in Peers)
        {
            var agora = peer.GetComponentInChildren<AgoraWebRTC>();
            if (agora.RemotePeerId == uid)
            {
                Object.Destroy(peer);
                Peers.Remove(peer);
                return;
            }
        }
    }
}
