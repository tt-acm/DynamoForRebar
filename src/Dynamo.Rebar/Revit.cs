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
using Autodesk.Revit.DB;
using DynamoServices;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace Revit.Elements
{
    [RegisterForTrace]
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
        private Rebar(System.Collections.Generic.List<Curve> curve,
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
            SafeInit(() => InitRebar(curve, barType, barStyle,host,startHook,endHook,startHookOrientation,endHookOrientation,normal,useExistingShape,createNewShape));
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
        private void InitRebar(System.Collections.Generic.List<Curve> curve, 
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
            Autodesk.Revit.DB.Document Document = DocumentManager.Instance.CurrentDBDocument;

            // This creates a new wall and deletes the old one
            TransactionManager.Instance.EnsureInTransaction(Document);

            //Phase 1 - Check to see if the object exists and should be rebound
            var rebarElem = ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.Structure.Rebar>(Document);

            bool existingRebar = false;
            if (rebarElem != null) existingRebar = true;

            var rebar = existingRebar ? rebarElem :
                     Autodesk.Revit.DB.Structure.Rebar.CreateFromCurves(Document,barStyle,barType,startHook,endHook,host,normal,curve,startHookOrientation,endHookOrientation,useExistingShape,createNewShape);
            InternalSetRebar(rebar);

  

            TransactionManager.Instance.TransactionTaskDone();

            // delete the element stored in trace and add this new one
            ElementBinder.CleanupAndSetElementForTrace(Document, InternalRebar);
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
        /// Create Rebar by Curve
        /// </summary>
        /// <param name="curves">Set of Curves</param>
        /// <param name="hostId">Host Element Id</param>
        public static Rebar ByCurve(System.Collections.Generic.List<Autodesk.DesignScript.Geometry.Curve> curves, int hostId)
        {
            if (curves == null) throw new ArgumentNullException("curves");

            System.Collections.Generic.List<Curve> revitCurves = new System.Collections.Generic.List<Curve>();

            foreach (Autodesk.DesignScript.Geometry.Curve curve in curves)
            {
                revitCurves.Add(curve.ToRevitType());
            }


            ElementId id = new ElementId(hostId);
            Autodesk.Revit.DB.Element host = DocumentManager.Instance.CurrentDBDocument.GetElement(id);

            FilteredElementCollector fec = new FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument).OfClass(typeof(Autodesk.Revit.DB.Structure.RebarHookType));
            Autodesk.Revit.DB.Structure.RebarHookType hookType = (Autodesk.Revit.DB.Structure.RebarHookType)fec.FirstElement();


            FilteredElementCollector fec2 = new FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument).OfClass(typeof(Autodesk.Revit.DB.Structure.RebarBarType));
             Autodesk.Revit.DB.Structure.RebarBarType rebarBarType = (Autodesk.Revit.DB.Structure.RebarBarType)fec2.FirstElement();

          //  FilteredElementCollector fec3 = new FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument).OfClass(typeof(Autodesk.Revit.DB.Structure.RebarContainerType));
          //  Autodesk.Revit.DB.Structure.RebarContainerType rebarCType = (Autodesk.Revit.DB.Structure.RebarContainerType)fec3.FirstElement();

           //  Autodesk.Revit.DB.Structure.RebarContainer container = Autodesk.Revit.DB.Structure.RebarContainer.Create(DocumentManager.Instance.CurrentDBDocument, host, rebarCType.Id);         
           // container.AppendItemFromCurves(Autodesk.Revit.DB.Structure.RebarStyle.StirrupTie, rebarBarType, hookType, hookType, new XYZ(0,0,1),revitCurves,Autodesk.Revit.DB.Structure.RebarHookOrientation.Left, Autodesk.Revit.DB.Structure.RebarHookOrientation.Left, true, false);
            
            
            

             return new Rebar(revitCurves, rebarBarType, Autodesk.Revit.DB.Structure.RebarStyle.Standard, host, hookType, hookType, Autodesk.Revit.DB.Structure.RebarHookOrientation.Left, Autodesk.Revit.DB.Structure.RebarHookOrientation.Left, XYZ.BasisZ, false, true);
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
               // IsRevitOwned = isRevitOwned
            };
        }

        #endregion
    }
}
