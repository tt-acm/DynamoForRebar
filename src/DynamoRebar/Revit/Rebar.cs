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
    public class Rebar : Element
    {
        #region Internal Properties

        /// <summary>
        /// Internal reference to the Revit Element
        /// </summary>
        internal Autodesk.Revit.DB.Structure.Rebar InternalRebar
        {
            get;
            private set;
        }

        /// <summary>
        /// Reference to the Element
        /// </summary>
        public override Autodesk.Revit.DB.Element InternalElement
        {
            get { return InternalRebar; }
        }

        #endregion

        #region Private constructors

        /// <summary>
        /// Create from an existing Revit Element
        /// </summary>
        /// <param name="rebar"></param>
        private Rebar(Autodesk.Revit.DB.Structure.Rebar rebar)
        {
            SafeInit(() => InitRebar(rebar));
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
        private Rebar(object curve,
            Autodesk.Revit.DB.Structure.RebarBarType barType,
            Autodesk.Revit.DB.Structure.RebarStyle barStyle,
            Autodesk.Revit.DB.Element host,
            Autodesk.Revit.DB.Structure.RebarHookType startHook,
            Autodesk.Revit.DB.Structure.RebarHookType endHook,
            Autodesk.Revit.DB.Structure.RebarHookOrientation startHookOrientation,
            Autodesk.Revit.DB.Structure.RebarHookOrientation endHookOrientation,
            Autodesk.Revit.DB.XYZ normal,
            bool useExistingShape,
            bool createNewShape)
        {
            SafeInit(() => InitRebar(curve, barType, barStyle, host, startHook, endHook, startHookOrientation, endHookOrientation, normal, useExistingShape, createNewShape));
        }

        private Rebar(IList<IList<Curve>> curves,
    Autodesk.Revit.DB.Structure.RebarBarType barType,
    Autodesk.Revit.DB.Element host)
        {
            SafeInit(() => InitRebarFreeForm(curves, barType, host));
        }

        #endregion

        #region Helpers for private constructors

        /// <summary>
        /// Initialize a Rebar element
        /// </summary>
        /// <param name="rebar"></param>
        private void InitRebar(Autodesk.Revit.DB.Structure.Rebar rebar)
        {
            InternalSetRebar(rebar);
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
        private void InitRebar(object curve,
            Autodesk.Revit.DB.Structure.RebarBarType barType,
            Autodesk.Revit.DB.Structure.RebarStyle barStyle,
            Autodesk.Revit.DB.Element host,
            Autodesk.Revit.DB.Structure.RebarHookType startHook,
            Autodesk.Revit.DB.Structure.RebarHookType endHook,
            Autodesk.Revit.DB.Structure.RebarHookOrientation startHookOrientation,
            Autodesk.Revit.DB.Structure.RebarHookOrientation endHookOrientation,
            Autodesk.Revit.DB.XYZ normal,
            bool useExistingShape,
            bool createNewShape)
        {
            Autodesk.Revit.DB.Document document = DocumentManager.Instance.CurrentDBDocument;
            
            TransactionManager.Instance.EnsureInTransaction(document);

            var rebarElem = ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.Structure.Rebar>(document);



            // geometry wrapper for polycurves

            List<Curve> geometry = new List<Curve>();

            if (curve.GetType() == typeof(DynamoRebar.RevitPolyCurve))
            {
                DynamoRebar.RevitPolyCurve polycurve = (DynamoRebar.RevitPolyCurve)curve;
                geometry = polycurve.Curves;
            }
            else
            {
                geometry.Add((Curve)curve);
            }


            bool changed = false;



            // Check for existing Geometry

            if (rebarElem != null)
            {
                
                foreach (Curve existingCurve in rebarElem.GetShapeDrivenAccessor().ComputeDrivingCurves())
                {
                    bool curveIsExisting = false;

                    foreach (Curve newCurve in geometry)
                        if (CurveUtils.CurvesAreSimilar(newCurve, existingCurve)) { curveIsExisting = true; break; }

                    if (!curveIsExisting) changed = true;
                }
            }




            if (rebarElem == null || changed)
            {
                // Delete exsiting Rebar Element
                if (rebarElem != null && rebarElem.Id != ElementId.InvalidElementId) 
                    document.Delete(rebarElem.Id);

                rebarElem = Autodesk.Revit.DB.Structure.Rebar.CreateFromCurves(document, barStyle, barType, startHook, endHook, host, normal, geometry, startHookOrientation, endHookOrientation, useExistingShape, createNewShape);
            }
            else
            {
                rebarElem.SetHostId(document, host.Id);
                rebarElem.SetHookTypeId(0, startHook.Id);
                rebarElem.SetHookTypeId(1, endHook.Id);
                rebarElem.SetHookOrientation(0, startHookOrientation);
                rebarElem.SetHookOrientation(1, endHookOrientation);
                rebarElem.ChangeTypeId(barType.Id);
            }


            InternalSetRebar(rebarElem);

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

        private void InitRebarFreeForm(IList<IList<Curve>> curves,
            Autodesk.Revit.DB.Structure.RebarBarType barType,
            Autodesk.Revit.DB.Element host)
        {
            Autodesk.Revit.DB.Document document = DocumentManager.Instance.CurrentDBDocument;

            TransactionManager.Instance.EnsureInTransaction(document);

            var rebarElem = ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.Structure.Rebar>(document);



            bool changed = false;



            // Check for existing Geometry
            // TBI




            if (rebarElem == null || changed)
            {
                // Delete exsiting Rebar Element
                if (rebarElem != null && rebarElem.Id != ElementId.InvalidElementId)
                    document.Delete(rebarElem.Id);

                Autodesk.Revit.DB.Structure.RebarFreeFormValidationResult res = new Autodesk.Revit.DB.Structure.RebarFreeFormValidationResult();
                rebarElem = Autodesk.Revit.DB.Structure.Rebar.CreateFreeForm(document, barType, host, curves, out res);
                if (res != Autodesk.Revit.DB.Structure.RebarFreeFormValidationResult.Success)
                {
                    TransactionManager.Instance.ForceCloseTransaction();
                    throw new Exception(res.ToString());
                }
            }
            else
            {
                rebarElem.SetHostId(document, host.Id);
                rebarElem.ChangeTypeId(barType.Id);
            }


            InternalSetRebar(rebarElem);

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

        #endregion

        #region Private mutators

        /// <summary>
        /// Set the internal Element, ElementId, and UniqueId
        /// </summary>
        /// <param name="rebar"></param>
        private void InternalSetRebar(Autodesk.Revit.DB.Structure.Rebar rebar)
        {
            InternalRebar = rebar;
            InternalElementId = rebar.Id;
            InternalUniqueId = rebar.UniqueId;
        }

        #endregion

        #region Public static constructors

        /// <summary>
        /// Create Rebar by Curves
        /// </summary>
        /// <param name="curve">Input Curves</param>
        /// <param name="hostElementId">Host Element Id</param>
        /// <param name="rebarStyle">Rebar Style</param>
        /// <param name="rebarBarType">Bar Type</param>
        /// <param name="startHookOrientation">Hokk orientation at the start</param>
        /// <param name="endHookOrientation">Hook orientation at the end</param>
        /// <param name="startHookType">Hook type at the start</param>
        /// <param name="endHookType">Hook type at the end</param>
        /// <param name="vector">Normal Vectors</param>
        /// <returns></returns>
        public static Rebar ByCurve(
            Autodesk.DesignScript.Geometry.Curve curve,
            int hostElementId,
            string rebarStyle,
            Revit.Elements.Element rebarBarType,
            string startHookOrientation,
            string endHookOrientation,
            Revit.Elements.Element startHookType,
            Revit.Elements.Element endHookType,
            Autodesk.DesignScript.Geometry.Vector vector
            )
        {
            if (curve == null) throw new ArgumentNullException("Input Curve missing");
            if (hostElementId == null) throw new ArgumentNullException("Host ElementId missing");
            if (rebarStyle == null) throw new ArgumentNullException("Rebar Style missing");
            if (rebarBarType == null) throw new ArgumentNullException("Rebar Bar Type missing");
            if (startHookOrientation == null) throw new ArgumentNullException("Start Hook Orientation missing");
            if (endHookOrientation == null) throw new ArgumentNullException("End Hook Orientation missing");
            //if (startHookType == null) throw new ArgumentNullException("Start Hook Type missing");
            //if (endHookType == null) throw new ArgumentNullException("End Hook Type missing");
            if (vector == null) throw new ArgumentNullException("Normal Vector missing");

            ElementId elementId = new ElementId(hostElementId);
            if (elementId == ElementId.InvalidElementId) throw new ArgumentNullException("Host ElementId error");

            Autodesk.Revit.DB.Element host = DocumentManager.Instance.CurrentDBDocument.GetElement(elementId);
            

            Autodesk.Revit.DB.Structure.RebarStyle barStyle = Autodesk.Revit.DB.Structure.RebarStyle.StirrupTie;
            Enum.TryParse<Autodesk.Revit.DB.Structure.RebarStyle>(rebarStyle, out barStyle);

            Autodesk.Revit.DB.Structure.RebarHookOrientation startOrientation = Autodesk.Revit.DB.Structure.RebarHookOrientation.Left;
            Enum.TryParse<Autodesk.Revit.DB.Structure.RebarHookOrientation>(startHookOrientation, out startOrientation);
            Autodesk.Revit.DB.Structure.RebarHookOrientation endOrientation = Autodesk.Revit.DB.Structure.RebarHookOrientation.Left;
            Enum.TryParse<Autodesk.Revit.DB.Structure.RebarHookOrientation>(endHookOrientation, out endOrientation);

            Autodesk.Revit.DB.Structure.RebarHookType startHookT = (startHookType == null) ? null : (Autodesk.Revit.DB.Structure.RebarHookType)startHookType.InternalElement;
            Autodesk.Revit.DB.Structure.RebarHookType endHookT = (endHookType == null) ? null : (Autodesk.Revit.DB.Structure.RebarHookType)endHookType.InternalElement;

            return new Rebar(curve.ApproximateToRvt(), (Autodesk.Revit.DB.Structure.RebarBarType)rebarBarType.InternalElement, barStyle, host, startHookT, endHookT
                , startOrientation, endOrientation, vector.ToRevitType(), true, true);
        }

        /// <summary>
        /// Create FreeForm Rebar
        /// </summary>
        /// <param name="curves">nested List of Curves</param>
        /// <param name="hostElementId">Rebar Host ID</param>
        /// <param name="rebarBarType">Bar Types</param>
        /// <returns></returns>
        public static Rebar FreeFormRebarByCurves(
    IEnumerable<IEnumerable<Autodesk.DesignScript.Geometry.Curve>> curves,
    int hostElementId,
    Revit.Elements.Element rebarBarType
    )
        {
            if (curves == null) throw new ArgumentNullException("Input Curves missing");
            if (hostElementId == null) throw new ArgumentNullException("Host ElementId missing");
            if (rebarBarType == null) throw new ArgumentNullException("Rebar Bar Type missing");

            ElementId elementId = new ElementId(hostElementId);
            if (elementId == ElementId.InvalidElementId) throw new ArgumentNullException("Host ElementId error");

            Autodesk.Revit.DB.Element host = DocumentManager.Instance.CurrentDBDocument.GetElement(elementId);

            IList<IList<Curve>> rvtcurvescontainer = new List<IList<Curve>>();
            foreach (IEnumerable<Autodesk.DesignScript.Geometry.Curve> dyncurves in curves)
            {
                List<Curve> rvtcurves = new List<Curve>();
                foreach (Autodesk.DesignScript.Geometry.Curve dyncurve in dyncurves)
                {
                    rvtcurves.Add(dyncurve.ToRevitType());
                }
                rvtcurvescontainer.Add(rvtcurves);
            }

            return new Rebar(rvtcurvescontainer, (Autodesk.Revit.DB.Structure.RebarBarType)rebarBarType.InternalElement, host);
        }

        /// <summary>
        /// Set unobscured in specified View
        /// </summary>
        /// <param name="rebar">Single Rebar</param>
        /// <param name="view">View</param>
        /// <param name="unobscured">Unobscured</param>
        public static void SetUnobscuredInView(Revit.Elements.Element rebar, Revit.Elements.Views.View view, bool unobscured)
        {
            Autodesk.Revit.DB.Structure.Rebar bar = rebar.InternalElement as Autodesk.Revit.DB.Structure.Rebar;
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
        /// <param name="rebar">Single Rebar</param>
        /// <param name="view">3D View</param>
        /// <param name="solid">Solid</param>
        public static void SetSolidInView(Revit.Elements.Element rebar, Revit.Elements.Views.View3D view, bool solid)
        {
            Autodesk.Revit.DB.Structure.Rebar bar = rebar.InternalElement as Autodesk.Revit.DB.Structure.Rebar;
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
        /// Set Bar hidden Status
        /// </summary>
        /// <param name="rebar">Rebar element</param>
        /// <param name="view">Revit View</param>
        /// <param name="barIndex">Bar Index</param>
        /// <param name="hidden">Hidden Status</param>
        public static void SetBarHiddenStatus(Revit.Elements.Element rebar, Revit.Elements.Views.View view, int barIndex, bool hidden)
        {
            Autodesk.Revit.DB.Structure.Rebar bar = rebar.InternalElement as Autodesk.Revit.DB.Structure.Rebar;
            if (bar != null)
            {
                Autodesk.Revit.DB.Document document = DocumentManager.Instance.CurrentDBDocument;
                TransactionManager.Instance.EnsureInTransaction(document);
                
                Autodesk.Revit.DB.View viewElement = (Autodesk.Revit.DB.View)view.InternalElement;
                bar.SetBarHiddenStatus(viewElement, barIndex, hidden);

                TransactionManager.Instance.TransactionTaskDone();
            }
        }

        #endregion

        #region Internal static constructors

        /// <summary>
        /// Create a Rebar from an existing reference
        /// </summary>
        /// <param name="rebar"></param>
        /// <param name="isRevitOwned"></param>
        /// <returns></returns>
        internal static Rebar FromExisting(Autodesk.Revit.DB.Structure.Rebar rebar, bool isRevitOwned)
        {
            return new Rebar(rebar)
            {
                // Cannot access base classes internal bool IsRevitOwned
                //IsRevitOwned = isRevitOwned
            };
        }

        #endregion
    }

}
