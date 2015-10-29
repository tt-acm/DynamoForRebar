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
using DSRevitNodesUI;
using RVT = Autodesk.Revit.DB;
using RevitServices.Persistence;
using RevitServices.Transactions;
using DSCoreNodesUI;
using Dynamo.Utilities;
using Dynamo.Models;
using Revit.GeometryConversion;

namespace DynamoRebar
{

    /// <summary>
    /// Rebar Nodes
    /// </summary>
    public static class Rebar
    {
        /// <summary>
        /// Create morphed Curves
        /// </summary>
        /// <param name="fromCurve">Source Curve</param>
        /// <param name="toCurve">Target Curve</param>
        /// <param name="numberOfCurves">Number of curves to create</param>
        /// <param name="offset">Offset</param>
        /// <returns>List of morphed curves</returns>
        public static List<Curve> Morphed(Curve fromCurve, Curve toCurve, int numberOfCurves, double offset = 0)
        {
            List<Curve> data = fromCurve.MorphTo(toCurve, numberOfCurves, 50, offset);

            return data;
        }

        /// <summary>
        /// Create curves perpendicular to Face
        /// </summary>
        /// <param name="face">Face to use</param>
        /// <param name="boundary">Boundary Faces</param>
        /// <param name="height">Location Parameter</param>
        /// <param name="numberOfCurves">Number of curves to create</param>
        /// <param name="flip">Flip orientation</param>
        /// <param name="offset">Offset</param>
        /// <returns>List of perpendicular curves</returns>
        public static List<Curve> Perpendicular(Surface face, List<Surface> boundary, double height, int numberOfCurves, bool flip = true, double offset = 0)
        {
            List<Curve> data = face.NormalCurves(boundary, numberOfCurves, offset, height, flip);

            return data;
        }


        /// <summary>
        /// Create Rebar following a selected surface
        /// </summary>
        /// <param name="face">Surface</param>
        /// <param name="numberOfCurves">Define number of curves or distance between bars</param>
        /// <param name="distanceBetweenCurves">Define distance between curves or number of bars</param>
        /// <param name="flip">Flip orientation</param>
        /// <param name="offset">Offset</param>
        /// <param name="idealize">Idealize surfaces to rectangles</param>
        /// <returns>List of rebar</returns>
        public static List<Curve> FollowingSurface(Surface face, int numberOfCurves, double distanceBetweenCurves = 0, bool flip = true, double offset = 0, bool idealize = true)
        {
            if (idealize)
            {
                return face.Follow(50, offset, distanceBetweenCurves, numberOfCurves, flip);
            }
            else
            {

                // Create return value collection
                List<Curve> curves = new List<Curve>();

                // Get a reference curve from the surface
                UV p1 = UV.ByCoordinates(0, 0);
                UV p2 = UV.ByCoordinates(1, 0);

                double length = (!flip) ? face.DistanceBetweenPoints(p1, p2) : face.DistanceBetweenPoints(p1.Flip(), p2.Flip());

                // If there is a distance applied use it to determine the number of lines to create
                if (distanceBetweenCurves > 0) numberOfCurves = (int)(length / (distanceBetweenCurves));

                numberOfCurves++;

                Surface surface = face;

                if (offset != 0) surface = (Surface)face.Offset(offset);
                

                TrimmedSurface trimmedSurface = new TrimmedSurface(surface);

                // Walk thru the amount of lines to create
                for (int j = 1; j < numberOfCurves; j++)
                {
                    // Create a set of points for createing a curve
                    List<Point> points = new List<Point>();

                    // Get the height parameter
                    double height = (double)j / (double)numberOfCurves;

                    curves.Add(trimmedSurface.GetCurveAtParameter(height, flip));
                }

                return curves;
            }
        }

        /// <summary>
        /// Create Curves using Isolines of a Surface
        /// </summary>
        /// <param name="face">Surface</param>
        /// <param name="isoDirection">Iso Line Direction</param>
        /// <param name="parameters">Parameters to Evaluate</param>
        /// <returns>List of IsoCurves</returns>
        public static List<Curve> FollowingIsoLineSurface(Surface face, int isoDirection, List<double> parameters)
        {
            List<Curve> result = new List<Curve>();

            foreach (double parameter in parameters)
                result.Add(face.GetIsoline(isoDirection, parameter));

            return result;
        }

        /// <summary>
        /// Cuts a set of Rebars by Plane
        /// </summary>
        /// <param name="plane">Plane to cut by</param>
        /// <param name="rebarContainerElement">Rebar Container</param>
        /// <param name="firstPart">Return the first or the last part of the splitted elements</param>
        public static void Cut(Surface plane, Revit.Elements.Element rebarContainerElement, bool firstPart)
        {
            // Get Rebar Container Element
            Autodesk.Revit.DB.Structure.RebarContainer rebarContainer = (Autodesk.Revit.DB.Structure.RebarContainer)rebarContainerElement.InternalElement;

            // Get the active Document
            Autodesk.Revit.DB.Document document = DocumentManager.Instance.CurrentDBDocument;

            // Open a new Transaction
            TransactionManager.Instance.EnsureInTransaction(document);

            // Get all single Rebar elements from the container
            List<Autodesk.Revit.DB.Structure.RebarContainerItem> rebars = rebarContainer.ToList();

            // Walk through all rebar elements
            foreach (Autodesk.Revit.DB.Structure.RebarContainerItem rebar in rebars)
            {
                // Buffer Rebar properties for recreation
                RVT.Structure.RebarBarType barType = (RVT.Structure.RebarBarType)document.GetElement(rebar.BarTypeId);
                RVT.Structure.RebarHookType hookTypeStart = (RVT.Structure.RebarHookType)document.GetElement(rebar.GetHookTypeId(0));
                RVT.Structure.RebarHookType hookTypeEnd = (RVT.Structure.RebarHookType)document.GetElement(rebar.GetHookTypeId(1));
                RVT.Structure.RebarHookOrientation hookOrientationStart = rebar.GetHookOrientation(0);
                RVT.Structure.RebarHookOrientation hookOrientationEnd = rebar.GetHookOrientation(1);

                // create a list to store the remaining part of the curve after cutting it
                List<RVT.Curve> result = new List<RVT.Curve>();

                // get the center line curves of the rebar elements
                foreach (RVT.Curve curve in rebar.GetCenterlineCurves(false, true, true))
                {
                    // if the curve is a line or an arc consider it being valid
                    if (curve.GetType() == typeof(RVT.Line) || curve.GetType() == typeof(RVT.Arc))
                    {
                        // Get a DesignScript Curve from the Revit curve
                        Curve geocurve = curve.ToProtoType();

                        // Intersect the selected plane with the curve
                        foreach (Geometry geometry in plane.Intersect(geocurve))
                        {
                            // if the intersection is a point
                            if (geometry.GetType() == typeof(Point))
                            {
                                // Get the closest point to the intersection on the curve
                                Point p = geocurve.ClosestPointTo((Point)geometry);

                                // Split the curve at this point
                                Curve[] curves = geocurve.ParameterSplit(geocurve.ParameterAtPoint(p));

                                // If the curve has been split into two parts
                                if (curves.Length == 2)
                                {
                                    // return the first or the second part of the splitted curve
                                    if (firstPart)
                                        result.Add(curves[0].ToRevitType());
                                    else
                                        result.Add(curves[1].ToRevitType());
                                }
                            }
                        }
                    }
                }

                // If the result has some elements, create a new rebar container from those curves
                // using the same properties as the initial one.
                if (result.Count > 0)
                {
                    rebar.SetFromCurves(RVT.Structure.RebarStyle.Standard, barType, hookTypeStart, hookTypeEnd, rebar.Normal, result, hookOrientationStart, hookOrientationEnd, true, false);
                }
            }

            // Commit and Dispose the transaction
            TransactionManager.Instance.TransactionTaskDone();
        }

        /// <summary>
        /// Shorten curves from both sides
        /// </summary>
        /// <param name="curves">Curves to shorten</param>
        /// <param name="length">Length to shorten</param>
        /// <returns>Shortened curves</returns>
        public static List<Curve> Shorten(List<Curve> curves, double length)
        {
            // result list
            List<Curve> result = new List<Curve>();

            // Walk through all curves
            for (int i = 0; i < curves.Count; i++)
            {
                Curve curve = curves[i];

                // get the offset distance at the start of the curve
                double startDistance = curve.DistanceAtTangentLength(0, length);
                if (startDistance != -1)
                {
                    // if the distance is valid cut the curve
                    Curve[] startcuttedcurves = curve.DivideByLengthFromParameter(curve.Length - startDistance, 1);

                    // get the offset distance for the end of the curve
                    double endParameter = startcuttedcurves[0].DistanceAtTangentLength(1, length);

                    if (endParameter != -1)
                    {
                        // If the distance is valid cut the curve
                        Curve[] endcuttedcurves = startcuttedcurves[0].DivideByLengthFromParameter(endParameter, 0);

                        // Add the cutted middle segment to the result array
                        result.Add(endcuttedcurves[0]);
                    }
                }
            }
            return result;
        }


    }
}


