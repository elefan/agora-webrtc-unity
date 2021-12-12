//
// SessionTypes.cs
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
    /// <summary>
    ///    RFC 4566 Specs:
    //Session description
    //         v=  (protocol version)
    //         o = (originator and session identifier)
    //         s=  (session name)
    //         i = *(session information)
    //         u=* (URI of description)
    //         e=* (email address)
    //         p = *(phone number)
    //         c=* (connection information -- not required if included in
    //              all media)
    //         b=* (zero or more bandwidth information lines)
    //         One or more time descriptions("t=" and "r=" lines; see below)
    //         z=* (time zone adjustments)
    //         k=* (encryption key)
    //         a = *(zero or more session attribute lines)
    //         Zero or more media descriptions
    //
    //      Time description
    //         t = (time the session is active)
    //         r=* (zero or more repeat times)
    //
    //      Media description, if present
    //         m = (media name and transport address)
    //         i=* (media title)
    //         c = *(connection information -- optional if included at
    //              session level)
    //         b=* (zero or more bandwidth information lines)
    //         k=* (encryption key)
    //         a = *(zero or more media attribute lines)
    /// </summary>
    public class SessionDescription
    {
        /// <summary>
        /// The "v=" field gives the version of the Session Description Protocol.
        /// </summary>
        public int version { get; set; }

        /// <summary>
        /// The "o=" field gives the originator of the session (her username and
        /// the address of the user's host) plus a session identifier and version
        /// number
        /// </summary>
        public Origin origin { get; set; }

        /// <summary>
        /// The "s=" field is the textual session name.
        /// </summary>
        public string sessionName { get; set; }

        /// <summary>
        /// The "i=" field provides textual information about the session
        /// </summary>
        public string sessionInformation { get; set; }

        public string uri { get; set; }

        public IList<string> emails { get; set; }

        public IList<string> phones { get; set; }

        public Connection connection { get; set; }
        public IList<Bandwidth> bandwidths { get; set; }

        // Time Fields
        // SDPLib defines
        //public IList<SDPTimezoneInfo> TimeZones { get; set; }
        //public IList<TimingInfo> Timings { get; set; }

        // TS defines
        public IList<TimeField> timeFields { get; set; }

        // Key
        public string key { get; set; }

        public IAttributes attributes { get; set; }

        public IList<MediaDescription> mediaDescriptions { get; set; }
    }

    public class Media
    {
        public string mediaType { get; set; }
        public string port { get; set; }
        public IList<string> protos { get; set; } 
        public IList<string> fmts { get; set; }
        //   public Media(string mt, string po, IList<string> pro, IList<string> fm)
        //   {
        //       mediaType = mt;
        //       port = po;
        //       protos = pro;
        //       fmts = fm;
        //}
        public override string ToString()
        {
            string s_protos = string.Join("/", protos);
            string s_fmts = string.Join(" ", fmts);
            return $"{mediaType} {port} {s_protos} {s_fmts}"; 
        }
    }

    public class Origin
    {
        public string username { get; set; }
        public string sessId { get; set; }
        public string sessVersion { get; set; }
        public string nettype { get; set; }
        public string adrtype { get; set; }
        public string unicastAddress { get; set; }
    }

    public class Bandwidth
    {
        public string bwtype { get; set; }
        public string bandwidth { get; set; }

     //   public Bandwidth(string type, string value)
     //   {
     //       Type = type;
     //       Value = value;
	    //}
    }

    public class Connection
    {
        public string nettype { get; set; }

        public string addrtype { get; set; }

        public string address { get; set; }
    }

    //public class EncriptionKey
    //{
    //    public const string ClearMethod = "clear";
    //    public const string Base64Method = "base64";
    //    public const string UriMethod = "uri";
    //    public const string PromptMethod = "prompt";

    //    public string Method { get; set; }
    //    public string Value { get; set; }
    //}

    public class MediaDescription
    {
        public Media media { get; set; }
        public string information { get; set; }
        public IList<Connection> connections { get; set; }
        public IList<Bandwidth> bandwidths { get; set; }
        public string ky { get; set; }
        public MediaAttributes attributes { get; set; }
    }

    public class TimeZoneAdjustment
    {
        public string time { get; set; }
        public string typedTime { get; set; }
        public bool back { get; set; } 
    }

    // SDPLib define
    //public class SDPTimezoneInfo
    //{
    //    public long AdjustmentTime { get; set; }
    //    public string Offset { get; set; }
    //}

    public class TimingInfo
    {
        public string startTime { get; set; }
        public string stopTime { get; set; }
    }

    public class TimeField
    {
        public TimingInfo time { get; set; }
        public IList<Repeat> repeats { get; set; }
        public IList<TimeZoneAdjustment> zoneAdjustments { get; set; }  
    }

    public class Repeat
    {
        public string repeatInterval { get; set; }
        public IList<string> typedTimes { get; set; }
    }

    public class Attribute
    {
        public bool? ignored { get; set; }
        private string _field = default(string);
        public string attField { 
	        get { return _field; } 
	        set { _field = Pack(value); } 
	    }
        public string attValue { get; set; }
        public int _cur { get; set; }
        public Attribute() { }
        public Attribute(string field, int cur, string value = "", bool? ignore = null)
        {
            attField = field;
            attValue = value;
            _cur = cur;
	    }

        private string Pack(string str) {
            string retstr = "";
            for(int i = 0; i < str.Length; i++)
            { 
	            if (str[i] != Constants.SP) {
                    retstr += str[i];
		        } 
	        }

            return retstr;
	    }

        /// <summary>
        ///   Remove any space starting from _cur
        /// </summary>
        public void PackValue()
        {
            string tail = attValue.Substring(0, _cur);

            int i = _cur;
            while(i<attValue.Length) { 
                if (attValue[i] != Constants.SP)
                {
                    tail += attValue[i];
		        }
                i++;
	        }

            attValue = tail;
	    }
    }
    // This is internal use
    public class Record
    {
        public RECORD_TYPE type { get; set; }
        public string value { get; set; }
        public int cur { get; set; }
        public int line { get; set; }
        public Record(RECORD_TYPE t, string v, int c, int l)
        {
            type = t;
            value = v;
            cur = c;
            line = l;
        }
    }
}