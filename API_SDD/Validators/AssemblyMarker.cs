using System.Reflection;

namespace ApiSDD.Validators;

/// <summary>nv-02: expone el assembly donde viven los validators espejo.</summary>
internal static class AssemblyMarker
{
    internal static Assembly Value => typeof(AssemblyMarker).Assembly;
}
