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
using pe.pi.sctp4j.sctp.messages;
using System;
using System.Collections.Generic;

/**
 *
 * @author tim
 */
namespace pe.pi.sctp4j.sctp.behave {
	internal class OrderedStreamBehaviour : SCTPStreamBehaviour {

		protected bool _ordered = true;

		public void deliver(SCTPStream s, SortedArray<DataChunk> stash, SCTPStreamListener l) {
			//stash is the list of all DataChunks that have not yet been turned into whole messages
			//we assume that it is sorted by stream sequence number.
			List<DataChunk> delivered = new List<DataChunk>();
			SortedArray<DataChunk> message = null;
			if (stash.Count == 0) {
				return; // I'm not fond of these early returns 
			}
			long expectedTsn = stash.First.getTsn(); // This ignores gaps - but _hopefully_ messageNo will catch any
													 // gaps we care about - ie gaps in the sequence for _this_ stream 
													 // we can deliver ordered messages on this stream even if earlier messages from other streams are missing
													 // - this does assume that the tsn's of a message are contiguous -which is odd.


			foreach (DataChunk dc in stash) {
				int messageNo = s.getNextMessageSeqIn();

				int flags = dc.getFlags() & DataChunk.SINGLEFLAG; // mask to the bits we want
				long tsn = dc.getTsn();
				bool lookingForOrderedMessages = _ordered || (message != null);
				// which is to say for unordered messages we can tolerate gaps _between_ messages
				// but not within them
				if (lookingForOrderedMessages && (tsn != expectedTsn)) {
					Logger.Debug("Hole in chunk sequence  " + tsn + " expected " + expectedTsn);
					break;
				}
				switch (flags) {
					case DataChunk.SINGLEFLAG:
						// singles are easy - just dispatch.
						if (_ordered && (messageNo != dc.getSSeqNo())) {
							Logger.Debug("Hole (single) in message sequence  " + dc.getSSeqNo() + " expected " + messageNo);
							break; // not the message we are looking for...
						}
						SCTPMessage single = new SCTPMessage(s, dc);
						if (single.deliver(l)) {
							delivered.Add(dc);
							messageNo++;
							s.setNextMessageSeqIn(messageNo);
						}
						break;
					case DataChunk.BEGINFLAG:
						if (_ordered && (messageNo != dc.getSSeqNo())) {
							Logger.Debug("Hole (begin) in message sequence  " + dc.getSSeqNo() + " expected " + messageNo);
							break; // not the message we are looking for...
						}
						message = new SortedArray<DataChunk>();
						message.Add(dc);
						Logger.Trace("new message no" + dc.getSSeqNo() + " starts with  " + dc.getTsn());
						break;
					case 0: // middle 
						if (message != null) {
							message.Add(dc);
							Logger.Trace("continued message no" + dc.getSSeqNo() + " with  " + dc.getTsn());
						} else {
							// perhaps check sno ?
							Logger.Debug("Middle with no start" + dc.getSSeqNo() + " tsn " + dc.getTsn());
						}
						break;
					case DataChunk.ENDFLAG:
						if (message != null) {
							message.Add(dc);
							Logger.Trace("finished message no" + dc.getSSeqNo() + " with  " + dc.getTsn());
							SCTPMessage deliverable = new SCTPMessage(s, message);
							if (deliverable.deliver(l)) {
								message.AddToList(delivered);
								messageNo++;
								s.setNextMessageSeqIn(messageNo);
							}
							message = null;
						} else {
							Logger.Debug("End with no start" + dc.getSSeqNo() + " tsn " + dc.getTsn());
							message = null;
						}
						break;
					default:
						throw new Exception("[IllegalStateException] Impossible value in stream logic");
				}
				expectedTsn = tsn + 1;
			}
			stash.RemoveWhere((dc) => { return delivered.Contains(dc); });
		}

		public Chunk[] respond(SCTPStream a) {
			return null;
		}

	}
}
