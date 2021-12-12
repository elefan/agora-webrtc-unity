//
// ParsingConsumerBase.cs
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
namespace io.agora.sdp
{

    public delegate bool ConsumeTillDelegate(char c);
    public delegate bool PredictDelegate(char c);
    public delegate int ConsumeDelegate(string value, int cur, object rest);


    // consume: (value: string, cur: number, ...rest: any) => number,
    public class ParsingConsumerBase
    {
        public ParsingConsumerBase()
        {
        }

        protected int consumeText(string str, int cur, object rest = null)
        {
            var peek = cur;
            while (peek < str.Length)
            {
                char c = str[peek];
                if (c != Constants.NUL && c != Constants.CR && c != Constants.LF)
                {
                    peek += 1;
                    continue;
                }
                else
                {
                    break;
                }
            }

            if (peek - cur == 0)
            {
                throw new SDPParseException($"Invalid text, at {str}");
            }
            return peek;
        }

        protected int consumeTill(string value, int cur,
            // till: ((char: string) => boolean) | string | undefined
            object till)
        {
            var peek = cur;
            while (peek < value.Length)
            {
                if (till == null)
                {
                    // no ending specified, so consume everything
                    peek = value.Length;
                    break;
		        }
                if (till.GetType().Name == "Char" && value[peek] == (char)till)
                {
                    break;
                }
                else if (till.GetType().Name == "ConsumeTillDelegate")
                {
                    ConsumeTillDelegate checkTill = (ConsumeTillDelegate)till;
                    if (checkTill(value[peek]))
                    {
                        break;
                    }
                    break;
                }

                peek++;
            }

            return peek;
        }

        protected int consumeUnicastAddress(string str, int cur, object s=null)
        {
            //todo better address lexing
            return this.consumeTill(str, cur, Constants.SP);
            // switch (type) {
            //   case "IP4":
            //   case "ip4":
            //     return this.consumeIP4Address(str, cur);
            //   case "IP6":
            //   case "ip6":
            //     return this.consumeIP6Address(str, cur);
            //   default: {
            //     try {
            //       return this.consumeFQDN(str, cur);
            //     } catch (e) {
            //       try {
            //         return this.consumeExtnAddr(str, cur);
            //       } catch (e) {
            //         throw new Error("Invalid unicast address");
            //       }
            //     }
            //   }
            // }
        }


        protected int consumeZeroOrMore(string str, int cur, PredictDelegate predict)
        {
            var peek = cur;

            while (peek < str.Length)
            {
                if (predict(str[peek]))
                {
                    peek++;
                }
                else
                {
                    break;
                }
            }
            return peek;
        }

        protected int consumeOneOrMore(string str, int cur, PredictDelegate predict)
        {
            var peek = cur;

            while (peek < str.Length)
            {
                char c = str[peek];
                if (predict(str[peek]))
                {
                    peek++;
                }
                else
                {
                    break;
                }
            }

            if (peek - cur == 0)
            {
                throw new SDPParseException($"Invalid rule at {cur}.");
            }

            return peek;
        }

        protected int consumeSpace(string str, int cur)
        {
            if (str[cur] == Constants.SP)
            {
                return cur + 1;
            }
            else
            {
                throw new SDPParseException($"Invalid space at {cur}.");
            }
        }

        protected int consumeIP4Address(string str, int cur)
        {
            var peek = cur;
            for (var i = 0; i < 4; i++)
            {
                peek = this.consumeDecimalUChar(str, peek);

                if (i != 3)
                {
                    if (str[peek] == '.')
                    {
                        peek++;
                    }
                    else
                    {
                        throw new SDPParseException("Invalid IP4 address.");
                    }
                }
            }

            return peek;
        }

        protected int consumeDecimalUChar(string str, int cur)
        {
            var peek = cur;
            for (var i = 0; i < 3; i++, peek++)
            {
                if (!Char.IsDigit(str[peek]))
                {
                    break;
                }
            }

            if (peek - cur == 0)
            {
                throw new SDPParseException("Invalid decimal uchar.");
            }

            string slice = str.Substring(cur, peek - cur);
            int integer = int.Parse(slice);

            if (integer >= 0 && integer <= 255)
            {
                return peek;
            }
            else
            {
                throw new SDPParseException("Invalid decimal uchar");
            }
        }

        protected int consumeIP6Address(string str, int cur)
        {
            var peek = this.consumeHexpart(str, cur);
            if (str[peek] == ':')
            {
                peek += 1;
                peek = this.consumeIP4Address(str, peek);
                return peek;
            }
            else
            {
                return peek;
            }
        }

        protected int consumeHexpart(string str, int cur)
        {
            var peek = cur;

            //"::" [ hexseq ]
            if (str[peek] == ':' && str[peek + 1] == ':')
            {
                peek += 2;
                try
                {
                    peek = this.consumeHexseq(str, peek);
                }
                catch  { }

                return peek;
            }

            peek = this.consumeHexseq(str, peek);

            //hexseq "::" [ hexseq ]
            if (str[peek] == ':' && str[peek + 1] == ':')
            {
                peek += 2;

                try
                {
                    peek = this.consumeHexseq(str, peek);
                }
                catch { }

                return peek;
            }

            //hexseq
            else
            {
                return peek;
            }
        }

        protected int consumeHexseq(string str, int cur)
        {
            var peek = cur;
            while (true)
            {
                peek = this.consumeHex4(str, peek);

                if (str[peek] == ':' && str[peek + 1] != ':')
                {
                    peek += 1;
                }
                else
                {
                    break;
                }
            }

            return peek;
        }

        public static bool isHexDig(char c)
        {
            return ((c >= '0' && c <= '9') ||
                     (c >= 'a' && c <= 'f') ||
                     (c >= 'A' && c <= 'F')
                   );
        }

        protected int consumeHex4(string str, int cur)
        {
            var i = 0;

            for (; i < 4; i++)
            {
                if (!isHexDig(str[cur + i]))
                {
                    if (i == 0)
                    {
                        throw new SDPParseException("Invalid hex 4");
                    }
                    break;
                }
            }

            return cur + i;
        }

        protected int consumeFQDN(string str, int cur)
        {
            var peek = cur;

            while (true)
            {
                if (
                  Char.IsDigit(str[peek]) ||
                  Char.IsLetter(str[peek]) ||
                  str[peek] == '-' ||
                  str[peek] == '.'
                )
                {
                    peek += 1;
                    continue;
                }
                else
                {
                    break;
                }
            }

            if (peek - cur < 4)
            {
                throw new SDPParseException("Invalid FQDN");
            }

            return peek;
        }

        public static bool IsNonWSChar(char c)
        {
            return (
          c == '\u0021' ||
          (c >= '\u0023' && c <= '\u0027') ||
          (c >= '\u002A' && c <= '\u002B') ||
          (c >= '\u002D' && c <= '\u002E') ||
          (c >= '\u0030' && c <= '\u0039') ||
          (c >= '\u0041' && c <= '\u005A') ||
          (c >= '\u005E' && c <= '\u007E')
        );
        }

        protected int consumeExtnAddr(string str, int cur)
        {
            return this.consumeOneOrMore(str, cur, IsNonWSChar);
        }

        protected int consumeMulticastAddress(string str, int cur, string type)
        {
            switch (type)
            {
                case "IP4":
                case "ip4":
                    return this.consumeIP4MulticastAddress(str, cur);
                case "IP6":
                case "ip6":
                    return this.consumeIP6MulticastAddress(str, cur);
                default:
                    {
                        try
                        {
                            cur = this.consumeFQDN(str, cur);
                            return cur;
                        }
                        catch
                        {
                            cur = this.consumeExtnAddr(str, cur);
                            return cur;
                        }
                    }
            }
        }

        protected int consumeIP6MulticastAddress(string str, int cur)
        {
            var peek = this.consumeHexpart(str, cur);

            if (str[peek] == '/')
            {
                return this.consumeInteger(str, peek + 1);
            }
            else
            {
                return peek;
            }
        }

        protected int consumeIP4MulticastAddress(string str, int cur)
        {
            var peek = cur + 3;

            var pre = str.Substring(cur, 3); // str.Slice(cur, peek);
            var pre_number = int.Parse(pre);

            if (pre_number < 224 || pre_number > 239)
            {
                throw new SDPParseException(
                  "Invalid IP4 multicast address, IPv4 multicast addresses may be in the range 224.0.0.0 to 239.255.255.255."
                );
            }

            for (var i = 0; i < 3; i++)
            {
                if (str[peek] != '.')
                {
                    throw new SDPParseException("Invalid IP4 multicast address.");
                }
                else
                {
                    peek += 1;
                }

                peek = this.consumeDecimalUChar(str, peek);
            }

            if (str[peek] == '/')
            {
                peek += 1;
            }

            peek = this.consumeTTL(str, peek);

            if (str[peek] == '/')
            {
                peek = this.consumeInteger(str, peek);
            }

            return peek;
        }

        public static bool IsPosDigit(char c)
        {
            return c >= '\u0031' && c <= '\u0039';
        }


        protected int consumeInteger(string str, int peek, object rest = null)
        {
            if (!IsPosDigit(str[peek]))
            {
                throw new SDPParseException("Invalid integer.");
            }
            else
            {
                peek += 1;
            }

            while (peek < str.Length)
            {
                if (Char.IsDigit(str[peek]))
                {
                    peek += 1;
                }
                else
                {
                    break;
                }
            }

            return peek;
        }

        protected int consumeTTL(string str, int peek)
        {
            // single 0
            if (str[peek] == '0')
            {
                return peek + 1;
            }

            if (!IsPosDigit(str[peek]))
            {
                throw new SDPParseException("Invalid TTL.");
            }
            else
            {
                peek += 1;
            }

            for (var i = 0; i < 2; i++)
            {
                if (Char.IsDigit(str[peek]))
                {
                    peek += 1;
                    continue;
                }
                else
                {
                    break;
                }
            }

            return peek;
        }

        public static bool IsTokenChar(char c)
        {
            return (
               c == '\u0021' ||
              (c >= '\u0023' && c <= '\u0027') ||
              (c >= '\u002A' && c <= '\u002B') ||
              (c >= '\u002D' && c <= '\u002E') ||
              (c >= '\u0030' && c <= '\u0039') ||
              (c >= '\u0041' && c <= '\u005A') ||
              (c >= '\u005E' && c <= '\u007E')
            );
        }

        protected int consumeToken(string str, int cur, object o)
        {
            return this.consumeOneOrMore(str, cur, IsTokenChar);
        }

        protected int consumeTime(string recordValue, int cur, object o)
        {
            var peek = cur;

            if (recordValue[peek] == '0')
            {
                return peek + 1;
            }

            if (IsPosDigit(recordValue[peek]))
            {
                peek += 1;
            }

            while (peek<recordValue.Length)
            {
                if (Char.IsDigit(recordValue[peek]))
                {
                    peek++;
                }
                else
                {
                    break;
                }
            }

            if (peek - cur < 10)
            {
                throw new SDPParseException("Invalid time");
            }

            return peek;
        }

        protected int consumeAddress(string value, int cur, object o)
        {
            //todo better address lexer
            return this.consumeTill(value, cur, Constants.SP);
            // const addressConsumer = [
            //   this.consumeIP4Address,
            //   this.consumeIP4MulticastAddress,
            //   this.consumeIP6Address,
            //   this.consumeIP6MulticastAddress,
            //   this.consumeFQDN,
            //   this.consumeExtnAddr,
            // ];
            //
            // var result: number | undefined;
            //
            // var i = 0;
            // while (i < addressConsumer.Length) {
            //   try {
            //     result = addressConsumer[i].call(this, value, cur);
            //     break;
            //   } catch (e) {
            //     i++;
            //   }
            // }
            //
            // if (result == undefined) {
            //   throw new SDPParseException("Invalid ICE address");
            // }
            //
            // return result;
        }

        public static bool IsFixedLenTimeUnit(char c)
        {
            return (c == '\u0064' || c == '\u0068' ||
                     c == '\u006D' || c == '\u0073'
                   );
        }

        public static bool IsICEChar(char c)
        {
            return Char.IsLetter(c) || Char.IsDigit(c) || c == '+' || c == '/';
        }

        public static bool IsTLSIdChar(char c)
        {
            return (
              Char.IsDigit(c) || Char.IsLetter(c) ||
              c == '+' ||
              c == '/' ||
              c == '-' ||
              c == '_'
            );
        }

        public static bool IsBase64Char(char c)
        {
            return Char.IsDigit(c) || Char.IsLetter(c) || c == '+' || c == '/';
        }


        public static bool IsVChar(char c)
        {
            return c >= '\u0021' && c <= '\u007E';
        }

        public static bool IsByteString(char c)
        {
            return (
              (c > '\u0001' && c < '\u0009') ||
              (c > '\u000B' && c < '\u000C') ||
              (c > '\u000E' && c < '\u00FF')
            );
        }
        protected int consumeTypedTime(string recordValue, int cur, object rest = null)
        {
            var peek = cur;
            peek = this.consumeOneOrMore(recordValue, peek, Char.IsDigit);

            if (IsFixedLenTimeUnit(recordValue[peek]))
            {
                return peek + 1;
            }
            else
            {
                return peek;
            }
        }

        protected int consumeRepeatInterval(string recordValue, int cur, object rest = null)
        {
            if (!IsPosDigit(recordValue[cur]))
            {
                throw new SDPParseException("Invalid repeat interval");
            }
            cur += 1;

            while (true)
            {
                if (!Char.IsDigit(recordValue[cur]))
                {
                    break;
                }
                cur += 1;
            }

            if (IsFixedLenTimeUnit(recordValue[cur]))
            {
                cur += 1;
            }

            return cur;
        }

        protected int consumePort(string value, int cur, object p)
        {
            return this.consumeOneOrMore(value, cur, Char.IsDigit);
        }

        protected int consume(string value, int cur, object p)
        {
            string predicate = (string)p;
            for (var i = 0; i < predicate.Length; i++)
            {
                if (cur + i >= value.Length)
                {
                    throw new SDPParseException("consume exceeding value Length");
                }

                if (value[cur + i] != predicate[i])
                {
                    throw new SDPParseException($"consume { predicate } failed at {i}");
                }
            }

            return cur + predicate.Length;
        }


        /// ======================== End of Class  ============================
    }

    public class SDPParseException : System.Exception
    { 
        public SDPParseException(string error) : base(error)
        { }
    }
}
