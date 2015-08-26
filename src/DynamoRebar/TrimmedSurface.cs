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
    /// Represents a trimmed Surface in Dynamo
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public class TrimmedSurface
    {
        /// <summary>
        /// Internal, untrimmed surface
        /// </summary>
        public Surface internalSurface;

        /// <summary>
        /// Outline Dictionary
        /// </summary>
        Dictionary<Vector, TwoCurves> curvesByDirections;

        /// <summary>
        /// Tolerance for outline sorting algorithm
        /// </summary>
        public double tolerance;

        /// <summary>
        /// Constructor creating a trimmed surface from a surface using an optional tolerance
        /// </summary>
        /// <param name="surface">Untrimmed surface</param>
        /// <param name="angleTolerance">Tolerance for outline interpretation, usually 60 deg</param>
        [IsVisibleInDynamoLibrary(false)]
        public TrimmedSurface(Surface surface, double angleTolerance = 60)
        {
            this.internalSurface = surface;
            this.curvesByDirections = new Dictionary<Vector, TwoCurves>();
            this.tolerance = angleTolerance;

            // Walk through all perimeter curves
            foreach (Curve curve in internalSurface.PerimeterCurves())
            {
                // get the curves direction
                Vector direction = Vector.ByTwoPoints(curve.StartPoint, curve.EndPoint);

                // If there hasnt been any curve collected add this as the first one
                if (curvesByDirections.Count == 0)
                    curvesByDirections.Add(direction, new TwoCurves(curve));
                else
                {
                    // indicator to check if any curve matches its direction
                    bool matchingOrientation = false;

                    // Walk through all collected curves and check if the direction vectors are
                    // more or less parallel.
                    foreach (Vector vector in curvesByDirections.Keys.ToList())
                    {
                        if (direction.Parallel(vector, tolerance))
                        {
                            matchingOrientation = true;

                            // If there is a matching direction already in the dictionary,
                            // Add this curve to the existing curve
                            TwoCurves existingCurve = curvesByDirections[vector];
                            existingCurve.AddToMatchingCurve(curve);
                        }
                    }

                    // If there is no matching direction, this seems to be a new
                    // Side of the surface
                    if (!matchingOrientation)
                    {
                        curvesByDirections.Add(direction, new TwoCurves(curve));
                    }
                }
            }
        }

        /// <summary>
        /// Create an Arc or a Line from the given properties of the surface
        /// </summary>
        /// <param name="compareTo">Parallel boundaries</param>
        /// <param name="A">Start point</param>
        /// <param name="B">End point</param>
        /// <param name="parameter">parameter for the 3rd point</param>
        /// <param name="refpoint">Startpoint of one orthogonal boundary</param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        private Curve CreateCurve(TwoCurves compareTo, Point A, Point B, double parameter, Point refpoint)
        {
            // Init an empty curve
            Curve returnValue = null;

            // If the boundaries to compare this curve to are arcs
            // create a middle line between the boundaries
            if (compareTo.Curve1.IsArc() || compareTo.Curve2.IsArc())
            {
                Line line = Line.ByStartPointEndPoint(compareTo.Curve1.PointAtParameter(0.5), compareTo.Curve2.PointAtParameter(0.5));

                // If the reference point is on curve 2 invert the parameter
                if (compareTo.Curve1.ClosestPointTo(refpoint).DistanceTo(refpoint) > 0)
                    parameter = 1 - parameter;


                try
                {
                    // try to create an arc by three points
                    returnValue = Arc.ByThreePoints(A, line.PointAtParameter(parameter), B);
                }
                catch (System.ApplicationException)
                {
                    // On error create a line from A to B
                    returnValue = Line.ByStartPointEndPoint(A, B);
                }
            }
            else
                // If the boundaries are lines, create a line from A to B
                returnValue = Line.ByStartPointEndPoint(A, B);

            return returnValue;
        }


        /// <summary>
        /// Get A Curve across the surface at one parameter
        /// </summary>
        /// <param name="parameter">parameter</param>
        /// <param name="flip">change direction</param>
        /// <returns>curve across the surface</returns>
        [IsVisibleInDynamoLibrary(false)]
        public Curve GetCurveAtParameter(double parameter, bool flip)
        {
            // get the orthogonal boundries and the parallel to compare to
            KeyValuePair<Vector, TwoCurves> data = this.curvesByDirections.First();
            TwoCurves compareTo = this.curvesByDirections.Last().Value;

            // if flip, swap the values
            if (flip)
            {
                data = this.curvesByDirections.Last();
                compareTo = this.curvesByDirections.First().Value;
            }

            // Get the orthogonal boundaries startpoints
            Point A = data.Value.Curve1.PointAtParameter(0);
            Point B = data.Value.Curve2.PointAtParameter(0);
            Line l = Line.ByStartPointEndPoint(A, B);

            // Get the desired startpoint
            Point Async = data.Value.Curve1.PointAtParameter(parameter);

            // Assuming the other boundary is upside down (other direction than Curve1)
            // Get the desired endpoint using the inverted parameter
            // eg. StartParameter = 0.2 therefore the endparameter must be 0.8
            // If the curves are pointing in different directions
            Point Bsync = data.Value.Curve2.PointAtParameter((1 - parameter));

            // Check if the assumptin was true, otherwise just use the same
            // parameter value: StartParam = 0.2 EndParam = 0.2
            if (compareTo.AreEndpoints(A, B))
                Bsync = data.Value.Curve2.PointAtParameter(parameter);

            // Create the curve for the parameters
            return CreateCurve(compareTo, Async, Bsync, parameter, A);
        }
    }


    /// <summary>
    /// Represents a set of two curves which are ment to be on opposite sides of a surface
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public class TwoCurves
    {
        /// <summary>
        /// Curve 1
        /// </summary>
        public Curve Curve1;

        /// <summary>
        /// Curve 2
        /// </summary>
        public Curve Curve2;

        /// <summary>
        /// Undefined curves found
        /// </summary>
        public List<Curve> Undefined;

        /// <summary>
        /// Constructor using one curve to start with
        /// </summary>
        /// <param name="curve1"></param>
        [IsVisibleInDynamoLibrary(false)]
        public TwoCurves(Curve curve1) { this.Curve1 = curve1; this.Curve2 = null; Undefined = new List<Curve>(); }

        /// <summary>
        /// Add a curve to Curve 1 or Curve 2
        /// </summary>
        /// <param name="curve">curve to add</param>
        [IsVisibleInDynamoLibrary(false)]
        public void AddToMatchingCurve(Curve curve)
        {
            // Prepare a curve list to create a polycurve from
            List<Curve> curvesTojoin = new List<Curve>();
            curvesTojoin.Add(curve);

            // If the curve is connected to Curve 1
            if (Curve1.StartPoint.IsAlmostEqualTo(curve.EndPoint) || Curve1.EndPoint.IsAlmostEqualTo(curve.StartPoint))
            {
                // Add curve1 to the list and create a polycurve
                curvesTojoin.Add(Curve1);
                Curve newCurve = PolyCurve.ByJoinedCurves(curvesTojoin);
                this.Curve1 = newCurve;
            }
            // If the curve is connected to curve 2
            else if (Curve2 != null && (Curve2.StartPoint.IsAlmostEqualTo(curve.EndPoint) || Curve2.EndPoint.IsAlmostEqualTo(curve.StartPoint)))
            {
                // Add curve 2 to the list and create a polycurve from them
                curvesTojoin.Add(Curve2);
                Curve newCurve = PolyCurve.ByJoinedCurves(curvesTojoin);
                this.Curve2 = newCurve;
            }
            else
            {
                // If the curve doesnt connect and curve 2 hasnt been defined yet
                // take is as curve 2 otherwise add the unconnectable curve to the undefined list and throw an error
                if (this.Curve2 == null)
                    this.Curve2 = curve;
                else
                {
                    Undefined.Add(curve);
                    throw new ArgumentNullException("Cannot parametrize surface, try increasing the tolerance.");
                }
            }
        }

        /// <summary>
        /// Check if A and B are Endpoints of one of the curves
        /// </summary>
        /// <param name="A">Point A</param>
        /// <param name="B">Point B</param>
        /// <returns>Align parallel</returns>
        [IsVisibleInDynamoLibrary(false)]
        public bool AreEndpoints(Point A, Point B)
        {
            return
            (
                    (A.IsAlmostEqualTo(Curve1.StartPoint) || A.IsAlmostEqualTo(Curve1.EndPoint))
                    &&
                    (B.IsAlmostEqualTo(Curve1.StartPoint) || B.IsAlmostEqualTo(Curve1.EndPoint))
                ) || (
                    (A.IsAlmostEqualTo(Curve2.StartPoint) || A.IsAlmostEqualTo(Curve2.EndPoint))
                    &&
                    (B.IsAlmostEqualTo(Curve2.StartPoint) || B.IsAlmostEqualTo(Curve2.EndPoint))
            );

        }
    }
}