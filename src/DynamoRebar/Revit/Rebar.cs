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
        private Rebar(Curve curve,
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
        private void InitRebar(Curve curve,
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

            //always create a new bar
            //if (rebarElem == null)
                rebarElem = Autodesk.Revit.DB.Structure.Rebar.CreateFromCurves(document, barStyle, barType, startHook, endHook, host, normal, new List<Curve>(){curve}, startHookOrientation, endHookOrientation, useExistingShape, createNewShape);           
            
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
        /// <param name="curves">Input Curves</param>
        /// <param name="hostElementId">Host Element Id</param>
        /// <param name="rebarStyle">Rebar Style</param>
        /// <param name="rebarBarType">Bar Type</param>
        /// <param name="startHookOrientation">Hokk orientation at the start</param>
        /// <param name="endHookOrientation">Hook orientation at the end</param>
        /// <param name="startHookType">Hook type at the start</param>
        /// <param name="endHookType">Hook type at the end</param>
        /// <param name="vector">Curve's Normal Vector</param>
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
            if (startHookType == null) throw new ArgumentNullException("Start Hook Type missing");
            if (endHookType == null) throw new ArgumentNullException("End Hook Type missing");
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

            return new Rebar(curve.Approximate(), (Autodesk.Revit.DB.Structure.RebarBarType)rebarBarType.InternalElement, barStyle, host,
                (Autodesk.Revit.DB.Structure.RebarHookType)startHookType.InternalElement,
                (Autodesk.Revit.DB.Structure.RebarHookType)endHookType.InternalElement, startOrientation, endOrientation, vector.ToRevitType(), true, true);
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
