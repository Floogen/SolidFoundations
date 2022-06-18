using System;
using HarmonyLib;
using System.Linq.Expressions;
using System.Reflection;

#nullable enable

namespace SolidFoundations.Framework.Integrations;
internal static class DGAIntegration
{
    internal const string DGAModataKey = "spacechase0.DynamicGameAssets/preserved-parent-ID";

    private static Lazy<Func<object, bool>?> isDGASObject = new(() =>
    {
        Type? dgaSObject = AccessTools.TypeByName("DynamicGameAssets.Game.CustomObject, DynamicGameAssets");
        if (dgaSObject is null)
            return null;
        var obj = Expression.Parameter(typeof(object));
        var isinst = Expression.TypeIs(obj, dgaSObject);
        return Expression.Lambda<Func<object, bool>>(isinst, obj).Compile();
    });

    /// <summary>
    /// A function that returns true if an object is a DGA custom object.
    /// If null, means DGA wasn't loaded (or could not grab the custom type).
    /// </summary>
    internal static Func<object, bool>? IsDGASObject => isDGASObject.Value;

    private static Lazy<Func<object, string?>?> getDGAFullID = new(() =>
    {
        Type? dgaSObject = AccessTools.TypeByName("DynamicGameAssets.Game.CustomObject, DynamicGameAssets");
        if (dgaSObject is null)
            return null;

        var obj = Expression.Parameter(typeof(object));
        var isinst = Expression.TypeIs(obj, dgaSObject);

        var loc = Expression.Parameter(typeof(string), "fullID");

        var returnnull = Expression.Assign(loc, Expression.Constant(null));

        MethodInfo idGetter = dgaSObject.GetProperty("FullId").GetGetMethod() ?? throw new InvalidOperationException("DGA SObject's FullId not found....");
        var getID = Expression.Assign(loc, Expression.Call(obj, idGetter));

        var branch = Expression.IfThenElse(isinst, getID, returnnull);
        return Expression.Lambda<Func<object, string?>>(branch, obj).Compile();
    });

    /// <summary>
    /// A function that returns a DGA object's full ID (if applicable).
    /// Null otherwise.
    /// </summary>
    internal static Func<object, string?>? GetDGAFullID => getDGAFullID.Value;
}
