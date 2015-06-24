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
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;

/// <summary>
/// Rebar Nodes
/// </summary>
public static class Rebar
{
    /// <summary>
    /// Morph Bars from one Curve to another
    /// </summary>
    /// <param name="fromCurve">Source Curve</param>
    /// <param name="toCurve">Target Curve</param>
    /// <param name="numberOfBars">Number of Bars to create</param>
    /// <param name="coverParameter">Offset</param>
    /// <returns>List of morphed bars</returns>
    public static List<Curve> MorphedRebar(Curve fromCurve, Curve toCurve, int numberOfBars, double coverParameter = 0)
    {
        return fromCurve.MorphTo(toCurve, numberOfBars, 20, coverParameter);
    }

    /// <summary>
    /// Create Rebar perpendicular to Face
    /// </summary>
    /// <param name="face">Face to Rebar</param>
    /// <param name="boundary">Boundary Faces</param>
    /// <param name="height">Height Parameter</param>
    /// <param name="numberOfBars">Number of Bars to create</param>
    /// <param name="horizontal">Horizontal or Vertical</param>
    /// <param name="coverParameter">Offset</param>
    /// <returns>List of perpendicular bars</returns>
    public static List<Curve> PerpendicularRebar(Surface face, List<Surface> boundary, double height, int numberOfBars, bool horizontal = true, double coverParameter = 0)
    {
        return face.NormalCurves(boundary, numberOfBars, coverParameter, height, horizontal);
    }

    /// <summary>
    /// Create Rebar following a selected surface
    /// </summary>
    /// <param name="face">Surface to rebar</param>
    /// <param name="numberOfBars">Define number of bars or distance between bars</param>
    /// <param name="distanceBetweenRebar">Define distance between bars or number of bars</param>
    /// <param name="flip">Flip orientation</param>
    /// <param name="coverParameter">Rebar cover parameter</param>
    /// <returns>List of rebar</returns>
    public static List<Curve> FollowingSurface(Surface face, int numberOfBars, double distanceBetweenRebar = 0, bool flip = true, double coverParameter = 0)
    {
       return face.Follow(20, coverParameter, distanceBetweenRebar, numberOfBars, flip);
    }


}
