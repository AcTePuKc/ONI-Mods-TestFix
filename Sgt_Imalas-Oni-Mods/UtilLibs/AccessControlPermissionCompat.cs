using System;
using System.Linq;
using System.Reflection;

namespace UtilLibs
{
        public static class AccessControlPermissionCompat
        {
                private static readonly Func<AccessControl, AccessControl.Permission> DefaultGetter = BuildDefaultGetter();
                private static readonly Action<AccessControl, AccessControl.Permission> DefaultSetter = BuildDefaultSetter();

                public static AccessControl.Permission GetDoorDefaultPermission(AccessControl accessControl)
                {
                        if (accessControl == null)
                                throw new ArgumentNullException(nameof(accessControl));

                        try
                        {
                                return DefaultGetter(accessControl);
                        }
                        catch (Exception e)
                        {
                                SgtLogger.error($"Failed to retrieve default permission via compatibility shim:\n{e}");
                                return AccessControl.Permission.Both;
                        }
                }

                public static void SetDoorDefaultPermission(AccessControl accessControl, AccessControl.Permission permission)
                {
                        if (accessControl == null)
                                throw new ArgumentNullException(nameof(accessControl));

                        try
                        {
                                DefaultSetter(accessControl, permission);
                        }
                        catch (Exception e)
                        {
                                SgtLogger.error($"Failed to assign default permission via compatibility shim:\n{e}");
                        }
                }

                private static Func<AccessControl, AccessControl.Permission> BuildDefaultGetter()
                {
                        var accessControlType = typeof(AccessControl);

                        var property = accessControlType.GetProperty("DefaultPermission", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (property?.CanRead == true && property.PropertyType == typeof(AccessControl.Permission))
                        {
                                var getter = property.GetGetMethod(true);
                                if (getter != null)
                                        return CreateGetterDelegate(getter);
                        }

                        var method = accessControlType
                                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                .FirstOrDefault(candidate => candidate.ReturnType == typeof(AccessControl.Permission)
                                        && candidate.GetParameters().Length == 0
                                        && candidate.Name.IndexOf("Default", StringComparison.OrdinalIgnoreCase) >= 0);

                        if (method != null)
                        {
                                return accessControl => (AccessControl.Permission)method.Invoke(accessControl, null);
                        }

                        var field = FindPermissionField(accessControlType);
                        if (field != null)
                        {
                                SgtLogger.warning($"Falling back to field '{field.Name}' for AccessControl default permission reads.");
                                return accessControl => (AccessControl.Permission)field.GetValue(accessControl);
                        }

                        SgtLogger.error("Unable to locate any API for AccessControl default permission reads; returning Both as a fallback.");
                        return _ => AccessControl.Permission.Both;
                }

                private static Action<AccessControl, AccessControl.Permission> BuildDefaultSetter()
                {
                        var accessControlType = typeof(AccessControl);

                        var property = accessControlType.GetProperty("DefaultPermission", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (property?.CanWrite == true && property.PropertyType == typeof(AccessControl.Permission))
                        {
                                var setter = property.GetSetMethod(true);
                                if (setter != null)
                                        return (accessControl, permission) => setter.Invoke(accessControl, new object[] { permission });
                        }

                        var method = accessControlType
                                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                .FirstOrDefault(candidate => candidate.ReturnType == typeof(void)
                                        && candidate.GetParameters().Length == 1
                                        && candidate.GetParameters()[0].ParameterType == typeof(AccessControl.Permission)
                                        && candidate.Name.IndexOf("Default", StringComparison.OrdinalIgnoreCase) >= 0);

                        if (method != null)
                        {
                                return (accessControl, permission) => method.Invoke(accessControl, new object[] { permission });
                        }

                        var field = FindPermissionField(accessControlType);
                        if (field != null)
                        {
                                SgtLogger.warning($"Falling back to field '{field.Name}' for AccessControl default permission writes.");
                                return (accessControl, permission) => field.SetValue(accessControl, permission);
                        }

                        SgtLogger.error("Unable to locate any API for AccessControl default permission writes; assignments will be ignored.");
                        return (_, _) => { };
                }

                private static Func<AccessControl, AccessControl.Permission> CreateGetterDelegate(MethodInfo getter)
                {
                        return accessControl => (AccessControl.Permission)getter.Invoke(accessControl, null);
                }

                private static FieldInfo FindPermissionField(Type accessControlType)
                {
                        return accessControlType
                                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                .Where(field => field.FieldType == typeof(AccessControl.Permission))
                                .OrderByDescending(field => field.Name.IndexOf("default", StringComparison.OrdinalIgnoreCase) >= 0)
                                .ThenBy(field => field.Name)
                                .FirstOrDefault();
                }
        }
}
