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
        internal Autodesk.Revit.DB.Structure.RebarContainer InternalRebar
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
        private RebarContainer(Autodesk.Revit.DB.Structure.RebarContainer rebar)
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
            SafeInit(() => InitRebar(curve, barType, barStyle,host,startHook,endHook,startHookOrientation,endHookOrientation,normal,useExistingShape,createNewShape));
        }

        #endregion

        #region Helpers for private constructors

        /// <summary>
        /// Initialize a Rebar element
        /// </summary>
        /// <param name="rebar"></param>
        private void InitRebar(Autodesk.Revit.DB.Structure.RebarContainer rebar)
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
        private void InitRebar(System.Collections.Generic.List<Curve> curves, 
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





            if (rebarElem == null)
            {
                ElementId stdC = Autodesk.Revit.DB.Structure.RebarContainerType.CreateDefaultRebarContainerType(DocumentManager.Instance.CurrentDBDocument);
                container = Autodesk.Revit.DB.Structure.RebarContainer.Create(DocumentManager.Instance.CurrentDBDocument, host, stdC);

                //existingRebar = true;

            }
            else
            {
                container.ClearItems();
            }


            foreach (Curve curve in curves)
            {
                System.Collections.Generic.List<Curve> revitCurves = new System.Collections.Generic.List<Curve>();
                revitCurves.Add(curve);

                container.AppendItemFromCurves(Autodesk.Revit.DB.Structure.RebarStyle.Standard, barType, startHook, endHook, XYZ.BasisZ, revitCurves, Autodesk.Revit.DB.Structure.RebarHookOrientation.Left, Autodesk.Revit.DB.Structure.RebarHookOrientation.Left, false, true);
            }

            InternalSetRebar(container);


            //var rebar = existingRebar ? rebarElem :
            //         Autodesk.Revit.DB.Structure.Rebar.CreateFromCurves(Document,barStyle,barType,startHook,endHook,host,normal,curves,startHookOrientation,endHookOrientation,useExistingShape,createNewShape);
            
            

  

            TransactionManager.Instance.TransactionTaskDone();

            // delete the element stored in trace and add this new one
            //ElementBinder.CleanupAndSetElementForTrace(Document, InternalRebar);


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
        private void InternalSetRebar(Autodesk.Revit.DB.Structure.RebarContainer rebar)
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
        /// <param name="hostElementId">Host Element Id</param>
        public static RebarContainer ByCurve(System.Collections.Generic.List<Autodesk.DesignScript.Geometry.Curve> curves, int hostElementId)
        {
            if (curves == null) throw new ArgumentNullException("curves");
            if (hostElementId == null) throw new ArgumentNullException("hostElementId");

            ElementId elementId = new ElementId(hostElementId);
            if (elementId == ElementId.InvalidElementId) throw new ArgumentNullException("hostElementId");

            Autodesk.Revit.DB.Element host = DocumentManager.Instance.CurrentDBDocument.GetElement(elementId);

            FilteredElementCollector fec = new FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument).OfClass(typeof(Autodesk.Revit.DB.Structure.RebarHookType));
            Autodesk.Revit.DB.Structure.RebarHookType hookType = (Autodesk.Revit.DB.Structure.RebarHookType)fec.FirstElement();

            FilteredElementCollector fec2 = new FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument).OfClass(typeof(Autodesk.Revit.DB.Structure.RebarBarType));
            Autodesk.Revit.DB.Structure.RebarBarType rebarBarType = (Autodesk.Revit.DB.Structure.RebarBarType)fec2.FirstElement();

            System.Collections.Generic.List<Curve> revitCurves = new System.Collections.Generic.List<Curve>();

            foreach (Autodesk.DesignScript.Geometry.Curve curve in curves)
            {
               

                if (curve.GetType() == typeof(Autodesk.DesignScript.Geometry.NurbsCurve))
                
                    revitCurves.Add(Arc.Create(curve.StartPoint.ToRevitType(), curve.EndPoint.ToRevitType(), curve.PointAtParameter(0.5).ToRevitType()));
                
                else

                    revitCurves.Add(curve.ToRevitType());

            }

           

            return new RebarContainer(revitCurves, rebarBarType, Autodesk.Revit.DB.Structure.RebarStyle.Standard, host, hookType, hookType, Autodesk.Revit.DB.Structure.RebarHookOrientation.Left, Autodesk.Revit.DB.Structure.RebarHookOrientation.Left, XYZ.BasisZ, false, true);
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
               // IsRevitOwned = isRevitOwned
            };
        }

        #endregion
    }

}
