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
    public static class CurveExtensions
    {
        /// <summary>
        /// Evaluates a parameter using a plane translated along a tangent vector
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="parameter">Parameter to get initial plane from, usually 0 or 1</param>
        /// <param name="length">Offset along the tangent vector</param>
        /// <returns>parameter on curve</returns>
        [IsVisibleInDynamoLibrary(false)]
        public static double DistanceAtTangentLength(this Curve curve, int parameter, double length)
        {
            // Get the tangent vector
            Vector tangent = curve.TangentAtParameter(parameter);

            // Create a plane using the tangent vector and a point on the curve
            Plane plane = Plane.ByOriginNormal(curve.PointAtParameter(parameter), tangent);

            // Translate the plane along the tangent and get the intersection point with the curve
            double tangentParameter = curve.IntersectWithPlaneAlongVector(plane, tangent, length);

            // If there was no intersection (-1) try again with the reversed vector
            if (tangentParameter == -1)
                tangentParameter = curve.IntersectWithPlaneAlongVector(plane, tangent.Reverse(), length);

            // If there is no useful result, return -1;
            // Otherwise return the distance
            if (tangentParameter == -1)
                return -1;
            else
                return curve.SegmentLengthAtParameter(tangentParameter);
        }

        /// <summary>
        /// Checks if a curve is an Arc
        /// </summary>
        [IsVisibleInDynamoLibrary(false)]
        public static bool IsArc(this Curve curve)
        {
            return (curve.StartPoint.DistanceTo(curve.EndPoint) < curve.Length);
        }


        /// <summary>
        /// Intersect the Curve with a plane translated along a vector using a distance
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="plane">Plane to use for intersection</param>
        /// <param name="tangent">Vector to translate the plane by, usually a tangent</param>
        /// <param name="length">Distance to translate the plane by</param>
        /// <returns>parameter of the intersection point along the curve</returns>
        [IsVisibleInDynamoLibrary(false)]
        private static double IntersectWithPlaneAlongVector(this Curve curve, Plane plane, Vector tangent, double length)
        {
            // translate the plane along a vector using a distance
            Plane offsetPlane = (Plane)plane.Translate(tangent, length);

            // Intersect the plane with the curve
            Geometry[] intersection = offsetPlane.Intersect(curve);

            // If the intersection is one element and it is a point return the parameter of this point
            // Otherwise return minus one
            if (intersection.Count() == 1 && intersection[0].GetType() == typeof(Point))
                return curve.ParameterAtPoint((Point)intersection[0]);
            else
                return -1;
        }


        /// <summary>
        /// Approximate Curve to Revit Curve
        /// </summary>
        /// <param name="curve"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public static object Approximate(this Curve curve)
        {
            if (curve.GetType() == typeof(Autodesk.DesignScript.Geometry.NurbsCurve))
            {
                NurbsCurve nurbsCurve = (NurbsCurve)curve;

                if (nurbsCurve.IsLinear())
                {
                    return Autodesk.Revit.DB.Line.CreateBound(curve.StartPoint.ToRevitType(), curve.EndPoint.ToRevitType());
                }
                else
                {
                    return Autodesk.Revit.DB.Arc.Create(curve.StartPoint.ToRevitType(), curve.EndPoint.ToRevitType(), curve.PointAtParameter(0.5).ToRevitType());
                }
            }
            else if (curve.GetType() == typeof(Autodesk.DesignScript.Geometry.PolyCurve))
            {
                DynamoRebar.RevitPolyCurve revitpolycurve = new RevitPolyCurve((Autodesk.DesignScript.Geometry.PolyCurve)curve);
                return revitpolycurve;
            }
            else
            {
                Autodesk.Revit.DB.Curve result = curve.ToRevitType();

                if (result.GetType() == typeof(Autodesk.Revit.DB.NurbSpline))
                {
                    return Autodesk.Revit.DB.Arc.Create(curve.StartPoint.ToRevitType(), curve.EndPoint.ToRevitType(), curve.PointAtParameter(0.5).ToRevitType());
                }
                else
                    return result;
            }
        }


        /// <summary>
        /// Checks if a curve is linear
        /// </summary>
        /// <param name="curve"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public static bool IsLinear(this NurbsCurve curve)
        {
            Line line = Line.ByStartPointEndPoint(curve.StartPoint, curve.EndPoint);
            Point midLine = line.PointAtParameter(0.5);
            Point midCurve = curve.PointAtParameter(0.5);
            return midLine.IsAlmostEqualTo(midCurve);
        }


        /// <summary>
        /// Divides a Curve into equal pieces
        /// </summary>
        /// <param name="curve">Curve to divide</param>
        /// <param name="counter">Number of pieces</param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public static Point[] PointDivide(this Curve curve, int counter)
        {
            Point[] points = new Point[counter + 1];

            // Walk through the counter and evaluate normalized points
            for (int i = 0; i <= counter; i++)
            {
                double parameter = (double)i / (double)counter;
                points[i] = curve.PointAtParameter(parameter);
            }

            return points;
        }


        /// <summary>
        /// Intersect a Curve with a set of surfaces
        /// </summary>
        /// <param name="curve">Curve</param>
        /// <param name="surfaces">Boundary to test</param>
        /// <returns>List of sorted parameters along the curve</returns>
        [IsVisibleInDynamoLibrary(false)]
        public static List<double> Insersection(this Curve curve, List<Surface> surfaces)
        {
            List<double> intersectionParameters = new List<double>();

            // Walk through all surfaces and check for intersections
            // The IntersectAll Method doesnt seem to work correctly
            // So we get the intersection geometry for each surface
            foreach (Surface surface in surfaces)
            {
                // Get intersection geometry
                Geometry[] intersections = curve.Intersect(surface);

                // Get geometries from the intersection result
                foreach (Geometry geometry in intersections)
                {
                    // If its a point, get its parameter and add it to the result collection
                    if (geometry.GetType() == typeof(Point))
                        intersectionParameters.Add(curve.ParameterAtPoint((Point)geometry));
                }
            }

            // Sort the parameters
            intersectionParameters.Sort();

            // Return a list without duplicates
            return intersectionParameters.RemoveDuplicates();
        }



        const double _inch = 1.0 / 12.0;
        const double _sixteenth = _inch / 16.0;

        /// <summary>
        /// Sort a list of curves to make them correctly 
        /// ordered and oriented to form a closed loop.
        /// From Jeremy Tammik
        /// </summary>
        [IsVisibleInDynamoLibrary(false)]
        public static void SortCurvesContiguous(IList<Autodesk.Revit.DB.Curve> curves)
        {
            int n = curves.Count;

            // Walk through each curve (after the first) 
            // to match up the curves in order

            for (int i = 0; i < n; ++i)
            {
                Autodesk.Revit.DB.Curve curve = curves[i];
                Autodesk.Revit.DB.XYZ endPoint = curve.GetEndPoint(1);


                Autodesk.Revit.DB.XYZ p;

                // Find curve with start point = end point

                bool found = (i + 1 >= n);

                for (int j = i + 1; j < n; ++j)
                {
                    p = curves[j].GetEndPoint(0);

                    // If there is a match end->start, 
                    // this is the next curve

                    if (_sixteenth > p.DistanceTo(endPoint))
                    {

                        if (i + 1 != j)
                        {
                            Autodesk.Revit.DB.Curve tmp = curves[i + 1];
                            curves[i + 1] = curves[j];
                            curves[j] = tmp;
                        }
                        found = true;
                        break;
                    }

                    p = curves[j].GetEndPoint(1);

                    // If there is a match end->end, 
                    // reverse the next curve

                    if (_sixteenth > p.DistanceTo(endPoint))
                    {
                        if (i + 1 == j)
                        {

                            curves[i + 1] = CreateReversedCurve(curves[j]);
                        }
                        else
                        {

                            Autodesk.Revit.DB.Curve tmp = curves[i + 1];
                            curves[i + 1] = CreateReversedCurve(curves[j]);
                            curves[j] = tmp;
                        }
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    throw new Exception("SortCurvesContiguous:"
                      + " non-contiguous input curves");
                }
            }
        }


        /// <summary>
        /// Create a new curve with the same 
        /// geometry in the reverse direction.
        /// rom Jeremy Tammik
        /// </summary>
        /// <param name="orig">The original curve.</param>
        /// <returns>The reversed curve.</returns>
        /// <throws cref="NotImplementedException">If the 
        /// curve type is not supported by this utility.</throws>
        [IsVisibleInDynamoLibrary(false)]
        static Autodesk.Revit.DB.Curve CreateReversedCurve(Autodesk.Revit.DB.Curve orig)
        {
            if (orig is Autodesk.Revit.DB.Line)
            {
                return Autodesk.Revit.DB.Line.CreateBound(
                  orig.GetEndPoint(1),
                  orig.GetEndPoint(0));
            }
            else if (orig is Autodesk.Revit.DB.Arc)
            {
                return Autodesk.Revit.DB.Arc.Create(orig.GetEndPoint(1),
                  orig.GetEndPoint(0),
                  orig.Evaluate(0.5, true));
            }
            else
            {
                throw new Exception(
                  "CreateReversedCurve - Unreachable");
            }
        }

        /// <summary>
        /// Morphes form one Curve to another
        /// </summary>
        /// <param name="curve1">Curve to start from</param>
        /// <param name="curve2">Curve to end</param>
        /// <param name="numberOfLines">Number of morphed curves to create</param>
        /// <param name="precision">Morphing precision</param>
        /// <param name="offset">Offset</param>
        /// <returns>List of morphed Curves</returns>
        [IsVisibleInDynamoLibrary(false)]
        public static List<Curve> MorphTo(this Curve curve1, Curve curve2, int numberOfLines, int precision = 10, double offset = 0)
        {
            numberOfLines++;

            Surface virtualSurface = Surface.ByLoft(new List<Curve>() { curve1, curve2 });

            Vector direction = virtualSurface.NormalAtParameter(0.5, 0.5);

            List<Curve> curves = new List<Curve>();

            // Divide both curves using the precision factor
            Point[] pointsCurve1 = curve1.PointDivide(precision);
            Point[] pointsCurve2 = curve2.PointDivide(precision);

            // Create a Matrix for the morphed points
            Point[][] points = new Point[precision + 1][];

            // If the Curves are in different directions, flip them.
            if (pointsCurve1[0].DistanceTo(pointsCurve2[pointsCurve2.Length - 1]) < pointsCurve1[0].DistanceTo(pointsCurve2[0]))
            {
                Array.Reverse(pointsCurve2);
            }


            // Draw construction lines between the two curves using the division points
            for (int i = 0; i < pointsCurve1.Length; i++)
            {
                Line line = Line.ByStartPointEndPoint(pointsCurve1[i], pointsCurve2[i]);

                // Divide the construction line into the number of curves to create
                // Add the divsion points to the matrix
                points[i] = line.PointDivide(numberOfLines);
            }

            // Create an empty Array holding the pointWeights
            double[] pointWeights = new double[numberOfLines];

            // Flip the Matrix to create new curves from
            Point[][] transposedPoints = points.TransposeRowsAndColumns();

            // Create Curves from the Matrix
            for (int i = 1; i < transposedPoints.Length - 1; i++)
            {
                NurbsCurve curve = NurbsCurve.ByControlPoints(transposedPoints[i].ToList());
                Curve curveToSegment = (offset != 0) ? (Curve)curve.Translate(direction, offset) : curve;

                curves.Add(curveToSegment);
            }

            return curves;
        }

    }
}









