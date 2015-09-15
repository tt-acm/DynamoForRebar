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


namespace Revit.Elements
{
    [DynamoServices.RegisterForTrace]
    public class RebarHookType : Element
    {
        #region Internal Properties

        /// <summary>
        /// Internal reference to the Revit Element
        /// </summary>
        internal Autodesk.Revit.DB.Structure.RebarHookType InternalRebar
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
        private RebarHookType(Autodesk.Revit.DB.Structure.RebarHookType rebarHookType)
        {
            SafeInit(() => InitRebarHookType(rebarHookType));
        }


        private RebarHookType(double angle, double multiplier)
        {
            SafeInit(() => InitRebarHookType(angle, multiplier));
        }

        #endregion

        #region Helpers for private constructors

        /// <summary>
        /// Initialize a Rebar element
        /// </summary>
        /// <param name="rebar"></param>
        private void InitRebarHookType(Autodesk.Revit.DB.Structure.RebarHookType rebar)
        {
            InternalSetRebarHookType(rebar);
        }


        private void InitRebarHookType(double angle, double multiplier)
        {
            Autodesk.Revit.DB.Document document = DocumentManager.Instance.CurrentDBDocument;

            TransactionManager.Instance.EnsureInTransaction(document);

            var hookTypeElem = ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.Structure.RebarHookType>(document);
           
            if (hookTypeElem == null)
            {
                hookTypeElem = Autodesk.Revit.DB.Structure.RebarHookType.Create(document, angle, multiplier);
            }

            TransactionManager.Instance.TransactionTaskDone();


            if (hookTypeElem != null)
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
        private void InternalSetRebarHookType(Autodesk.Revit.DB.Structure.RebarHookType rebarHookType)
        {
            InternalRebar = rebarHookType;
            InternalElementId = rebarHookType.Id;
            InternalUniqueId = rebarHookType.UniqueId;
        }

        #endregion

        #region Public static constructors

        /// <summary>
        /// Create Rebar by Curve
        /// </summary>
        /// <param name="curves">Set of Curves</param>
        /// <param name="hostElementId">Host Element Id</param>
        public static RebarHookType ByAngle(double angle, double multiplier)
        {
            if (angle == null) throw new ArgumentNullException("angle");
            if (multiplier == null) throw new ArgumentNullException("multiplier");

            return new RebarHookType(angle, multiplier);
        }

        public static RebarHookType ByName(string name)
        {
            if (name == null) throw new ArgumentNullException("name");

            FilteredElementCollector collector = new FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument).OfClass(typeof(Autodesk.Revit.DB.Structure.RebarHookType));
            foreach (Autodesk.Revit.DB.Structure.RebarHookType hook in collector.ToElements())
            {
                if (hook.Name == name) return new RebarHookType(hook);
            }

            return null;
        }

        #endregion

        #region Internal static constructors

        /// <summary>
        /// Create a Rebar from an existing reference
        /// </summary>
        /// <param name="rebar"></param>
        /// <param name="isRevitOwned"></param>
        /// <returns></returns>
        internal static RebarHookType FromExisting(Autodesk.Revit.DB.Structure.RebarHookType rebarHookType, bool isRevitOwned)
        {
            return new RebarHookType(rebarHookType)
            {
                 // Cannot access base classes internal bool IsRevitOwned
                 //IsRevitOwned = isRevitOwned
            };
        }

        #endregion
    }

}
