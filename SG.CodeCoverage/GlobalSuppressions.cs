// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "I don't know the exact excpetions that `ReadAssembly` may throw. Also they are not critical.", Scope = "member", Target = "~M:SG.CodeCoverage.Instrumentation.AssemblyResolver.Resolve(Mono.Cecil.AssemblyNameReference)~Mono.Cecil.AssemblyDefinition")]
