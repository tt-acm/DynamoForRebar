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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Dynamo.Controls;
using Dynamo.Interfaces;
using Dynamo.Nodes;
using Dynamo.Models;
using CoreNodeModels.Properties;
using Dynamo.Graph.Nodes;

namespace Revit.Elements
{
    /// <summary>
    /// Select edges
    /// </summary>
    [NodeName("Select Edges")]
    [NodeCategory("Revit.Rebar")]
    [NodeDescription("Select Edges")]
    [IsDesignScriptCompatible]
    public class Edges : Dynamo.Nodes.ReferenceSelection
    {
        /// <summary>
        /// Select Edges
        /// </summary>
        public Edges() : base(CoreNodeModels.SelectionType.Many, CoreNodeModels.SelectionObjectType.Edge, "Select edges.", "Edges") { }
    }
}
