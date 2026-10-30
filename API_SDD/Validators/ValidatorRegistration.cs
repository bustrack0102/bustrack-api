using System.Reflection;
using System.Runtime.CompilerServices;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ApiSDD.Validators;

/// <summary>
/// nv-02: registra los validators espejo del assembly en DI como Transient.
/// Sin paquete DI extra (constitución §3: sin librerías nuevas; usa el core
/// FluentValidation ya aprobado). Los services futuros llaman
/// <c>ValidateAndThrowAsync</c>; el middleware mapea a 422.
/// </summary>
internal static class ValidatorRegistration
{
    public static IServiceCollection AddValidatorsFromAssembly(
        this IServiceCollection services,
        Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assembly);

        foreach (var type in assembly.GetTypes())
        {
            if (type.IsAbstract || type.IsInterface || type.IsNested || IsCompilerGenerated(type))
            {
                continue;
            }

            foreach (var contract in MirrorContracts(type))
            {
                services.AddTransient(contract, type);
            }
        }

        return services;
    }

    private static bool IsCompilerGenerated(Type type) =>
        type.IsDefined(typeof(CompilerGeneratedAttribute), false);

    private static IEnumerable<Type> MirrorContracts(Type type)
    {
        foreach (var contract in type.GetInterfaces())
        {
            if (contract.IsGenericType
                && contract.GetGenericTypeDefinition() == typeof(IValidator<>)
                && InheritsMirror(type, contract))
            {
                yield return contract;
            }
        }
    }

    private static bool InheritsMirror(Type type, Type contract)
    {
        var current = type.BaseType;
        while (current is not null)
        {
            if (current.IsGenericType
                && current.GetGenericTypeDefinition() == typeof(AbstractValidator<>)
                && current.GetGenericArguments()[0] == contract.GetGenericArguments()[0])
            {
                return true;
            }

            current = current.BaseType;
        }

        return false;
    }
}
