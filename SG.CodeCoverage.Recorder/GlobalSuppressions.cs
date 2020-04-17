// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1825:Avoid zero-length array allocations.", Justification = "The `TypesCount` field will be modified later by `Instrumenter`", Scope = "member", Target = "~M:SG.CodeCoverage.Recorder.HitsRepository.GetAndResetHits~System.Int32[][]")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1825:Avoid zero-length array allocations.", Justification = "The `TypesCount` field will be modified later by `Instrumenter`", Scope = "member", Target = "~F:SG.CodeCoverage.Recorder.HitsRepository.TypeMethodsHits")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "The exceptions should be sent back to the client that invoked the command.", Scope = "member", Target = "~M:SG.CodeCoverage.Recorder.RecordingControllerServer.ProcessCommand(System.String)~System.String")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "This method is async and is not awaited anywhere, so it should not raise any exceptions.", Scope = "member", Target = "~M:SG.CodeCoverage.Recorder.RecordingControllerServer.StartAsync(System.Int32)~System.Threading.Tasks.Task")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "This method is async and is not awaited anywhere, so it should not raise any exceptions.", Scope = "member", Target = "~M:SG.CodeCoverage.Recorder.RecordingControllerServer.AcceptAsync(System.Net.Sockets.TcpClient)~System.Threading.Tasks.Task")]
