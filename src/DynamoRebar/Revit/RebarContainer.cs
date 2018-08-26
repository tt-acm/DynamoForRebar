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
using Autodesk.Revit.DB;
using DynamoServices;
using Autodesk.DesignScript.Runtime;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using RevitServices.Transactions;
using System.Collections.Generic;
using DynamoRebar;

namespace Revit.Elements
{
    [DynamoServices.RegisterForTrace]
    public class RebarContainer : Element
    {
        #region Internal Properties

        /// <summary>
        /// Internal reference to the Revit Element
        /// </summary>
        internal Autodesk.Revit.DB.Structure.RebarContainer InternalRebarContainer
        {
            get;
            private set;
        }

        /// <summary>
        /// Reference to the Element
        /// </summary>
        public override Autodesk.Revit.DB.Element InternalElement
        {
            get { return InternalRebarContainer; }
        }

        #endregion

        #region Private constructors

        /// <summary>
        /// Create from an existing Revit Element
        /// </summary>
        /// <param name="rebar"></param>
        private RebarContainer(Autodesk.Revit.DB.Structure.RebarContainer rebar)
        {
            SafeInit(() => InitRebarContainer(rebar));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="barType"></param>
        /// <param name="barStyle"></param>
        /// <param name="host"></param>
        /// <param name="startHook"></param>
        /// <param name="endHook"></param>
        /// <param name="startHookOrientation"></param>
        /// <param name="endHookOrientation"></param>
        /// <param name="normal"></param>
        /// <param name="useExistingShape"></param>
        /// <param name="createNewShape"></param>
        private RebarContainer(System.Collections.Generic.List<object> curve,
            Autodesk.Revit.DB.Structure.RebarBarType barType,
            Autodesk.Revit.DB.Structure.RebarStyle barStyle,
            Autodesk.Revit.DB.Element host,
            Autodesk.Revit.DB.Structure.RebarHookType startHook,
            Autodesk.Revit.DB.Structure.RebarHookType endHook,
            Autodesk.Revit.DB.Structure.RebarHookOrientation startHookOrientation,
            Autodesk.Revit.DB.Structure.RebarHookOrientation endHookOrientation,
            System.Collections.Generic.List<Autodesk.Revit.DB.XYZ> normals,
            bool useExistingShape,
            bool createNewShape)
        {
            SafeInit(() => InitRebarContainer(curve, barType, barStyle, host, startHook, endHook, startHookOrientation, endHookOrientation, normals, useExistingShape, createNewShape));
        }

        private RebarContainer(System.Collections.Generic.List<Revit.Elements.Rebar> rebars)
        {
            SafeInit(() => InitRebarContainer(rebars));
        }

        #endregion

        #region Helpers for private constructors

        /// <summary>
        /// Initialize a Rebar element
        /// </summary>
        /// <param name="rebar"></param>
        private void InitRebarContainer(Autodesk.Revit.DB.Structure.RebarContainer rebar)
        {
            InternalSetRebarContainer(rebar);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="barType"></param>
        /// <param name="barStyle"></param>
        /// <param name="host"></param>
        /// <param name="startHook"></param>
        /// <param name="endHook"></param>
        /// <param name="startHookOrientation"></param>
        /// <param name="endHookOrientation"></param>
        /// <param name="normal"></param>
        /// <param name="useExistingShape"></param>
        /// <param name="createNewShape"></param>
        private void InitRebarContainer(System.Collections.Generic.List<object> curves, 
            Autodesk.Revit.DB.Structure.RebarBarType barType,
            Autodesk.Revit.DB.Structure.RebarStyle barStyle,
            Autodesk.Revit.DB.Element host, 
            Autodesk.Revit.DB.Structure.RebarHookType startHook,
            Autodesk.Revit.DB.Structure.RebarHookType endHook,
            Autodesk.Revit.DB.Structure.RebarHookOrientation startHookOrientation,
            Autodesk.Revit.DB.Structure.RebarHookOrientation endHookOrientation,
            System.Collections.Generic.List<Autodesk.Revit.DB.XYZ> normals,
            bool useExistingShape,
            bool createNewShape)
        {
            Autodesk.Revit.DB.Document document = DocumentManager.Instance.CurrentDBDocument;

            TransactionManager.Instance.EnsureInTransaction(document);

            var rebarElem = ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.Structure.RebarContainer>(document);

            // Parse Geometry

            List<List<Curve>> curvature = new List<List<Curve>>();
            foreach (object curve in curves)
            {
                List<Curve> geometry = new List<Curve>();

                if (curve.GetType() == typeof(DynamoRebar.RevitPolyCurve))
                {
                    DynamoRebar.RevitPolyCurve polycurve = (DynamoRebar.RevitPolyCurve)curve;
                    geometry = polycurve.Curves;
                }
                else
                    geometry.Add((Curve)curve);

                curvature.Add(geometry);
            }



            if (rebarElem == null)
            {
                ElementId stdC = Autodesk.Revit.DB.Structure.RebarContainerType.CreateDefaultRebarContainerType(DocumentManager.Instance.CurrentDBDocument);
                rebarElem = Autodesk.Revit.DB.Structure.RebarContainer.Create(DocumentManager.Instance.CurrentDBDocument, host, stdC);
            }
            else
            {
                //rebarElem.ClearItems();
                rebarElem.SetHostId(document, host.Id);
            }


            int counter = rebarElem.ItemsCount;

            for (int i = 0; i < counter; i++)
            {
                Autodesk.Revit.DB.Structure.RebarContainerItem item = rebarElem.GetItem(i);

                int index = GeometryMatches(item.ComputeDrivingCurves(), curvature);

                if (index == -1)
                    rebarElem.RemoveItem(item);
                else
                {
                    item.SetHookOrientation(0, startHookOrientation);
                    item.SetHookOrientation(1, endHookOrientation);
                    item.SetHookTypeId(0, startHook.Id);
                    item.SetHookTypeId(1, endHook.Id);
                    curvature.RemoveAt(index);
                    if (normals.Count > 1) normals.RemoveAt(index);
                }

            }



            for (int i = 0; i < curvature.Count; i++)
            {
                // If there is only one normal in the list use this one for all curves
                XYZ normal = (normals.Count == 1) ? normals[0] : normals[i];
                List<Curve> geometry = curvature[i];

                rebarElem.AppendItemFromCurves(barStyle, barType, startHook, endHook, normal, geometry, startHookOrientation, endHookOrientation, useExistingShape, createNewShape);
            }



            // Update Quantity Parameter
            Autodesk.Revit.DB.Parameter quantityParameter = rebarElem.get_Parameter(BuiltInParameter.REBAR_ELEM_QUANTITY_OF_BARS);
            Autodesk.Revit.DB.Structure.RebarContainerParameterManager containerParameters = rebarElem.GetParametersManager();
            containerParameters.AddOverride(quantityParameter.Id, curves.Count);
            

            InternalSetRebarContainer(rebarElem);

            TransactionManager.Instance.TransactionTaskDone();


            if (rebarElem != null)
            {
                ElementBinder.CleanupAndSetElementForTrace(document, this.InternalElement);
            }
            else
            {
                ElementBinder.SetElementForTrace(this.InternalElement);
            }

        }


        private int GeometryMatches(IList<Curve> existingCurves, List<List<Curve>> newGeometry)
        {
                for (int i = 0; i < newGeometry.Count; i++ )
                {
                    List<Curve> newCurves = newGeometry[i];
                
                    bool match = true;

                    foreach (Curve existingCurve in existingCurves)
                    {
                        bool existing = false;

                        foreach (Curve newCurve in newCurves)
                            if (CurveUtils.CurvesAreSimilar(newCurve, existingCurve)) { existing = true; break; }

                        if (!existing) match = false;
                    }

                    if (match) return i;
                }

            return -1;
        }

        private void InitRebarContainer(List<Revit.Elements.Rebar> rebars)
        {
            if (rebars.Count > 0)
            {
                Autodesk.Revit.DB.Document document = DocumentManager.Instance.CurrentDBDocument;

                TransactionManager.Instance.EnsureInTransaction(document);

                var rebarElem = ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.Structure.RebarContainer>(document);

                if (rebarElem == null)
                {
                    ElementId stdC = Autodesk.Revit.DB.Structure.RebarContainerType.CreateDefaultRebarContainerType(DocumentManager.Instance.CurrentDBDocument);
                    Autodesk.Revit.DB.Element host = DocumentManager.Instance.CurrentDBDocument.GetElement(rebars[0].InternalRebar.GetHostId());
                    rebarElem = Autodesk.Revit.DB.Structure.RebarContainer.Create(DocumentManager.Instance.CurrentDBDocument, host, stdC);
                }
                else
                {
                    rebarElem.ClearItems();
                }


                foreach (Revit.Elements.Rebar rebar in rebars)
                    rebarElem.AppendItemFromRebar(rebar.InternalRebar);

                InternalSetRebarContainer(rebarElem);

                TransactionManager.Instance.TransactionTaskDone();


                if (rebarElem != null)
                {
                    ElementBinder.CleanupAndSetElementForTrace(document, this.InternalElement);
                }
                else
                {
                    ElementBinder.SetElementForTrace(this.InternalElement);
                }
            }
        }

        #endregion

        #region Private mutators

        /// <summary>
        /// Set the internal Element, ElementId, and UniqueId
        /// </summary>
        /// <param name="rebar"></param>
        private void InternalSetRebarContainer(Autodesk.Revit.DB.Structure.RebarContainer rebar)
        {
            InternalRebarContainer = rebar;
            InternalElementId = rebar.Id;
            InternalUniqueId = rebar.UniqueId;
        }

        #endregion

        #region Public static constructors

        /// <summary>
        /// Create Rebar Container by Curves
        /// </summary>
        /// <param name="curves">Input Curves</param>
        /// <param name="hostElementId">Host Element Id</param>
        /// <param name="rebarStyle">Rebar Style</param>
        /// <param name="rebarBarType">Bar Type</param>
        /// <param name="startHookOrientation">Hokk orientation at the start</param>
        /// <param name="endHookOrientation">Hook orientation at the end</param>
        /// <param name="startHookType">Hook type at the start</param>
        /// <param name="endHookType">Hook type at the end</param>
        /// <param name="vectors">Normal Vectors</param>
        /// <returns></returns>
        public static RebarContainer ByCurve(
            System.Collections.Generic.List<Autodesk.DesignScript.Geometry.Curve> curves,
            int hostElementId,
            string rebarStyle,
            Revit.Elements.Element rebarBarType,
            string startHookOrientation,
            string endHookOrientation,            
            Revit.Elements.Element startHookType,
            Revit.Elements.Element endHookType,
            System.Collections.Generic.List<Autodesk.DesignScript.Geometry.Vector> vectors
            )
        {
            if (curves == null) throw new ArgumentNullException("Input Curves missing");
            if (hostElementId == null) throw new ArgumentNullException("Host ElementId missing");
            if (rebarStyle == null) throw new ArgumentNullException("Rebar Style missing");
            if (rebarBarType == null) throw new ArgumentNullException("Rebar Bar Type missing");
            if (startHookOrientation == null) throw new ArgumentNullException("Start Hook Orientation missing");
            if (endHookOrientation == null) throw new ArgumentNullException("End Hook Orientation missing");
            //if (startHookType == null) throw new ArgumentNullException("Start Hook Type missing");
            //if (endHookType == null) throw new ArgumentNullException("End Hook Type missing");
            if (vectors == null) throw new ArgumentNullException("Normal Vector missing");

            ElementId elementId = new ElementId(hostElementId);
            if (elementId == ElementId.InvalidElementId) throw new ArgumentNullException("Host ElementId error");

            Autodesk.Revit.DB.Element host = DocumentManager.Instance.CurrentDBDocument.GetElement(elementId);

            System.Collections.Generic.List<object> revitCurves = new System.Collections.Generic.List<object>();
            foreach (Autodesk.DesignScript.Geometry.Curve curve in curves) revitCurves.Add(curve.ApproximateToRvt());

            // Parse Rebar Style
            Autodesk.Revit.DB.Structure.RebarStyle barStyle = Autodesk.Revit.DB.Structure.RebarStyle.StirrupTie;
            Enum.TryParse<Autodesk.Revit.DB.Structure.RebarStyle>(rebarStyle, out barStyle);
            
            // Parse Rebar Hook Orientation
            Autodesk.Revit.DB.Structure.RebarHookOrientation startOrientation = Autodesk.Revit.DB.Structure.RebarHookOrientation.Left;
            Enum.TryParse<Autodesk.Revit.DB.Structure.RebarHookOrientation>(startHookOrientation, out startOrientation);

            // Parse Rebar Hook Orientation
            Autodesk.Revit.DB.Structure.RebarHookOrientation endOrientation = Autodesk.Revit.DB.Structure.RebarHookOrientation.Left;
            Enum.TryParse<Autodesk.Revit.DB.Structure.RebarHookOrientation>(endHookOrientation, out endOrientation);

            List<XYZ> normals = new List<XYZ>();
            foreach (Autodesk.DesignScript.Geometry.Vector vector in vectors) normals.Add(vector.ToRevitType());

            Autodesk.Revit.DB.Structure.RebarHookType startHookT = (startHookType == null) ? null : (Autodesk.Revit.DB.Structure.RebarHookType)startHookType.InternalElement;
            Autodesk.Revit.DB.Structure.RebarHookType endHookT = (endHookType == null) ? null : (Autodesk.Revit.DB.Structure.RebarHookType)endHookType.InternalElement;

            return new RebarContainer(revitCurves, (Autodesk.Revit.DB.Structure.RebarBarType)rebarBarType.InternalElement, barStyle, host,
                startHookT,
                endHookT, startOrientation, endOrientation, normals, true, true);
        }

        /// <summary>
        /// Create Rebar Container by Bars
        /// </summary>
        /// <param name="rebars">Bars to create the container from</param>
        /// <returns></returns>
        public static RebarContainer ByBars(System.Collections.Generic.List<Revit.Elements.Rebar> rebars)
        {
            return new RebarContainer(rebars);
        }

        /// <summary>
        /// Set unobscured in specified View
        /// </summary>
        /// <param name="rebarContainer">Rebar Container</param>
        /// <param name="view">View</param>
        /// <param name="unobscured">Unobscured</param>
        public static void SetUnobscuredInView(Element rebarContainer, Revit.Elements.Views.View view, bool unobscured)
        {
            Autodesk.Revit.DB.Structure.RebarContainer bar = rebarContainer.InternalElement as Autodesk.Revit.DB.Structure.RebarContainer;
            if (bar != null)
            {
                Autodesk.Revit.DB.Document document = DocumentManager.Instance.CurrentDBDocument;
                TransactionManager.Instance.EnsureInTransaction(document);

                Autodesk.Revit.DB.View viewElement = (Autodesk.Revit.DB.View)view.InternalElement;
                bar.SetUnobscuredInView(viewElement, unobscured);

                TransactionManager.Instance.TransactionTaskDone();
            }
        }

        /// <summary>
        /// Set Solid In View
        /// </summary>
        /// <param name="rebarContainer">Rebar Container</param>
        /// <param name="view">3D View</param>
        /// <param name="solid">Solid</param>
        public static void SetSolidInView(Element rebarContainer, Revit.Elements.Views.View3D view, bool solid)
        {
            Autodesk.Revit.DB.Structure.RebarContainer bar = rebarContainer.InternalElement as Autodesk.Revit.DB.Structure.RebarContainer;
            if (bar != null)
            {
                Autodesk.Revit.DB.Document document = DocumentManager.Instance.CurrentDBDocument;
                TransactionManager.Instance.EnsureInTransaction(document);

                Autodesk.Revit.DB.View3D viewElement = (Autodesk.Revit.DB.View3D)view.InternalElement;
                bar.SetSolidInView(viewElement, solid);

                TransactionManager.Instance.TransactionTaskDone();
            }
        }

        /// <summary>
        /// Appends a Bar to an existing Rebar container (or creates it if no container is supplied)
        /// </summary>
        /// <param name="rebars">List of Bars to add</param>
        /// <param name="container">Optional: Existing Rebar Container</param>
        public static Revit.Elements.RebarContainer AppendBar(List<Revit.Elements.Rebar> rebars, Revit.Elements.RebarContainer container = null)
        {
            if (container == null)
            {
                container = Revit.Elements.RebarContainer.ByBars(rebars);
            }
            else
            {
                foreach (Revit.Elements.Rebar rebar in rebars) container.InternalRebarContainer.AppendItemFromRebar(rebar.InternalRebar);
            }


            // Delete Bars from Document

            #region delete Bars

            Autodesk.Revit.DB.Document document = DocumentManager.Instance.CurrentDBDocument;
            TransactionManager.Instance.EnsureInTransaction(document);

            List<ElementId> idsToDelete = new List<ElementId>();
            foreach (Revit.Elements.Rebar rebar in rebars)
                if (rebar.InternalRebar != null && rebar.InternalRebar.Id != ElementId.InvalidElementId) idsToDelete.Add(rebar.InternalRebar.Id);

            document.Delete(idsToDelete);

            TransactionManager.Instance.TransactionTaskDone();

            #endregion

            return container;
        }

        #endregion

        #region Internal static constructors

        /// <summary>
        /// Create a Rebar from an existing reference
        /// </summary>
        /// <param name="rebar"></param>
        /// <param name="isRevitOwned"></param>
        /// <returns></returns>
        internal static RebarContainer FromExisting(Autodesk.Revit.DB.Structure.RebarContainer rebar, bool isRevitOwned)
        {
            return new RebarContainer(rebar)
            {
               // Cannot access base classes internal bool IsRevitOwned
               //IsRevitOwned = isRevitOwned
            };
        }

        #endregion
    }

}
