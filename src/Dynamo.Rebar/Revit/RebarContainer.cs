﻿//
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
using Autodesk.Revit.DB;
using DynamoServices;
using Autodesk.DesignScript.Runtime;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using RevitServices.Transactions;
using System.Collections.Generic;


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
        private RebarContainer(System.Collections.Generic.List<Curve> curve,
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
            SafeInit(() => InitRebarContainer(curve, barType, barStyle, host, startHook, endHook, startHookOrientation, endHookOrientation, normal, useExistingShape, createNewShape));
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
        private void InitRebarContainer(System.Collections.Generic.List<Curve> curves, 
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

            // This creates a new wall and deletes the old one
            TransactionManager.Instance.EnsureInTransaction(document);

            //Phase 1 - Check to see if the object exists and should be rebound
            var rebarElem = ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.Structure.RebarContainer>(document);
            
            var container = rebarElem;

            bool contentUpdated = false;

            if (rebarElem == null)
            {
                ElementId stdC = Autodesk.Revit.DB.Structure.RebarContainerType.CreateDefaultRebarContainerType(DocumentManager.Instance.CurrentDBDocument);
                container = Autodesk.Revit.DB.Structure.RebarContainer.Create(DocumentManager.Instance.CurrentDBDocument, host, stdC);
            }
            else
            {
                container.ClearItems();
            }


            foreach (Curve curve in curves)
            {
                System.Collections.Generic.List<Curve> revitCurves = new System.Collections.Generic.List<Curve>();
                revitCurves.Add(curve);

                container.AppendItemFromCurves(barStyle, barType, startHook, endHook, XYZ.BasisZ, revitCurves, startHookOrientation, endHookOrientation, false, true);
            }

            InternalSetRebarContainer(container);

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
        private void InternalSetRebarContainer(Autodesk.Revit.DB.Structure.RebarContainer rebar)
        {
            InternalRebarContainer = rebar;
            InternalElementId = rebar.Id;
            InternalUniqueId = rebar.UniqueId;
        }

        #endregion

        #region Public static constructors

        /// <summary>
        /// Create Rebar by Curve
        /// </summary>
        /// <param name="curves">Set of Curves</param>
        /// <param name="hostElementId">Host Element Id</param>
        public static RebarContainer ByCurve(
            System.Collections.Generic.List<Autodesk.DesignScript.Geometry.Curve> curves,
            int hostElementId,
            string rebarStyle,
            Revit.Elements.Element rebarBarType,
            string startHookOrientation,
            string endHookOrientation,            
            Revit.Elements.Element startHookType,
            Revit.Elements.Element endHookType,
            Autodesk.DesignScript.Geometry.Vector normal
            )
        {
            if (normal == null) normal = Autodesk.DesignScript.Geometry.Vector.ZAxis();

            if (curves == null) throw new ArgumentNullException("curves");
            if (hostElementId == null) throw new ArgumentNullException("hostElementId");

            ElementId elementId = new ElementId(hostElementId);
            if (elementId == ElementId.InvalidElementId) throw new ArgumentNullException("hostElementId");

            Autodesk.Revit.DB.Element host = DocumentManager.Instance.CurrentDBDocument.GetElement(elementId);

            System.Collections.Generic.List<Curve> revitCurves = new System.Collections.Generic.List<Curve>();

            Autodesk.Revit.DB.Structure.RebarStyle barStyle = Autodesk.Revit.DB.Structure.RebarStyle.StirrupTie;
            Enum.TryParse<Autodesk.Revit.DB.Structure.RebarStyle>(rebarStyle, out barStyle);

            Autodesk.Revit.DB.Structure.RebarHookOrientation startOrientation = Autodesk.Revit.DB.Structure.RebarHookOrientation.Left;
            Enum.TryParse<Autodesk.Revit.DB.Structure.RebarHookOrientation>(startHookOrientation, out startOrientation);
            Autodesk.Revit.DB.Structure.RebarHookOrientation endOrientation = Autodesk.Revit.DB.Structure.RebarHookOrientation.Left;
            Enum.TryParse<Autodesk.Revit.DB.Structure.RebarHookOrientation>(endHookOrientation, out endOrientation);

            foreach (Autodesk.DesignScript.Geometry.Curve curve in curves)
            {
               
                    revitCurves.Add(curve.Approximate());

            }

            return new RebarContainer(revitCurves, (Autodesk.Revit.DB.Structure.RebarBarType)rebarBarType.InternalElement, barStyle, host,
                (Autodesk.Revit.DB.Structure.RebarHookType)startHookType.InternalElement,
                (Autodesk.Revit.DB.Structure.RebarHookType)endHookType.InternalElement, startOrientation, endOrientation, normal.ToRevitType(), false, true);
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
               //IsRevitOwned = isRevitOwned
            };
        }

        #endregion
    }

}