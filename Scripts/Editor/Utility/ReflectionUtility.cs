using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace BrunoMikoski.UIManager.Utility
{
    public static partial class ReflectionHelpers
    {
        /// <summary>Copy the fields from one object to another</summary>
        /// <param name="src">The source object to copy from</param>
        /// <param name="dst">The destination object to copy to</param>
        /// <param name="bindingAttr">The mask to filter the attributes.
        /// Only those fields that get caught in the filter will be copied</param>
        public static void CopyFields(Object src, object dst, BindingFlags bindingAttr = BindingFlags.Public
                                                                                         | BindingFlags.NonPublic
                                                                                         | BindingFlags.Instance)
        {
            if (src != null && dst != null)
            {
                Type type = src.GetType();
                FieldInfo[] fields = type.GetFields(bindingAttr);
                for (int i = 0; i < fields.Length; ++i)
                    if (!fields[i].IsStatic)
                        fields[i].SetValue(dst, fields[i].GetValue(src));
            }
        }

        /// <summary>Search the assembly for all types that match a predicate</summary>
        /// <param name="assembly">The assembly to search</param>
        /// <param name="predicate">The type to look for</param>
        /// <returns>A list of types found in the assembly that inherit from the predicate</returns>
        public static IEnumerable<Type> GetTypesInAssembly(Assembly assembly, Predicate<Type> predicate)
        {
            if (assembly == null)
                return null;

            Type[] types = new Type[0];
            try
            {
                types = assembly.GetTypes();
            }
            catch (Exception)
            {
                // Can't load the types in this assembly
            }
            types = (from t in types
                     where t != null && predicate(t)
                     select t).ToArray();
            return types;
        }

        /// <summary>Cheater extension to access internal field of an object</summary>
        /// <typeparam name="T">The field type</typeparam>
        /// <param name="type">The type of the field</param>
        /// <param name="obj">The object to access</param>
        /// <param name="memberName">The string name of the field to access</param>
        /// <returns>The value of the field in the objects</returns>
        public static T AccessInternalField<T>(this Type type, object obj, string memberName)
        {
            if (string.IsNullOrEmpty(memberName) || (type == null))
                return default(T);

            BindingFlags bindingFlags = BindingFlags.NonPublic;
            if (obj != null)
                bindingFlags |= BindingFlags.Instance;
            else
                bindingFlags |= BindingFlags.Static;

            FieldInfo field = type.GetField(memberName, bindingFlags);
            if ((field != null) && (field.FieldType == typeof(T)))
                return (T)field.GetValue(obj);
            
            return default(T);
        }
        
        /// <summary>Get the object owner of a field.  This method processes
        /// the '.' separator to get from the object that owns the compound field
        /// to the object that owns the leaf field</summary>
        /// <param name="path">The name of the field, which may contain '.' separators</param>
        /// <param name="obj">the owner of the compound field</param>
        /// <returns>The object owner of the field</returns>
        public static object GetParentObject(string path, object obj)
        {
            string[] fields = path.Split('.');
            if (fields.Length == 1)
                return obj;

            FieldInfo info = obj.GetType().GetField(
                    fields[0], BindingFlags.Public 
                        | BindingFlags.NonPublic 
                        | BindingFlags.Instance);
            obj = info.GetValue(obj);

            return GetParentObject(string.Join(".", fields, 1, fields.Length - 1), obj);
        }

        /// <summary>Returns a string path from an expression - mostly used to retrieve serialized properties
        /// without hardcoding the field path. Safer, and allows for proper refactoring.</summary>
        /// <typeparam name="TType">Magic expression</typeparam>
        /// <typeparam name="TValue">Magic expression</typeparam>
        /// <param name="expr">Magic expression</param>
        /// <returns>The string version of the field path</returns>
        public static string GetFieldPath<TType, TValue>(Expression<Func<TType, TValue>> expr)
        {
            MemberExpression me;
            switch (expr.Body.NodeType)
            {
                case ExpressionType.MemberAccess:
                    me = expr.Body as MemberExpression;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            List<string> members = new List<string>();
            while (me != null)
            {
                members.Add(me.Member.Name);
                me = me.Expression as MemberExpression;
            }

            StringBuilder sb = new StringBuilder();
            for (int i = members.Count - 1; i >= 0; i--)
            {
                sb.Append(members[i]);
                if (i > 0) sb.Append('.');
            }
            return sb.ToString();
        }
    }
}
