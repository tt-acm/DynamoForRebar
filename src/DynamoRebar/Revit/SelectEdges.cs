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
using System.ComponentModel;
using System.Linq;

using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Dynamo.Controls;
using Dynamo.Interfaces;

using Dynamo.Models;
using Dynamo.Wpf;
using ProtoCore.AST.AssociativeAST;

using Revit.Elements;
using Revit.GeometryConversion;
using Revit.GeometryObjects;
//using Revit.Interactivity;

using RevitServices.Elements;
using RevitServices.Persistence;

using DividedSurface = Autodesk.Revit.DB.DividedSurface;
using Element = Autodesk.Revit.DB.Element;
using RevitDynamoModel = Dynamo.Applications.Models.RevitDynamoModel;
using Point = Autodesk.DesignScript.Geometry.Point;
using String = System.String;
using UV = Autodesk.DesignScript.Geometry.UV;
//using RevitServices.EventHandler;
using Autodesk.Revit.DB.Events;
using Dynamo.Applications;

namespace Revit.Elements
{
    ///// <summary>
    ///// Select edges
    ///// </summary>
    //public class Edges : Dynamo.Nodes.ReferenceSelection
    //{
    //    /// <summary>
    //    /// Select Edges
    //    /// </summary>
    //    public Edges() : base(SelectionType.Many,SelectionObjectType.Edge,"Select edges.","Edges") { }
    //}
}
