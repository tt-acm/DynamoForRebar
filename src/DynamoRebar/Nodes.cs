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

using RVT = Autodesk.Revit.DB;
using RevitServices.Persistence;
using RevitServices.Transactions;
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
        /// Cover value to Offset value
        /// </summary>
        /// <param name="cover">Cover Value</param>
        /// <param name="barType">Rebar Bar Type</param>
        /// <param name="autoconvert">Optional: Convert Units (default: false)</param>
        /// <returns>Offset</returns>
        public static double CoverToOffset(double cover, Revit.Elements.Element barType, bool autoconvert = false)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            var getDocUnits = doc.GetUnits();
            var getDisplayUnits = getDocUnits.GetFormatOptions(RVT.UnitType.UT_Length).DisplayUnits;

            double offset = cover;

            if (barType.InternalElement != null)
            {
                Autodesk.Revit.DB.Structure.RebarBarType revitBarType = (Autodesk.Revit.DB.Structure.RebarBarType)barType.InternalElement;
                var diam = (autoconvert) ? RVT.UnitUtils.ConvertFromInternalUnits(revitBarType.BarDiameter, getDisplayUnits) : revitBarType.BarDiameter;
                offset = cover + diam / 2;            
            }

            return offset;
        }


        /// <summary>
        /// Cover value to Offset value
        /// </summary>
        /// <param name="cover">Cover Value</param>
        /// <param name="barType">Rebar Bar Type</param>
        /// <returns>Offset</returns>
        [MultiReturn(new[] { "BarDiameter", "StandardBendDiameter", "StandardHookBendDiameter", "StirrupTieBendDiameter" })]
        public static Dictionary<string, object> GetRebarTypeProperties(double cover, Revit.Elements.Element barType)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            Autodesk.Revit.DB.Structure.RebarBarType revitBarType = (Autodesk.Revit.DB.Structure.RebarBarType)barType.InternalElement;
            var getDocUnits = doc.GetUnits();
            var getDisplayUnits = getDocUnits.GetFormatOptions(RVT.UnitType.UT_Length).DisplayUnits;

            return new Dictionary<string, object>
            {
                { "BarDiameter", RVT.UnitUtils.ConvertFromInternalUnits(revitBarType.BarDiameter, getDisplayUnits) },
                { "StandardBendDiameter", RVT.UnitUtils.ConvertFromInternalUnits(revitBarType.StandardBendDiameter, getDisplayUnits) },
                { "StandardHookBendDiameter", RVT.UnitUtils.ConvertFromInternalUnits(revitBarType.StandardHookBendDiameter, getDisplayUnits) },
                { "StirrupTieBendDiameter", RVT.UnitUtils.ConvertFromInternalUnits(revitBarType.StirrupTieBendDiameter, getDisplayUnits) },

            };
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
        /// <param name="desiredDistanceBetweenCurves">Define a desired distance between curves, the actual result will be a best fit</param>     
        /// <param name="flip">Flip orientation</param>
        /// <param name="offset">Offset</param>
        /// <param name="idealize">Idealize surfaces to rectangles</param>
        /// <param name="includeFirstEdge">Include first Edge of the surface</param>
        /// <param name="includeLastEdge">Include last Edge of the surface</param>
        /// <returns>List of rebar</returns>
        public static List<Curve> FollowingSurface(Surface face, int numberOfCurves = 0, double desiredDistanceBetweenCurves = 0, bool flip = true, double offset = 0, bool idealize = true, bool includeFirstEdge = false, bool includeLastEdge = false)
        {
            if (idealize)
            {
                return face.Follow(50, offset, desiredDistanceBetweenCurves, numberOfCurves, flip);
            }
            else
            {

                // Create return value collection
                List<Curve> curves = new List<Curve>();

                // Get the distance between two corner points
                double length = (!flip) ?
                    face.DistanceBetweenPoints(UV.ByCoordinates(0, 0), UV.ByCoordinates(0, 1))
                    :
                    face.DistanceBetweenPoints(UV.ByCoordinates(0, 0), UV.ByCoordinates(1, 0));

                // If there is a distance applied use it to determine the number of lines to create
                if (desiredDistanceBetweenCurves > 0) numberOfCurves = (int)(length / (desiredDistanceBetweenCurves));
                else numberOfCurves++;


                Surface surface = face;

                if (offset != 0) surface = (Surface)face.Offset(offset);
                

                TrimmedSurface trimmedSurface = new TrimmedSurface(surface);

                if (includeFirstEdge) curves.Add(trimmedSurface.GetCurveAtParameter(0, flip));

                // Walk thru the amount of lines to create
                for (int j = 1; j < numberOfCurves; j++)
                {
                    // Create a set of points for createing a curve
                    List<Point> points = new List<Point>();

                    // Get the height parameter
                    double height = (double)j / (double)numberOfCurves;

                    curves.Add(trimmedSurface.GetCurveAtParameter(height, flip));
                }

                if (includeLastEdge) curves.Add(trimmedSurface.GetCurveAtParameter(1, flip));

                return curves;
            }
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
                                Curve[] curves = geocurve.SplitByParameter(geocurve.ParameterAtPoint(p));

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

                //if (length > 0)
                //{
                //    // get the offset distance at the start of the curve
                //    double startDistance = curve.DistanceAtTangentLength(0, length);
                //    if (startDistance != -1)
                //    {

                //        // if the distance is valid cut the curve
                //        Curve[] startcuttedcurves = curve.DivideByLengthFromParameter(curve.Length - startDistance, 1);


                //        // get the offset distance for the end of the curve
                //        double endParameter = startcuttedcurves[0].DistanceAtTangentLength(1, length);

                //        if (endParameter != -1)
                //        {
                //            // If the distance is valid cut the curve
                //            Curve[] endcuttedcurves = startcuttedcurves[0].DivideByLengthFromParameter(endParameter, 0);

                //            // Add the cutted middle segment to the result array
                //            result.Add(endcuttedcurves[0]);
                //        }
                //    }
                //}
                //else
                //{
                Curve extended = curve.ExtendStart(length * -1).ExtendEnd(length * -1);
                result.Add(extended);
                //}
            }
            return result;
        }


        /// <summary>
        /// Get Material Names from a Revit Element
        /// </summary>
        /// <param name="element">Revit Element</param>
        /// <param name="paintMaterials">Paint Materials</param>
        /// <returns>List of Names</returns>
        public static List<string> GetMaterialNames(Revit.Elements.Element element, bool paintMaterials = false)
        {
            // Get the active Document
            Autodesk.Revit.DB.Document document = DocumentManager.Instance.CurrentDBDocument;

            List<string> materialnames = new List<string>();

            foreach (Autodesk.Revit.DB.ElementId id in element.InternalElement.GetMaterialIds(paintMaterials))
            {
                RVT.Material material = (RVT.Material)document.GetElement(id);
                
                if (!materialnames.Contains(material.Name))
                    materialnames.Add(material.Name);
            }

            return materialnames;
        }

        /// <summary>
        /// Get Parameter Name and Value
        /// </summary>
        /// <param name="parameter">Revit Parameter</param>
        /// <returns>Name and Value</returns>
        [MultiReturn(new[] { "Name", "Value" })]
        public static Dictionary<string, object> GetParameterNameAndValue(Autodesk.Revit.DB.Parameter parameter)
        {
            return new Dictionary<string, object>
            {
                { "Name", parameter.Definition.Name },
                { "Value", parameter.AsValueString() },
            };
        }

        /// <summary>
        /// Update an elements location Point
        /// </summary>
        /// <param name="element"></param>
        /// <param name="point"></param>
        public static void UpdateLocationPoint(Revit.Elements.Element element, Point point)
        {
            if (element.InternalElement.Location.GetType() == typeof(Autodesk.Revit.DB.LocationPoint)){
                Autodesk.Revit.DB.Document document = DocumentManager.Instance.CurrentDBDocument;
                TransactionManager.Instance.EnsureInTransaction(document);
                Autodesk.Revit.DB.LocationPoint pt = (Autodesk.Revit.DB.LocationPoint)element.InternalElement.Location;
                pt.Point = point.ToRevitType(true);
                TransactionManager.Instance.TransactionTaskDone();
            }
        }

        /// <summary>
        /// Update an elements location curve
        /// </summary>
        /// <param name="element"></param>
        /// <param name="curve"></param>
        public static void UpdateLocationCurve(Revit.Elements.Element element, Curve curve)
        {
            if (element.InternalElement.Location.GetType() == typeof(Autodesk.Revit.DB.LocationCurve))
            {
                Autodesk.Revit.DB.Document document = DocumentManager.Instance.CurrentDBDocument;
                TransactionManager.Instance.EnsureInTransaction(document);
                Autodesk.Revit.DB.LocationCurve pt = (Autodesk.Revit.DB.LocationCurve)element.InternalElement.Location;
                pt.Curve = curve.ToRevitType(true);
                TransactionManager.Instance.TransactionTaskDone();
            }
        }

        /// <summary>
        /// Sets the Layout Rule property of rebar set to SetLayoutAsMaximumSpacing
        /// </summary>
        /// <param name="rebar"></param>
        /// <param name="spacing"></param>
        /// <param name="arrayLength"></param>
        /// <param name="barsOnNormalSide"></param>
        /// <param name="includeFirstBar"></param>
        /// <param name="includeLastBar"></param>
        public static void SetLayoutAsMaximumSpacing(Revit.Elements.Element rebar, double spacing, double arrayLength, bool barsOnNormalSide, bool includeFirstBar, bool includeLastBar)
        {
            Autodesk.Revit.DB.Structure.Rebar bar = rebar.InternalElement as Autodesk.Revit.DB.Structure.Rebar;
            if (bar != null)
            {
                var sda = bar.GetShapeDrivenAccessor();
                if (sda != null)
                {
                    Autodesk.Revit.DB.Document document = DocumentManager.Instance.CurrentDBDocument;
                    TransactionManager.Instance.EnsureInTransaction(document);
                    sda.SetLayoutAsMaximumSpacing(spacing, arrayLength, barsOnNormalSide, includeFirstBar, includeLastBar);
                    TransactionManager.Instance.TransactionTaskDone();
                }
            }
        }

        /// <summary>
        /// Sets the Layout Rule property of rebar set to SetLayoutAsMinimumClearSpacing
        /// </summary>
        /// <param name="rebar"></param>
        /// <param name="spacing"></param>
        /// <param name="arrayLength"></param>
        /// <param name="barsOnNormalSide"></param>
        /// <param name="includeFirstBar"></param>
        /// <param name="includeLastBar"></param>
        public static void SetLayoutAsMinimumClearSpacing(Revit.Elements.Element rebar, double spacing, double arrayLength, bool barsOnNormalSide, bool includeFirstBar, bool includeLastBar)
        {
            Autodesk.Revit.DB.Structure.Rebar bar = rebar.InternalElement as Autodesk.Revit.DB.Structure.Rebar;
            if (bar != null)
            {
                var sda = bar.GetShapeDrivenAccessor();
                if (sda != null)
                {
                    Autodesk.Revit.DB.Document document = DocumentManager.Instance.CurrentDBDocument;
                    TransactionManager.Instance.EnsureInTransaction(document);
                    sda.SetLayoutAsMinimumClearSpacing(spacing, arrayLength, barsOnNormalSide, includeFirstBar, includeLastBar);
                    TransactionManager.Instance.TransactionTaskDone();
                }
            }
        }

        /// <summary>
        /// Sets the Layout Rule property of rebar set to SetLayoutAsFixedNumber
        /// </summary>
        /// <param name="rebar"></param>
        /// <param name="numberOfBarPositions"></param>
        /// <param name="arrayLength"></param>
        /// <param name="barsOnNormalSide"></param>
        /// <param name="includeFirstBar"></param>
        /// <param name="includeLastBar"></param>
        public static void SetLayoutAsFixedNumber(Revit.Elements.Element rebar, int numberOfBarPositions, double arrayLength, bool barsOnNormalSide, bool includeFirstBar, bool includeLastBar)
        {
            Autodesk.Revit.DB.Structure.Rebar bar = rebar.InternalElement as Autodesk.Revit.DB.Structure.Rebar;
            if (bar != null)
            {
                var sda = bar.GetShapeDrivenAccessor();
                if (sda != null)
                {
                    Autodesk.Revit.DB.Document document = DocumentManager.Instance.CurrentDBDocument;
                    TransactionManager.Instance.EnsureInTransaction(document);
                    sda.SetLayoutAsFixedNumber(numberOfBarPositions, arrayLength, barsOnNormalSide, includeFirstBar, includeLastBar);
                    TransactionManager.Instance.TransactionTaskDone();
                }
            }
        }

        /// <summary>
        /// Sets the Layout Rule property of rebar set to NumberWithSpacing
        /// </summary>
        /// <param name="rebar"></param>
        /// <param name="numberOfBarPositions"></param>
        /// <param name="arrayLength"></param>
        /// <param name="barsOnNormalSide"></param>
        /// <param name="includeFirstBar"></param>
        /// <param name="includeLastBar"></param>
        public static void SetLayoutAsNumberWithSpacing(Revit.Elements.Element rebar, int numberOfBarPositions, double arrayLength, bool barsOnNormalSide, bool includeFirstBar, bool includeLastBar)
        {
            Autodesk.Revit.DB.Structure.Rebar bar = rebar.InternalElement as Autodesk.Revit.DB.Structure.Rebar;
            if (bar != null)
            {
                var sda = bar.GetShapeDrivenAccessor();
                if (sda != null)
                {
                    Autodesk.Revit.DB.Document document = DocumentManager.Instance.CurrentDBDocument;
                    TransactionManager.Instance.EnsureInTransaction(document);
                    sda.SetLayoutAsNumberWithSpacing(numberOfBarPositions, arrayLength, barsOnNormalSide, includeFirstBar, includeLastBar);
                    TransactionManager.Instance.TransactionTaskDone();
                }
            }
        }

        /// <summary>
        /// Sets the Layout Rule property of rebar set to Single
        /// </summary>
        /// <param name="rebar"></param>
        public static void SetLayoutAsSingle(Revit.Elements.Element rebar)
        {
            Autodesk.Revit.DB.Structure.Rebar bar = rebar.InternalElement as Autodesk.Revit.DB.Structure.Rebar;
            if (bar != null)
            {
                var sda = bar.GetShapeDrivenAccessor();
                if (sda != null)
                {
                    Autodesk.Revit.DB.Document document = DocumentManager.Instance.CurrentDBDocument;
                    TransactionManager.Instance.EnsureInTransaction(document);
                    sda.SetLayoutAsSingle();
                    TransactionManager.Instance.TransactionTaskDone();
                }
            }
        }

        /// <summary>
        /// Distribution path length
        /// </summary>
        /// <param name="rebar"></param>
        /// <returns></returns>
        public static double GetArrayLength(Revit.Elements.Element rebar)
        {
            Autodesk.Revit.DB.Structure.Rebar bar = rebar.InternalElement as Autodesk.Revit.DB.Structure.Rebar;
            if (bar != null)
            {
                var sda = bar.GetShapeDrivenAccessor();
                if (sda != null)
                {
                    return sda.ArrayLength;
                }
            }
            return 0.0;
        }

        /// <summary>
        /// Overall height for spirals
        /// </summary>
        /// <param name="rebar"></param>
        /// <returns></returns>
        public static double GetHeight(Revit.Elements.Element rebar)
        {
            Autodesk.Revit.DB.Structure.Rebar bar = rebar.InternalElement as Autodesk.Revit.DB.Structure.Rebar;
            if (bar != null)
            {
                var sda = bar.GetShapeDrivenAccessor();
                if (sda != null)
                {
                    return sda.Height;
                }
            }
            return 0.0;
        }

        /// <summary>
        /// Depth of multiplanar rebar
        /// </summary>
        /// <param name="rebar">Rebar element</param>
        /// <returns>Multiplanar depth</returns>
        public static double GetMultiplanarDepth(Revit.Elements.Element rebar)
        {
            Autodesk.Revit.DB.Structure.Rebar bar = rebar.InternalElement as Autodesk.Revit.DB.Structure.Rebar;
            if (bar != null)
            {
                var sda = bar.GetShapeDrivenAccessor();
                if (sda != null)
                {
                    return sda.MultiplanarDepth;
                }
            }
            return 0.0;
        }

        /// <summary>
        /// Normal Vector
        /// </summary>
        /// <param name="rebar"></param>
        /// <returns></returns>
        public static Vector GetNormal(Revit.Elements.Element rebar)
        {
            Autodesk.Revit.DB.Structure.Rebar bar = rebar.InternalElement as Autodesk.Revit.DB.Structure.Rebar;
            if (bar != null)
            {
                var sda = bar.GetShapeDrivenAccessor();
                if (sda != null)
                {
                    return Vector.ByCoordinates(sda.Normal.X, sda.Normal.Y, sda.Normal.Z);
                }
            }
            return Vector.ByCoordinates(0, 0, 0);
        }


        /// <summary>
        /// Get Material Properties By Name
        /// </summary>
        /// <param name="materialname">Material Name</param>
        /// <returns>Material Properties</returns>
        [MultiReturn(new[] { "Name", "Id", "Category", "Class", "Transparency", "Smoothness", "Shininess", "Color", "Appearance Name", "Appearance Parameters", "Structural Name", "Structural Parameters", "Thermal Name", "Thermal Parameters" })]
        public static Dictionary<string, object> GetMaterialProperties(string materialname)
        {
            Revit.Elements.Material mat = Revit.Elements.Material.ByName(materialname);
            
            RVT.Material material = (RVT.Material)mat.InternalElement;
            

            // Get the active Document
            Autodesk.Revit.DB.Document document = DocumentManager.Instance.CurrentDBDocument;

            string appearancename = "None";
            List<Autodesk.Revit.DB.Parameter> appearances = new List<Autodesk.Revit.DB.Parameter>();
            if (material.AppearanceAssetId != Autodesk.Revit.DB.ElementId.InvalidElementId)
            {
                RVT.AppearanceAssetElement appearance = (RVT.AppearanceAssetElement)document.GetElement(material.AppearanceAssetId);
                appearancename = appearance.Name;

                foreach (RVT.Parameter parameter in appearance.Parameters)
                    if (!appearances.Contains(parameter))
                        appearances.Add(parameter);
            }

            string thermalname = "None";
            List<Autodesk.Revit.DB.Parameter> thermals = new List<Autodesk.Revit.DB.Parameter>();
            if (material.ThermalAssetId != Autodesk.Revit.DB.ElementId.InvalidElementId)
            {
                RVT.PropertySetElement thermal = (RVT.PropertySetElement)document.GetElement(material.ThermalAssetId);
                thermalname = thermal.Name;

                foreach (RVT.Parameter parameter in thermal.Parameters)
                    if (!thermals.Contains(parameter))
                        thermals.Add(parameter);
            }

            string structuralname = "None";
            List<Autodesk.Revit.DB.Parameter> structurals = new List<Autodesk.Revit.DB.Parameter>();
            if (material.StructuralAssetId != Autodesk.Revit.DB.ElementId.InvalidElementId)
            {
                RVT.PropertySetElement structural = (RVT.PropertySetElement)document.GetElement(material.StructuralAssetId);
                structuralname = structural.Name;

                foreach (RVT.Parameter parameter in structural.Parameters)
                    if (!structurals.Contains(parameter))
                        structurals.Add(parameter);
            }

            return new Dictionary<string, object>
            {
                { "Name", material.Name },
                { "Id", material.Id.IntegerValue },
                { "Category", material.MaterialCategory },
                { "Class", material.MaterialClass },
                { "Transparency", material.Transparency },
                { "Smoothness", material.Smoothness },
                { "Shininess", material.Shininess },
                { "Color", material.Color },
                { "Appearance Name", appearancename },
                { "Appearance Parameters", appearances },
                { "Structural Name", structuralname },
                { "Structural Parameters", structurals },
                { "Thermal Name", thermalname },
                { "Thermal Parameters", thermals },
            };
        }


    }
}


