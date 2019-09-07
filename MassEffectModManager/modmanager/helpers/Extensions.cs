﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MassEffectModManager.modmanager.helpers
{
    /// <summary>
    /// Extension methods for various things, both WPF and WinForms
    /// </summary>
    public static class Extensions
    {
        private static readonly char[] InvalidPathingChars;

        static Extensions()
        {
            List<char> enumerable = new List<char>();
            enumerable.AddRange((IEnumerable<char>)Path.GetInvalidFileNameChars());
            enumerable.AddRange((IEnumerable<char>)Path.GetInvalidPathChars());
            Extensions.InvalidPathingChars = enumerable.ToArray<char>(enumerable.Count);
        }

        /// <summary>
        /// Extracts a sub array from another array with a specified number of elements.
        /// </summary>
        /// <typeparam name="T">Content of array.</typeparam>
        /// <param name="oldArray">Current array.</param>
        /// <param name="offset">Start index in oldArray.</param>
        /// <param name="length">Length to extract.</param>
        /// <returns>New array containing elements within the specified range.</returns>
        public static T[] GetRange<T>(this T[] oldArray, int offset, int length)
        {
            T[] objArray = new T[length];
            Array.Copy((Array)oldArray, offset, (Array)objArray, 0, length);
            return objArray;
        }

        /// <summary>
        /// Extracts a sub array from another array starting at offset and reading to end.
        /// </summary>
        /// <typeparam name="T">Content of array.</typeparam>
        /// <param name="oldArray">Current array.</param>
        /// <param name="offset">Start index in oldArray.</param>
        /// <returns>New array containing elements within the specified range.</returns>
        public static T[] GetRange<T>(this T[] oldArray, int offset)
        {
            return oldArray.GetRange<T>(offset, oldArray.Length - offset);
        }

        /// <summary>
        /// Write data to this stream at the current position from another stream at it's current position.
        /// </summary>
        /// <param name="TargetStream">Stream to copy from.</param>
        /// <param name="SourceStream">Stream to copy to.</param>
        /// <param name="Length">Number of bytes to read.</param>
        /// <param name="bufferSize">Size of buffer to use while copying.</param>
        /// <returns>Number of bytes read.</returns>
        public static int ReadFrom(
          this Stream TargetStream,
          Stream SourceStream,
          int Length,
          int bufferSize = 4096)
        {
            byte[] buffer = new byte[bufferSize];
            int num = 0;
            do
            {
                int count = SourceStream.Read(buffer, 0, Math.Min(bufferSize, Length));
                if (count != 0)
                {
                    Length -= count;
                    TargetStream.Write(buffer, 0, count);
                    num += count;
                }
                else
                    break;
            }
            while (Length > 0);
            return num;
        }

        /// <summary>
        /// Reads an int from stream at the current position and advances 4 bytes.
        /// </summary>
        /// <param name="stream">Stream to read from.</param>
        /// <returns>Integer read from stream.</returns>
        public static int ReadInt32FromStream(this Stream stream)
        {
            using (BinaryReader binaryReader = new BinaryReader(stream, Encoding.Default, true))
                return binaryReader.ReadInt32();
        }

        /// <summary>
        /// Reads an uint from stream at the current position and advances 4 bytes.
        /// </summary>
        /// <param name="stream">Stream to read from.</param>
        /// <returns>Integer read from stream.</returns>
        public static uint ReadUInt32FromStream(this Stream stream)
        {
            using (BinaryReader binaryReader = new BinaryReader(stream, Encoding.Default, true))
                return binaryReader.ReadUInt32();
        }

        public static void WriteInt64ToStream(this Stream stream, long value)
        {
            using (BinaryWriter binaryWriter = new BinaryWriter(stream, Encoding.Default, true))
                binaryWriter.Write(value);
        }

        /// <summary>Reads a long from stream at the current position.</summary>
        /// <param name="stream">Stream to read from.</param>
        /// <returns>Long read from stream.</returns>
        public static long ReadInt64FromStream(this Stream stream)
        {
            using (BinaryReader binaryReader = new BinaryReader(stream, Encoding.Default, true))
                return binaryReader.ReadInt64();
        }

        /// <summary>
        /// Reads a number of bytes from stream at the current position and advances that number of bytes.
        /// </summary>
        /// <param name="stream">Stream to read from.</param>
        /// <param name="Length">Number of bytes to read.</param>
        /// <returns>Bytes read from stream.</returns>
        public static byte[] ReadBytesFromStream(this Stream stream, int Length)
        {
            using (BinaryReader binaryReader = new BinaryReader(stream, Encoding.Default, true))
                return binaryReader.ReadBytes(Length);
        }

        /// <summary>
        /// Reads a string from a stream. Must be null terminated or have the length written at the start (Pascal strings or something?)
        /// </summary>
        /// <param name="stream">Stream to read from.</param>
        /// <param name="HasLengthWritten">True = Attempt to read string length from stream first.</param>
        /// <returns>String read from stream.</returns>
        public static string ReadStringFromStream(this Stream stream, bool HasLengthWritten = false)
        {
            if (stream == null || !stream.CanRead)
                throw new IOException("Stream cannot be read.");
            List<char> enumerable = new List<char>();
            if (HasLengthWritten)
            {
                int num = stream.ReadInt32FromStream();
                for (int index = 0; index < num; ++index)
                    enumerable.Add((char)stream.ReadByte());
            }
            else
            {
                char ch;
                while ((ch = (char)stream.ReadByte()) > char.MinValue)
                    enumerable.Add(ch);
            }
            return new string(enumerable.ToArray<char>(enumerable.Count));
        }

        /// <summary>
        /// KFreon: Borrowed this from the DevIL C# Wrapper found here: https://code.google.com/p/devil-net/
        /// 
        /// Reads a stream until the end is reached into a byte array. Based on
        /// <a href="http://www.yoda.arachsys.com/csharp/readbinary.html">Jon Skeet's implementation</a>.
        /// It is up to the caller to dispose of the stream.
        /// </summary>
        /// <param name="stream">Stream to read all bytes from</param>
        /// <param name="initialLength">Initial buffer length, default is 32K</param>
        /// <returns>The byte array containing all the bytes from the stream</returns>
        public static byte[] ReadStreamFully(this Stream stream, int initialLength = 32768)
        {
            stream.Seek(0L, SeekOrigin.Begin);
            if (initialLength < 1)
                initialLength = 32768;
            byte[] buffer = new byte[initialLength];
            int length = 0;
            int num1;
            while ((num1 = stream.Read(buffer, length, buffer.Length - length)) > 0)
            {
                length += num1;
                if (length == buffer.Length)
                {
                    int num2 = stream.ReadByte();
                    if (num2 == -1)
                        return buffer;
                    byte[] numArray = new byte[buffer.Length * 2];
                    Array.Copy((Array)buffer, (Array)numArray, buffer.Length);
                    numArray[length] = (byte)num2;
                    buffer = numArray;
                    ++length;
                }
            }
            byte[] numArray1 = new byte[length];
            Array.Copy((Array)buffer, (Array)numArray1, length);
            return numArray1;
        }

        /// <summary>
        /// FROM GIBBED.
        /// Writes an int to stream at the current position.
        /// </summary>
        /// <param name="stream">Stream to write to.</param>
        /// <param name="value">Integer to write.</param>
        public static void WriteInt32ToStream(this Stream stream, int value)
        {
            using (BinaryWriter binaryWriter = new BinaryWriter(stream, Encoding.Default, true))
                binaryWriter.Write(value);
        }

        /// <summary>
        /// Writes string to stream. Terminated by a null char, and optionally writes string length at start of string. (Pascal strings?)
        /// </summary>
        /// <param name="stream">Stream to write to.</param>
        /// <param name="str">String to write.</param>
        /// <param name="WriteLength">True = Writes str length before writing string.</param>
        public static void WriteStringToStream(this Stream stream, string str, bool WriteLength = false)
        {
            if (WriteLength)
                stream.WriteInt32ToStream(str.Length);
            foreach (char ch in str)
                stream.WriteByte((byte)ch);
            stream.WriteByte((byte)0);
        }

        /// <summary>Returns index of minimum value based on comparer.</summary>
        /// <param name="enumerable">Collection to search.</param>
        /// <param name="comparer">Comparer to use. e.g. item =&gt; item - x</param>
        /// <returns>Index of minimum value in enumerable based on comparer.</returns>
        public static int IndexOfMin(this IEnumerable<int> enumerable, Func<int, int> comparer)
        {
            int num1 = int.MaxValue;
            int num2 = 0;
            int num3 = 0;
            foreach (int num4 in enumerable)
            {
                int num5 = comparer(num4);
                if (num5 < num1)
                {
                    num1 = num5;
                    num3 = num2;
                }
                ++num2;
            }
            return num3;
        }

        /// <summary>Returns index of minimum value based on comparer.</summary>
        /// <param name="enumerable">Collection to search.</param>
        /// <param name="comparer">Comparer to use. e.g. item =&gt; item - x</param>
        /// <returns>Index of minimum value in enumerable based on comparer.</returns>
        public static int IndexOfMin(this IEnumerable<byte> enumerable, Func<byte, int> comparer)
        {
            int num1 = int.MaxValue;
            int num2 = 0;
            int num3 = 0;
            foreach (byte num4 in enumerable)
            {
                int num5 = comparer(num4);
                if (num5 < num1)
                {
                    num1 = num5;
                    num3 = num2;
                }
                ++num2;
            }
            return num3;
        }

        /// <summary>
        /// Adds elements of a Dictionary to another Dictionary. No checking for duplicates.
        /// </summary>
        /// <typeparam name="T">Key.</typeparam>
        /// <typeparam name="U">Value.</typeparam>
        /// <param name="mainDictionary">Dictionary to add to.</param>
        /// <param name="newAdditions">Dictionary of elements to be added.</param>
        public static void AddRange<T, U>(
          this Dictionary<T, U> mainDictionary,
          Dictionary<T, U> newAdditions)
        {
            if (newAdditions == null)
                throw new ArgumentNullException();
            foreach (KeyValuePair<T, U> newAddition in newAdditions)
                mainDictionary.Add(newAddition.Key, newAddition.Value);
        }

        /// <summary>Add range of elements to given collection.</summary>
        /// <typeparam name="T">Type of items in collection.</typeparam>
        /// <param name="collection">Collection to add to.</param>
        /// <param name="additions">Elements to add.</param>
        public static void AddRangeKinda<T>(this ConcurrentBag<T> collection, IEnumerable<T> additions)
        {
            foreach (T addition in additions)
                collection.Add(addition);
        }

        /// <summary>Add range of elements to given collection.</summary>
        /// <typeparam name="T">Type of items in collection.</typeparam>
        /// <param name="collection">Collection to add to.</param>
        /// <param name="additions">Elements to add.</param>
        public static void AddRangeKinda<T>(this ICollection<T> collection, IEnumerable<T> additions)
        {
            foreach (T addition in additions)
                collection.Add(addition);
        }

        /// <summary>Removes element from collection at index.</summary>
        /// <typeparam name="T">Type of objects in collection.</typeparam>
        /// <param name="collection">Collection to remove from.</param>
        /// <param name="index">Index to remove from.</param>
        /// <returns>Removed element.</returns>
        public static T Pop<T>(this ICollection<T> collection, int index)
        {
            T obj = collection.ElementAt<T>(index);
            collection.Remove(obj);
            return obj;
        }

        /// <summary>
        /// Converts enumerable to List in a more memory efficient way by providing size of list.
        /// </summary>
        /// <typeparam name="T">Type of elements in lists.</typeparam>
        /// <param name="enumerable">Enumerable to convert to list.</param>
        /// <param name="size">Size of list.</param>
        /// <returns>List containing enumerable contents.</returns>
        public static List<T> ToList<T>(this IEnumerable<T> enumerable, int size)
        {
            return new List<T>(enumerable);
        }

        /// <summary>
        /// Converts enumerable to array in a more memory efficient way by providing size of list.
        /// </summary>
        /// <typeparam name="T">Type of elements in list.</typeparam>
        /// <param name="enumerable">Enumerable to convert to array.</param>
        /// <param name="size">Size of lists.</param>
        /// <returns>Array containing enumerable elements.</returns>
        public static T[] ToArray<T>(this IEnumerable<T> enumerable, int size)
        {
            T[] objArray = new T[size];
            int num = 0;
            foreach (T obj in enumerable)
                objArray[num++] = obj;
            return objArray;
        }

        /// <summary>Splits string on (possibly) multiple elements.</summary>
        /// <param name="str">String to split.</param>
        /// <param name="options">Options to use while splitting.</param>
        /// <param name="splitStrings">Elements to split string on. (Delimiters)</param>
        /// <returns></returns>
        public static string[] Split(
          this string str,
          StringSplitOptions options,
          params string[] splitStrings)
        {
            return str.Split(splitStrings, options);
        }

        /// <summary>Compares strings with culture and case sensitivity.</summary>
        /// <param name="str">Main string to check in.</param>
        /// <param name="toCheck">Substring to check for in Main String.</param>
        /// <param name="CompareType">Type of comparison.</param>
        /// <returns>True if toCheck found in str, false otherwise.</returns>
        public static bool Contains(this string str, string toCheck, StringComparison CompareType)
        {
            return str.IndexOf(toCheck, CompareType) >= 0;
        }

        /// <summary>Removes invalid characters from path.</summary>
        /// <param name="str">String to remove chars from.</param>
        /// <returns>New string containing no invalid characters.</returns>
        public static string GetPathWithoutInvalids(this string str)
        {
            StringBuilder stringBuilder = new StringBuilder(str);
            foreach (char invalidPathingChar in Extensions.InvalidPathingChars)
                stringBuilder.Replace(invalidPathingChar.ToString() ?? "", "");
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Gets parent directory, optionally to a certain depth (or height?)
        /// </summary>
        /// <param name="str">String (hopefully path) to get parent of.</param>
        /// <param name="depth">Depth to get parent of.</param>
        /// <returns>Parent of string.</returns>
        public static string GetDirParent(this string str, int depth = 1)
        {
            string path = (string)null;
            try
            {
                path = Path.GetDirectoryName(str.Trim(Path.DirectorySeparatorChar));
                for (int index = 1; index < depth; ++index)
                    path = Path.GetDirectoryName(path);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to get parent directory: " + ex.Message);
            }
            return path;
        }

        /// <summary>
        /// Determines if string is a Directory.
        /// Returns True if directory, false otherwise.
        /// </summary>
        /// <param name="str">String to check.</param>
        /// <returns>True if is a directory, false if not.</returns>
        public static bool isDirectory(this string str)
        {
            return str != null && (File.Exists(str) || Directory.Exists(str)) && File.GetAttributes(str).HasFlag((Enum)FileAttributes.Directory);
        }

        /// <summary>
        /// Determines if string is a file.
        /// Returns True if file, false otherwise.
        /// </summary>
        /// <param name="str">String to check.</param>
        /// <returns>True if a file, false if not</returns>
        public static bool isFile(this string str)
        {
            return !str.isDirectory();
        }

        /// <summary>Determines if string is a number.</summary>
        /// <param name="str">String to check.</param>
        /// <returns>True if string is a number.</returns>
        public static bool isDigit(this string str)
        {
            int result = -1;
            return int.TryParse(str, out result);
        }

        /// <summary>Determines if character is a number.</summary>
        /// <param name="c">Character to check.</param>
        /// <returns>True if c is a number.</returns>
        public static bool isDigit(this char c)
        {
            return (c.ToString() ?? "").isDigit();
        }

        /// <summary>Determines if character is a letter.</summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool isLetter(this char c)
        {
            return !c.isDigit();
        }

        /// <summary>Determines if string is a letter.</summary>
        /// <param name="str">String to check.</param>
        /// <returns>True if str is a letter.</returns>
        public static bool isLetter(this string str)
        {
            if (str.Length == 1)
                return !str.isDigit();
            return false;
        }

        /// <summary>Adds text to a FixedPage.</summary>
        /// <param name="page">Page to add text to.</param>
        /// <param name="text">Text to add.</param>
        public static void AddTextToPage(this FixedPage page, string text)
        {
            page.Children.Add((UIElement)new TextBlock()
            {
                Inlines = {
          text
        }
            });
        }

        /// <summary>
        /// A simple WPF threading extension method, to invoke a delegate
        /// on the correct thread if it is not currently on the correct thread
        /// Which can be used with DispatcherObject types
        /// </summary>
        /// <param name="disp">The Dispatcher object on which to do the Invoke</param>
        /// <param name="dotIt">The delegate to run</param>
        /// <param name="priority">The DispatcherPriority</param>
        public static void InvokeIfRequired(
          this Dispatcher disp,
          Action dotIt,
          DispatcherPriority priority)
        {
            if (disp.Thread != Thread.CurrentThread)
                disp.Invoke(priority, (Delegate)dotIt);
            else
                dotIt();
        }

        /// <summary>
        /// Returns pixels of image as RGBA channels in a stream. (R, G, B, A). 1 byte each.
        /// </summary>
        /// <param name="bmp">Image to extract pixels from.</param>
        /// <returns>RGBA channels as stream.</returns>
        public static MemoryStream GetPixelsAsStream(this BitmapSource bmp)
        {
            byte[] pixels = bmp.GetPixels();
            MemoryStream memoryStream = new MemoryStream(pixels.Length);
            memoryStream.Write(pixels, 0, pixels.Length);
            return memoryStream;
        }

        /// <summary>Gets pixels of image as byte[].</summary>
        /// <param name="bmp">Image to extract pixels from.</param>
        /// <returns>Pixels of image.</returns>
        public static byte[] GetPixels(this BitmapSource bmp)
        {
            byte[] numArray = new byte[4 * bmp.PixelWidth * bmp.PixelHeight];
            int stride = bmp.PixelWidth * 4;
            bmp.CopyPixels((Array)numArray, stride, 0);
            return numArray;
        }

        ///// <summary>
        ///// Begins an animation that automatically sets final value to be held. Used with FillType.Stop rather than default FillType.Hold.
        ///// </summary>
        ///// <param name="element">Content Element to animate.</param>
        ///// <param name="anim">Animation to use on element.</param>
        ///// <param name="dp">Property of element to animate using anim.</param>
        ///// <param name="To">Final value of element's dp.</param>
        //public static void BeginAdjustableAnimation(
        //  this ContentElement element,
        //  DependencyProperty dp,
        //  GridLengthAnimation anim,
        //  object To)
        //{
        //    if (dp.IsValidType(To))
        //    {
        //        element.SetValue(dp, To);
        //        element.BeginAnimation(dp, (AnimationTimeline)anim);
        //    }
        //    else
        //        throw new Exception("To object value passed is of the wrong Type. Given: " + (object)To.GetType() + "  Expected: " + (object)dp.PropertyType);
        //}

        ///// <summary>
        ///// Begins adjustable animation for a GridlengthAnimation.
        ///// Holds animation end value without Holding it. i.e. Allows it to change after animation without resetting it. Should be possible in WPF...maybe it is.
        ///// </summary>
        ///// <param name="element">Element to start animation on.</param>
        ///// <param name="dp">Property to animate.</param>
        ///// <param name="anim">Animation to perform. GridLengthAnimation only for now.</param>
        //public static void BeginAdjustableAnimation(
        //  this ContentElement element,
        //  DependencyProperty dp,
        //  GridLengthAnimation anim)
        //{
        //    element.BeginAdjustableAnimation(dp, anim, (object)anim.To);
        //}
    }
}