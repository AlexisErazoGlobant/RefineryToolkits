﻿using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using System.Collections.Generic;
using System.Linq;
using Autodesk.RefineryToolkits.Core.Utillites;
using Dynamo.Graph.Nodes;

namespace Autodesk.RefineryToolkits.SpacePlanning.Generate
{
    public static class AmenitySpace
    {
        private const string amenitySurfaceOutputPort = "amenitySurface";
        private const string remainingSurfaceOutputPort = "remainingSurface";

        /// <summary>
        /// Creates an amenity space on a given surface, returning both the amenity space and the remaining space within the original surface.
        /// </summary>
        /// <param name="surface">Surface to create Amenity Spaces on.</param>
        /// <param name="offset">How much to offset from surface perimeter.</param>
        /// <param name="depth">The depth of the amenity space.</param> 
        /// <returns name="amenitySurface">The created amenity surface.</returns>
        /// <returns name="remainingSurface">The created amenity surface.</returns>
        [MultiReturn([amenitySurfaceOutputPort, remainingSurfaceOutputPort])]
        [NodeCategory("Create")]
        public static Dictionary<string, Surface> Create(
            Surface surface,
            double offset,
            double depth)
        {
            // offset perimeter curves by the specified offset and create new surface.
            // makes sure there are space between outer perimeter and the amenity space
            List<Curve> inCrvs = [.. surface.OffsetPerimeterCurves(offset)["insetCrvs"]];
            Surface inSrf = Surface.ByPatch(PolyCurve.ByJoinedCurves(inCrvs, 0.001, false));

            // get longest curve of the inSrf
            Curve max;
            List<Curve> others;
            Dictionary<string, dynamic> dict = inCrvs.MaximumLength();
            if (dict["maxCrv"].Count < 1)
            {
                max = dict["otherCrvs"][0] as Curve;
                int count = dict["otherCrvs"].Count;
                List<Curve> rest = dict["otherCrvs"];
                others = rest.GetRange(1, (count - 1));
            }
            else
            {
                max = dict["maxCrv"][0] as Curve;
                others = dict["otherCrvs"];
            }

            // offset the perimeter curves of input surface close to the offset curve.
            List<Curve> inCrvs2 = [.. surface.OffsetPerimeterCurves(offset*0.95)["insetCrvs"]];

            // get longest curve  
            Curve max2;
            Dictionary<string, dynamic> dict2 = inCrvs2.MaximumLength();
            if (dict2["maxCrv"].Count < 1)
            {
                max2 = dict2["otherCrvs"][0] as Curve;
            }
            else
            {
                max2 = dict2["maxCrv"][0] as Curve;
            }

            Vector vec = max2.ByTwoCurves(max);

            Curve transLine = max.Translate(vec, depth) as Curve;
            Line extendLine = transLine.ExtendAtBothEnds(1);


            List<Curve> crvList = [max, extendLine];
            Surface loftSrf = Surface.ByLoft(crvList);

            List<bool> boolLst = [];
            foreach (var crv in others)
            {
                bool b = max.DoesIntersect(crv);
                boolLst.Add(b);
            }

            List<Curve> intersectingCurves = others
                .Zip(boolLst, (name, filter) => new { name, filter, })
                .Where(item => item.filter == true)
                .Select(item => item.name)
                .ToList();
            List<Curve> extendCurves = [];
            foreach (Curve crv in intersectingCurves)
            {
                var l = crv.ExtendAtBothEnds(1);
                extendCurves.Add(l);
            }

            List<Surface> split = loftSrf
                .SplitPlanarSurfaceByMultipleCurves(extendCurves)
                .OfType<Surface>()
                .ToList();

            Surface amenitySurf = split.MaximumArea()["maxSrf"] as Surface;

            Surface remainSurf = inSrf.Split(amenitySurf)[0] as Surface;

            Dictionary<string, Surface> newOutput;
            newOutput = new Dictionary<string, Surface>
            {
                {amenitySurfaceOutputPort,amenitySurf},
                {remainingSurfaceOutputPort,remainSurf}
            };

            //Dispose redundant geometry
            inCrvs.ForEach(crv => crv.Dispose());
            inCrvs2.ForEach(crv => crv.Dispose());
            inSrf.Dispose();
            max.Dispose();
            max2.Dispose();
            vec.Dispose();
            transLine.Dispose();
            extendLine.Dispose();
            crvList.ForEach(crv => crv.Dispose());
            loftSrf.Dispose();
            intersectingCurves.ForEach(crv => crv.Dispose());
            extendCurves.ForEach(crv => crv.Dispose());

            return newOutput;
        }
    }
}
