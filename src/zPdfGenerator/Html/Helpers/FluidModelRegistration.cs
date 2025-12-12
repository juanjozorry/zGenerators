using Fluid;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace zPdfGenerator.Html.Helpers
{
    internal static class FluidModelRegistration
    {
        /// <summary>
        /// Registers CLR types used by a model (dictionary/list/object graph) into Fluid MemberAccessStrategy.
        /// Skips primitives/common framework types and is optimized to avoid deep/expensive scans.
        /// </summary>
        public static void RegisterModelTypes(
            TemplateOptions options,
            object? model,
            int maxDepth = 6,
            int maxTypes = 256)
        {
            if (options is null) throw new ArgumentNullException(nameof(options));
            if (model is null) return;

            var visitedTypes = new HashSet<Type>();
            var visitedRefs = new HashSet<object>(ReferenceEqualityComparer.Instance);
            int registeredCount = 0;

            void RegisterType(Type t)
            {
                if (registeredCount >= maxTypes) return;

                t = Nullable.GetUnderlyingType(t) ?? t;

                if (ShouldSkipType(t)) return;
                if (!visitedTypes.Add(t)) return;

                options.MemberAccessStrategy.Register(t);
                registeredCount++;
            }

            void Walk(object? value, int depth)
            {
                if (value is null) return;
                if (depth > maxDepth) return;
                if (registeredCount >= maxTypes) return;

                var t = value.GetType();
                t = Nullable.GetUnderlyingType(t) ?? t;

                if (ShouldSkipType(t)) return;

                // Avoid reference cycles for reference types
                if (!t.IsValueType && !(value is string))
                {
                    if (!visitedRefs.Add(value)) return;
                }

                // Register the value's type if useful
                RegisterType(t);

                // Dictionaries: walk values (keys are typically string)
                if (value is IDictionary dict)
                {
                    foreach (DictionaryEntry entry in dict)
                    {
                        // keys are often string; ignore keys, walk values
                        Walk(entry.Value, depth + 1);

                        if (registeredCount >= maxTypes) break;
                    }
                    return;
                }

                // IEnumerable (lists/arrays): register collection type + first non-null element type
                if (value is IEnumerable en && value is not string)
                {
                    // Register generic element type if we can find it cheaply
                    var elemType = TryGetElementType(t);
                    if (elemType is not null) RegisterType(elemType);

                    foreach (var item in en)
                    {
                        if (item is null) continue;
                        Walk(item, depth + 1);
                        break; // only the first item for efficiency
                    }
                    return;
                }

                // Plain CLR object: register property types shallowly (optional but helpful)
                // We only register property types, we don't read property values (avoid side effects)
                foreach (var p in t.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    if (!p.CanRead) continue;
                    if (p.GetIndexParameters().Length != 0) continue;

                    var pt = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
                    if (ShouldSkipType(pt)) continue;

                    RegisterType(pt);

                    if (registeredCount >= maxTypes) break;
                }
            }

            Walk(model, depth: 0);
        }

        private static bool ShouldSkipType(Type t)
        {
            // primitives
            if (t.IsPrimitive) return true;

            // common leaf types we never need to register explicitly
            if (t == typeof(string) ||
                t == typeof(decimal) ||
                t == typeof(DateTime) ||
                t == typeof(DateTimeOffset) ||
                t == typeof(TimeSpan) ||
                t == typeof(Guid) ||
                t == typeof(Uri))
                return true;

            // enums are fine without registration (render as string/number)
            if (t.IsEnum) return true;

            // Json types should not be registered; convert them to object graph earlier
            if (t == typeof(JsonElement) ||
                t == typeof(JsonDocument) ||
                typeof(JsonNode).IsAssignableFrom(t))
                return true;

            // Skip System.* / Microsoft.* framework types (usually not what you want to expose)
            // BUT allow collections/dictionaries since those are useful in templates.
            if ((t.Namespace?.StartsWith("System.", StringComparison.Ordinal) ?? false) ||
                (t.Namespace?.StartsWith("Microsoft.", StringComparison.Ordinal) ?? false))
            {
                // allow collections/dicts
                if (typeof(IDictionary).IsAssignableFrom(t)) return false;
                if (typeof(IEnumerable).IsAssignableFrom(t) && t != typeof(string)) return false;

                return true;
            }

            // allow your own domain types
            return false;
        }

        private static Type? TryGetElementType(Type t)
        {
            // arrays
            if (t.IsArray) return t.GetElementType();

            // IEnumerable<T>
            foreach (var it in t.GetInterfaces().Append(t))
            {
                if (!it.IsGenericType) continue;
                var def = it.GetGenericTypeDefinition();
                if (def == typeof(IEnumerable<>)
                    || def == typeof(ICollection<>)
                    || def == typeof(IList<>)
                    || def == typeof(IReadOnlyCollection<>)
                    || def == typeof(IReadOnlyList<>))
                {
                    return Nullable.GetUnderlyingType(it.GetGenericArguments()[0]) ?? it.GetGenericArguments()[0];
                }
            }

            return null;
        }

        private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
        {
            public static readonly ReferenceEqualityComparer Instance = new();
            public new bool Equals(object? x, object? y) => ReferenceEquals(x, y);
            public int GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
        }
    }

}
