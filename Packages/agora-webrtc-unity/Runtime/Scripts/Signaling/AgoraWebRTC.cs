// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading;
using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Collections.Generic;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.ComponentModel;
using System.IO;
using UnityEngine.Events;
using Microsoft.MixedReality.WebRTC.Unity;

using io.agora.sdp;

namespace io.agora.webrtc
{
    #region value type

    public enum State {
        Start = 0,
        Disconnected,
        APRequested,
        VOSConnected,
        Joined,
        Busy,
    }

    public class APReqMessage
    {
        public int opid;
        public int flag;
        public long uid;
        public long ts;
        public string key;
        public string cname;
        public string sid;
        public Dictionary<string, string> detail;
    }

    public class APRspMessage
    {
        public class APRspAddress
        {
            public string ip;
            public string port;
            public string ticket;
        }

        public int code;
        public int opid;
        public int flag;
        public long cid;
        public long uid;
        public long server_ts;
        public string cname;
        public List<APRspAddress> addresses;
        public Dictionary<string, string> detail;
    }

    public class VOSReqJoinMessage
    {
        public class Message
        {
            public class Features
            {
                public bool rejoin;
            }
            public class APResponse
            {
                public int code;
                public int opid;
                public int flag;
                public long cid;
                public long uid;
                public long server_ts;
                public string cname;
                public Dictionary<string, string> detail;
                public string ticket;
            }

            public class Sdp
            {
                public class IceParameters
                {
                    public string iceUfrag;
                    public string icePwd;
                }
                public class DtlsParameters
                {
                    public List<FingerPrint> fingerprints;
                }
                public class RtpCapabilities
                {
                    public List<PayloadAttribute> audioCodecs;
                    public List<PayloadAttribute> videoCodecs;
                    public List<Extmap> audioExtensions;
                    public List<Extmap> videoExtensions;
                }

                public IceParameters iceParameters;
                public DtlsParameters dtlsParameters;
                public RtpCapabilities rtpCapabilities;
            }

            public int p2p_id;
            public string session_id;
            public string app_id;
            public string channel_key;
            public string channel_name;
            public string sdk_version;
            public string browser;
            public string process_id;
            public string mode;
            public string codec;
            public string role;
            public bool has_changed_gateway;
            public APResponse ap_response;
            public string extends;
            public Dictionary<string, string> details;
            public Features features;
            public Sdp ortc;
        }

        public string _id;
        public string _type;
        public Message _message;
    }

    public class VOSRspJoinMessage
    {
        public class Message
        {
            public class Sdp
            {
                public class DtlsParameters
                {
                    public List<FingerPrint> fingerprints;
                    public string role;
                }
                public class IceParameters
                {
                    public List<Candidate> candidates;
                    public string iceUfrag;
                    public string icePwd;
                }
                public class RtpCapabilities
                {
                    public List<PayloadAttribute> audioCodecs;
                    public List<PayloadAttribute> videoCodecs;
                    public List<Extmap> videoExtensions;
                    public List<Extmap> audioExtensions;
                }

                public string cname;
                public DtlsParameters dtls;
                public IceParameters ice;
                public RtpCapabilities rtpCapabilities;
            }

            public Sdp ortc;
        }
        public string _id;
        public Message _message;
        public long uid;
        public long vid;
        public string _result;
    }

    public class VOSReqLeaveMessage {
        public class Message {
        }

        public string _id;
        public string _type;
        public Message _message;
    }

    public class VOSReqPublishMessage
    {
        public class Message
        {
            public class Sdp
            {
                public class Attributes
                {
                    // for audio
                    public bool dtx;
                    public bool hq;
                    public bool lq;
                    public bool stereo;
                    public bool speech;
                    // for video
                    public string resolution;
                    public int maxVideoBW;
                    public int minVideoBW;
                    public int maxFrameRate;
                    public int minFrameRate;
                    public string codec;
                }

                public string stream_type;
                public Attributes attributes;
                public List<SSRC> ssrcs;
                public string mid;
            }
            public string state;
            public int p2p_id;
            public List<Sdp> ortc;
            public string mode;
            public string extend;
        }

        public string _id;
        public string _type;
        public Message _message;
    }

    public class VOSRspPublishMessage
    {
        public class Message
        {
            public class SubMessage
            {
                public class Sdp
                {
                    public class Attributes
                    {
                        public string remb;
                        public string tcc;
                    }

                    public Attributes attributes;
                    public List<SSRC> ssrcs;
                    public string stream_type;
                }

                public List<Sdp> ortc;
            }

            public SubMessage _message;
            public int p2pid;
            public long uid;
        }

        public string _id;
        public Message _message;
        public string _result;
    }

    public class VOSReqMuteMessage
    {
        public class Message {
            public class Sdp {
                public class SSRCID {
                    public long ssrcId;
                }

                public string stream_type;
                public List<SSRCID> ssrcs;
                public string mid;
            }

            public string action;
            public int p2p_id;
            public List<Sdp> ortc;
            public long stream_id;
        }

        public string _id;
        public string _type;
        public Message _message;
    }

    public class VOSReqUnPublishMessage
    {
        public class Message
        {
            public class Sdp
            {
                public class SSRCID
                {
                    public long ssrcId;
                }

                public string stream_type;
                public List<SSRCID> ssrcs;
                public string mid;
            }

            public string stream_id;
            public List<Sdp> ortc;
        }

        public string _id;
        public string _type;
        public Message _message;
    }

    public class VOSReqSubscribeMessage
    {
        public class Message
        {
            public long stream_id;
            public string stream_type;
            public string mode;
            public string codec;
            public int p2p_id;
            public bool tcc;
            public string extend;
            public long ssrcId;
        }

        public string _id;
        public string _type;
        public Message _message;
    }

    public class VOSRspSubscribeMessage
    {
        public class Message
        {
            public int p2pid;
            public long uid;
        }

        public string _id;
        public Message _message;
        public string _result;
    }

    public class VOSReqPingMessage
    {
        public string _id;
        public string _type;
    }

    public class VOSRspPingMessage
    {
        public string _id;
        public string _result;
    }

    public class VOSReqPingBackMessage
    {
        public class Message
        {
            public long pingpongElapse;
        }

        public string _id;
        public string _type;
        public Message _message;
    }

    public class VOSRspPingBackMessage
    {
        public string _id;
        public string _result;
    }

    public class VOSNotifyMessage
    {
        public class Message
        {
            public bool audio;
            public bool video;
            public string cname;
            public long ssrcId;
            public long uid;
        }

        public Message _message;
        public string _type;
    }

    public class VOSRecvMessage
    {
        public string _id;
        public string _result;
        public string _type;
    }

    public class BypassCertificate : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            //Simply return true no matter what
            return true;
        }
    }

    public class UserMessage
    {
        public class Message {
            public long uid;
        }

        public string _type;
        public Message _message;
    }

    public class UserOnlineEvent : UnityEvent<string>
    {
    }

    public class UserOfflineEvent : UnityEvent<string>
    {
    }

    #endregion
    /// <summary>
    /// Simple signaler for debug and testing.
    /// This is based on https://github.com/bengreenier/node-dss and SHOULD NOT BE USED FOR PRODUCTION.
    /// </summary>
    [AddComponentMenu("MixedReality-WebRTC/AgoraWebRTC Signaler")]
    public class AgoraWebRTC : MonoBehaviour
    {
        #region public data

        public bool AutoLogErrors = true;

        public string AppID;
        public string Token;
        public string ChannelName;

        public string LocalPeerId = "0";
        public string RemotePeerId = "0";

        public bool Main = false;

        public PeerConnection PC;

        #endregion

        #region Events

        public UserOnlineEvent OnUserOnline = new UserOnlineEvent();
        public UserOfflineEvent OnUserOffline = new UserOfflineEvent();

        #endregion

        #region private data

        private State State = State.Start;

        private bool Publish = true;
        private bool MuteAudio = false;
        private bool MuteVideo = false;

        private bool Published = false;
        private bool AudioMuted = false;
        private bool VideoMuted = false;

        private string DefaultSDP =
            "v=0"
            + "\no=- 7747875453871191419 2 IN IP4 127.0.0.1"
            + "\ns=AgoraGateway"
            + "\nt=0 0"
            + "\na=group:BUNDLE 0 1"
            + "\na=extmap-allow-mixed"
            + "\na=msid-semantic: WMS"
            + "\na=ice-lite"
            + "\n";

        private string DefaultAudioMediaJson =
            @"{""media"":{""mediaType"":""audio"",""port"":""9"",""protos"":[""UDP"",""TLS"",""RTP"",""SAVPF""],""fmts"":[""111"",""126""]},""information"":null,""connections"":[{""nettype"":""IN"",""addrtype"":""IP4"",""address"":""0.0.0.0""}],""bandwidths"":[],""ky"":null,""attributes"":{""mid"":""0"",""iceUfrag"":""xCGj"",""icePwd"":""Ckt3P0m/wsvcdGj4ELPSfz9+"",""iceLite"":null,""iceOptions"":[],""candidates"":[{""foundation"":""1"",""componentId"":""1"",""protocol"":""udp"",""priority"":""2103266323"",""ip"":""101.71.83.15"",""port"":""4716"",""type"":""host"",""relAddr"":null,""relPort"":null,""extension"":{}},{""foundation"":""1"",""componentId"":""1"",""protocol"":""udp"",""priority"":""2103266323"",""ip"":""183.249.96.15"",""port"":""4716"",""type"":""host"",""relAddr"":null,""relPort"":null,""extension"":{}},{""foundation"":""1"",""componentId"":""1"",""protocol"":""udp"",""priority"":""2103266323"",""ip"":""122.227.251.207"",""port"":""4716"",""type"":""host"",""relAddr"":null,""relPort"":null,""extension"":{}}],""remoteCandidatesList"":[],""endOfCandidates"":null,""fingerprints"":[{""algorithm"":""sha-256"",""fingerprint"":""79:AA:25:C7:75:9F:74:F8:AB:A6:2E:F6:D1:97:6D:49:11:58:F4:D5:2A:56:12:F3:63:81:D6:0E:5C:D6:54:F4""}],""ptime"":null,""maxPtime"":null,""direction"":2,""ssrcs"":[],""extmaps"":[],""rtcpMux"":true,""rtcpMuxOnly"":null,""rtcpRsize"":true,""rtcp"":{""port"":""9"",""netType"":""IN"",""addressType"":""IP4"",""address"":""0.0.0.0""},""msids"":[],""imageattr"":[],""rids"":[],""simulcast"":null,""sctpPort"":null,""maxMessageSize"":null,""unrecognized"":[],""setup"":""active"",""payloads"":[{""rtpMap"":{""encodingName"":""opus"",""clockRate"":48000,""encodingParameters"":2},""fmtp"":{""parameters"":{""minptime"":""10"",""useinbandfec"":""1""}},""rtcpFeedbacks"":[{""type"":""transport-cc"",""parameter"":null,""additional"":null,""interval"":null},{""type"":""nack"",""parameter"":null,""additional"":null,""interval"":null}],""payloadType"":111},{""rtpMap"":{""encodingName"":""telephone-event"",""clockRate"":8000,""encodingParameters"":0},""fmtp"":{""parameters"":{}},""rtcpFeedbacks"":[{""type"":""transport-cc"",""parameter"":null,""additional"":null,""interval"":null},{""type"":""nack"",""parameter"":null,""additional"":null,""interval"":null}],""payloadType"":126}],""rtcpFeedbackWildcards"":[],""ssrcGroups"":[],""xGoogleFlag"":null,""connection"":null}}";
        private string DefaultVideoMediaJson =
            @"{""media"":{""mediaType"":""video"",""port"":""9"",""protos"":[""UDP"",""TLS"",""RTP"",""SAVPF""],""fmts"":[""96"",""98""]},""information"":null,""connections"":[{""nettype"":""IN"",""addrtype"":""IP4"",""address"":""0.0.0.0""}],""bandwidths"":[],""ky"":null,""attributes"":{""mid"":""1"",""iceUfrag"":""xCGj"",""icePwd"":""Ckt3P0m/wsvcdGj4ELPSfz9+"",""iceLite"":null,""iceOptions"":[],""candidates"":[{""foundation"":""1"",""componentId"":""1"",""protocol"":""udp"",""priority"":""2103266323"",""ip"":""101.71.83.15"",""port"":""4716"",""type"":""host"",""relAddr"":null,""relPort"":null,""extension"":{}},{""foundation"":""1"",""componentId"":""1"",""protocol"":""udp"",""priority"":""2103266323"",""ip"":""183.249.96.15"",""port"":""4716"",""type"":""host"",""relAddr"":null,""relPort"":null,""extension"":{}},{""foundation"":""1"",""componentId"":""1"",""protocol"":""udp"",""priority"":""2103266323"",""ip"":""122.227.251.207"",""port"":""4716"",""type"":""host"",""relAddr"":null,""relPort"":null,""extension"":{}}],""remoteCandidatesList"":[],""endOfCandidates"":null,""fingerprints"":[{""algorithm"":""sha-256"",""fingerprint"":""79:AA:25:C7:75:9F:74:F8:AB:A6:2E:F6:D1:97:6D:49:11:58:F4:D5:2A:56:12:F3:63:81:D6:0E:5C:D6:54:F4""}],""ptime"":null,""maxPtime"":null,""direction"":2,""ssrcs"":[],""extmaps"":[{""entry"":3,""extensionName"":""http://www.webrtc.org/experiments/rtp-hdrext/abs-send-time"",""direction"":null,""extensionAttributes"":null},{""entry"":4,""extensionName"":""urn:3gpp:video-orientation"",""direction"":null,""extensionAttributes"":null},{""entry"":5,""extensionName"":""http://www.ietf.org/id/draft-holmer-rmcat-transport-wide-cc-extensions-01"",""direction"":null,""extensionAttributes"":null},{""entry"":6,""extensionName"":""http://www.webrtc.org/experiments/rtp-hdrext/playout-delay"",""direction"":null,""extensionAttributes"":null}],""rtcpMux"":true,""rtcpMuxOnly"":null,""rtcpRsize"":true,""rtcp"":{""port"":""9"",""netType"":""IN"",""addressType"":""IP4"",""address"":""0.0.0.0""},""msids"":[],""imageattr"":[],""rids"":[],""simulcast"":null,""sctpPort"":null,""maxMessageSize"":null,""unrecognized"":[],""setup"":""active"",""payloads"":[{""rtpMap"":{""encodingName"":""VP8"",""clockRate"":90000,""encodingParameters"":0},""fmtp"":{""parameters"":{}},""rtcpFeedbacks"":[{""type"":""goog-remb"",""parameter"":null,""additional"":null,""interval"":null},{""type"":""transport-cc"",""parameter"":null,""additional"":null,""interval"":null},{""type"":""ccm"",""parameter"":""fir"",""additional"":null,""interval"":null},{""type"":""nack"",""parameter"":null,""additional"":null,""interval"":null},{""type"":""nack"",""parameter"":""pli"",""additional"":null,""interval"":null}],""payloadType"":96},{""rtpMap"":{""encodingName"":""VP9"",""clockRate"":90000,""encodingParameters"":0},""fmtp"":{""parameters"":{""x-google-profile-id"":""0""}},""rtcpFeedbacks"":[{""type"":""goog-remb"",""parameter"":null,""additional"":null,""interval"":null},{""type"":""transport-cc"",""parameter"":null,""additional"":null,""interval"":null},{""type"":""ccm"",""parameter"":""fir"",""additional"":null,""interval"":null},{""type"":""nack"",""parameter"":null,""additional"":null,""interval"":null},{""type"":""nack"",""parameter"":""pli"",""additional"":null,""interval"":null}],""payloadType"":98}],""rtcpFeedbackWildcards"":[],""ssrcGroups"":[],""xGoogleFlag"":null,""connection"":null}}";

        /// <summary>
        /// The Agora AP service address to connect to
        /// </summary>
        private string[] APlist = new string[] {
            "webrtc2-ap-web-1.agora.io",
            "webrtc2-ap-web-2.agoraio.cn",
            "webrtc2-ap-web-3.agora.io",
            "webrtc2-ap-web-4.agoraio.cn"
        };

        /// <summary>
        /// The Websocket for vosweb
        /// </summary>
        private ClientWebSocket ClientWss;

        private SessionDescription LocalSDP, RemoteSDP;
        private int LocalSDPAudioIndex = 0, LocalSDPVideoIndex = 1;
        private int Opid = 133;
        private int Flag = 4096;
        private string Pid = Guid.NewGuid().ToString();
        private string Sid;
        private int P2P_id;

        private string Ticket;

        private float PollTimeMs = 3000f;
        private float TimeSincePollMs = 0f;

        private APRspMessage APRsp;
        private VOSRspJoinMessage VOSRspJoin;
        private VOSRspPublishMessage VOSRspPublish;

        private string PubId;
        private string UnPubVideoId;
        private string UnPubAudioId;
        private string SubAudioId;
        private string SubVideoId;
        private string JoinId;
        private string LeaveId;
        private string PingId;
        private string PingBackId;
        private string MuteId;

        private bool LastRecvCompleted = true;

        private long LastMsecs;

        private long CmdID = 0;

        private Task SDPTask;

        #endregion

        #region Actions

        public void ActionPublish(bool action)
        {
            Publish = action;
        }

        public void ActionMuteAudio(bool action)
        {
            MuteAudio = action;
        }

        public void ActionMuteVideo(bool action)
        {
            MuteVideo = action;
        }

        #endregion

        #region prviate functions

        private void DebugLog(string message)
        {
            if (AutoLogErrors)
            {
                Debug.Log($"[{LocalPeerId}] " + message);
            }
        }

        private IEnumerator APRequest()
        {
            DebugLog("APConnect start");
            Sid = Guid.NewGuid().ToString("N");
            var rd = new System.Random();
            P2P_id = rd.Next();

            // Get VOSWeb
            for (int i = 0; i < APlist.Length; i++) {
                var addr = APlist[i];
                APReqMessage req = new APReqMessage() {
                    opid = Opid,
                    flag = Flag,
                    uid = Convert.ToInt64(LocalPeerId),
                    ts = Convert.ToInt64(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds),
                    key = Token == null || Token == "" ? AppID : Token,
                    cname = ChannelName,
                    sid = Sid,
                    detail = new Dictionary<string, string>(),
                };
                string url = $"https://{addr}/api/v1";
                byte[] databyte = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(req));
                DebugLog("POST: " + url);
                DebugLog(JsonConvert.SerializeObject(req));
                var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
                request.certificateHandler = new BypassCertificate();
                request.uploadHandler = new UploadHandlerRaw(databyte);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json;charset=utf-8");
                request.SetRequestHeader("X-Packet-URI", "69");
                request.SetRequestHeader("X-Packet-Service-Type", "0");
                yield return request.SendWebRequest();
                if (request.isHttpError || request.isNetworkError)
                {
                    DebugLog($"POST ERROR: {url} {request.error}");
                    continue;
                }

                var json = request.downloadHandler.text;
                DebugLog("POST OVER: " + json);

                APRsp = JsonConvert.DeserializeObject<APRspMessage>(json);

                LocalPeerId = $"{APRsp.uid}";
                State = State.APRequested;
                yield break;
            }

            State = State.Disconnected;
        }

        private IEnumerator VOSConnect()
        {
            DebugLog("VosConnect start");

            List<List<string>> addrs = new List<List<string>>();

            for (int i = 0; i < APRsp.addresses.Count; i++) {
                string ipstr = APRsp.addresses[i].ip.Replace('.', '-');
                int port = System.Convert.ToInt32(APRsp.addresses[i].port);
                string[] valuesio = {
                    $"wss://{ipstr}.edge.agora.io:{port}",
                    APRsp.addresses[i].ticket};
                addrs.Add(new List<string>(valuesio));

                string[] valuescn = {
                    $"wss://{ipstr}.edge.agora.cn:{port}",
                    APRsp.addresses[i].ticket};
                addrs.Add(new List<string>(valuescn));

            }

            foreach (var addr in addrs) {
                string url = addr[0];
                DebugLog("url:\n" + url);
                System.Net.WebSockets.ClientWebSocket cln = new System.Net.WebSockets.ClientWebSocket();
                var task = cln.ConnectAsync(new Uri(url), new CancellationTokenSource(3000).Token);
                yield return new WaitUntil(() => task.IsCompleted || task.IsCanceled || task.IsFaulted);

                DebugLog($"VOS Client state: {cln.State}");

                if (cln.State != WebSocketState.Open)
                    continue;

                ClientWss = cln;
                Ticket = addr[1];

                DebugLog("VOSConnected\n");

                State = State.VOSConnected;
                yield break;
            }

            State = State.APRequested;
        }

        private List<PayloadAttribute> ConvertFeedback(List<PayloadAttribute> payloads)
        {
            var newPayloads = JsonConvert.DeserializeObject<List<PayloadAttribute>>(JsonConvert.SerializeObject(payloads));

            for (int i = 0; i < payloads.Count; i++)
            {
                newPayloads[i].rtcpFeedbacks = new List<RTCPFeedback>();
                for (int j = 0; j < payloads[i].rtcpFeedbacks.Count; j++)
                {
                    var fb = payloads[i].rtcpFeedbacks[j];
                    switch (fb.type)
                    {
                        case "trr-int":
                            var feedback = new TRRINTFeedback(fb.type, fb.interval)
                            {
                                parameter = fb.parameter,
                                additional = fb.additional,
                            };
                            newPayloads[i].rtcpFeedbacks.Add(feedback);
                            break;
                        case "ack":
                        case "nack":
                        default:
                            RTCPACKCommmonFeedback feedback1;

                            if (fb.type == "ack")
                            {
                                feedback1 = new ACKFeedback(fb.type);
                            }
                            else if (fb.type == "nack")
                            {
                                feedback1 = new NACKFeedback(fb.type);
                            }
                            else
                            {
                                feedback1 = new OtherFeedback(fb.type);
                            }

                            feedback1.parameter = fb.parameter;
                            feedback1.additional = fb.additional;
                            newPayloads[i].rtcpFeedbacks.Add(feedback1);
                            break;

                    }
                }
            }
            return newPayloads;
        }

        private MediaDescription RemoteMediaCreate(bool isAudio, Direction direction, string mid)
        {
            DebugLog($"RemoteMediaCreate, isAudio:{isAudio}, direct:{direction}, mid:{mid}");

            string json = isAudio ? DefaultAudioMediaJson : DefaultVideoMediaJson;
            MediaDescription mediaDesc = JsonConvert.DeserializeObject<MediaDescription>(json);

            var codecs = isAudio ? VOSRspJoin._message.ortc.rtpCapabilities.audioCodecs : VOSRspJoin._message.ortc.rtpCapabilities.videoCodecs;

            mediaDesc.media.fmts = new List<string>();
            foreach (var codec in codecs)
                mediaDesc.media.fmts.Add($"{codec.payloadType}");

            mediaDesc.attributes.candidates = VOSRspJoin._message.ortc.ice.candidates;
            for (int i = 0; i < mediaDesc.attributes.candidates.Count; i++)
                mediaDesc.attributes.candidates[i].componentId = "1";

            if (isAudio && (VOSRspJoin._message.ortc.rtpCapabilities.audioExtensions != null))
                mediaDesc.attributes.extmaps = VOSRspJoin._message.ortc.rtpCapabilities.audioExtensions;
            if (!isAudio && (VOSRspJoin._message.ortc.rtpCapabilities.videoExtensions != null))
                mediaDesc.attributes.extmaps = VOSRspJoin._message.ortc.rtpCapabilities.videoExtensions;

            mediaDesc.attributes.payloads = ConvertFeedback(codecs);
            mediaDesc.attributes.iceUfrag = VOSRspJoin._message.ortc.ice.iceUfrag;
            mediaDesc.attributes.icePwd = VOSRspJoin._message.ortc.ice.icePwd;
            mediaDesc.attributes.fingerprints = VOSRspJoin._message.ortc.dtls.fingerprints;
            mediaDesc.attributes.direction = direction;
            mediaDesc.attributes.ssrcs = new List<SSRC>();
            mediaDesc.attributes.mid = mid;

            string setup = "active";
            switch (VOSRspJoin._message.ortc.dtls.role)
            {
                case "client":
                    setup = "active";
                    break;
                case "server":
                    setup = "passive";
                    break;
                case "auto":
                    setup = "actpass";
                    break;
            }
            mediaDesc.attributes.setup = setup;

            return mediaDesc;
        }

        private SessionDescription RemoteSDPCreate()
        {
            Parser parser = new Parser();
            var sdp = parser.Parse(DefaultSDP);

            for (int i = 0; i < LocalSDP.mediaDescriptions.Count; i++)
            {
                var media = LocalSDP.mediaDescriptions[i];
                sdp.mediaDescriptions.Add(RemoteMediaCreate(media.media.mediaType == "audio", Direction.inactive, media.attributes.mid));
            }

            DebugLog("Remote SDP Created");
            return sdp;
        }

        private async Task RemoteSDPSet(SessionDescription sdp, Microsoft.MixedReality.WebRTC.SdpMessageType type)
        {
            try
            {
                if (SDPTask != null)
                    await SDPTask;

                DebugLog($"Remote SDP type:{type}");

                string sdpjson = JsonConvert.SerializeObject(sdp);
                DebugLog("Remote SDP json:\n" + sdpjson);

                string sdpstr = Printer.Print(sdp) + "\n";
                DebugLog("Remote SDP content:\n" + sdpstr);

                var sdpMessage = new Microsoft.MixedReality.WebRTC.SdpMessage { Type = type, Content = sdpstr };
                SDPTask = PC.HandleConnectionMessageAsync(sdpMessage).ContinueWith((Task t) =>
                {
                    if (type == Microsoft.MixedReality.WebRTC.SdpMessageType.Offer)
                        PC.Peer.CreateAnswer();
                });
            }
            catch (Exception ex)
            {
                string ss = ex.ToString();
                DebugLog("RemoteSDPSet error:\n" + ss);
            }
        }

        private async void JoinAsync(bool reJoin)
        {
            DebugLog("Join start");

            if (ClientWss.State != WebSocketState.Open)
            {
                DebugLog("Vos Not Connected");
                State = State.APRequested;
                return;
            }

            try
            {
                if (LocalSDP == null)
                {
                    var t = Task.Run(() =>
                    {
                        while (LocalSDP == null)
                            Thread.Sleep(100);
                    });
                    await t;
                }

                VOSReqJoinMessage joinReq = new VOSReqJoinMessage()
                {
                    _id = $"{CmdID++}",
                    _type = "join_v3",
                    _message = new VOSReqJoinMessage.Message
                    {
                        p2p_id = P2P_id,
                        session_id = Sid,
                        app_id = AppID,
                        channel_key = Token == null || Token == "" ? AppID : Token,
                        channel_name = ChannelName,
                        sdk_version = "0.1.0",
                        browser = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.93 Safari/537.36",
                        process_id = Pid,
                        mode = "live",
                        codec = "h264",
                        role = Publish ? "host" : "client",
                        has_changed_gateway = false,
                        ap_response = new VOSReqJoinMessage.Message.APResponse
                        {
                            code = APRsp.code,
                            flag = APRsp.flag,
                            opid = APRsp.opid,
                            cid = APRsp.cid,
                            uid = APRsp.uid,
                            server_ts = APRsp.server_ts,
                            cname = APRsp.cname,
                            detail = APRsp.detail,
                            ticket = Ticket,
                        },
                        extends = "",
                        details = new Dictionary<string, string>(),
                        features = new VOSReqJoinMessage.Message.Features { rejoin = reJoin },
                        ortc = new VOSReqJoinMessage.Message.Sdp()
                        {
                            iceParameters = new VOSReqJoinMessage.Message.Sdp.IceParameters()
                            {
                                iceUfrag = LocalSDP.mediaDescriptions[0].attributes.iceUfrag,
                                icePwd = LocalSDP.mediaDescriptions[0].attributes.icePwd,
                            },
                            dtlsParameters = new VOSReqJoinMessage.Message.Sdp.DtlsParameters()
                            {
                                fingerprints = new List<FingerPrint>(LocalSDP.mediaDescriptions[0].attributes.fingerprints),
                            },
                            rtpCapabilities = new VOSReqJoinMessage.Message.Sdp.RtpCapabilities()
                            {
                                audioCodecs = new List<PayloadAttribute>(LocalSDP.mediaDescriptions[LocalSDPAudioIndex].attributes.payloads),
                                videoCodecs = new List<PayloadAttribute>(LocalSDP.mediaDescriptions[LocalSDPVideoIndex].attributes.payloads),
                                audioExtensions = new List<Extmap>(LocalSDP.mediaDescriptions[LocalSDPAudioIndex].attributes.extmaps),
                                videoExtensions = new List<Extmap>(LocalSDP.mediaDescriptions[LocalSDPVideoIndex].attributes.extmaps),
                            }
                        }
                    }
                };
                JoinId = joinReq._id;
                string join = JsonConvert.SerializeObject(joinReq);
                DebugLog($"VOS Client Send: \n" + join);
                await ClientWss.SendAsync(new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes(join)), WebSocketMessageType.Text, true, new CancellationTokenSource(3000).Token);
            }
            catch (Exception ex)
            {
                string ss = ex.ToString();
                DebugLog("VosJoin error:\n" + ss);
                State = State.APRequested;
                return;
            }
        }

        private async Task LeaveAsync()
        {
            DebugLog("Leave start");

            if (ClientWss == null || ClientWss.State != WebSocketState.Open)
            {
                DebugLog("Vos Not Connected");
                State = State.APRequested;
                return;
            }

            try
            {
                VOSReqLeaveMessage req = new VOSReqLeaveMessage()
                {
                    _id = $"{CmdID++}",
                    _type = "leave",
                };
                LeaveId = req._id;
                string data = JsonConvert.SerializeObject(req);
                DebugLog($"VOS Client Send: \n" + data);
                await ClientWss.SendAsync(new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes(data)), WebSocketMessageType.Text, true, new CancellationTokenSource(3000).Token);
            }
            catch (Exception ex)
            {
                string ss = ex.ToString();
                DebugLog("VosLeave error:\n" + ss);
                return;
            }
        }

        private async void MuteAsync(string Action)
        {
            DebugLog("Mute start");

            if (ClientWss == null || ClientWss.State != WebSocketState.Open)
            {
                DebugLog("Vos Not Connected");
                State = State.APRequested;
                return;
            }

            try
            {
                VOSReqMuteMessage req = new VOSReqMuteMessage()
                {
                    _id = $"{CmdID++}",
                    _type = "control",
                    _message = new VOSReqMuteMessage.Message() {
                        action = Action,
                        p2p_id = P2P_id,
                        ortc = new List<VOSReqMuteMessage.Message.Sdp>(),
                        stream_id = Convert.ToInt64(LocalPeerId),
                    },
                };

                int mediaIndex = LocalSDPAudioIndex;
                string stream_type = "audio";
                if (Action == "mute_local_video" || Action == "unmute_local_video")
                {
                    mediaIndex = LocalSDPVideoIndex;
                    stream_type = "high";
                }

                req._message.ortc.Add(new VOSReqMuteMessage.Message.Sdp() {
                    stream_type = stream_type,
                    mid = LocalSDP.mediaDescriptions[mediaIndex].attributes.mid,
                    ssrcs = new List<VOSReqMuteMessage.Message.Sdp.SSRCID>(),
                });
                for (int i = 0; i < LocalSDP.mediaDescriptions[mediaIndex].attributes.ssrcs.Count; i++)
                    req._message.ortc[0].ssrcs.Add(new VOSReqMuteMessage.Message.Sdp.SSRCID(){
                        ssrcId = LocalSDP.mediaDescriptions[mediaIndex].attributes.ssrcs[i].ssrcId,
                    });

                MuteId = req._id;
                string data = JsonConvert.SerializeObject(req);
                DebugLog($"VOS Client Send: \n" + data);
                await ClientWss.SendAsync(new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes(data)), WebSocketMessageType.Text, true, new CancellationTokenSource(3000).Token);
            }
            catch (Exception ex)
            {
                string ss = ex.ToString();
                DebugLog("VosLeave error:\n" + ss);
                return;
            }
        }

        private async void PublishAsync()
        {
            DebugLog("Publish start");

            if (ClientWss.State != WebSocketState.Open)
            {
                DebugLog("Vos Not Connected");
                State = State.APRequested;
                return;
            }

            try
            {
                for (int i = 0; i < RemoteSDP.mediaDescriptions.Count; i++)
                    if (RemoteSDP.mediaDescriptions[i].attributes.direction == Direction.inactive)
                        RemoteSDP.mediaDescriptions[i].attributes.direction = Direction.recvonly;
                    else if (RemoteSDP.mediaDescriptions[i].attributes.direction == Direction.sendonly)
                        RemoteSDP.mediaDescriptions[i].attributes.direction = Direction.sendrecv;
                LocalSDP = null;
                await RemoteSDPSet(RemoteSDP, Microsoft.MixedReality.WebRTC.SdpMessageType.Offer);
                if (LocalSDP == null)
                {
                    var t = Task.Run(() =>
                    {
                        while (LocalSDP == null)
                            Thread.Sleep(100);
                    });
                    await t;
                }

                VOSReqPublishMessage publishReq = new VOSReqPublishMessage()
                {
                    _id = $"{CmdID++}",
                    _type = "publish",
                    _message = new VOSReqPublishMessage.Message
                    {
                        state = "offer",
                        p2p_id = P2P_id,
                        ortc = new List<VOSReqPublishMessage.Message.Sdp> { },
                        mode = "live",
                        extend = ""
                    }
                };
                PubId = publishReq._id;

                var sdpAudio = new VOSReqPublishMessage.Message.Sdp
                {
                    stream_type = "audio",
                    attributes = new VOSReqPublishMessage.Message.Sdp.Attributes
                    {
                        dtx = false,
                        hq = false,
                        lq = false,
                        stereo = false,
                        speech = false
                    },
                    ssrcs = new List<SSRC>(LocalSDP.mediaDescriptions[LocalSDPAudioIndex].attributes.ssrcs),
                    mid = LocalSDP.mediaDescriptions[LocalSDPAudioIndex].attributes.mid
                };
                publishReq._message.ortc.Add(sdpAudio);

                var videoDevices = await PeerConnection.GetVideoCaptureDevicesAsync();
                if (videoDevices.Count > 0)
                {
                    var formats = await Microsoft.MixedReality.WebRTC.DeviceVideoTrackSource.GetCaptureFormatsAsync(videoDevices[0].id);
                    if (formats.Count > 0)
                    {
                        var sdpVideo = new VOSReqPublishMessage.Message.Sdp
                        {
                            stream_type = "high",
                            attributes = new VOSReqPublishMessage.Message.Sdp.Attributes
                            {
                                resolution = $"{formats[0].width}x{formats[0].height}",
                                maxVideoBW = 5000,
                                minVideoBW = 100,
                                maxFrameRate = Convert.ToInt32(formats[0].framerate),
                                minFrameRate = Convert.ToInt32(formats[0].framerate),
                                codec = "h264"
                            },
                            ssrcs = new List<SSRC>(LocalSDP.mediaDescriptions[LocalSDPVideoIndex].attributes.ssrcs),
                            mid = LocalSDP.mediaDescriptions[LocalSDPVideoIndex].attributes.mid
                        };
                        publishReq._message.ortc.Add(sdpVideo);
                    }
                }
                string publish = JsonConvert.SerializeObject(publishReq);
                DebugLog($"VOS Client Send: \n" + publish);
                await ClientWss.SendAsync(new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes(publish)), WebSocketMessageType.Text, true, new CancellationTokenSource(3000).Token);
            }
            catch (Exception ex)
            {
                string ss = ex.ToString();
                DebugLog("VosPublish error:\n" + ss);
                return;
            }
        }

        private async void UnPublishAsync(string type)
        {
            DebugLog("UnPublish start");

            if (ClientWss.State != WebSocketState.Open)
            {
                DebugLog("Vos Not Connected");
                State = State.APRequested;
                return;
            }

            try
            {
                VOSReqUnPublishMessage req = new VOSReqUnPublishMessage()
                {
                    _id = $"{CmdID++}",
                    _type = "unpublish",
                    _message = new VOSReqUnPublishMessage.Message()
                    {
                        stream_id = LocalPeerId,
                        ortc = new List<VOSReqUnPublishMessage.Message.Sdp>(),
                    },
                };

                int mediaIndex = type == "audio" ? LocalSDPAudioIndex : LocalSDPVideoIndex;
                req._message.ortc.Add(new VOSReqUnPublishMessage.Message.Sdp()
                {
                    stream_type = type,
                    mid = LocalSDP.mediaDescriptions[mediaIndex].attributes.mid,
                    ssrcs = new List<VOSReqUnPublishMessage.Message.Sdp.SSRCID>(),
                });
                for (int i = 0; i < LocalSDP.mediaDescriptions[mediaIndex].attributes.ssrcs.Count; i++)
                    req._message.ortc[0].ssrcs.Add(new VOSReqUnPublishMessage.Message.Sdp.SSRCID()
                    {
                        ssrcId = LocalSDP.mediaDescriptions[mediaIndex].attributes.ssrcs[i].ssrcId,
                    });

                if (type == "audio")
                    UnPubAudioId = req._id;
                else
                    UnPubVideoId = req._id;

                string data = JsonConvert.SerializeObject(req);
                DebugLog($"VOS Client Send: \n" + data);
                await ClientWss.SendAsync(new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes(data)), WebSocketMessageType.Text, true, new CancellationTokenSource(3000).Token);
            }
            catch (Exception ex)
            {
                string ss = ex.ToString();
                DebugLog("UnPublish error:\n" + ss);
                return;
            }
        }

        private async void Subscribe(VOSNotifyMessage notify)
        {
            if (RemotePeerId != null && RemotePeerId != "0" && RemotePeerId != $"{notify._message.uid}")
                return;

            if (ClientWss.State != WebSocketState.Open)
            {
                DebugLog("Vos Not Connected");
                State = State.APRequested;
                return;
            }

            try
            {
                var rd = new System.Random();
                string label = $"track-{rd.Next().ToString()}";

                VOSReqSubscribeMessage sub = new VOSReqSubscribeMessage()
                {
                    _id = $"{CmdID++}",
                    _type = "subscribe",
                    _message = new VOSReqSubscribeMessage.Message()
                    {
                        stream_id = notify._message.uid,
                        stream_type = notify._type == "on_add_audio_stream" ? "audio" : "video",
                        mode = "live",
                        codec = notify._type == "on_add_audio_stream" ? "opus" : "h264",
                        p2p_id = P2P_id,
                        tcc = true,
                        extend = "",
                        ssrcId = notify._message.ssrcId,
                    },
                };

                string subscribe = JsonConvert.SerializeObject(sub);
                DebugLog($"VOS Client Send: \n" + subscribe);
                await ClientWss.SendAsync(new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes(subscribe)), WebSocketMessageType.Text, true, new CancellationTokenSource(3000).Token);
                if (notify._type == "on_add_audio_stream")
                    SubAudioId = sub._id;
                else
                    SubVideoId = sub._id;

                Dictionary<string, string> attributes = new Dictionary<string, string>();
                attributes.Add("cname", $"{notify._message.cname}");
                attributes.Add("msid", $"{notify._message.uid} {label}");
                attributes.Add("mslabel", $"{notify._message.uid}");
                attributes.Add("label", label);

                for (int i = 0; i < RemoteSDP.mediaDescriptions.Count; i++)
                {
                    var media = RemoteSDP.mediaDescriptions[i];
                    if ((media.media.mediaType == "audio") == (notify._type == "on_add_audio_stream"))
                    {
                        media.attributes.ssrcs.Clear();
                        media.attributes.ssrcs.Add(new SSRC(notify._message.ssrcId, attributes));
                        if (media.attributes.direction == Direction.inactive)
                            media.attributes.direction = Direction.sendonly;
                        else if (media.attributes.direction == Direction.recvonly)
                            media.attributes.direction = Direction.sendrecv;
                        break;
                    }
                }

                await RemoteSDPSet(RemoteSDP, Microsoft.MixedReality.WebRTC.SdpMessageType.Offer);
            }
            catch (Exception ex)
            {
                string ss = ex.ToString();
                DebugLog("VosSubscribe error:\n" + ss);
                return;
            }
        }

        private async void VOSPing()
        {
            if (ClientWss.State != WebSocketState.Open)
            {
                DebugLog("Vos Not Connected");
                State = State.APRequested;
                return;
            }

            try
            {
                VOSReqPingMessage req = new VOSReqPingMessage()
                {
                    _id = $"{CmdID++}",
                    _type = "ping",
                };

                string reqdata = JsonConvert.SerializeObject(req);
                DebugLog($"VOS Client Send: \n" + reqdata);
                await ClientWss.SendAsync(new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes(reqdata)), WebSocketMessageType.Text, true, new CancellationTokenSource(3000).Token);
                LastMsecs = Convert.ToInt64(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds);
                PingId = req._id;
            }
            catch (Exception ex)
            {
                string ss = ex.ToString();
                DebugLog("VosPing error:\n" + ss);
                return;
            }
        }

        private async void VOSPingBack(long interMsecs)
        {
            if (ClientWss.State != WebSocketState.Open)
            {
                DebugLog("Vos Not Connected");
                State = State.APRequested;
                return;
            }

            try
            {
                VOSReqPingBackMessage req = new VOSReqPingBackMessage()
                {
                    _id = $"{CmdID++}",
                    _type = "ping_back",
                    _message = new VOSReqPingBackMessage.Message()
                    {
                        pingpongElapse = interMsecs,
                    },
                };

                string reqdata = JsonConvert.SerializeObject(req);
                DebugLog($"VOS Client Send: \n" + reqdata);
                await ClientWss.SendAsync(new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes(reqdata)), WebSocketMessageType.Text, true, new CancellationTokenSource(3000).Token);
                PingBackId = req._id;
            }
            catch (Exception ex)
            {
                string ss = ex.ToString();
                DebugLog("VosPingBack error:\n" + ss);
                return;
            }
        }

        private IEnumerator VOSRecv()
        {
            if (ClientWss.State != WebSocketState.Open)
            {
                DebugLog("Vos Not Connected");
                State = State.APRequested;
                yield break;
            }

            var buffer = new byte[10240];

            var task = ClientWss.ReceiveAsync(new ArraySegment<byte>(buffer), new CancellationToken());
            yield return new WaitUntil(() => task.IsCompleted || task.IsCanceled || task.IsFaulted);

            if (task.IsCanceled || task.IsFaulted || task.Result.Count <= 0)
                yield break;

            string recvData = System.Text.Encoding.UTF8.GetString(buffer);

            VOSRecvMessage recvMsg = JsonConvert.DeserializeObject<VOSRecvMessage>(recvData);
            if (recvMsg != null && recvMsg._id != null)
            {
                if (recvMsg._id == JoinId)
                {
                    VOSRspJoin = JsonConvert.DeserializeObject<VOSRspJoinMessage>(recvData);
                    DebugLog("JOIN RSP:\n" + JsonConvert.SerializeObject(VOSRspJoin));
                    RemoteSDP = RemoteSDPCreate();
                    var taskSet = RemoteSDPSet(RemoteSDP, Microsoft.MixedReality.WebRTC.SdpMessageType.Answer);
                    yield return new WaitUntil(() => taskSet.IsCompleted || taskSet.IsCanceled || taskSet.IsFaulted);
                    State = State.Joined;
                }
                else if (recvMsg._id == LeaveId)
                {
                    DebugLog("LEAVE RSP:\n" + recvData);
                    var taskClose = ClientWss.CloseAsync(WebSocketCloseStatus.NormalClosure, "Leave Channel", new CancellationToken());
                    yield return new WaitUntil(() => taskClose.IsCompleted || taskClose.IsCanceled || taskClose.IsFaulted);
                    State = State.APRequested;
                }
                else if (recvMsg._id == PubId)
                {
                    VOSRspPublish = JsonConvert.DeserializeObject<VOSRspPublishMessage>(recvData);
                    DebugLog("PUBLISH RSP:\n" + JsonConvert.SerializeObject(VOSRspPublish));
                }
                else if (recvMsg._id == SubAudioId || recvMsg._id == SubVideoId)
                {
                    DebugLog("SUBSCRIBE RSP:\n" + recvData);
                }
                else if (recvMsg._id == UnPubAudioId || recvMsg._id == UnPubVideoId)
                {
                    DebugLog("UNPUBLISH RSP:\n" + recvData);
                }
                else if (recvMsg._id == PingId)
                {
                    var pingrsp = JsonConvert.DeserializeObject<VOSRspPingMessage>(recvData);
                    long interMsecs = Convert.ToInt64(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds) - LastMsecs;
                    DebugLog("PING RSP:\n" + recvData);
                    VOSPingBack(interMsecs);
                }
                else if (recvMsg._id == MuteId)
                {
                    DebugLog("MUTE RSP:\n" + recvData);
                }
                else
                {
                    DebugLog("RECV UNKNOWN:\n" + recvData);
                }
            }
            else if (recvMsg != null && recvMsg._type != null)
            {
                VOSNotifyMessage notify = JsonConvert.DeserializeObject<VOSNotifyMessage>(recvData);
                switch (notify._type)
                {
                    case "on_user_online":
                    case "on_user_offline":
                        if (!Main)
                            break;
                        DebugLog($"USER NOTIFY: {notify._type} {notify._message.uid}");
                        if (notify._type == "on_user_online")
                            OnUserOnline.Invoke($"{notify._message.uid}");
                        else
                            OnUserOffline.Invoke($"{notify._message.uid}");
                        break;
                    case "on_add_audio_stream":
                    case "on_add_video_stream":
                        if (Main)
                            break;
                        DebugLog("STREAM ADD NOTIFY:\n" + recvData);
                        Subscribe(notify);
                        break;
                    case "on_remove_stream":
                        if (Main)
                            break;
                        DebugLog("STREAM REMOVE NOTIFY:\n" + recvData);
                        if (RemoteSDP.mediaDescriptions != null)
                        {
                            for (int i = RemoteSDP.mediaDescriptions.Count - 1; i >= 0; i--)
                            {
                                var media = RemoteSDP.mediaDescriptions[i];
                                if (media.attributes.ssrcs != null)
                                {
                                    for (int j = media.attributes.ssrcs.Count - 1; j >= 0; j--)
                                    {
                                        if (media.attributes.ssrcs[j].attributes["mslabel"] == $"{notify._message.uid}")
                                        {
                                            media.attributes.ssrcs.Remove(media.attributes.ssrcs[j]);
                                            media.attributes.direction = Direction.inactive;
                                        }

                                    }
                                }
                            }
                        }
                        break;
                    default:
                        DebugLog("UNKNOWN NOTIFY:\n" + recvData);
                        break;
                }
            }
            else
            {
                DebugLog("RECV UNKNOWN:\n" + recvData);
            }
            LastRecvCompleted = true;
        }

        #endregion

        #region inherit interface

        /// <summary>
        /// Helper to split SDP offer and answer messages and dispatch to the appropriate handler.
        /// </summary>
        /// <param name="message">The SDP message ready to be sent to the remote peer.</param>
        private void OnLocalSdpReadyToSend_Listener(Microsoft.MixedReality.WebRTC.SdpMessage message)
        {
            Task.Run(() =>
            {
                Parser parser = new Parser();
                var sdp = parser.Parse(message.Content);

                for (int i = 0; i < sdp.mediaDescriptions.Count; i++)
                {
                    if (sdp.mediaDescriptions[i].media.mediaType == "video")
                        LocalSDPVideoIndex = i;
                    else if (sdp.mediaDescriptions[i].media.mediaType == "audio")
                        LocalSDPAudioIndex = i;
                    else
                        DebugLog("INVALID MEDIA TYPE");
                }

                LocalSDP = sdp;
            });

            DebugLog($"Local SDP Create, type:{message.Type} content:\n{message.Content}");
        }

        private void OnPeerInitialized()
        {
            // Register handlers for the SDP events
            PC.Peer.LocalSdpReadytoSend += OnLocalSdpReadyToSend_Listener;
            State = State.Disconnected;
        }

        private void OnEnable()
        {
            PC.OnInitialized.AddListener(OnPeerInitialized);
        }

        private async void OnDisable()
        {
            PC.OnInitialized.RemoveListener(OnPeerInitialized);
            await LeaveAsync().ContinueWith(async (Task t) => {
                State = State.Start;
                if (ClientWss != null && ClientWss.State == WebSocketState.Open)
                    await ClientWss.CloseAsync(WebSocketCloseStatus.NormalClosure, "Leave Channel", new CancellationToken());
                ClientWss = null;
            });
        }

        /// <summary>
        /// Unity Engine Update() hook
        /// </summary>
        /// <remarks>
        /// https://docs.unity3d.com/ScriptReference/MonoBehaviour.Update.html
        /// </remarks>
        private void Update()
        {
            switch (State)
            {
                case State.Disconnected:
                    State = State.Busy;
                    StartCoroutine(APRequest());
                    break;
                case State.APRequested:
                    State = State.Busy;
                    StartCoroutine(VOSConnect());
                    break;
                case State.VOSConnected:
                    State = State.Busy;
                    PC.StartConnection();
                    JoinAsync(false);
                    break;
                case State.Joined:
                    if (Publish != Published)
                    {
                        Published = Publish;
                        if (Publish == true)
                            PublishAsync();
                        else
                        {
                            UnPublishAsync("high");
                            UnPublishAsync("audio");
                        }
                        break;
                    }
                    if (MuteAudio != AudioMuted)
                    {
                        AudioMuted = MuteAudio;
                        MuteAsync(MuteAudio == true ? "mute_local_audio" : "unmute_local_audio");
                        break;
                    }
                    if (MuteVideo != VideoMuted)
                    {
                        VideoMuted = MuteVideo;
                        MuteAsync(MuteVideo == true ? "mute_local_video" : "unmute_local_video");
                        break;
                    }
                    break;
                default:
                    break;
            }

            if (ClientWss != null && ClientWss.State == WebSocketState.Open)
            {
                if (LastRecvCompleted)
                {
                    LastRecvCompleted = false;
                    StartCoroutine(VOSRecv());
                }
                TimeSincePollMs += Time.deltaTime * 1000.0f;
                if (TimeSincePollMs > PollTimeMs)
                {
                    TimeSincePollMs = 0f;
                    VOSPing();
                    DebugLog($"State:{State}");
                }
            }
        }

        #endregion
    }
}

