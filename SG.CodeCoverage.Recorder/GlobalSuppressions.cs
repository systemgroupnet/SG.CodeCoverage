// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.


using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "The exceptions should be sent back to the client that invoked the command.", Scope = "member", Target = "~M:SG.CodeCoverage.Recorder.RecordingController.Server.ProcessCommand(System.String)~System.String")]
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "This method is async and is not awaited anywhere, so it should not raise any exceptions.", Scope = "member", Target = "~M:SG.CodeCoverage.Recorder.RecordingController.Server.StartAsync(System.Int32)~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "This method is async and is not awaited anywhere, so it should not raise any exceptions.", Scope = "member", Target = "~M:SG.CodeCoverage.Recorder.RecordingController.Server.AcceptAsync(System.Net.Sockets.TcpClient)~System.Threading.Tasks.Task")]
