using System;
using System.Linq;
using System.Collections.Generic;

namespace io.agora.sdp
{
    public class Parser : ParsingConsumerBase
    {
        IList<Record> records;
        int currentLine = 0;

        public Parser()
        {
            records = new List<Record>();
        }

        public Parser(IList<Record> recs)
        {
            records = recs;
	    }

        public SessionDescription Parse(string sdp)
        {
            string EOL = this.probeEOL(sdp);
            string[] lines = sdp.Split(new string[] { EOL }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < lines.Length; i++)
            {
                Record record = ParseLine(lines[i].Trim(), i);
                records.Add(record);
            }
            this.currentLine = 0;

            var version = this.parseVersion();
            var origin = this.parseOrigin();
            var sessionName = this.parseSessionName();
            var information = this.parseInformation();
            var uri = this.parseUri();
            var emails = this.parseEmail();
            var phones = this.parsePhone();
            var connection = this.parseConnection();
            var bandwidths = this.parseBandWidth();
            var timeFields = this.parseTimeFields();
            var key = this.parseKey();
            var attributes = this.parseSessionAttribute();
            var mediaDescriptions = this.parseMediaDescription();

            if (this.currentLine != this.records.Count)
            {
                throw new Exception("parsing failed, non exhaustive sdp lines.");
            }

            return new SessionDescription
            {
                version = version,
                origin = origin,
                sessionName = sessionName,
                sessionInformation = information,
                uri = uri,
                emails = emails,
                phones = phones,
                connection = connection,
                bandwidths = bandwidths,
                timeFields = timeFields,
                key = key,
                attributes = attributes,
                mediaDescriptions = mediaDescriptions
            };
        }

        private string extract(Record record, ConsumeDelegate consume, object rest = null)
        {
            var peek = consume(record.value, record.cur, rest);
            var result = record.value.slice(record.cur, peek);
            record.cur = peek;

            return result;
        }

        private string extractOneOrMore(Record record, PredictDelegate predict)
        {
            var peek = this.consumeOneOrMore(record.value, record.cur, predict);
            var result = record.value.slice(record.cur, peek);
            record.cur = peek;

            return result;
        }

        private void consumeSpaceForRecord(Record record)
        {
            if (record.value[record.cur] == Constants.SP)
            {
                record.cur += 1;
            }
            else
            {
                throw new Exception($"Invalid space at { record.cur}.");
            }
        }


        Record getCurrentRecord()
        {
            if (currentLine < records.Count)
            {
                return records[currentLine];
            }

            throw new Exception($"Record {currentLine} doesn't exist");
        }

        string ProbeEOL(string sdp)
        {
            for (int i = 0; i < sdp.Length; i++)
            {
                if (sdp[i] == Constants.LF)
                {
                    if (sdp[i - 1] == Constants.CR)
                    {
                        return Constants.CRLF;
                    }
                    else
                    {
                        return Constants.LF.ToString();
                    }
                }
            }

            throw new Exception("Invalid newline character.");
        }

        private string probeEOL(string sdp)
        {
            for (int i = 0; i < sdp.Length; i++)
            {
                if (sdp[i] == Constants.LF)
                {
                    if (i > 0 && sdp[i - 1] == Constants.CR)
                    {
                        return Constants.CRLF;
                    }
                    else
                    {
                        return Constants.LF.ToString();
                    }
                }
            }

            throw new Exception("Invalid newline character.");
        }


        /// <summary>
        ///    Parse a line into Record, line is trimmed before passing in
        /// </summary>
        /// <param name="line"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static Record ParseLine(string line, int index)
        {
            if (line.Length < 2)
            {
                throw new Exception(
                  "Invalid sdp line, sdp line should be of form <type>=<value>."
                );
            }

            int i = 0;

            var type = (RECORD_TYPE)line[i];  // may raise casting exception
            i++;

            // consume space
            while (i < line.Length && line[i] == Constants.SP)
            {
                i++;
            }

            if (line[i] != '=')
            {
                throw new Exception(
        "Invalid sdp line, < type > should be a single character followed by an = sign. current line:" + line
      );
            }
            i++;

            // consume space
            while (i < line.Length && line[i] == Constants.SP)
            {
                i++;
            }

            var value = line.Substring(i);

            return new Record(type, value, 0, index);
        }

        private IAttributes parseSessionAttribute()
        {
            var attributeParser = new SessionAttributeParser();

            while (this.currentLine < this.records.Count)
            {
                var record = this.getCurrentRecord();

                if (record.type != RECORD_TYPE.ATTRIBUTE)
                {
                    break;
                }

                var attField = this.extractOneOrMore(
                  record,
                  (character) => IsTokenChar(character) && character != ':'
                );
                var attribute = new Attribute(attField, 0);

                if (record.cur < record.value.Length && record.value[record.cur] == ':')
                {
                    record.cur += 1;
                    attribute.attValue = this.extractOneOrMore(record, IsByteString);
                }

                attributeParser.parse(attribute);
                this.currentLine++;
            }

            return attributeParser.digest();
        }

        private IAttributes parseMediaAttributes(Media media)
        {
            var attributeParser = new MediaAttributeParser(media);

            while (this.currentLine < this.records.Count)
            {
                var record = this.getCurrentRecord();

                if (record.type != RECORD_TYPE.ATTRIBUTE)
                {
                    break;
                }

                var attField = this.extractOneOrMore(
                  record,
                  (character) => (IsTokenChar(character) || character == Constants.SP) && character != ':'
                );
                var attribute = new Attribute { attField = attField, _cur = 0 };

                if (record.cur < record.value.Length && record.value[record.cur] == ':')
                {
                    record.cur += 1;
                    attribute.attValue = this.extractOneOrMore(record, IsByteString);
                }

                attributeParser.parse(attribute);
                this.currentLine++;
            }

            return attributeParser.digest();
        }

        private string parseKey(Record rec = null)
        {
            var record = rec?? this.getCurrentRecord();
            if (record.type != RECORD_TYPE.KEY)
            {
                return null;
            }

            if (
              record.value == "prompt" ||
              record.value == "clear:" ||
              record.value == "base64:" ||
              record.value == "uri:"
            )
            {
                return record.value;
            }

            this.currentLine++;

            throw new Exception("Invalid key.");
        }

        /// <summary>
        ///    z=* (time zone adjustments)
        /// </summary>
        /// <returns></returns>
        private IList<TimeZoneAdjustment> parseZone()
        {
            if (currentLine >= records.Count) return null;
            var record = this.getCurrentRecord();
            var adjustments = new List<TimeZoneAdjustment>();
            if (record.type == RECORD_TYPE.ZONE_ADJUSTMENTS)
            {

                while (currentLine < records.Count)
                {
                    try
                    {
                        var time = this.extract(record, this.consumeTime);
                        this.consumeSpaceForRecord(record);

                        bool back = false;
                        if (record.value[record.cur] == '-')
                        {
                            back = true;
                            record.cur += 1;
                        }

                        var typedTime = this.extract(record, this.consumeTypedTime);
                        var adjust = new TimeZoneAdjustment { time = time, typedTime = typedTime, back = back };

                        adjustments.Add(adjust);
                    }
                    catch
                    {
                        break;
                    }
                }

                if (adjustments.Count == 0)
                {
                    throw new Exception("Invalid zone adjustments");
                }

                this.currentLine++;

                // return adjustments;
            }

            return adjustments;
        }

        private IList<Repeat> parseRepeat()
        {
            if (currentLine >= records.Count) return null;
            var repeats = new List<Repeat>();

            while (currentLine < records.Count)
            {
                var record = this.getCurrentRecord();
                if (record.type == RECORD_TYPE.REPEAT)
                {
                    var repeatInterval = this.extract(record, this.consumeRepeatInterval);
                    var typedTimes = this.parseTypedTime(record);

                    repeats.Add(new Repeat { repeatInterval = repeatInterval, typedTimes = typedTimes });
                    this.currentLine++;
                }
                else
                {
                    break;
                }
            }

            return repeats;
        }

        private IList<string> parseTypedTime(Record record)
        {
            var typedTimes = new List<string>();

            while (currentLine < records.Count)
            {
                try
                {
                    this.consumeSpaceForRecord(record);
                    typedTimes.Add(this.extract(record, this.consumeTypedTime));
                }
                catch 
                {
                    break;
                }
            }

            if (typedTimes.Count == 0)
            {
                throw new Exception("Invalid typed time.");
            }

            return typedTimes;
        }

        private TimingInfo parseTime()
        {
            var record = this.getCurrentRecord();
            var startTime = this.extract(record, this.consumeTime);
            this.consumeSpaceForRecord(record);
            var stopTime = this.extract(record, this.consumeTime);

            this.currentLine++;

            return new TimingInfo { startTime = startTime, stopTime = stopTime };
        }

        private IList<Bandwidth> parseBandWidth()
        {
            var bandwidths = new List<Bandwidth>();

            while (this.currentLine < this.records.Count)
            {
                var record = this.getCurrentRecord();

                if (record.type == RECORD_TYPE.BANDWIDTH)
                {
                    var bwtype = this.extractOneOrMore(record, IsTokenChar);

                    if (record.value[record.cur] != ':')
                    {
                        throw new Exception("Invalid bandwidth field.");
                    }
                    else
                    {
                        record.cur++;
                    }

                    var bandwidth = this.extractOneOrMore(record, Char.IsDigit);

                    bandwidths.Add(new Bandwidth { bwtype = bwtype, bandwidth = bandwidth });

                    this.currentLine++;
                }
                else
                {
                    break;
                }
            }

            return bandwidths;
        }

        private int parseVersion(Record rec = null) 
        {
            var record = rec?? this.getCurrentRecord();

            if (record.type != RECORD_TYPE.VERSION)
            {
                throw new Exception("first sdp record must be version");
            }

            //var version = record.value.slice(
            //  0,
            //  this.consumeOneOrMore(record.value, 0, Char.IsDigit)
            //);

            //if (version.Length != record.value.Length)
            //{
            //    throw new Exception($"invalid proto version, v={record.value}");
            //}

            this.currentLine++;

            return int.Parse(record.value);
        }


        private Origin parseOrigin(Record rec = null)
        {
            var record = rec ?? this.getCurrentRecord();

            if (record.type != RECORD_TYPE.ORIGIN)
            {
                throw new Exception("second line of sdp must be origin");
            }

            var username = this.extractOneOrMore(record, IsNonWSChar);
            this.consumeSpaceForRecord(record);
            var sessId = this.extractOneOrMore(record, Char.IsDigit);
            this.consumeSpaceForRecord(record);
            var sessVersion = this.extractOneOrMore(record, Char.IsDigit);
            this.consumeSpaceForRecord(record);
            var nettype = this.extractOneOrMore(record, IsTokenChar);
            this.consumeSpaceForRecord(record);
            var addrtype = this.extractOneOrMore(record, IsTokenChar);
            this.consumeSpaceForRecord(record);
            var unicastAddress = this.extract(record, this.consumeUnicastAddress);

            this.currentLine++;

            return new Origin
            {
                username = username,
                sessId = sessId,
                sessVersion = sessVersion,
                nettype = nettype,
                adrtype = addrtype,
                unicastAddress = unicastAddress,
            };
        }

        private string parseSessionName(Record rec = null)
        {
            var record = rec ?? this.getCurrentRecord();

            if (record.type == RECORD_TYPE.SESSION_NAME)
            {
                var sessionName = this.extract(record, this.consumeText);
                this.currentLine++;
                return sessionName;
            }

            return null;
        }


        private string parseInformation(Record rec = null)
        {
            var record = rec ?? this.getCurrentRecord();

            if (record.type != RECORD_TYPE.INFORMATION)
            {
                return null;
            }

            var information = this.extract(record, this.consumeText);

            this.currentLine++;

            return information;
        }

        private string parseUri()
        {
            //TODO pending parsing URI
            var record = this.getCurrentRecord();

            if (record.type == RECORD_TYPE.URI)
            {
                this.currentLine++;
                return record.value;
            }

            return null;
        }


        private IList<string> parseEmail()
        {
            //todo parsing email
            var emails = new List<string>();

            while (currentLine < records.Count)
            {
                var record = this.getCurrentRecord();
                if (record.type == RECORD_TYPE.EMAIL)
                {
                    emails.Add(record.value);
                    this.currentLine++;
                }
                else
                {
                    break;
                }
            }
            return emails;
        }

        private IList<string> parsePhone()
        {
            //todo parsing phone
            var phones = new List<string>();

            while (currentLine < records.Count)
            {
                var record = this.getCurrentRecord();
                if (record.type == RECORD_TYPE.PHONE)
                {
                    phones.Add(record.value);
                    this.currentLine++;
                }
                else
                {
                    break;
                }
            }

            return phones;
        }

        private Connection parseConnection(Record rec = null)
        {
            var record = rec?? this.getCurrentRecord();

            if (record.type == RECORD_TYPE.CONNECTION)
            {
                var nettype = this.extractOneOrMore(record, IsTokenChar);
                this.consumeSpaceForRecord(record);
                var addrtype = this.extractOneOrMore(record, IsTokenChar);
                this.consumeSpaceForRecord(record);
                var address = this.extract(record, this.consumeAddress);

                this.currentLine++;

                return new Connection { nettype = nettype, addrtype = addrtype, address = address };
            }

            return null;
        }

        private Media parseMedia(Record rec = null)
        {
            var record = rec??this.getCurrentRecord();

            var mediaType = this.extract(record, this.consumeToken);

            this.consumeSpaceForRecord(record);

            var port = this.extract(record, this.consumePort);
            if (record.value[record.cur] == '/')
            {
                record.cur += 1;
                port += this.extract(record, this.consumeInteger);
            }

            this.consumeSpaceForRecord(record);

            List<string> protos = new List<string>();
            protos.Add(this.extract(record, this.consumeToken));
            // record.cur += 1;

            // Parse Protocol1/Protocol2/Protocol3, note there could be space in between /
            while (record.cur < record.value.Length)
            {
                char character = record.value[record.cur];
                //if (record.value[record.cur] == Constants.SP)
                if(character == Constants.SP)
                {
                    record.cur += 1;
                    continue;
                }
                //if (record.value[record.cur] == '/')
                if(character == '/')
                {
                    record.cur += 1;
                    while(record.cur < record.value.Length) {
                        if (record.value[record.cur] == Constants.SP)
                        {
                            record.cur += 1;
                        }
                        else break;
		            }
                    protos.Add(this.extract(record, this.consumeToken));
                }
                else
                {
                    break;
                }
            }

            if (protos.Count == 0)
            {
                throw new Exception("Invalid proto");
            }

            //while (record.cur < record.value.Length)
            //{
            //    if (record.value[record.cur] == Constants.SP)
            //    {
            //        record.cur += 1;
            //    }
            //}
            var fmts = this.parseFmt(record);

            this.currentLine++;

            return new Media { mediaType = mediaType, port = port, protos = protos, fmts = fmts };
        }

        /// <summary>
        ///         t=  (time the session is active)
        ///         r=* (zero or more repeat times)
        ///         z=* (time zone adjustments)
        /// </summary>
        /// <returns></returns>
        private IList<TimeField> parseTimeFields()
        {
            var timeFields = new List<TimeField>();

            while (this.currentLine < this.records.Count)
            {
                var record = this.getCurrentRecord();

                if (record.type == RECORD_TYPE.TIME)
                {
                    var time = this.parseTime();
                    var repeats = this.parseRepeat();
                    var zones = this.parseZone();
                    timeFields.Add(new TimeField { time = time, repeats = repeats, zoneAdjustments = zones });
                }
                else
                {
                    break;
                }
            }

            return timeFields;
        }


        private IList<MediaDescription> parseMediaDescription()
        {
            var mediaDescriptions = new List<MediaDescription>();

            while (this.currentLine < this.records.Count)
            {
                var record = this.getCurrentRecord();

                if (record.type == RECORD_TYPE.MEDIA)
                {
                    var media = this.parseMedia();
                    var information = this.parseInformation();
                    var connections = this.parseConnections();
                    var bandwidths = this.parseBandWidth();
                    var key = this.parseKey();
                    var attributes = this.parseMediaAttributes(media);
                    // var attributeMap = this.parseAttributeMap(attributes);

                    mediaDescriptions.Add(new MediaDescription
                    {
                        media = media,
                        information = information,
                        connections = connections,
                        bandwidths = bandwidths,
                        ky = key,
                        attributes = (MediaAttributes)attributes
                    });
                }
                else
                {
                    break;
                }
            }

            return mediaDescriptions;

            // Object.values(this.mediaExtMap).forEach((extension) => {
            //   let ext = extension.digest();
            //
            //   if (ext) {
            //     Object.assign(result.exts, ext);
            //   }
            //
            //   extension.reset();
            // });
            //
            // return [result, i];
        }


        private IList<Connection> parseConnections()
        {
            var connections = new List<Connection>();
            while (this.currentLine < this.records.Count)
            {
                var record = this.getCurrentRecord();
                if (record.type != RECORD_TYPE.CONNECTION)
                {
                    break;
                }
                connections.Add(parseConnection());
            }

            return connections;
        }

        private IList<string> parseFmt(Record record)
        {
            var fmts = new List<string>();

            while (true)
            {
                try
                {
                    this.consumeSpaceForRecord(record);
                }
                catch  { 
                    // ignore whether not there is a space
		        }
                try
                { 
                    fmts.Add(this.extract(record, this.consumeToken));
                }
                catch { 
                    break;
                }
            }

            if (fmts.Count == 0)
            {
                throw new Exception("Invalid fmts");
            }

            return fmts;
        }


#if ENABLE_UNIT_TEST
        #region --- Exposion to Unit Tests ---
        public string ParseKey(Record record) => parseKey(record);
        public IAttributes ParseSessionAttribute() => parseSessionAttribute();
        public IList<string> ParseFmt(Record record) => parseFmt(record);
        public IList<Bandwidth> ParseBandWidth() => parseBandWidth();
        public int ParseVersion(Record rec) => parseVersion(rec);
        public Origin ParseOrigin(Record rec) => parseOrigin(rec);
        public string ParseSessionName(Record rec) => parseSessionName(rec);
        public string ParseInformation(Record rec) => parseInformation(rec);
        public IList<string> ParseEmail() => parseEmail();
        public Connection ParseConnection(Record rec) => parseConnection(rec);
        public IList<string> ParsePhone() => parsePhone();
        public Media ParseMedia(Record rec) => parseMedia(rec);
        public IList<TimeField> ParseTimeFields() => parseTimeFields();
        public IList<MediaDescription> ParseMediaDescription() => parseMediaDescription();
        public IList<Connection> ParseConnections() => parseConnections();
        #endregion
#endif
    }
}
