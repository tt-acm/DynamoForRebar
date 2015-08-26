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
    public class RebarBarType : Element
    {
        #region Internal Properties

        /// <summary>
        /// Internal reference to the Revit Element
        /// </summary>
        internal Autodesk.Revit.DB.Structure.RebarBarType InternalRebar
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
        private RebarBarType(Autodesk.Revit.DB.Structure.RebarBarType rebarBarType)
        {
            SafeInit(() => InitRebarBarType(rebarBarType));
        }


        private RebarBarType(double diameter, Autodesk.Revit.DB.Structure.RebarDeformationType deformationType)
        {
            SafeInit(() => InitRebarBarType(diameter, deformationType));
        }

        #endregion

        #region Helpers for private constructors

        /// <summary>
        /// Initialize a Rebar element
        /// </summary>
        /// <param name="rebar"></param>
        private void InitRebarBarType(Autodesk.Revit.DB.Structure.RebarBarType rebar)
        {
            InternalSetRebarBarType(rebar);
        }


        private void InitRebarBarType(double diameter, Autodesk.Revit.DB.Structure.RebarDeformationType deformationType)
        {
            Autodesk.Revit.DB.Document document = DocumentManager.Instance.CurrentDBDocument;

            // This creates a new wall and deletes the old one
            TransactionManager.Instance.EnsureInTransaction(document);

            //Phase 1 - Check to see if the object exists and should be rebound
            var barTypeElem = ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.Structure.RebarBarType>(document);

            if (barTypeElem == null)
            {
                barTypeElem = Autodesk.Revit.DB.Structure.RebarBarType.Create(document);               
            }

            barTypeElem.BarDiameter = diameter;
            barTypeElem.DeformationType = deformationType;

            TransactionManager.Instance.TransactionTaskDone();


            if (barTypeElem != null)
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
        private void InternalSetRebarBarType(Autodesk.Revit.DB.Structure.RebarBarType rebarBarType)
        {
            InternalRebar = rebarBarType;
            InternalElementId = rebarBarType.Id;
            InternalUniqueId = rebarBarType.UniqueId;
        }

        #endregion

        #region Public static constructors

        /// <summary>
        /// Create Rebar by Curve
        /// </summary>
        /// <param name="curves">Set of Curves</param>
        /// <param name="hostElementId">Host Element Id</param>
        public static RebarBarType ByAngle(double diameter, bool deformed)
        {
            if (diameter == null) throw new ArgumentNullException("diameter");
            if (deformed == null) throw new ArgumentNullException("deformed");

            Autodesk.Revit.DB.Structure.RebarDeformationType type = (deformed) ? Autodesk.Revit.DB.Structure.RebarDeformationType.Deformed : Autodesk.Revit.DB.Structure.RebarDeformationType.Plain;

            return new RebarBarType(diameter, type);
        }

        public static RebarBarType ByName(string name)
        {
            if (name == null) throw new ArgumentNullException("name");

            FilteredElementCollector collector = new FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument).OfClass(typeof(Autodesk.Revit.DB.Structure.RebarBarType));
            foreach (Autodesk.Revit.DB.Structure.RebarBarType type in collector.ToElements())
            {
                if (type.Name == name) return new RebarBarType(type);
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
        internal static RebarBarType FromExisting(Autodesk.Revit.DB.Structure.RebarBarType rebarBarType, bool isRevitOwned)
        {
            return new RebarBarType(rebarBarType)
            {
                //IsRevitOwned = isRevitOwned
            };
        }

        #endregion
    }

}
