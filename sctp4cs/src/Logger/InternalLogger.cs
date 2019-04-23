﻿using System;

namespace SCTP4CS {
	internal sealed class InternalLogger : ILogger {
		public void Error(string message) {
			Console.WriteLine(message);
			System.Diagnostics.Debug.WriteLine(message);
		}

		public void Warn(string message) {
			Console.WriteLine(message);
			System.Diagnostics.Debug.WriteLine(message);
		}

		public void Info(string message) {
			Console.WriteLine(message);
			System.Diagnostics.Debug.WriteLine(message);
		}
		
		public void Debug(string message) {
			Console.WriteLine(message);
			System.Diagnostics.Debug.WriteLine(message);
		}
		
		public void Trace(string message) {
			Console.WriteLine(message);
			System.Diagnostics.Debug.WriteLine(message);
		}

		public void TraceVerbose(string message) {
			Console.WriteLine(message);
			System.Diagnostics.Debug.WriteLine(message);
		}
	}
}
