using UnityEngine;
using KSerialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Klei.AI;

namespace DietVariety
{
    [SerializationConfig(MemberSerialization.OptIn)]
    class VarietyMonitor : KMonoBehaviour
    {
        public const string EFFECT_ID = "DietVarietyEffect";

        private struct EatCompleteSubscription
        {
            public object Handle;
            public Delegate Handler;
            public object Target;
        }

        private struct TagSubscription
        {
            public object Handle;
            public Delegate Handler;
            public KPrefabID Prefab;
            public bool UsedModernApi;
        }

        private EatCompleteSubscription eatCompleteSubscription;
        private TagSubscription deadTagSubscription;

        protected override void OnSpawn()
        {
            base.OnSpawn();

            this.eatCompleteSubscription = SubscribeToEatComplete();
            this.deadTagSubscription = SubscribeToDeadTag();

            InitalizeEffect();
        }

        protected override void OnCleanUp()
        {
            UnsubscribeFromEatComplete(eatCompleteSubscription);
            UnsubscribeFromDeadTag(deadTagSubscription);

            base.OnCleanUp();
        }

        private EatCompleteSubscription SubscribeToEatComplete()
        {
            var eater = this.GetComponent<Eater>();
            if (eater != null && EventBindingAdapter.TrySubscribeToEatComplete(eater, this, out var handle, out var handler))
                return new EatCompleteSubscription { Handle = handle, Handler = handler, Target = eater };

            this.Subscribe((int)GameHashes.EatCompleteEater, OnEatCompleteLegacy);
            return default;
        }

        private void UnsubscribeFromEatComplete(EatCompleteSubscription subscription)
        {
            if (subscription.Target is Eater eater && EventBindingAdapter.TryUnsubscribeFromEatComplete(eater, subscription.Handle, subscription.Handler))
                return;

            this.Unsubscribe((int)GameHashes.EatCompleteEater, OnEatCompleteLegacy);
        }

        private TagSubscription SubscribeToDeadTag()
        {
            var prefab = this.GetComponent<KPrefabID>();
            if (prefab != null && TagBindingAdapter.TrySubscribeToTagAdded(prefab, GameTags.Dead, this, out var handle, out var handler))
                return new TagSubscription { Handle = handle, Handler = handler, Prefab = prefab, UsedModernApi = true };

            GameUtil.SubscribeToTags<VarietyMonitor>(this, OnDeadTagAddedLegacy, true);
            return default;
        }

        private void UnsubscribeFromDeadTag(TagSubscription subscription)
        {
            if (subscription.UsedModernApi)
            {
                if (subscription.Prefab != null)
                    TagBindingAdapter.TryUnsubscribeFromTagAdded(subscription.Prefab, GameTags.Dead, subscription.Handle, subscription.Handler);
                return;
            }

            GameUtil.UnsubscribeToTags<VarietyMonitor>(this, OnDeadTagAddedLegacy, true);
        }

        private void OnEatCompleteLegacy(object data)
        {
            HandleMealCompletion(null, data);
        }

        private void OnEatCompleteEvent(object source, object payload)
        {
            HandleMealCompletion(source, payload);
        }

        private void HandleMealCompletion(object source, object payload)
        {
            var edible = ExtractEdible(source) ?? ExtractEdible(payload);
            if (edible == null)
                return;

            string id = edible.FoodID;
            PastMealsEaten.Instance?.RegisterNewMeal(this.gameObject, id);
            RefreshEffect();
        }

        private void OnDeath(object data)
        {
            if(PastMealsEaten.Instance != null && this.gameObject != null)
                PastMealsEaten.Instance.StopTrackingDeadDupe(this.gameObject);
        }

        private void HandleDeadTagEvent(object source, object payload)
        {
            Tag? tag = ExtractTag(payload) ?? ExtractTag(source);
            if (tag.HasValue && tag.Value == GameTags.Dead)
                OnDeath(payload);
        }

        private static readonly EventSystem.IntraObjectHandler<VarietyMonitor> OnDeadTagAddedLegacy = GameUtil.CreateHasTagHandler<VarietyMonitor>(GameTags.Dead, (component, data) => component.OnDeath(data));

        private void InitalizeEffect()
        {
            Effects effects = this.gameObject.GetComponent<Effects>();
            if (effects == null)
                return;

            if (!effects.HasEffect(EFFECT_ID))
                effects.Add(GetEffect(), true);
        }

        public void RefreshEffect()
        {
            Effects effects = this.gameObject.GetComponent<Effects>();
            if (effects == null)
                return;

            effects.Remove(EFFECT_ID);
            effects.Add(GetEffect(), true);
        }

        private int GetUniqueCount()
        {
            return PastMealsEaten.Instance.GetUniqueMealsCount(this.gameObject);
        }

        public Effect GetEffect()
        {
            int uniqueCount = GetUniqueCount();
            float duration = 0;
            int moraleBonus = Mathf.FloorToInt(Settings.Instance.MoralePerFoodType * (uniqueCount - Settings.Instance.MinFoodTypesRequired));
            string name = string.Format(STRINGS.EFFECTS.VARIED_DIET.NAME, moraleBonus);
            string desc = string.Format(STRINGS.EFFECTS.VARIED_DIET.DESC, uniqueCount, Settings.Instance.MaxMealsCounted);

            Effect effect = new Effect(EFFECT_ID, name, desc, duration, true, false, false);
            effect.SelfModifiers = new List<AttributeModifier>();
            effect.SelfModifiers.Add(new AttributeModifier(Db.Get().Attributes.QualityOfLife.Id, moraleBonus, STRINGS.EFFECTS.VARIED_DIET.REASON));

            return effect;
        }

        private Edible ExtractEdible(object candidate)
        {
            if (candidate == null)
                return null;

            if (candidate is Edible edible)
                return edible;

            if (candidate is GameObject go)
                return go.GetComponent<Edible>();

            var type = candidate.GetType();

            // Common field/property names for the new event payload.
            const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            foreach (string memberName in new[] { "edible", "Edible", "food", "Food", "item", "Item", "payload", "Payload" })
            {
                var field = type.GetField(memberName, bindingFlags);
                if (field != null)
                {
                    var value = field.GetValue(candidate);
                    if (value is Edible fieldEdible)
                        return fieldEdible;
                    if (value is GameObject fieldGo)
                        return fieldGo.GetComponent<Edible>();
                }

                var property = type.GetProperty(memberName, bindingFlags);
                if (property != null)
                {
                    var value = property.GetValue(candidate, null);
                    if (value is Edible propertyEdible)
                        return propertyEdible;
                    if (value is GameObject propertyGo)
                        return propertyGo.GetComponent<Edible>();
                }
            }

            return null;
        }

        private Tag? ExtractTag(object candidate)
        {
            if (candidate == null)
                return null;

            if (candidate is Tag tagCandidate)
                return tagCandidate;

            if (candidate is GameObject go)
                return go.HasTag(GameTags.Dead) ? GameTags.Dead : (Tag?)null;

            var type = candidate.GetType();
            const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            foreach (string memberName in new[] { "tag", "Tag" })
            {
                var field = type.GetField(memberName, bindingFlags);
                if (field != null)
                {
                    var value = field.GetValue(candidate);
                    if (value is Tag fieldTag)
                        return fieldTag;
                }

                var property = type.GetProperty(memberName, bindingFlags);
                if (property != null)
                {
                    var value = property.GetValue(candidate, null);
                    if (value is Tag propertyTag)
                        return propertyTag;
                }
            }

            return null;
        }

        private static class EventBindingAdapter
        {
            private static readonly MethodInfo[] SubscribeCandidates;
            private static readonly MethodInfo[] UnsubscribeCandidates;

            static EventBindingAdapter()
            {
                SubscribeCandidates = FindMethods(typeof(Eater), "Subscribe", "EatComplete");
                UnsubscribeCandidates = FindMethods(typeof(Eater), "Unsubscribe", "EatComplete");
            }

            private static MethodInfo[] FindMethods(Type type, string methodContains, string nameContains)
            {
                return type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(m => m.Name.IndexOf(methodContains, System.StringComparison.OrdinalIgnoreCase) >= 0
                             && m.Name.IndexOf(nameContains, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToArray();
            }

            internal static bool TrySubscribeToEatComplete(Eater eater, VarietyMonitor monitor, out object handle, out Delegate handler)
            {
                handle = null;
                handler = null;

                foreach (var method in SubscribeCandidates)
                {
                    if (TryInvoke(method, eater, monitor, out handle, out handler))
                        return true;
                }

                return false;
            }

            internal static bool TryUnsubscribeFromEatComplete(Eater eater, object handle, Delegate handler)
            {
                foreach (var method in UnsubscribeCandidates)
                {
                    if (TryInvoke(method, eater, handle, handler))
                        return true;
                }

                return false;
            }

            private static bool TryInvoke(MethodInfo method, Eater eater, VarietyMonitor monitor, out object handle, out Delegate handler)
            {
                handle = null;
                handler = null;

                var parameters = method.GetParameters();
                var args = new object[parameters.Length];
                Delegate candidateDelegate = null;

                for (int i = 0; i < parameters.Length; i++)
                {
                    var parameterType = parameters[i].ParameterType;
                    if (typeof(Delegate).IsAssignableFrom(parameterType) || parameterType.BaseType == typeof(MulticastDelegate))
                    {
                        candidateDelegate = DelegateFactory.CreateForEatComplete(parameterType, monitor);
                        if (candidateDelegate == null)
                            return false;
                        args[i] = candidateDelegate;
                    }
                    else if (parameterType == typeof(bool))
                    {
                        args[i] = true;
                    }
                    else if (parameterType.IsValueType && parameterType.Name.IndexOf("Handle", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        args[i] = System.Activator.CreateInstance(parameterType);
                    }
                    else
                    {
                        return false;
                    }
                }

                var result = method.Invoke(eater, args);
                handle = result;
                if (handle == null)
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (parameters[i].ParameterType.IsByRef)
                            handle = args[i];
                    }
                }

                handler = candidateDelegate;
                return true;
            }

            private static bool TryInvoke(MethodInfo method, Eater eater, object handle, Delegate handler)
            {
                var parameters = method.GetParameters();
                var args = new object[parameters.Length];

                for (int i = 0; i < parameters.Length; i++)
                {
                    var parameterType = parameters[i].ParameterType;

                    if (typeof(Delegate).IsAssignableFrom(parameterType) || parameterType.BaseType == typeof(MulticastDelegate))
                    {
                        args[i] = handler;
                    }
                    else if (parameterType.IsInstanceOfType(handle))
                    {
                        args[i] = handle;
                    }
                    else if (parameterType.IsByRef && handle != null)
                    {
                        args[i] = handle;
                    }
                    else
                    {
                        return false;
                    }
                }

                method.Invoke(eater, args);
                return true;
            }
        }

        private static class TagBindingAdapter
        {
            private static readonly MethodInfo[] SubscribeCandidates;
            private static readonly MethodInfo[] UnsubscribeCandidates;

            static TagBindingAdapter()
            {
                SubscribeCandidates = FindMethods();
                UnsubscribeCandidates = FindMethods(false);
            }

            private static MethodInfo[] FindMethods(bool subscribe = true)
            {
                string keyword = subscribe ? "Subscribe" : "Unsubscribe";
                string tagKeyword = "TagAdded";

                var gameUtilMethods = typeof(GameUtil).GetMethods(BindingFlags.Static | BindingFlags.Public)
                    .Where(m => m.Name.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) >= 0
                             && m.Name.IndexOf(tagKeyword, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToArray();

                if (gameUtilMethods.Length > 0)
                    return gameUtilMethods;

                return typeof(KPrefabID).GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .Where(m => m.Name.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) >= 0
                             && m.Name.IndexOf(tagKeyword, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToArray();
            }

            internal static bool TrySubscribeToTagAdded(KPrefabID prefab, Tag tag, VarietyMonitor monitor, out object handle, out Delegate handler)
            {
                handle = null;
                handler = null;

                foreach (var method in SubscribeCandidates)
                {
                    if (TryInvoke(method, prefab, tag, monitor, out handle, out handler))
                        return true;
                }

                return false;
            }

            internal static bool TryUnsubscribeFromTagAdded(KPrefabID prefab, Tag tag, object handle, Delegate handler)
            {
                foreach (var method in UnsubscribeCandidates)
                {
                    if (TryInvoke(method, prefab, tag, handle, handler))
                        return true;
                }

                return false;
            }

            private static bool TryInvoke(MethodInfo method, KPrefabID prefab, Tag tag, VarietyMonitor monitor, out object handle, out Delegate handler)
            {
                handle = null;
                handler = null;

                var parameters = method.GetParameters();
                var args = new object[parameters.Length];
                Delegate candidateDelegate = null;

                for (int i = 0; i < parameters.Length; i++)
                {
                    var paramType = parameters[i].ParameterType;
                    if (paramType.IsByRef)
                        paramType = paramType.GetElementType();

                    if (paramType == null)
                        return false;

                    if (typeof(Component).IsAssignableFrom(paramType))
                    {
                        args[i] = prefab;
                    }
                    else if (paramType == typeof(GameObject))
                    {
                        args[i] = prefab.gameObject;
                    }
                    else if (paramType == typeof(Tag))
                    {
                        args[i] = tag;
                    }
                    else if (paramType == typeof(bool))
                    {
                        args[i] = true;
                    }
                    else if (typeof(Delegate).IsAssignableFrom(paramType) || paramType.BaseType == typeof(MulticastDelegate))
                    {
                        candidateDelegate = DelegateFactory.CreateForTag(paramType, monitor);
                        if (candidateDelegate == null)
                            return false;
                        args[i] = candidateDelegate;
                    }
                    else if (paramType.Name.IndexOf("Handle", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        args[i] = System.Activator.CreateInstance(paramType);
                    }
                    else
                    {
                        return false;
                    }
                }

                object target = method.IsStatic ? null : prefab;
                var result = method.Invoke(target, args);
                handle = result;

                if (handle == null)
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (parameters[i].ParameterType.IsByRef)
                            handle = args[i];
                    }
                }

                handler = candidateDelegate;
                return true;
            }

            private static bool TryInvoke(MethodInfo method, KPrefabID prefab, Tag tag, object handle, Delegate handler)
            {
                var parameters = method.GetParameters();
                var args = new object[parameters.Length];

                for (int i = 0; i < parameters.Length; i++)
                {
                    var paramType = parameters[i].ParameterType;
                    if (paramType.IsByRef)
                        paramType = paramType.GetElementType();

                    if (paramType == null)
                        return false;

                    if (typeof(Component).IsAssignableFrom(paramType))
                    {
                        args[i] = prefab;
                    }
                    else if (paramType == typeof(GameObject))
                    {
                        args[i] = prefab.gameObject;
                    }
                    else if (paramType == typeof(Tag))
                    {
                        args[i] = tag;
                    }
                    else if (typeof(Delegate).IsAssignableFrom(paramType) || paramType.BaseType == typeof(MulticastDelegate))
                    {
                        args[i] = handler;
                    }
                    else if (handle != null && paramType.IsInstanceOfType(handle))
                    {
                        args[i] = handle;
                    }
                    else if (paramType.IsByRef)
                    {
                        args[i] = handle;
                    }
                    else if (paramType == typeof(bool))
                    {
                        args[i] = true;
                    }
                    else
                    {
                        return false;
                    }
                }

                object target = method.IsStatic ? null : prefab;
                method.Invoke(target, args);
                return true;
            }
        }

        private static class DelegateFactory
        {
            internal static Delegate CreateForEatComplete(Type delegateType, VarietyMonitor monitor)
            {
                return CreateDelegate(delegateType, monitor, nameof(VarietyMonitor.OnEatCompleteEvent));
            }

            internal static Delegate CreateForTag(Type delegateType, VarietyMonitor monitor)
            {
                return CreateDelegate(delegateType, monitor, nameof(VarietyMonitor.HandleDeadTagEvent));
            }

            private static Delegate CreateDelegate(Type delegateType, VarietyMonitor monitor, string methodName)
            {
                var invoke = delegateType.GetMethod("Invoke");
                if (invoke == null)
                    return null;

                var parameters = invoke.GetParameters();
                var parameterExpressions = parameters.Select(p => System.Linq.Expressions.Expression.Parameter(p.ParameterType, p.Name)).ToArray();

                System.Linq.Expressions.Expression sourceExpr = System.Linq.Expressions.Expression.Constant(null, typeof(object));
                System.Linq.Expressions.Expression payloadExpr = System.Linq.Expressions.Expression.Constant(null, typeof(object));

                if (parameterExpressions.Length == 1)
                {
                    payloadExpr = System.Linq.Expressions.Expression.Convert(parameterExpressions[0], typeof(object));
                }
                else if (parameterExpressions.Length >= 2)
                {
                    sourceExpr = System.Linq.Expressions.Expression.Convert(parameterExpressions[0], typeof(object));
                    payloadExpr = System.Linq.Expressions.Expression.Convert(parameterExpressions[1], typeof(object));
                }

                var monitorExpression = System.Linq.Expressions.Expression.Constant(monitor);
                var handlerMethod = typeof(VarietyMonitor).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                var body = System.Linq.Expressions.Expression.Call(monitorExpression, handlerMethod, sourceExpr, payloadExpr);
                var lambda = System.Linq.Expressions.Expression.Lambda(delegateType, body, parameterExpressions);
                return lambda.Compile();
            }
        }
    }
}
