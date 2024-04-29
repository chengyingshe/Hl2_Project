﻿using UnityEngine;
using System.Linq;

namespace Crosstales
{
   /// <summary>Various extension methods.</summary>
   public static class ExtensionMethods
   {
      #region Strings

      /// <summary>
      /// Extension method for strings.
      /// Converts a string to title case (first letter uppercase).
      /// </summary>
      /// <param name="str">String-instance.</param>
      /// <returns>Converted string in title case.</returns>
      public static string CTToTitleCase(this string str)
      {
         if (str == null)
            throw new System.ArgumentNullException(nameof(str));
#if UNITY_WSA
         return toTitleCase(str);
#else
         return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
#endif
      }

#if UNITY_WSA
     /// <summary>
     /// Converts to title case: each word starts with an upper case.
     /// </summary>
     private static string toTitleCase(string str)
     {
         if (str.Length == 0)
             return str;

         System.Text.StringBuilder result = new System.Text.StringBuilder(str);

         result[0] = char.ToUpper(result[0]);

         for (int ii = 1; ii < result.Length; ii++)
         {
             if (char.IsWhiteSpace(result[ii - 1]))
                 result[ii] = char.ToUpper(result[ii]);
             else
                 result[ii] = char.ToLower(result[ii]);
         }

         return result.ToString();
     }
#endif

      /// <summary>
      /// Extension method for strings.
      /// Reverses a string.
      /// </summary>
      /// <param name="str">String-instance.</param>
      /// <returns>Reversed string.</returns>
      public static string CTReverse(this string str)
      {
         if (str == null)
            throw new System.ArgumentNullException(nameof(str));

         char[] charArray = str.ToCharArray();
         System.Array.Reverse(charArray);

         return new string(charArray);
      }

      /// <summary>
      /// Extension method for strings.
      /// Case insensitive 'Replace'.
      /// </summary>
      /// <param name="str">String-instance.</param>
      /// <param name="oldString">String to replace.</param>
      /// <param name="newString">New replacement string.</param>
      /// <param name="comp">StringComparison-method (default: StringComparison.OrdinalIgnoreCase, optional)</param>
      /// <returns>Replaced string.</returns>
      public static string CTReplace(this string str, string oldString, string newString, System.StringComparison comp = System.StringComparison.OrdinalIgnoreCase)
      {
         if (str == null)
            throw new System.ArgumentNullException(nameof(str));

         if (oldString == null)
            throw new System.ArgumentNullException(nameof(oldString));

         if (newString == null)
            throw new System.ArgumentNullException(nameof(newString));

         int index = str.IndexOf(oldString, comp);

         bool MatchFound = index >= 0;

         if (MatchFound)
         {
            str = str.Remove(index, oldString.Length);

            str = str.Insert(index, newString);
         }

         return str;
      }

      /// <summary>
      /// Extension method for strings.
      /// Case insensitive 'Equals'.
      /// </summary>
      /// <param name="str">String-instance.</param>
      /// <param name="toCheck">String to check.</param>
      /// <param name="comp">StringComparison-method (default: StringComparison.OrdinalIgnoreCase, optional)</param>
      /// <returns>True if the string contains the given string.</returns>
      public static bool CTEquals(this string str, string toCheck, System.StringComparison comp = System.StringComparison.OrdinalIgnoreCase)
      {
         if (str == null)
            throw new System.ArgumentNullException(nameof(str));

         //if (toCheck == null)
         //    throw new System.ArgumentNullException("toCheck");

         return str.Equals(toCheck, comp);
      }

      /// <summary>
      /// Extension method for strings.
      /// Case insensitive 'Contains'.
      /// </summary>
      /// <param name="str">String-instance.</param>
      /// <param name="toCheck">String to check.</param>
      /// <param name="comp">StringComparison-method (default: StringComparison.OrdinalIgnoreCase, optional)</param>
      /// <returns>True if the string contains the given string.</returns>
      public static bool CTContains(this string str, string toCheck, System.StringComparison comp = System.StringComparison.OrdinalIgnoreCase)
      {
         if (str == null)
            throw new System.ArgumentNullException(nameof(str));

         //if (toCheck == null)
         //    throw new System.ArgumentNullException("toCheck");

         return str.IndexOf(toCheck, comp) >= 0;
      }

      /// <summary>
      /// Extension method for strings.
      /// Contains any given string.
      /// </summary>
      /// <param name="str">String-instance.</param>
      /// <param name="searchTerms">Search terms separated by the given split-character.</param>
      /// <param name="splitChar">Split-character (default: ' ', optional)</param>
      /// <returns>True if the string contains any parts of the given string.</returns>
      public static bool CTContainsAny(this string str, string searchTerms, char splitChar = ' ')
      {
         if (str == null)
            throw new System.ArgumentNullException(nameof(str));

         if (string.IsNullOrEmpty(searchTerms))
            return true;

         char[] split = {splitChar};

         return searchTerms.Split(split, System.StringSplitOptions.RemoveEmptyEntries).Any(searchTerm => str.CTContains(searchTerm));
      }

      /// <summary>
      /// Extension method for strings.
      /// Contains all given strings.
      /// </summary>
      /// <param name="str">String-instance.</param>
      /// <param name="searchTerms">Search terms separated by the given split-character.</param>
      /// <param name="splitChar">Split-character (default: ' ', optional)</param>
      /// <returns>True if the string contains all parts of the given string.</returns>
      public static bool CTContainsAll(this string str, string searchTerms, char splitChar = ' ')
      {
         if (str == null)
            throw new System.ArgumentNullException(nameof(str));

         if (string.IsNullOrEmpty(searchTerms))
            return true;

         char[] split = {splitChar};

         return searchTerms.Split(split, System.StringSplitOptions.RemoveEmptyEntries).All(searchTerm => str.CTContains(searchTerm));
      }

      /// <summary>
      /// Extension method for strings.
      /// Checks if the string is numeric.
      /// </summary>
      /// <param name="str">String-instance.</param>
      /// <returns>True if the string is numeric.</returns>
      public static bool CTisNumeric(this string str)
      {
         if (str == null)
            throw new System.ArgumentNullException(nameof(str));

         return double.TryParse(str, out double output);
      }

      /// <summary>
      /// Extension method for strings.
      /// Checks if the string is integer.
      /// </summary>
      /// <param name="str">String-instance.</param>
      /// <returns>True if the string is integer.</returns>
      public static bool CTisInteger(this string str)
      {
         if (str == null)
            throw new System.ArgumentNullException(nameof(str));

         return !str.Contains(".") && long.TryParse(str, out long output);
      }

      /// <summary>
      /// Extension method for strings.
      /// Checks if the string starts with another string.
      /// </summary>
      /// <param name="str">String-instance.</param>
      /// <param name="toCheck">String to check.</param>
      /// <param name="comp">StringComparison-method (default: StringComparison.OrdinalIgnoreCase, optional)</param>
      /// <returns>True if the string is integer.</returns>
      public static bool CTStartsWith(this string str, string toCheck, System.StringComparison comp = System.StringComparison.OrdinalIgnoreCase)
      {
         if (str == null)
            throw new System.ArgumentNullException(nameof(str));

         return string.IsNullOrEmpty(toCheck) || str.StartsWith(toCheck, comp);
      }

      /// <summary>
      /// Extension method for strings.
      /// Checks if the string ends with another string.
      /// </summary>
      /// <param name="str">String-instance.</param>
      /// <param name="toCheck">String to check.</param>
      /// <param name="comp">StringComparison-method (default: StringComparison.OrdinalIgnoreCase, optional)</param>
      /// <returns>True if the string is integer.</returns>
      public static bool CTEndsWith(this string str, string toCheck, System.StringComparison comp = System.StringComparison.OrdinalIgnoreCase)
      {
         if (str == null)
            throw new System.ArgumentNullException(nameof(str));

         return string.IsNullOrEmpty(toCheck) || str.EndsWith(toCheck, comp);
      }

      /// <summary>
      /// Extension method for strings.
      /// Returns the index of the last occurence of a given string.
      /// </summary>
      /// <param name="str">String-instance.</param>
      /// <param name="toCheck">String for the index.</param>
      /// <param name="comp">StringComparison-method (default: StringComparison.OrdinalIgnoreCase, optional)</param>
      /// <returns>The index of the last occurence of the given string if the string is integer.</returns>
      public static int CTLastIndexOf(this string str, string toCheck, System.StringComparison comp = System.StringComparison.OrdinalIgnoreCase)
      {
         if (str == null)
            throw new System.ArgumentNullException(nameof(str));

         return string.IsNullOrEmpty(toCheck) ? 0 : str.LastIndexOf(toCheck, comp);
      }

      /// <summary>
      /// Extension method for strings.
      /// Returns the index of the first occurence of a given string.
      /// </summary>
      /// <param name="str">String-instance.</param>
      /// <param name="toCheck">String for the index.</param>
      /// <param name="comp">StringComparison-method (default: StringComparison.OrdinalIgnoreCase, optional)</param>
      /// <returns>The index of the first occurence of the given string if the string is integer.</returns>
      public static int CTIndexOf(this string str, string toCheck, System.StringComparison comp = System.StringComparison.OrdinalIgnoreCase)
      {
         if (str == null)
            throw new System.ArgumentNullException(nameof(str));

         return string.IsNullOrEmpty(toCheck) ? 0 : str.IndexOf(toCheck, comp);
      }

      /// <summary>
      /// Extension method for strings.
      /// Returns the index of the first occurence of a given string.
      /// </summary>
      /// <param name="str">String-instance.</param>
      /// <param name="toCheck">String for the index.</param>
      /// <param name="startIndex">Start index for the check.</param>
      /// <param name="comp">StringComparison-method (default: StringComparison.OrdinalIgnoreCase, optional)</param>
      /// <returns>The index of the first occurence of the given string if the string is integer.</returns>
      public static int CTIndexOf(this string str, string toCheck, int startIndex, System.StringComparison comp = System.StringComparison.OrdinalIgnoreCase)
      {
         if (str == null)
            throw new System.ArgumentNullException(nameof(str));

         return string.IsNullOrEmpty(toCheck) ? 0 : str.IndexOf(toCheck, startIndex, comp);
      }

      #endregion


      #region Arrays

      /// <summary>
      /// Extension method for Arrays.
      /// Shuffles an Array.
      /// </summary>
      /// <param name="array">Array-instance to shuffle.</param>
      /// <param name="seed">Seed for the PRNG (default: 0 (=standard), optional)</param>
      public static void CTShuffle<T>(this T[] array, int seed = 0)
      {
         if (array == null || array.Length <= 0)
            throw new System.ArgumentNullException(nameof(array));

         System.Random rnd = seed == 0 ? new System.Random() : new System.Random(seed);
         int n = array.Length;
         while (n > 1)
         {
            int k = rnd.Next(n--);
            T temp = array[n];
            array[n] = array[k];
            array[k] = temp;
         }
      }

      /// <summary>
      /// Extension method for Arrays.
      /// Dumps an array to a string.
      /// </summary>
      /// <param name="array">Array-instance to dump.</param>
      /// <param name="prefix">Prefix for every element (default: empty, optional).</param>
      /// <param name="postfix">Postfix for every element (default: empty, optional).</param>
      /// <returns>String with lines for all array entries.</returns>
      public static string CTDump<T>(this T[] array, string prefix = "", string postfix = "")
      {
         if (array == null) // || array.Length <= 0)
            throw new System.ArgumentNullException(nameof(array));

         System.Text.StringBuilder sb = new System.Text.StringBuilder();

         foreach (T element in array)
         {
            if (0 < sb.Length)
               sb.Append(System.Environment.NewLine);

            sb.Append(prefix);
            sb.Append(element);
            sb.Append(postfix);
         }

         return sb.ToString();
      }

      /// <summary>
      /// Extension method for Quaternion-Arrays.
      /// Dumps an array to a string.
      /// </summary>
      /// <param name="array">Quaternion-Array-instance to dump.</param>
      /// <returns>String with lines for all array entries.</returns>
      public static string CTDump(this Quaternion[] array)
      {
         if (array == null) // || array.Length <= 0)
            throw new System.ArgumentNullException(nameof(array));

         System.Text.StringBuilder sb = new System.Text.StringBuilder();

         foreach (Quaternion element in array)
         {
            if (0 < sb.Length)
               sb.Append(System.Environment.NewLine);

            sb.Append(element.x);
            sb.Append(", ");
            sb.Append(element.y);
            sb.Append(", ");
            sb.Append(element.z);
            sb.Append(", ");
            sb.Append(element.w);
         }

         return sb.ToString();
      }

      /// <summary>
      /// Extension method for Vector2-Arrays.
      /// Dumps an array to a string.
      /// </summary>
      /// <param name="array">Vector2-Array-instance to dump.</param>
      /// <returns>String with lines for all array entries.</returns>
      public static string CTDump(this Vector2[] array)
      {
         if (array == null) // || array.Length <= 0)
            throw new System.ArgumentNullException(nameof(array));

         System.Text.StringBuilder sb = new System.Text.StringBuilder();

         foreach (Vector2 element in array)
         {
            if (0 < sb.Length)
               sb.Append(System.Environment.NewLine);

            sb.Append(element.x);
            sb.Append(", ");
            sb.Append(element.y);
         }

         return sb.ToString();
      }

      /// <summary>
      /// Extension method for Vector3-Arrays.
      /// Dumps an array to a string.
      /// </summary>
      /// <param name="array">Vector3-Array-instance to dump.</param>
      /// <returns>String with lines for all array entries.</returns>
      public static string CTDump(this Vector3[] array)
      {
         if (array == null) // || array.Length <= 0)
            throw new System.ArgumentNullException(nameof(array));

         System.Text.StringBuilder sb = new System.Text.StringBuilder();

         foreach (Vector3 element in array)
         {
            if (0 < sb.Length)
               sb.Append(System.Environment.NewLine);

            sb.Append(element.x);
            sb.Append(", ");
            sb.Append(element.y);
            sb.Append(", ");
            sb.Append(element.z);
         }

         return sb.ToString();
      }

      /// <summary>
      /// Extension method for Vector4-Arrays.
      /// Dumps an array to a string.
      /// </summary>
      /// <param name="array">Vector4-Array-instance to dump.</param>
      /// <returns>String with lines for all array entries.</returns>
      public static string CTDump(this Vector4[] array)
      {
         if (array == null) // || array.Length <= 0)
            throw new System.ArgumentNullException(nameof(array));

         System.Text.StringBuilder sb = new System.Text.StringBuilder();

         foreach (Vector4 element in array)
         {
            if (0 < sb.Length)
               sb.Append(System.Environment.NewLine);

            sb.Append(element.x);
            sb.Append(", ");
            sb.Append(element.y);
            sb.Append(", ");
            sb.Append(element.z);
            sb.Append(", ");
            sb.Append(element.w);
         }

         return sb.ToString();
      }

      /// <summary>
      /// Extension method for Arrays.
      /// Generates a string array with all entries (via ToString).
      /// </summary>
      /// <param name="array">Array-instance to ToString.</param>
      /// <returns>String array with all entries (via ToString).</returns>
      public static string[] CTToString<T>(this T[] array)
      {
         if (array == null) // || array.Length <= 0)
            throw new System.ArgumentNullException(nameof(array));

         string[] result = new string[array.Length];

         for (int ii = 0; ii < array.Length; ii++)
         {
            result[ii] = null == array[ii] ? "null" : array[ii].ToString();
         }

         return result;
      }

      #endregion


      #region Lists

      /// <summary>
      /// Extension method for IList.
      /// Shuffles a List.
      /// </summary>
      /// <param name="list">IList-instance to shuffle.</param>
      /// <param name="seed">Seed for the PRNG (default: 0 (=standard), optional)</param>
      public static void CTShuffle<T>(this System.Collections.Generic.IList<T> list, int seed = 0)
      {
         if (list == null)
            throw new System.ArgumentNullException(nameof(list));

         System.Random rnd = seed == 0 ? new System.Random() : new System.Random(seed);
         int n = list.Count;

         while (n > 1)
         {
            int k = rnd.Next(n--);
            T temp = list[n];
            list[n] = list[k];
            list[k] = temp;
         }
      }

      /// <summary>
      /// Extension method for IList.
      /// Dumps a list to a string.
      /// </summary>
      /// <param name="list">IList-instance to dump.</param>
      /// <param name="prefix">Prefix for every element (default: empty, optional).</param>
      /// <param name="postfix">Postfix for every element (default: empty, optional).</param>
      /// <returns>String with lines for all list entries.</returns>
      public static string CTDump<T>(this System.Collections.Generic.IList<T> list, string prefix = "", string postfix = "")
      {
         if (list == null)
            throw new System.ArgumentNullException(nameof(list));

         System.Text.StringBuilder sb = new System.Text.StringBuilder();

         foreach (T element in list)
         {
            if (0 < sb.Length)
               sb.Append(System.Environment.NewLine);

            sb.Append(prefix);
            sb.Append(element);
            sb.Append(postfix);
         }

         return sb.ToString();
      }

      /// <summary>
      /// Extension method for Quaternion-IList.
      /// Dumps a list to a string.
      /// </summary>
      /// <param name="list">Quaternion-IList-instance to dump.</param>
      /// <returns>String with lines for all list entries.</returns>
      public static string CTDump(this System.Collections.Generic.IList<Quaternion> list)
      {
         if (list == null)
            throw new System.ArgumentNullException(nameof(list));

         System.Text.StringBuilder sb = new System.Text.StringBuilder();

         foreach (Quaternion element in list)
         {
            if (0 < sb.Length)
               sb.Append(System.Environment.NewLine);

            sb.Append(element.x);
            sb.Append(", ");
            sb.Append(element.y);
            sb.Append(", ");
            sb.Append(element.z);
            sb.Append(", ");
            sb.Append(element.w);
         }

         return sb.ToString();
      }

      /// <summary>
      /// Extension method for Vector2-IList.
      /// Dumps a list to a string.
      /// </summary>
      /// <param name="list">Vector2-IList-instance to dump.</param>
      /// <returns>String with lines for all list entries.</returns>
      public static string CTDump(this System.Collections.Generic.IList<Vector2> list)
      {
         if (list == null)
            throw new System.ArgumentNullException(nameof(list));

         System.Text.StringBuilder sb = new System.Text.StringBuilder();

         foreach (Vector2 element in list)
         {
            if (0 < sb.Length)
               sb.Append(System.Environment.NewLine);

            sb.Append(element.x);
            sb.Append(", ");
            sb.Append(element.y);
         }

         return sb.ToString();
      }

      /// <summary>
      /// Extension method for Vector3-IList.
      /// Dumps a list to a string.
      /// </summary>
      /// <param name="list">Vector3-IList-instance to dump.</param>
      /// <returns>String with lines for all list entries.</returns>
      public static string CTDump(this System.Collections.Generic.IList<Vector3> list)
      {
         if (list == null)
            throw new System.ArgumentNullException(nameof(list));

         System.Text.StringBuilder sb = new System.Text.StringBuilder();

         foreach (Vector3 element in list)
         {
            if (0 < sb.Length)
               sb.Append(System.Environment.NewLine);

            sb.Append(element.x);
            sb.Append(", ");
            sb.Append(element.y);
            sb.Append(", ");
            sb.Append(element.z);
         }

         return sb.ToString();
      }

      /// <summary>
      /// Extension method for Vector4-IList.
      /// Dumps a list to a string.
      /// </summary>
      /// <param name="list">Vector4-IList-instance to dump.</param>
      /// <returns>String with lines for all list entries.</returns>
      public static string CTDump(this System.Collections.Generic.IList<Vector4> list)
      {
         if (list == null)
            throw new System.ArgumentNullException(nameof(list));

         System.Text.StringBuilder sb = new System.Text.StringBuilder();

         foreach (Vector4 element in list)
         {
            if (0 < sb.Length)
               sb.Append(System.Environment.NewLine);

            sb.Append(element.x);
            sb.Append(", ");
            sb.Append(element.y);
            sb.Append(", ");
            sb.Append(element.z);
            sb.Append(", ");
            sb.Append(element.w);
         }

         return sb.ToString();
      }

      /// <summary>
      /// Extension method for IList.
      /// Generates a string list with all entries (via ToString).
      /// </summary>
      /// <param name="list">IList-instance to ToString.</param>
      /// <returns>String list with all entries (via ToString).</returns>
      public static System.Collections.Generic.List<string> CTToString<T>(this System.Collections.Generic.IList<T> list)
      {
         if (list == null)
            throw new System.ArgumentNullException(nameof(list));

         System.Collections.Generic.List<string> result = new System.Collections.Generic.List<string>(list.Count);
         result.AddRange(list.Select(element => null == element ? "null" : element.ToString()));

         return result;
      }

      #endregion


      #region Dictionaries

      /// <summary>
      /// Extension method for IDictionary.
      /// Dumps a dictionary to a string.
      /// </summary>
      /// <param name="dict">IDictionary-instance to dump.</param>
      /// <param name="prefix">Prefix for every element (default: empty, optional).</param>
      /// <param name="postfix">Postfix for every element (default: empty, optional).</param>
      /// <returns>String with lines for all dictionary entries.</returns>
      public static string CTDump<K, V>(this System.Collections.Generic.IDictionary<K, V> dict, string prefix = "", string postfix = "")
      {
         if (dict == null)
            throw new System.ArgumentNullException(nameof(dict));

         System.Text.StringBuilder sb = new System.Text.StringBuilder();

         foreach (System.Collections.Generic.KeyValuePair<K, V> kvp in dict)
         {
            if (0 < sb.Length)
               sb.Append(System.Environment.NewLine);

            sb.Append(prefix);
            sb.Append("Key = ");
            sb.Append(kvp.Key);
            sb.Append(", Value = ");
            sb.Append(kvp.Value);
            sb.Append(postfix);
         }

         return sb.ToString();
      }

      /// <summary>
      /// Extension method for IDictionary.
      /// Adds a dictionary to an existing one.
      /// </summary>
      /// <param name="dict">IDictionary-instance.</param>
      /// <param name="collection">Dictionary to add.</param>
      public static void CTAddRange<K, V>(this System.Collections.Generic.IDictionary<K, V> dict, System.Collections.Generic.IDictionary<K, V> collection)
      {
         if (dict == null)
            throw new System.ArgumentNullException(nameof(dict));

         if (collection == null)
            throw new System.ArgumentNullException(nameof(collection));

         foreach (System.Collections.Generic.KeyValuePair<K, V> item in collection)
         {
            if (!dict.ContainsKey(item.Key))
            {
               dict.Add(item.Key, item.Value);
            }
            else
            {
               // handle duplicate key issue here
               Debug.LogWarning($"Duplicate key found: {item.Key}");
            }
         }
      }

      #endregion


      #region Unity specific

      /// <summary>
      /// Extension method for Renderer.
      /// Determines if the renderer is visible from a certain camera.
      /// </summary>
      /// <param name="renderer">Renderer to test the visibility.</param>
      /// <param name="camera">Camera for the test.</param>
      /// <returns>True if the renderer is visible by the given camera.</returns>
      public static bool CTIsVisibleFrom(this Renderer renderer, Camera camera)
      {
         if (renderer == null)
            throw new System.ArgumentNullException(nameof(renderer));

         if (camera == null)
            throw new System.ArgumentNullException(nameof(camera));

         Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
         return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
      }

      /// <summary>
      /// Extension method for Transform.
      /// Recursively searches all children of a parent transform for specific named transform
      /// </summary>
      /// <param name="parent">Parent of the current children.</param>
      /// <param name="name">Name of the transform.</param>
      /// <returns>True if the renderer is visible by the given camera.</returns>
      public static Transform CTDeepSearch(Transform parent, string name)
      {
         if (parent == null)
            throw new System.ArgumentNullException(nameof(parent));

         if (name == null)
            throw new System.ArgumentNullException(nameof(name));

         Transform tf = parent.Find(name);

         if (tf != null)
            return tf;

         foreach (Transform child in parent)
         {
            tf = CTDeepSearch(child, name);
            if (tf != null)
               return tf;
         }

         return null;
      }

      #endregion


      #region Streams

      /// <summary>
      /// Extension method for Stream.
      /// Reads the full content of a Stream.
      /// </summary>
      /// <param name="input">Stream-instance to read.</param>
      /// <param name="bufferSize">Buffer size in bytes (default: 16384, optional).</param>
      /// <returns>Byte-array of the Stream content.</returns>
      public static byte[] CTReadFully(this System.IO.Stream input, int bufferSize = 16384)
      {
         if (input == null)
            throw new System.ArgumentNullException(nameof(input));

         byte[] buffer = new byte[bufferSize];
         using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
         {
            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
               ms.Write(buffer, 0, read);
            }

            return ms.ToArray();
         }
      }

      #endregion

      /*
      /// <summary>
      /// Perform a deep Copy of the object.
      /// </summary>
      /// <typeparam name="T">The type of object being copied.</typeparam>
      /// <param name="source">The object instance to copy.</param>
      /// <returns>The copied object.</returns>
      public static T Clone<T>(this T source)
      {
          if (!typeof(T).IsSerializable)
          {
              throw new ArgumentException("The type must be serializable.", "source");
          }

          // Don't serialize a null object, simply return the default for that object
          if (Object.ReferenceEquals(source, null))
          {
              return default(T);
          }

          IFormatter formatter = new BinaryFormatter();
          Stream stream = new MemoryStream();
          using (stream)
          {
              formatter.Serialize(stream, source);
              stream.Seek(0, SeekOrigin.Begin);
              return (T)formatter.Deserialize(stream);
          }
      }
      */
      /*
          /// <summary>
          /// Clone a List with elememts containing a copy constructor.
          /// </summary>
          /// <param name="list">List-instance to clone.</param>
          /// <returns>Clones list.</returns>
          public static List<T> CTClone<T>(this List<T> listToClone) where T : ICopyable
          {
              List<T> newList = new List<T>(listToClone.Count);
  
              listToClone.ForEach((item) =>
              {
                  newList.Add(new T(item));
              });
  
              return newList;
  
              //return listToClone.Select(item => (T)item.Clone()).ToList();
          }
        */

      /*
      public static string[] CTToUppercase(string[] array)
      {
          if (array == null || array.Length <= 0)
              throw new ArgumentNullException("array");

          string[] result = new string[array.Length];

          for (int ii = 0; ii < array.Length; ii++)
          {
              result[ii] = array[ii].ToUpper();
          }

          return result;
      }

      public static string[] CTToLowercase(string[] array)
      {
          if (array == null || array.Length <= 0)
              throw new ArgumentNullException("array");

          string[] result = new string[array.Length];

          for (int ii = 0; ii < array.Length; ii++)
          {
              result[ii] = array[ii].ToLower();
          }

          return result;
      }
  */
   }
}
// © 2016-2020 crosstales LLC (https://www.crosstales.com)