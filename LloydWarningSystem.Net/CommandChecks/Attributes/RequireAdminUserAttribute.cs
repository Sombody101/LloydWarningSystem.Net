﻿using DSharpPlus.Commands.ContextChecks;

namespace LloydWarningSystem.Net.CommandChecks.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RequireAdminUserAttribute : ContextCheckAttribute
{
}
