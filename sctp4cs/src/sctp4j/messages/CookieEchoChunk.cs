/*
 * Copyright 2017 pi.pe gmbh .
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */
// Modified by Andrés Leone Gámez


using SCTP4CS;
using SCTP4CS.Utils;

/**
 *
 * @author Westhawk Ltd<thp@westhawk.co.uk>
 * 
 * <code> 
 * 3.3.11.  Cookie Echo (COOKIE ECHO) (10)

   This chunk is used only during the initialization of an association.
   It is sent by the initiator of an association to its peer to complete
   the initialization process.  This chunk MUST precede any DATA chunk
   sent within the association, but MAY be bundled with one or more DATA
   chunks in the same packet.

        0                   1                   2                   3
        0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
       +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
       |   Type = 10   |Chunk  Flags   |         Length                |
       +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
       /                     Cookie                                    /
       \                                                               \
       +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+

   Chunk Flags: 8 bit

      Set to 0 on transmit and ignored on receipt.

   Length: 16 bits (unsigned integer)

      Set to the size of the chunk in bytes, including the 4 bytes of
      the chunk header and the size of the cookie.





Stewart                     Standards Track                    [Page 50]

RFC 4960          Stream Control Transmission Protocol    September 2007


   Cookie: variable size

      This field must contain the exact cookie received in the State
      Cookie parameter from the previous INIT ACK.

      An implementation SHOULD make the cookie as small as possible to
      ensure interoperability.

      Note: A Cookie Echo does NOT contain a State Cookie parameter;
      instead, the data within the State Cookie's Parameter Value
      becomes the data within the Cookie Echo's Chunk Value.  This
      allows an implementation to change only the first 2 bytes of the
      State Cookie parameter to become a COOKIE ECHO chunk.
 * </code>
 */
namespace pe.pi.sctp4j.sctp.messages {
	public class CookieEchoChunk : Chunk {
		private byte[] _cookieData;

		public CookieEchoChunk(CType type, byte flags, int length, ByteBuffer pkt) : base(type, flags, length, pkt) {
			_cookieData = new byte[_body.remaining()];
			_body.GetBytes(_cookieData, _cookieData.Length);
		}

		public CookieEchoChunk() : base(CType.COOKIE_ECHO) { }

		public override void validate() {
			if (_cookieData.Length != Association.COOKIESIZE) {
				throw new SctpPacketFormatException("cookie Echo wrong length for our association " + _cookieData.Length + " != " + Association.COOKIESIZE);
			}
		}

		public void setCookieData(byte[] cd) {
			_cookieData = cd;
		}

		public byte[] getCookieData() {
			return _cookieData;
		}

		protected override void putFixedParams(ByteBuffer ret) {
			Logger.Debug("cookie is " + _cookieData + "and buffer is " + ret);
			ret.Put(_cookieData);
		}
	}
}
