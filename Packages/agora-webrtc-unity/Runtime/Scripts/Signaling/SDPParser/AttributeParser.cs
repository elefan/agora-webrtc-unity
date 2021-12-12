//
// AttributeParser.cs
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
using System.Collections.Generic;

namespace io.agora.sdp
{
    public class RangeType
    {
        public int? min { get; set; }
        public int? max { get; set; }
        public RangeType() { }

        public RangeType(int? min, int? max)
        {
            this.min = min;
            this.max = max;
        }
    }

    public abstract class AttributeParser : ParsingConsumerBase
    {
        protected abstract IAttributes attributes { get; set; } //  SessionAttributes | MediaAttributes;
        protected bool digested = false;

        public abstract IAttributes digest(); // : SessionAttributes | MediaAttributes;

        protected string extractOneOrMore(
          Attribute attribute, PredictDelegate predict, RangeType range = null
        // predict: (char: string) => boolean,
        //  range?: [number | undefined, number | undefined]
        )
        {
            var peek = this.consumeOneOrMore(
              attribute.attValue,
              attribute._cur,
              predict
            );

            var result = attribute.attValue.Substring(attribute._cur, peek - attribute._cur);

            if (range != null)
            {
                // var[min, max] = range || [];
                if (range.min != null && result.Length < range.min)
                {
                    throw new SDPAttributeParseException($"error in result length, should be more or equal than { range.min} characters.");
                }

                if (range.max != null && result.Length > range.max)
                {
                    throw new SDPAttributeParseException(
                      $"error in result length, should be less or equal than {range.max} characters."
                    );
                }
            }

            attribute._cur = peek;

            return result;
        }

        protected void consumeAttributeSpace(Attribute attribute, bool zeroOrMore = false)
        {
            int start = attribute._cur;
            while (attribute._cur < attribute.attValue.Length && attribute.attValue[attribute._cur] == Constants.SP)
            {
                attribute._cur += 1;
            }

            if (!zeroOrMore && start == attribute._cur)
            {
                throw new SDPAttributeParseException($"Invalid space at {attribute._cur }.");
            }
        }

        protected string extract(Attribute attribute, ConsumeDelegate consume, object rest = null)
        {
            if (attribute.attValue == null)
            {
                throw new SDPAttributeParseException("Nothing to extract from attValue.");
            }

            var peek = consume(attribute.attValue, attribute._cur, rest);
            var result = attribute.attValue.slice(attribute._cur, peek);
            attribute._cur = peek;
            return result;
        }

        protected bool atEnd(Attribute attribute)
        {
            if (null == attribute.attValue)
            {
                throw new SDPAttributeParseException();
            }

            return attribute._cur >= attribute.attValue.Length;
        }

        protected char peekChar(Attribute attribute)
        {
            if (null == attribute.attValue)
            {
                throw new SDPAttributeParseException();
            }
            if (attribute._cur < attribute.attValue.Length)
            {
                return attribute.attValue[attribute._cur];
            } else {
                return Constants.NUL; // avoid index overflow
            }
        }

        protected bool peek(Attribute attribute, string value)
        {
            if (null == attribute.attValue)
            {
                throw new SDPAttributeParseException();
            }

            for (int i = 0; i < value.Length; i++)
            {
                try
                {
                    if (value[i] != attribute.attValue[attribute._cur + i])
                    {
                        return false;
                    }
                } catch {
                    // avoid index out of bound
                    break;
                }
            }

            return true;
        }

        /// <summary>
        ///     Check if the current position of attribute value string contains
        /// pattern  (start)(SP)*(end)
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        protected bool peekWithSpace(Attribute attribute, string start, string end)
        {
            if (null == attribute.attValue)
            {
                throw new SDPAttributeParseException();
            }

            // check start
            var str = attribute.attValue.Substring(attribute._cur);

            if (str.Length <= (start.Length + end.Length) || !str.StartsWith(start)) { return false; }

            int peek = start.Length;

            // skip spaces
            while (peek < str.Length && str[peek] == Constants.SP)
            {
                peek++;
            }

            // match end
            return str.Substring(peek).StartsWith(end);
        }

        protected void parseIceLite()
        {
            if (attributes.iceLite != null && attributes.iceLite != false)
            {
                throw new SDPAttributeParseException(
                  "Invalid ice-lite, should be only a single line of 'a=ice-lite'"
                );
            }
            attributes.iceLite = true;
        }

        protected void parseExtmapAllowMixed()
        {
            if (((SessionAttributes)attributes).extmapAllowMixed != null && ((SessionAttributes)attributes).extmapAllowMixed != false)
            {
                throw new SDPAttributeParseException(
                  "Invalid extmapAllowMixed, should be only a single line of 'a=extmap-allow-mixed'"
                );
            }
            ((SessionAttributes)attributes).extmapAllowMixed = true;
        }
        
        protected void parseIceUfrag(Attribute attribute)
        {
            if (this.attributes.iceUfrag != null)
            {
                throw new SDPAttributeParseException(
                  "Invalid ice-ufrag, should be only a single line if 'a=ice-ufrag'"
                );
            }

            this.attributes.iceUfrag = this.extractOneOrMore(attribute, IsICEChar, new RangeType(4, 256));
        }

        protected void parseIcePwd(Attribute attribute)
        {
            if (this.attributes.icePwd != null)
            {
                throw new SDPAttributeParseException(
                  "Invalid ice-pwd, should be only a single line if 'a=ice-pwd'"
                );
            }

            this.attributes.icePwd = this.extractOneOrMore(attribute, IsICEChar, new RangeType(22, 256));
        }

        protected void parseIceOptions(Attribute attribute)
        {
            if (this.attributes.iceOptions.Count > 0)
            {
                throw new SDPAttributeParseException(
                  "Invalid ice-options, should be only one 'ice-options' line"
                );
            }

            var options = new List<string>();
            while (!this.atEnd(attribute))
            {
                options.Add(this.extractOneOrMore(attribute, IsICEChar, new RangeType()));
                try
                {
                    this.consumeAttributeSpace(attribute);
                }
                catch (Exception e)
                {
                    if (this.atEnd(attribute))
                    {
                        break;
                    }
                    else
                    {
                        throw e;
                    }
                }
            }

            this.attributes.iceOptions = options;
        }

        protected void parseFingerprint(Attribute attribute)
        {
            var hashFunction = this.extract(attribute, this.consumeToken);
            this.consumeAttributeSpace(attribute);
            // TODO: cheated
            var fingerprint = this.extract(attribute, this.consumeTill);

            this.attributes.fingerprints.Add(new FingerPrint(hashFunction, fingerprint));
        }

        protected void parseExtmap(Attribute attribute)
        {
            var mapEntry = this.extractOneOrMore(attribute, Char.IsDigit, new RangeType());
            string direction = null;
            if (this.peekChar(attribute) == '/')
            {
                this.extract(attribute, this.consume, "/");
                direction = this.extract(attribute, this.consumeToken);
            }

            this.consumeAttributeSpace(attribute);

            var extensionName = this.extract(attribute, this.consumeTill, Constants.SP);

            int entry = int.Parse(mapEntry);

            Extmap map = new Extmap();
            map.entry = entry;
            map.direction = direction;
            map.extensionName = extensionName;

            //var map: Extmap = {
            //// mapEntry,
            //entry: parseInt(mapEntry, 10),
            //      ...(direction && { direction }),
            //      extensionName,
            //    };

            if (this.peekChar(attribute) == Constants.SP)
            {
                this.consumeAttributeSpace(attribute);
                map.extensionAttributes = this.extract(attribute, this.consumeTill);
            }

            this.attributes.extmaps.Add(map);
        }

        protected void parseSetup(Attribute attribute)
        {
            if (this.attributes.setup != null)
            {
                throw new SDPAttributeParseException("must only be one single 'a=setup' line.");
            }

            var role = this.extract(attribute, this.consumeTill);
            if (
              role != "active" &&
              role != "passive" &&
              role != "actpass" &&
              role != "holdconn"
            )
            {
                throw new SDPAttributeParseException(
                  "role must be one of 'active', 'passive', 'actpass', 'holdconn'."
                );
            }

            this.attributes.setup = role;
        }
    }


    // ======================= Session Parser ========================================
    #region ======== Session Parser ===========
    public class SessionAttributeParser : AttributeParser
    {
        SessionAttributes _attributes;
        protected override IAttributes attributes
        {
            get { return _attributes; }
            set { _attributes = (SessionAttributes)value; }
        }//  SessionAttributes | MediaAttributes;

        public SessionAttributeParser()
        {
            attributes = new SessionAttributes();
        }

        public void parse(Attribute attribute)
        {
            if (this.digested)
            {
                throw new SDPAttributeParseException("already digested");
            }

            try
            {
                switch (attribute.attField)
                {
                    case "group":
                        this.parseGroup(attribute);
                        break;
                    case "ice-lite":
                        this.parseIceLite();
                        break;
                    case "extmap-allow-mixed":
                        this.parseExtmapAllowMixed();
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
                    case "fingerprint":
                        this.parseFingerprint(attribute);
                        break;
                    case "setup":
                        this.parseSetup(attribute);
                        break;
                    case "tls-id":
                        this.parseTlsId(attribute);
                        break;
                    case "identity":
                        this.parseIdentity(attribute);
                        break;
                    case "extmap":
                        this.parseExtmap(attribute);
                        break;
                    case "msid-semantic":
                        this.parseMsidSemantic(attribute);
                        break;
                    default:
                        attribute.ignored = true;
                        attribute.attValue = null;
                        _attributes.unrecognized.Add(attribute);
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"parsing session attribute { attribute.attField} error, a={attribute.attField}:{attribute.attValue}");
                throw e;
            }

            if ((attribute.ignored == null || attribute.ignored == false) && attribute.attValue != null && !this.atEnd(attribute))
            {
                throw new SDPAttributeParseException("attribute parsing error");
            }
        }

        public override IAttributes digest()
        {
            this.digested = true;
            return this.attributes;
        }

        private void parseGroup(Attribute attribute)
        {
            var semantic = this.extract(attribute, this.consumeToken);

            var identificationTag = new List<string>();
            while (!this.atEnd(attribute) && this.peekChar(attribute) == Constants.SP)
            {
                this.consumeAttributeSpace(attribute);
                identificationTag.Add(this.extract(attribute, this.consumeToken));
            }

            _attributes.groups.Add(new Group(semantic, identificationTag));
        }

        private void parseTlsId(Attribute attribute)
        {
            if (this._attributes.tlsId != null)
            {
                throw new SDPAttributeParseException("must be only one tld-id line");
            }

            _attributes.tlsId = this.extractOneOrMore(attribute, IsTLSIdChar);
        }

        private void parseIdentity(Attribute attribute)
        {
            var assertionValue = this.extractOneOrMore(attribute, IsBase64Char);

            var extensions = new List<Extension>();
            while (!this.atEnd(attribute) && this.peekChar(attribute) == Constants.SP)
            {
                this.consumeAttributeSpace(attribute);
                var name = this.extract(attribute, this.consumeToken);
                this.extract(attribute, this.consume, "=");
                var value = this.extractOneOrMore(
                  attribute,
                  (character) => character != Constants.SP && IsByteString(character)
              );
                extensions.Add(new Extension(name, value));
            }

            this._attributes.identities.Add(new Identity(assertionValue, extensions));
        }

        private void parseMsidSemantic(Attribute attribute)
        {
            //handle Chrome msid-semantic erroneous syntax
            if (this.peekChar(attribute) == Constants.SP)
            {
                this.consumeAttributeSpace(attribute);
            }

            var semantic = this.extract(attribute, this.consumeToken);
            MsidSemantic msidSemantic = new MsidSemantic(semantic, new List<string>());

            while (true)
            {
                try
                {
                    this.consumeAttributeSpace(attribute);
                }
                catch
                {
                    break;
                }

                if (this.peekChar(attribute) == '*')
                {
                    this.extract(attribute, this.consume, "*");
                    msidSemantic.applyForAll = true;
                    break;
                }
                else
                {
                    var identifier = this.extract(attribute, this.consumeTill, Constants.SP);
                    msidSemantic.identifierList.Add(identifier);
                }
            }

            ((SessionAttributes)attributes).msidSemantic = msidSemantic;
        }
    }

    #endregion
    public class SDPAttributeParseException : System.Exception{
        public SDPAttributeParseException() : base() { }
        public SDPAttributeParseException(string error) : base(error) { }
    }
}
