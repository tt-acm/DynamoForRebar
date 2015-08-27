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
    public class Tag : Element
    {
        #region Internal Properties

        /// <summary>
        /// Internal reference to the Revit Element
        /// </summary>
        internal Autodesk.Revit.DB.IndependentTag InternalTag
        {
            get;
            private set;
        }

        /// <summary>
        /// Reference to the Element
        /// </summary>
        public override Autodesk.Revit.DB.Element InternalElement
        {
            get { return InternalTag; }
        }

        #endregion

        #region Private constructors

        /// <summary>
        /// Create from an existing Revit Element
        /// </summary>
        /// <param name="rebar"></param>
        private Tag(Autodesk.Revit.DB.IndependentTag tag)
        {
            SafeInit(() => InitTag(tag));
        }


        private Tag(
            Autodesk.Revit.DB.Element host,
            Autodesk.Revit.DB.TagOrientation orientation,
            Autodesk.Revit.DB.TagMode mode,
            bool addLeader,
            Autodesk.Revit.DB.XYZ point)
        {
            SafeInit(() => InitTag(host,orientation,mode,addLeader,point));
        }

        #endregion

        #region Helpers for private constructors

        /// <summary>
        /// Initialize a Rebar element
        /// </summary>
        /// <param name="rebar"></param>
        private void InitTag(Autodesk.Revit.DB.IndependentTag tag)
        {
            InternalSetTag(tag);
        }


        private void InitTag(
            Autodesk.Revit.DB.Element host,
            Autodesk.Revit.DB.TagOrientation orientation,
            Autodesk.Revit.DB.TagMode mode,
            bool addLeader,
            Autodesk.Revit.DB.XYZ point)
        {
            Autodesk.Revit.DB.Document document = DocumentManager.Instance.CurrentDBDocument;

            // This creates a new wall and deletes the old one
            TransactionManager.Instance.EnsureInTransaction(document);

            //Phase 1 - Check to see if the object exists and should be rebound
            var tagElem = ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.IndependentTag>(document);

            if (tagElem == null)
                tagElem = document.Create.NewTag(document.ActiveView, host, addLeader, mode, orientation, point);

            InternalSetTag(tagElem);

            TransactionManager.Instance.TransactionTaskDone();


            if (tagElem != null)
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
        private void InternalSetTag(Autodesk.Revit.DB.IndependentTag tag)
        {
            InternalTag = tag;
            InternalElementId = tag.Id;
            InternalUniqueId = tag.UniqueId;
        }

        #endregion

        #region Public static constructors

        /// <summary>
        /// Create an element based Tag
        /// </summary>
        /// <param name="element">Element to tag</param>
        /// <param name="horizontal">Horizontal alignment</param>
        /// <param name="addLeader">Add a leader</param>
        /// <returns></returns>
        public static Tag ByElement(Element element, bool horizontal, bool addLeader)
        {
            Autodesk.Revit.DB.Document document = DocumentManager.Instance.CurrentDBDocument;
            Autodesk.Revit.DB.XYZ point = null;
            Autodesk.Revit.DB.TagMode tagMode = TagMode.TM_ADDBY_CATEGORY;
            Autodesk.Revit.DB.TagOrientation orientation = (horizontal)? TagOrientation.Horizontal : TagOrientation.Vertical;

            if (document.ActiveView.ViewType != ViewType.FloorPlan && 
                document.ActiveView.ViewType != ViewType.EngineeringPlan &&
                document.ActiveView.ViewType != ViewType.Detail &&
                document.ActiveView.ViewType != ViewType.Section &&
                document.ActiveView.ViewType != ViewType.Elevation &&
                document.ActiveView.ViewType != ViewType.CeilingPlan)
                throw new ArgumentException("Cannot place a Tag on active View");


                if (element.InternalElement.Location.GetType() == typeof(LocationPoint))
                {
                    LocationPoint locationPoint = (LocationPoint)element.InternalElement.Location;
                    point = locationPoint.Point;
                }
                else if (element.InternalElement.Location.GetType() == typeof(LocationCurve))
                {
                    LocationCurve locationCurve = (LocationCurve)element.InternalElement.Location;
                    point = locationCurve.Curve.GetEndPoint(0);
                }
                else
                {
                    BoundingBoxXYZ box = element.InternalElement.get_BoundingBox(document.ActiveView);
                    if (box == null) box = element.InternalElement.get_BoundingBox(null);
                    if (box != null)
                    {
                        point = box.Min + ((box.Max - box.Min) / 2);
                    }
                    else throw new ArgumentNullException("Cannot determine location");
                }


            return new Tag(element.InternalElement, orientation, tagMode, addLeader,point);
        }

        #endregion

        #region Internal static constructors

        /// <summary>
        /// Create a Rebar from an existing reference
        /// </summary>
        /// <param name="rebar"></param>
        /// <param name="isRevitOwned"></param>
        /// <returns></returns>
        internal static Tag FromExisting(Autodesk.Revit.DB.IndependentTag tag, bool isRevitOwned)
        {
            return new Tag(tag)
            {
                //IsRevitOwned = isRevitOwned
            };
        }

        #endregion
    }

}
