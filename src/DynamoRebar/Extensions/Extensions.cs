//
// Copyright 2015 Autodesk, Inc.
// Author: Thornton Tomasetti Ltd, CORE Studio
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Revit.GeometryConversion;

namespace DynamoRebar
{
    /// <summary>
    /// Extensions
    /// </summary>
    public static class Extensions
    {

        /// <summary>
        /// Compares the direction of two vectors
        /// </summary>
        /// <param name="thisVector"></param>
        /// <param name="vector">vector to compare</param>
        /// <param name="tolerance">tolerance</param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public static bool Parallel(this Vector thisVector, Vector vector, double tolerance)
        {
            double angle = thisVector.AngleBetween(vector);

            if (Math.Abs(angle) < tolerance || Math.Abs(180 - angle) < tolerance) return true;
            else return false;

        }


        /// <summary>
        /// Remove Duplicates from double List
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public static List<double> RemoveDuplicates(this List<double> list)
        {
            // If the list contains one element return it
            if (list.Count == 1)
                return list;

            // If it contains two points return only one of them
            else if (list.Count == 2)
            {
                list.RemoveAt(1);
                return list;
            }

            else return list;
        }

        /// <summary>
        /// Flip Coordinates
        /// </summary>
        /// <param name="uv"></param>
        /// <returns>Flipped UV</returns>
        [IsVisibleInDynamoLibrary(false)]
        public static UV Flip(this UV uv)
        {
            double u = uv.U;
            double v = uv.V;
            return UV.ByCoordinates(v, u);
        }


        /// <summary>
        /// Flips the Array
        /// </summary>
        [IsVisibleInDynamoLibrary(false)]
        public static T[][] TransposeRowsAndColumns<T>(this T[][] arr)
        {
            int rowCount = arr.Length;
            int columnCount = arr[0].Length;
            T[][] transposed = new T[columnCount][];
            if (rowCount == columnCount)
            {
                transposed = (T[][])arr.Clone();
                for (int i = 1; i < rowCount; i++)
                {
                    for (int j = 0; j < i; j++)
                    {
                        T temp = transposed[i][j];
                        transposed[i][j] = transposed[j][i];
                        transposed[j][i] = temp;
                    }
                }
            }
            else
            {
                for (int column = 0; column < columnCount; column++)
                {
                    transposed[column] = new T[rowCount];
                    for (int row = 0; row < rowCount; row++)
                    {
                        transposed[column][row] = arr[row][column];
                    }
                }
            }
            return transposed;
        }

    }








}
