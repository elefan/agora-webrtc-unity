//
// MediaAttributeParser.cs
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
using System;
using System.Linq;
using System.Collections.Generic;

namespace io.agora.sdp
{
    public class MediaAttributeParser : AttributeParser
    {
        MediaAttributes _attributes;

        // For Unit Test
        public IAttributes Attributes { get { return _attributes; } }

        protected override IAttributes attributes
        {
            get { return _attributes; }
            set { _attributes = (MediaAttributes)value; }
        }//  SessionAttributes | MediaAttributes;

        public MediaAttributeParser() {
            _attributes = new MediaAttributes();
	    }

        public MediaAttributeParser(Media media)
        {
            _attributes = new MediaAttributes();

            if (media.protos.IndexOf("RTP") != -1 || media.protos.IndexOf("rtp") != -1)
            {
                // Can't tell what the difference is for the TS code
                Console.WriteLine("media attribute has RTP");
            }
        }

        public void parse(Attribute attribute)
        {
            if (this.digested)
            {
                throw new Exception("already digested");
            }

            try
            {
                switch (attribute.attField)
                {
                    case "extmap":
                        this.parseExtmap(attribute);
                        break;
                    case "setup":
                        this.parseSetup(attribute);
                        break;
                    case "ice-lite":
                        this.parseIceLite();
                        break;
                    case "ice-ufrag":
                        this.parseIceUfrag(attribute);
                        break;
                    case "ice-pwd":
                        this.parseIcePwd(attribute);
                        break;
                    case "ice-options":
                        this.parseIceOptions(attribute);
                        break;
                    case "candidate":
                        this.parseCandidate(attribute);
                        break;
                    case "remote-candidates":
                        this.parseRemoteCandidate(attribute);
                        break;
                    case "end-of-candidates":
                        this.parseEndOfCandidates();
                        break;
                    case "fingerprint":
                        this.parseFingerprint(attribute);
                        break;
                    case "rtpmap":
                        this.parseRtpmap(attribute);
                        break;
                    case "ptime":
                        this.parsePtime(attribute);
                        break;
                    case "maxptime":
                        this.parseMaxPtime(attribute);
                        break;
                    case "sendrecv":
                    case "recvonly":
                    case "sendonly":
                    case "inactive":
                        this.parseDirection(attribute);
                        break;
                    case "ssrc":
                        this.parseSSRC(attribute);
                        break;
                    // case "ssrc-group":
                    //   this.parseSSRCGroup(attribute, attributeMap);
                    //   break;
                    case "fmtp":
                        this.parseFmtp(attribute);
                        break;
                    case "rtcp-fb":
                        this.parseRtcpFb(attribute);
                        break;
                    case "rtcp-mux":
                        this.parseRTCPMux();
                        break;
                    case "rtcp-mux-only":
                        this.parseRTCPMuxOnly();
                        break;
                    case "rtcp-rsize":
                        this.parseRTCPRsize();
                        break;
                    case "rtcp":
                        this.parseRTCP(attribute);
                        break;
                    case "mid":
                        this.parseMid(attribute);
                        break;
                    case "msid":
                        this.parseMsid(attribute);
                        break;
                    case "imageattr":
                        this.parseImageAttr(attribute);
                        break;
                    case "rid":
                        this.parseRid(attribute);
                        break;
                    case "simulcast":
                        this.parseSimulcast(attribute);
                        break;
                    case "sctp-port":
                        this.parseSctpPort(attribute);
                        break;
                    case "max-message-size":
                        this.parseMaxMessageSize(attribute);
                        break;
                    case "ssrc-group":
                        this.parseSSRCGroup(attribute);
                        break;
                    case "x-google-flag":
                        this.parseXGoogleFlag(attribute);
                        break;
                    case "connection":
                        this.parseMediaConnection(attribute);
                        break;
                    default:
                        attribute.ignored = true;
                        _attributes.unrecognized.Add(attribute);
                        break;

                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"parsing media attribute {attribute.attField} error, a={attribute.attField}:{attribute.attValue}");
                throw e;
            }

            if ((attribute.ignored != null && attribute.ignored == true) && attribute.attValue != null && !this.atEnd(attribute))
            {
                throw new Exception("attribute parsing error");
            }
        }

        private void parseCandidate(Attribute attribute)
        {
            var foundation = this.extractOneOrMore(attribute, IsICEChar, new RangeType(1, 32));

            this.consumeAttributeSpace(attribute);

            var componentId = this.extractOneOrMore(attribute, Char.IsDigit, new RangeType(1, 5));

            this.consumeAttributeSpace(attribute);

            var transport = this.extract(attribute, this.consumeToken);

            this.consumeAttributeSpace(attribute);

            var priority = this.extractOneOrMore(attribute, Char.IsDigit, new RangeType(1, 10));

            this.consumeAttributeSpace(attribute);

            var connectionAddress = this.extract(attribute, this.consumeAddress);

            this.consumeAttributeSpace(attribute);

            var port = this.extract(attribute, this.consumePort);

            this.consumeAttributeSpace(attribute);

            this.extract(attribute, this.consume, "typ");

            this.consumeAttributeSpace(attribute);

            var type = this.extract(attribute, this.consumeToken);

            var candidate = new Candidate(
                foundation,
                componentId,
                transport,
                priority,
                connectionAddress,
                port,
                type
            );

            if (this.peek(attribute, " raddr"))
            {
                this.extract(attribute, this.consume, " raddr");
                this.consumeAttributeSpace(attribute);
                candidate.relAddr = this.extract(attribute, this.consumeAddress);
            }

            if (this.peek(attribute, " rport"))
            {
                this.extract(attribute, this.consume, " rport");
                this.consumeAttributeSpace(attribute);
                candidate.relPort = this.extract(attribute, this.consumePort);
            }

            while (attribute._cur < attribute.attValue.Length && this.peekChar(attribute) == Constants.SP)
            {
                this.consumeAttributeSpace(attribute);
                var extensionAttName = this.extract(attribute, this.consumeToken);
                this.consumeAttributeSpace(attribute);
                candidate.extension[extensionAttName] = this.extractOneOrMore(
                  attribute,
                  IsVChar
                );
            }

            _attributes.candidates.Add(candidate);
        }

        private void parseRemoteCandidate(Attribute attribute)
        {

            while (true)
            {
                var componentId = this.extractOneOrMore(attribute, Char.IsDigit, new RangeType(1, 5));
                this.consumeAttributeSpace(attribute);
                var connectionAddress = this.extract(attribute, this.consumeAddress);
                this.consumeAttributeSpace(attribute);
                var port = this.extract(attribute, this.consumePort);

                _attributes.remoteCandidatesList.Add(new RemoteCandidate(componentId, connectionAddress, port));

                try
                {
                    this.consumeAttributeSpace(attribute);
                }
                catch 
                {
                    break;
                }
            }
        }

        private void parseEndOfCandidates()
        {
            if (_attributes.endOfCandidates != null && _attributes.endOfCandidates != false)
            {
                throw new Exception("must be only one line of end-of-candidates");
            }

            _attributes.endOfCandidates = true;
        }

        private void parseRtpmap(Attribute attribute)
        {
            var payloadType = this.extract(attribute, this.consumeToken);
            this.consumeAttributeSpace(attribute);
            var encodingName = this.extract(attribute, this.consumeTill, '/').Trim();
            this.extract(attribute, this.consume, "/");
            this.consumeAttributeSpace(attribute, true);
            var clockRate = System.Convert.ToInt32(this.extractOneOrMore(attribute, Char.IsDigit));

            RTPMap rtpMap = new RTPMap(encodingName, clockRate);

            if (!this.atEnd(attribute) && this.peekChar(attribute) == '/')
            {
                this.extract(attribute, this.consume, "/");
                rtpMap.encodingParameters = int.Parse(
                  this.extract(attribute, this.consumeTill)
                );
            }

            var it = _attributes.payloads.Where(
              (payload) => payload.payloadType.ToString() == payloadType
            );
            bool foundPayload = false;
            foreach (var p in it)
            {
                p.rtpMap = rtpMap;
                foundPayload = true;
                break;
            }

            int payload_type = int.Parse(payloadType);
            if (!foundPayload)
            {
                _attributes.payloads.Add(new PayloadAttribute( 
		            rtpMap, payload_type ));
            }
        }

        private void parsePtime(Attribute attribute)
        {
            if (_attributes.ptime != null)
            {
                throw new Exception("must be only one line of ptime");
            }

            _attributes.ptime = this.extract(attribute, this.consumeTill);
        }

        private void parseMaxPtime(Attribute attribute)
        {
            if (_attributes.maxPtime != null)
            {
                throw new Exception("must be only one line of ptime");
            }

            _attributes.maxPtime = this.extract(attribute, this.consumeTill);
        }

        private void parseDirection(Attribute attribute)
        {
            if (_attributes.direction != null)
            {
                throw new Exception("must be only one line of direction info");
            }

            _attributes.direction = (Direction)Enum.Parse(typeof(Direction), attribute.attField);
        }

        private void parseSSRC(Attribute attribute)
        {
            var ssrcId = this.extractOneOrMore(attribute, Char.IsDigit);

            this.consumeAttributeSpace(attribute);

            var attributeName = this.extract(attribute, this.consumeTill, ':');
            string attributeValue = null;

            if (this.peekChar(attribute) == ':')
            {
                this.extract(attribute, this.consume, ":");
                attributeValue = this.extract(attribute, this.consumeTill);
            }

            var ssrc = ((List<SSRC>)_attributes.ssrcs).Find(
              (s) => s.ssrcId.ToString() == ssrcId
            );
            if (ssrc != null)
            {
                ssrc.attributes[attributeName] = attributeValue;
            }
            else
            {
                _attributes.ssrcs.Add(new SSRC(
                        ssrcId: Int64.Parse(ssrcId),
                    attributes: new Dictionary<string, string> { { attributeName , attributeValue }
                }));
            }
        }

        private void parseFmtp(Attribute attribute)
        {
            var format = this.extract(attribute, this.consumeTill, Constants.SP);
            this.consumeAttributeSpace(attribute, true);
            var parametersString = this.extract(attribute, this.consumeTill);

            if (string.IsNullOrEmpty(parametersString.Trim()))
            {
                return; 
	        }

            Dictionary<string, object> parameters = new Dictionary<string, object>();

            var keyValues = parametersString.Split(new char[] { ';' });
            foreach (string keyValue in keyValues)
            {
                string[] vals = keyValue.Split(new char[] { '=' });
                if (vals.Length > 0)
                {
                    string key = vals[0].Trim();
                    string value = null;

                    if (vals.Length > 1) { 
			            value = vals[1].Trim(); 
		            }

                    if (key.Length > 0)
                    {
                        parameters[key] = value;
                    }
                }
            };

            var payload = ((List<PayloadAttribute>)_attributes.payloads).Find(
              (p) => p.payloadType.ToString() == format
            );

            if (payload != null)
            {
                payload.fmtp = new Fmtp(parameters);
            }
            else
            {
                int payloadType = int.Parse(format);
                payload = new PayloadAttribute(null, payloadType);
                payload.fmtp = new Fmtp(parameters);
                _attributes.payloads.Add(payload);
            }
        }

        private object parseFmtParameters(Attribute attribute)
        {
            //var parameters: { [key: string]: string } = {};
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            var key = this.extract(attribute, this.consumeTill, '=');
            attribute._cur++; // skip "="
            var value = this.extract(attribute, this.consumeTill, ';');
            parameters[key] = value;

            while (attribute.attValue[attribute._cur] == ';')
            {
                key = this.extract(attribute, this.consumeTill, '=');
                attribute._cur++; // skip "="
                value = this.extract(attribute, this.consumeTill, ';');
                parameters[key] = value;
            }

            return parameters;
        }

        private void parseRtcpFb(Attribute attribute)
        {
            var payloadType = "";

            if (this.peekChar(attribute) == '*')
            {
                payloadType = this.extract(attribute, this.consume, '*');
            }
            else
            {
                payloadType = this.extract(attribute, this.consumeTill, Constants.SP);
            }

            this.consumeAttributeSpace(attribute);

            var feedbackType = this.extract(attribute, this.consumeTill, Constants.SP);
            RTCPFeedback rtcpFeedback;

            switch (feedbackType)
            {
                case "trr-int":
                    {
                        var interval = this.extract(attribute, this.consumeTill);
                        TRRINTFeedback feedback = new TRRINTFeedback(feedbackType, interval);
                        //  _attributes.rtcpFeedbacks.Add({ payloadType, feedback });
                        rtcpFeedback = feedback;
                        break;
                    }
                case "ack":
                case "nack":
                default:
                    RTCPACKCommmonFeedback feedback1;

                    if (feedbackType == "ack")
                    {
                        feedback1 = new ACKFeedback(feedbackType);
                    }
                    else if (feedbackType == "nack")
                    {
                        feedback1 = new NACKFeedback(feedbackType);
                    }
                    else
                    {
                        feedback1 = new OtherFeedback(feedbackType);
                    }

                    if (this.peekChar(attribute) == Constants.SP)
                    {
                        this.consumeAttributeSpace(attribute);
                        feedback1.parameter = this.extract(attribute, this.consumeToken);

                        if (this.peekChar(attribute) == Constants.SP)
                        {
                            feedback1.additional = this.extract(attribute, this.consumeTill);
                        }
                    }
                    rtcpFeedback = feedback1;

                    break;
            }

            if (payloadType == "*")
            {
                _attributes.rtcpFeedbackWildcards.Add(rtcpFeedback);
            }
            else
            {
                var payload = ((List<PayloadAttribute>)_attributes.payloads).Find(
                  (p) => p.payloadType.ToString() == payloadType
                );

                if (payload != null)
                {
                    payload.rtcpFeedbacks.Add(rtcpFeedback);
                }
                else
                {
                    int payType = int.Parse(payloadType);
                    PayloadAttribute payload1 = new PayloadAttribute(null, payType);
                    payload1.rtcpFeedbacks = new List<RTCPFeedback> { rtcpFeedback };
                    _attributes.payloads.Add(payload1);
                }
            }
        }

        private void parseRTCPMux()
        {
            if (_attributes.rtcpMux != null)
            {
                throw new Exception("must be single line of rtcp-mux");
            }

            _attributes.rtcpMux = true;
        }

        private void parseRTCPMuxOnly()
        {
            if (_attributes.rtcpMuxOnly != null && _attributes.rtcpMuxOnly != false)
            {
                throw new Exception("must be single line of rtcp-only");
            }

            _attributes.rtcpMuxOnly = true;
        }

        private void parseRTCPRsize()
        {
            if (_attributes.rtcpRsize != null && _attributes.rtcpRsize != false)
            {
                throw new Exception("must be single line of rtcp-rsize");
            }

            _attributes.rtcpRsize = true;
        }

        private void parseRTCP(Attribute attribute)
        {
            if (_attributes.rtcp != null)
            {
                throw new Exception("must be single line of rtcp");
            }

            var port = this.extract(attribute, this.consumePort);

            RTCP rtcp = new RTCP { port = port };

            if (this.peekChar(attribute) == Constants.SP)
            {
                this.consumeAttributeSpace(attribute);
                rtcp.netType = this.extractOneOrMore(attribute, IsTokenChar);
                this.consumeAttributeSpace(attribute);
                rtcp.addressType = this.extractOneOrMore(attribute, IsTokenChar);
                this.consumeAttributeSpace(attribute);
                rtcp.address = this.extract(attribute, this.consumeAddress);
            }

            _attributes.rtcp = rtcp;
        }

        private void parseMsid(Attribute attribute)
        {
            var id = this.extractOneOrMore(attribute, IsTokenChar, new RangeType(1, 64));
            MSID msid = new MSID() { id = id };

            if (this.peekChar(attribute) == Constants.SP)
            {
                this.consumeAttributeSpace(attribute);
                msid.appdata = this.extractOneOrMore(attribute, IsTokenChar, new RangeType(1, 64));
            }

            _attributes.msids.Add(msid);
        }

        private void parseImageAttr(Attribute attribute)
        {
            //TODO pending parsing image attribute... 'cause it's stupid complex
            _attributes.imageattr.Add(attribute.attValue);
        }

        public void ParseRid(Attribute attribute) => parseRid(attribute);
        private void parseRid(Attribute attribute)
        {
            var id = this.extractOneOrMore(
              attribute,
              (char character) =>
                Char.IsLetter(character) || Char.IsDigit(character) || character == '_' || character == '-'
            );

            this.consumeAttributeSpace(attribute);

            var direction = this.extract(
              attribute,
              this.consumeToken
            ); //as RID["direction"];

            RID rid = new RID { id = id, direction = direction };

            if (this.peekChar(attribute) == Constants.SP)
            {
                this.consumeAttributeSpace(attribute);

                //rid-pt-params-list
                if (this.peekWithSpace(attribute, "pt", "="))
                {
                    this.extract(attribute, this.consume, "pt");
                    int peek = this.consumeZeroOrMore(attribute.attValue, attribute._cur, (c) =>  c == Constants.SP );
                    attribute._cur = peek;
                    this.extract(attribute, this.consume, "=");
                    peek = this.consumeZeroOrMore(attribute.attValue, attribute._cur, (c) =>  c == Constants.SP );
                    attribute._cur = peek;

                    var payloads = new List<string>();
                    while (true)
                    {
                        var fmt = this.extract(attribute, this.consumeToken);
                        payloads.Add(fmt);
                        try
                        {
                            this.extract(attribute, this.consume, ",");
                        }
                        catch
                        {
                            break;
                        }
                    }

                    rid.payloads = payloads;

                    attribute._cur = consumeZeroOrMore(attribute.attValue, attribute._cur, (c) => c == Constants.SP);
                }


                // RID Params section,  [(key):(value)]*;
                attribute.PackValue();  // remove spaces
                int peek2 = this.consumeZeroOrMore(attribute.attValue, attribute._cur, (c) => c == ';');
                attribute._cur = peek2;

                while (true)
                {
                    var type = this.extract(attribute, this.consumeToken);

                    switch (type)
                    {
                        case "depend":
                            {
                                var val = this.extract(attribute, this.consume, "=");
                                var rids = val.Split(new char[] { ',' });
                                RIDDependParam param = new RIDDependParam() { rids = rids };
                                rid.@params.Add(param);
                                break;
                            }
                        case "max-width":
                        case "max-height":
                        case "height-width":
                        case "max-fps":
                        case "max-fs":
                        case "max-br":
                        case "max-pps":
                        case "max-bpp":
                        default:
                            {
                                RIDGenericParam param = new RIDGenericParam { type = type };

                                if (this.peekChar(attribute) == '=')
                                {
                                    this.extract(attribute, this.consume, "=");
                                    param.val = this.extract(attribute, this.consumeTill, ';');
                                }
                                rid.@params.Add(param);
                                break;
                            }
                    }

                    try
                    {
                        this.extract(attribute, this.consume, ";");
                    }
                    catch
                    {
                        break;
                    }
                }
            }

            _attributes.rids.Add(rid);
        }

        private void parseSimulcast(Attribute attribute)
        {
            //todo pending parsing simulcast
            if (_attributes.simulcast != null)
            {
                throw new Exception("must be single line of simulcast");
            }

            _attributes.simulcast = attribute.attValue;
            this.extract(attribute, this.consumeTill);
        }

        private void parseSctpPort(Attribute attribute)
        {
            _attributes.sctpPort = this.extractOneOrMore(attribute, Char.IsDigit, new RangeType(1, 5));
        }

        private void parseMaxMessageSize(Attribute attribute)
        {
            _attributes.maxMessageSize = this.extractOneOrMore(attribute, Char.IsDigit, new RangeType(1, null));
        }

        public override IAttributes digest()
        {
            this.digested = true;
            return _attributes;
        }

        private void parseMid(Attribute attribute)
        {
            _attributes.mid = this.extract(attribute, this.consumeToken);
        }

        private void parseSSRCGroup(Attribute attribute)
        {
            var semantic = this.extract(attribute, this.consumeToken);
            var ssrcIds = new List<long>();

            while (true)
            {
                try
                {
                    this.consumeAttributeSpace(attribute);
                    var ssrcId = this.extract(attribute, this.consumeInteger);
                    ssrcIds.Add(long.Parse(ssrcId));
                }
                catch 
                {
                    break;
                }
            }

            _attributes.ssrcGroups.Add(new SSRCGroup() { semantic = semantic, ssrcIds = ssrcIds });
        }

        private void parseXGoogleFlag(Attribute attribute)
        {
            _attributes.xGoogleFlag = this.extract(attribute, this.consumeToken);
        }

        private void parseMediaConnection(Attribute attribute)
        {
            _attributes.connection = attribute.attValue;
	    }
    }


}
