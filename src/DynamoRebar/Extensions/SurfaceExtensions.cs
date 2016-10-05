//
// Copyright 2015 Autodesk, Inc.
// Author: Thornton Tomasetti Ltd, CORE Studio (Maximilian Thumfart)
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
    public static class SurfaceExtensions
    {
        /// <summary>
        /// Returns Distance between two points on the surface
        /// </summary>
        /// <param name="surface"></param>
        /// <param name="horizontal">Horizontal or Vertical extends</param>
        /// <returns>Distance as double</returns>
        [IsVisibleInDynamoLibrary(false)]
        public static double DistanceBetweenPoints(this Surface surface, UV point1, UV point2)
        {
            Point A = surface.PointAtParameter(point1.U, point1.V);
            Point B = surface.PointAtParameter(point2.U, point2.V);

            return A.DistanceTo(B);
        }


        /// <summary>
        /// Returns a point evaluated by parameter and direction
        /// </summary>
        /// <param name="surface"></param>
        /// <param name="parameter">Parameter to evaluate</param>
        /// <param name="directionIsHorizontal">U or V</param>
        /// <returns>Evaluated Point</returns>
        [IsVisibleInDynamoLibrary(false)]
        public static Point PointAtParameterAndDirection(this Surface surface, double parameter, bool directionIsHorizontal)
        {
            if (directionIsHorizontal)
                return surface.PointAtParameter(0, parameter);
            else
                return surface.PointAtParameter(parameter, 0);
        }

        /// <summary>
        /// Create a set of horizontal Normal Curves on a line along a surface
        /// </summary>
        /// <param name="face">Surface</param>
        /// <param name="surfaces">Boundary conditions (length of normals)</param>
        /// <param name="numberOfLines">Number of normals to create</param>
        /// <param name="offset">Offset from surface</param>
        /// <param name="height">Height parameter of the line along the surface</param>
        /// <param name="horizontal">Horizontal or Vertical position</param>
        /// <returns>List of Curves</returns>
        [IsVisibleInDynamoLibrary(false)]
        public static List<Curve> NormalCurves(this Surface face, List<Surface> surfaces, int numberOfLines, double offset, double height, bool horizontal)
        {
            // List of return curves
            List<Curve> curves = new List<Curve>();

            numberOfLines++;

            // Array of points for a set of points to create the curve from
            Point[] points = new Point[numberOfLines - 1];

            // assume parameter V is in Y direction
            bool VisY = true;

            // Check if start and endpoint of the curve along the surface is vertical
            // If yes, V follows X direction
            if (face.PointAtParameter(0, height).X == face.PointAtParameter(1, height).X) VisY = false;

            // Invert V direction if the user wants a vertical direction
            if (!horizontal) VisY = !VisY;

            // Get points along the surface forming a set of points to create a new curve from
            for (int i = 1; i < numberOfLines; i++)
            {
                // get the parameter to use
                double parameter = (double)i / (double)numberOfLines;

                // Get the point according to the matching U or V setting
                if (VisY)
                    points[i - 1] = face.PointAtParameter(parameter, height);
                else
                    points[i - 1] = face.PointAtParameter(height, parameter);
            }

            // Create Normals for each Point
            for (int i = 0; i < points.Length; i++)
            {
                int intersections = GetIntersection(face, surfaces, points[i], offset, ref curves, false);

                if (intersections == 0)
                    GetIntersection(face, surfaces, points[i], offset, ref curves, true);

            }

            List<Curve> crvs = new List<Curve>();

            foreach (Curve curve in curves)
            {
                Curve[] splitted = curve.DivideByLengthFromParameter(curve.Length - offset, 0);
                crvs.Add(splitted[0]);
            }


            return crvs;
        }

        /// <summary>
        /// Creates a set of Curves following a Surface
        /// </summary>
        /// <param name="surface">Surface to follow</param>
        /// <param name="precision">Precision facotr for curves</param>
        /// <param name="offset">Offset from surface</param>
        /// <param name="distanceBetweenLinesInMM">Distance between curves</param>
        /// <param name="numberOfLines">Number of curves to create</param>
        /// <param name="flip">Flip direction</param>
        /// <returns>Set of Curves</returns>
        [IsVisibleInDynamoLibrary(false)]
        public static List<Curve> Follow(this Surface surface, int precision, double offset, double distanceBetweenLinesInMM, int numberOfLines, bool flip)
        {

            Vector normal = surface.NormalAtParameter(0.5, 0.5);
            Surface myface = (Surface)surface.Offset(offset);

            // Create return value collection
            List<Curve> curves = new List<Curve>();

            // Get a reference curve from the surface
            UV p1 = UV.ByCoordinates(0, 0);
            UV p2 = UV.ByCoordinates(1, 0);

            double length = (!flip) ? myface.DistanceBetweenPoints(p1, p2) : myface.DistanceBetweenPoints(p1.Flip(), p2.Flip());

            // If there is a distance applied use it to determine the number of lines to create
            if (distanceBetweenLinesInMM > 0) numberOfLines = (int)(length / (distanceBetweenLinesInMM));

            numberOfLines++;


            // Walk thru the amount of lines to create
            for (int j = 1; j < numberOfLines; j++)
            {
                // Create a set of points for createing a curve
                List<Point> points = new List<Point>();

                // Get the height parameter
                double height = (double)j / (double)numberOfLines;


                UV startPoint = UV.ByCoordinates(0, height);
                UV endPoint = UV.ByCoordinates(1, height);

                if (flip)
                {
                    startPoint = startPoint.Flip();
                    endPoint = endPoint.Flip();
                }

                Curve nurbsCurve = Curve.ByParameterLineOnSurface(myface, startPoint, endPoint);

                curves.Add(nurbsCurve);
            }

            return curves;
        }

        [IsVisibleInDynamoLibrary(false)]
        private static int GetIntersection(Surface face, List<Surface> surfaces, Point point, double offset, ref List<Curve> curves, bool reverse)
        {
            Surface my = face;

            if (face.GetType() == typeof(PolySurface))
            {
                PolySurface poly = (PolySurface)face;
                foreach (Surface s in poly.Surfaces())
                {
                    UV coords = s.UVParameterAtPoint(point);
                    if (coords != null && coords.U >= 0 && coords.U <= 1 && coords.V >= 0 && coords.V <= 1)
                    {
                        my = s;
                    }
                }
            }

            // Get the Normal at that point
            Vector normal = my.NormalAtPoint(point);




            // Remove Z Value in order to create horizontal normals
            Vector optimizedNormal = (reverse) ? normal.Reverse() : normal;
            // Vector.ByCoordinates(normal.X, normal.Y, 0).Reverse() : Vector.ByCoordinates(normal.X, normal.Y, 0);

            // Get an startpoint offset if it applies
            Point startPoint = point;
            if (offset != 0)
            {
                Line offsetLine = Line.ByStartPointDirectionLength(point, optimizedNormal, offset);
                startPoint = offsetLine.EndPoint;
            }

            // Create an almost endless line
            Line line = Line.ByStartPointDirectionLength(startPoint, optimizedNormal, 100000000);

            // Get intersection points with boundary surfaces
            List<double> intersections = line.Insersection(surfaces);

            // If there are any intersections
            if (intersections.Count > 1)
            {
                // trim the curve into segments if there are gaps or holes
                Curve[] segments = line.TrimSegmentsByParameter(intersections.ToArray(), false);

                // Walk through trimmed curves and add only those to the return collection
                // which are of a reasonable length
                foreach (Curve segment in segments)
                    //if (segment.Length < 100000) 
                    curves.Add(segment);
            }
            else if (intersections.Count == 1)
            {
                Curve[] segments = line.SplitByParameter(intersections[0]);
                curves.Add(segments[0]);
            }

            return intersections.Count;

        }

    }
}








