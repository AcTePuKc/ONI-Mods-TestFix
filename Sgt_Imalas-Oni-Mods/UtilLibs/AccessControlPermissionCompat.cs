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
            var getterType = typeof(Func<AccessControl, AccessControl.Permission>);

            // 1) Property getter (preferred)
            var property = accessControlType.GetProperty(
                "DefaultPermission",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (property?.CanRead == true && property.PropertyType == typeof(AccessControl.Permission))
            {
                var getter = property.GetGetMethod(true);
                if (getter != null)
                {
                    try
                    {
                        // Open instance delegate: (AccessControl) -> Permission
                        return (Func<AccessControl, AccessControl.Permission>)
                            Delegate.CreateDelegate(getterType, getter);
                    }
                    catch (Exception ex)
                    {
                        SgtLogger.warning($"Failed to bind property getter delegate for DefaultPermission, will try alternatives. {ex.GetType().Name}: {ex.Message}");
                    }
                }
            }

            // 2) Method with no parameters returning Permission and name containing "Default"
            var method = accessControlType
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .FirstOrDefault(candidate =>
                    candidate.ReturnType == typeof(AccessControl.Permission) &&
                    candidate.GetParameters().Length == 0 &&
                    candidate.Name.IndexOf("Default", StringComparison.OrdinalIgnoreCase) >= 0);

            if (method != null)
            {
                try
                {
                    // Open instance delegate: (AccessControl) -> Permission
                    return (Func<AccessControl, AccessControl.Permission>)
                        Delegate.CreateDelegate(getterType, method);
                }
                catch (Exception ex)
                {
                    SgtLogger.warning($"Failed to bind method delegate for default permission getter, will try field. {ex.GetType().Name}: {ex.Message}");
                }
            }

            // 3) Field fallback
            var field = FindPermissionField(accessControlType);
            if (field != null)
            {
                SgtLogger.warning($"Falling back to field '{field.Name}' for AccessControl default permission reads.");
                try
                {
                    var param = System.Linq.Expressions.Expression.Parameter(accessControlType, "accessControl");
                    var fieldAccess = System.Linq.Expressions.Expression.Field(param, field);
                    var lambda = System.Linq.Expressions.Expression.Lambda<Func<AccessControl, AccessControl.Permission>>(fieldAccess, param);
                    return lambda.Compile();
                }
                catch (Exception ex)
                {
                    SgtLogger.warning($"Failed to compile field getter delegate for '{field.Name}', falling back to reflection. {ex.GetType().Name}: {ex.Message}");
                    // Fallback to slower reflection on failure
                    return accessControl => (AccessControl.Permission)field.GetValue(accessControl);
                }
            }

            // 4) Final fallback
            SgtLogger.error("Unable to locate any API for AccessControl default permission reads; returning Both as a fallback.");
            return _ => AccessControl.Permission.Both;
        }

        private static Action<AccessControl, AccessControl.Permission> BuildDefaultSetter()
        {
            var accessControlType = typeof(AccessControl);
            var setterType = typeof(Action<AccessControl, AccessControl.Permission>);

            // 1) Property setter (preferred)
            var property = accessControlType.GetProperty(
                "DefaultPermission",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (property?.CanWrite == true && property.PropertyType == typeof(AccessControl.Permission))
            {
                var setter = property.GetSetMethod(true);
                if (setter != null)
                {
                    try
                    {
                        // Open instance delegate: (AccessControl, Permission) -> void
                        return (Action<AccessControl, AccessControl.Permission>)
                            Delegate.CreateDelegate(setterType, setter);
                    }
                    catch (Exception ex)
                    {
                        SgtLogger.warning($"Failed to bind property setter delegate for DefaultPermission, will try alternatives. {ex.GetType().Name}: {ex.Message}");
                    }
                }
            }

            // 2) Method with one Permission parameter, name contains "Default"
            var method = accessControlType
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .FirstOrDefault(candidate =>
                {
                    var ps = candidate.GetParameters();
                    return candidate.ReturnType == typeof(void) &&
                           ps.Length == 1 &&
                           ps[0].ParameterType == typeof(AccessControl.Permission) &&
                           candidate.Name.IndexOf("Default", StringComparison.OrdinalIgnoreCase) >= 0;
                });

            if (method != null)
            {
                try
                {
                    // Open instance delegate: (AccessControl, Permission) -> void
                    return (Action<AccessControl, AccessControl.Permission>)
                        Delegate.CreateDelegate(setterType, method);
                }
                catch (Exception ex)
                {
                    SgtLogger.warning($"Failed to bind method delegate for default permission setter, will try field. {ex.GetType().Name}: {ex.Message}");
                }
            }

            // 3) Field fallback
            var field = FindPermissionField(accessControlType);
            if (field != null)
            {
                SgtLogger.warning($"Falling back to field '{field.Name}' for AccessControl default permission writes.");
                try
                {
                    var targetExp = System.Linq.Expressions.Expression.Parameter(accessControlType, "target");
                    var valueExp = System.Linq.Expressions.Expression.Parameter(typeof(AccessControl.Permission), "value");
                    var fieldExp = System.Linq.Expressions.Expression.Field(targetExp, field);
                    var assignExp = System.Linq.Expressions.Expression.Assign(fieldExp, valueExp);
                    var lambda = System.Linq.Expressions.Expression.Lambda<Action<AccessControl, AccessControl.Permission>>(assignExp, targetExp, valueExp);
                    return lambda.Compile();
                }
                catch (Exception ex)
                {
                    SgtLogger.warning($"Failed to compile field setter delegate for '{field.Name}', falling back to reflection. {ex.GetType().Name}: {ex.Message}");
                    // Fallback to slower reflection on failure
                    return (accessControl, permission) => field.SetValue(accessControl, permission);
                }
            }

            // 4) Final fallback (no-op)
            SgtLogger.error("Unable to locate any API for AccessControl default permission writes; assignments will be ignored.");
            return (_, _) => { };
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
