//
// AttributeTypes.cs
//
// Author:
//       Rick Cheng <rick@agora.io>
//
// Copyright (c) 2021 
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System.Collections.Generic;

namespace io.agora.sdp
{

    public class Group
    {
        public string semantic { get; set; }
        public IList<string> identificationTag { get; set; }

        public Group(string sem, IList<string> idTags) {
            semantic = sem;
            identificationTag = idTags;
	    }
    }

    public class FingerPrint
    {
        public string algorithm { get; set; }
        public string fingerprint { get; set; }

        public FingerPrint(string hash, string fp)
        {
            algorithm = hash;
            fingerprint = fp;
	    }
    }

    //public enum Setup // "active" | "passive" | "actpass" | "holdconn";
    //{
    //    active,
    //    passive,
    //    actpass,
    //    holdconn
    //}

    public class Extension
    {
        public string name { get; set; }
        public string value { get; set; }
        public Extension(string n, string v)
        {
            name = n; value = v;
        }
    }

    public class Identity
    {
        public string assertionValue { get; set; }
        public IList<Extension> extensions { get; set; }

        public Identity(string av, IList<Extension> ext)
        {
            assertionValue = av;
            extensions = ext;
	    }
    }

    public class Extmap
    {
        public int entry;
        public string extensionName;
        public string direction;
        public string extensionAttributes;
    }

    public class Candidate
    {
        public string foundation { get; set; }
        public string componentId { get; set; } = "1";
        public string protocol { get; set; }
        public string priority { get; set; }
        public string ip { get; set; }
        public string port { get; set; }
        public string type { get; set; }
        public string relAddr { get; set; }
        public string relPort { get; set; }
        public IDictionary<string, string> extension { get; set; }

        public Candidate(string found, string com, string tp, string pr, string ca,  string po, string ty, string ra = null, string rp = null,
        IDictionary<string, string> ext = null)
        {
            foundation = found;
            componentId = com;
            protocol = tp;
            priority = pr;
            ip = ca;
            port = po;
            type = ty;
            relAddr = ra;
            relPort = rp;
            
            extension = ext?? new Dictionary<string, string>();
	    }
    }

    public class RemoteCandidate
    {
        public string componentId { get; set; }
        public string ip { get; set; }
        public string port { get; set; }

        public RemoteCandidate(string cid, string caddr, string po)
        {
            componentId = cid;
            ip = caddr;
            port = po;
	    }

        public override string ToString()
        {
            return $"{componentId} {ip} {port}";
        }
    }

    public class RTPMap
    {
        // payloadType: string;
        public string encodingName;
        public int clockRate;
        public int encodingParameters;

        public RTPMap(string cname, int crate, int enparams = 0)
        {
            encodingName = cname;
            clockRate = crate;
            encodingParameters = enparams;
        }

    }

    public class Fmtp
    {
        // format: string;
        public IDictionary<string, object> parameters;

        public Fmtp(IDictionary<string, object> paras)
        {
            parameters = paras ?? new Dictionary<string, object>();
	    }
    }

    public enum Direction
    {
        sendrecv,
        sendonly,
        recvonly,
        inactive
    };

    public class SSRC
    {
        public long ssrcId;
        public IDictionary<string, string> attributes;

        public SSRC(long ssrcId, IDictionary<string, string> attributes)
        {
            this.ssrcId = ssrcId;
            this.attributes = attributes?? new Dictionary<string, string>();
	    }
    }

    public class SSRCGroup
    {
        public string semantic;
        public IList<long> ssrcIds;
    }

    public class RTCPFeedback
    {
        public string type { get; set; }
        public string parameter { get; set; }
        public string additional { get; set; }
        public string interval { get; set; }

        public RTCPFeedback(string type) {
            this.type = type;
	    }
    };

    public class RTCPACKCommmonFeedback : RTCPFeedback
    { 
        public RTCPACKCommmonFeedback(string type) : base(type) { }
    }

    public class ACKFeedback : RTCPACKCommmonFeedback
    {
        //public string type { get; set; }
        //public string parameter { get; set; }// "rpsi" | "app" | string;y
        //public string additional { get; set; }
        public ACKFeedback(string type) : base(type) { }
    }

    public class NACKFeedback : RTCPACKCommmonFeedback
    {
        //public string type { get; set; }
        //public string parameter { get; set; }//"pli" | "sli" | "rpsi" | "app" | string
        //public string additional { get; set; }
        public NACKFeedback(string type) : base(type) { }
    }

    public class TRRINTFeedback : RTCPFeedback
    {
        //public string type { get; set; }
        public TRRINTFeedback(string type, string interval) : base(type)
        {
            this.type = type;
            this.interval = interval;
        }

    }

    public class OtherFeedback : RTCPACKCommmonFeedback
    {
        //public string type;
        //public string parameter { get; set; }//"app" | string
        //public string additional { get; set; }
        public OtherFeedback(string type) : base(type) { }
    }

    public class RTCP
    {
        public string port { get; set; }
        public string netType { get; set; }
        public string addressType { get; set; }
        public string address { get; set; }
    }

    public class MSID
    {
        public string id { get; set;  }
        public string appdata { get; set; }
    }

    #region RID classes

    public abstract class RIDParam
    {
        public abstract string type { get; set; }
    }

    public class RIDGenericParam : RIDParam { 
        public override string type { get; set; }
        public string val { get; set; }
    }


    //public class RIDWidthParam : RIDParam
    //{
    //    public override string type { get; } = "max-width";
    //    public string val { get; set; }
    //}

    //public class RIDHeightParam : RIDParam
    //{
    //    public override string type { get; } = "height-width";
    //    public string val { get; set; }
    //}

    //public class RIDFpsParam : RIDParam
    //{
    //    public override string type { get; } = "max-fps";
    //    public string val { get; set; }
    //}

    //public class RIDFsParam : RIDParam
    //{
    //    public override string type { get; } = "max-fs";
    //    public string val { get; set; }
    //}

    //public class RIDBrParam : RIDParam
    //{
    //    public override string type { get; } = "max-br";
    //    public string val { get; set; }
    //}

    //public class RIDPpsParam : RIDParam
    //{
    //    public override string type { get; } = "max-pps";
    //    public string val { get; set; }
    //}

    //public class RIDBppParam : RIDParam
    //{
    //    public override string type { get; } = "max-bpp";
    //    public string val { get; set; }
    //}

    //public class RIDOtherParam : RIDParam
    //{
    //    public override string type { get; } = "other";
    //    public string val { get; set; }
    //}

    public class RIDDependParam : RIDParam
    {
        public override string type { get; set; } = "depend";
        public IList<string> rids { get; set; }

        public RIDDependParam() {
            rids = new List<string>();
	    }
    }


    public class RID
    {
        public string id { get; set; }
        public string direction { get; set; } // "send" | "recv";
        public IList<string> payloads { get; set; }
        public IList<RIDParam> @params { get; set; }

        public RID()
        {
            payloads = new List<string>();
            @params = new List<RIDParam>();
	    }
    }
    #endregion



    public class MsidSemantic
    {
        public string semantic { get; set; }
        public bool? applyForAll { get; set; }
        public IList<string> identifierList { get; set; }

        public MsidSemantic(string sem, IList<string> list)
        {
            semantic = sem;
            identifierList = list;
	    }
    }

    //    public class ExtmapEntry = Record<string, Extmap>;

    public class PayloadAttribute
    {
        public RTPMap rtpMap { get; set; }
        public Fmtp fmtp { get; set; }
        public IList<RTCPFeedback> rtcpFeedbacks { get; set; }
        public int payloadType { get; set; }

        public PayloadAttribute(RTPMap rtpmap, int ptype, Fmtp fm = null) {
            rtpMap = rtpmap;
            fmtp = fm?? new Fmtp( null );
            payloadType = ptype;

            rtcpFeedbacks = new List<RTCPFeedback>();
	    }
    }

    //  public class PayloadMap = Record<string, PayloadAttribute>;
    public interface IAttributes {
         string iceUfrag { get; set; }
         bool? iceLite { get; set; }
         string icePwd { get; set; }
         IList<string> iceOptions { get; set; }
         IList<FingerPrint> fingerprints { get; set; }
         IList<Extmap> extmaps { get; set; }
         string setup { get; set; }
    }
    
    public class SessionAttributes : IAttributes
    {
        public IList<Group> groups { get; set; }
        public bool? iceLite { get; set; }
        public string iceUfrag { get; set; }
        public string icePwd { get; set; }
        public IList<string> iceOptions { get; set; }
        public IList<FingerPrint> fingerprints { get; set; }
        public string setup { get; set; }
        public string tlsId { get; set; }
        public IList<Identity> identities { get; set; }
        public IList<Extmap> extmaps { get; set; }
        public IList<Attribute> unrecognized { get; set; }
        public MsidSemantic msidSemantic { get; set; }
        public bool? extmapAllowMixed { get; set; }

        /// <summary>
        ///   Constructor
        /// </summary>
        public SessionAttributes() {
            groups = new List<Group>();
            iceOptions = new List<string>();
            fingerprints = new List<FingerPrint>();
            identities = new List<Identity>();
            extmaps = new List<Extmap>();
            unrecognized = new List<Attribute>();
	    }
    }

    public class MediaAttributes : IAttributes
    {
        public string mid { get; set; }
        public string iceUfrag { get; set; }
        public string icePwd { get; set; }
        public bool? iceLite { get; set; }
        public IList<string> iceOptions { get; set; }
        public IList<Candidate> candidates { get; set; }
        public IList<RemoteCandidate> remoteCandidatesList { get; set; }
        public bool? endOfCandidates { get; set; }
        public IList<FingerPrint> fingerprints { get; set; }
        //public string // rtpMaps: RTPMap[];
        //public string // fmtp: Fmtp[];
        public string ptime { get; set; }
        public string maxPtime { get; set; }
        public Direction? direction { get; set; }
        public IList<SSRC> ssrcs { get; set; }
        public IList<Extmap> extmaps { get; set; }
        //public string // rtcpFeedbacks: RTCPFeedback[];
        public bool? rtcpMux { get; set; }
        public bool? rtcpMuxOnly { get; set; }
        public bool? rtcpRsize { get; set; }
        public RTCP rtcp { get; set; }
        public IList<MSID> msids { get; set; }
        public IList<string> imageattr { get; set; }
        public IList<RID> rids { get; set; }
        public string simulcast { get; set; }
        public string sctpPort { get; set; }
        public string maxMessageSize { get; set; }
        public IList<Attribute> unrecognized { get; set; }
        public string setup { get; set; }
        public IList<PayloadAttribute> payloads { get; set; }
        public IList<RTCPFeedback> rtcpFeedbackWildcards { get; set; }
        public IList<SSRCGroup> ssrcGroups { get; set; }
        public string xGoogleFlag { get; set; }
        public string connection { get; set; }

        public MediaAttributes()
        {
            iceOptions = new List<string>();
            candidates = new List<Candidate>();
            remoteCandidatesList = new List<RemoteCandidate>();
            fingerprints = new List<FingerPrint>();
            ssrcs = new List<SSRC>();
            extmaps = new List<Extmap>();
            msids = new List<MSID>();
            imageattr = new List<string>();
            rids = new List<RID>();
            unrecognized = new List<Attribute>();
            payloads = new List<PayloadAttribute>();
            rtcpFeedbackWildcards = new List<RTCPFeedback>();
            ssrcGroups = new List<SSRCGroup>();
	    }

    }
}
