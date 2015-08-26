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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit.Elements
{
    public class RebarHookOrientation
    {
        #region private constructors

        private RebarHookOrientation(Autodesk.Revit.DB.Structure.RebarHookOrientation hookOrientation)
        {
            internalElement = hookOrientation;
        }

        #endregion

        #region private members

        private readonly Autodesk.Revit.DB.Structure.RebarHookOrientation internalElement;

        #endregion

        #region public properties

        /// <summary>
        /// The name of the Category.
        /// </summary>
        public string Name
        {
            get
            {
                return internalElement.ToString();
            }
        }


        #endregion

        internal Autodesk.Revit.DB.Structure.RebarHookOrientation InternalElement
        {
            get { return internalElement; }
        }

        #region public static constructors

        /// <summary>
        /// Gets a Revit category by the built-in category name.
        /// </summary>
        /// <param name="name">The built in category name.</param>
        /// <returns></returns>
        public static RebarHookOrientation ByName(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            Autodesk.Revit.DB.Structure.RebarHookOrientation hook = Autodesk.Revit.DB.Structure.RebarHookOrientation.Left;

            if (!Enum.TryParse<Autodesk.Revit.DB.Structure.RebarHookOrientation>(name, out hook))
                throw new Exception("Cannot parse " + name);

            return new RebarHookOrientation(hook);
        }

        #endregion

        public override string ToString()
        {
            return internalElement.ToString();
        }
    }
}
