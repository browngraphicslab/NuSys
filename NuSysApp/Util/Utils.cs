﻿// Copyright (c) Microsoft Corporation. All rights reserved.
//
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Windows.Storage;

namespace NuSysApp
{
    public static class Utils
    {
        public static List<T> GetEnumAsList<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>().ToList();
        }

        public static Matrix3x2 GetDisplayTransform(System.Numerics.Vector2 outputSize, System.Numerics.Vector2 sourceSize)
        {
            // Scale the display to fill the control.
            var scale = outputSize / sourceSize;
            var offset = System.Numerics.Vector2.Zero;

            // Letterbox or pillarbox to preserve aspect ratio.
            if (scale.X > scale.Y)
            {
                scale.X = scale.Y;
                offset.X = (outputSize.X - sourceSize.X * scale.X) / 2;
            }
            else
            {
                scale.Y = scale.X;
                offset.Y = (outputSize.Y - sourceSize.Y * scale.Y) / 2;
            }

            return Matrix3x2.CreateScale(scale) *
                   Matrix3x2.CreateTranslation(offset);
        }

        public static CanvasGeometry CreateStarGeometry(ICanvasResourceCreator resourceCreator, float scale, System.Numerics.Vector2 center)
        {
            System.Numerics.Vector2[] points =
            {
                new System.Numerics.Vector2(-0.24f, -0.24f),
                new System.Numerics.Vector2(0, -1),
                new System.Numerics.Vector2(0.24f, -0.24f),
                new System.Numerics.Vector2(1, -0.2f),
                new System.Numerics.Vector2(0.4f, 0.2f),
                new System.Numerics.Vector2(0.6f, 1),
                new System.Numerics.Vector2(0, 0.56f),
                new System.Numerics.Vector2(-0.6f, 1),
                new System.Numerics.Vector2(-0.4f, 0.2f),
                new System.Numerics.Vector2(-1, -0.2f),
            };

            var transformedPoints = from point in points
                                    select point * scale + center;

#if WINDOWS_UWP
            var convertedPoints = transformedPoints;
#else
            // Convert the System.Numerics.Vector2 type that we normally work with to the
            // Microsoft.Graphics.Canvas.Numerics.Vector2 struct used by WinRT. These casts
            // are usually inserted automatically, but auto conversion does not work for arrays.
            var convertedPoints = from point in transformedPoints
                                  select (Microsoft.Graphics.Canvas.Numerics.Vector2)point;
#endif

            return CanvasGeometry.CreatePolygon(resourceCreator, convertedPoints.ToArray());
        }

        public static float DegreesToRadians(float angle)
        {
            return angle * (float)Math.PI / 180;
        }

        static readonly Random random = new Random();

        public static Random Random
        {
            get { return random; }
        }

        public static byte[] Encrypt(byte[] bytes)
        {
            var sha = SHA256.Create();
            return sha.ComputeHash(bytes);
        }
        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public static string Checksum(string text)
        {
            var sha = SHA256.Create();
            return Convert.ToBase64String(sha.ComputeHash(GetBytes(text)));
        }

        public static float RandomBetween(float min, float max)
        {
            return min + (float)random.NextDouble() * (max - min);
        }

        public static async Task<byte[]> ReadAllBytes(string filename)
        {
            var uri = new Uri("ms-appx:///" + filename);
            var file = await StorageFile.GetFileFromApplicationUriAsync(uri);
            var buffer = await FileIO.ReadBufferAsync(file);

            return buffer.ToArray();
        }

        public static async Task<T> TimeoutAfter<T>(this Task<T> task, TimeSpan timeout)
        {
            if (task == await Task.WhenAny(task, Task.Delay(timeout)))
            {
                return await task;
            }
            else
            {
                throw new TimeoutException();
            }
        }

        public struct WordBoundary { public int Start; public int Length; }

        public static List<WordBoundary> GetEveryOtherWord(string str)
        {
            List<WordBoundary> result = new List<WordBoundary>();

            for (int i = 0; i < str.Length; ++i)
            {
                if (str[i] == ' ')
                {
                    int nextSpace = str.IndexOf(' ', i + 1);
                    int limit = nextSpace == -1 ? str.Length : nextSpace;

                    WordBoundary wb = new WordBoundary();
                    wb.Start = i + 1;
                    wb.Length = limit - i - 1;
                    result.Add(wb);
                    i = limit;
                }
            }
            return result;
        }
    }
}
