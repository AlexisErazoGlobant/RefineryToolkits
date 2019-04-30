﻿using NUnit.Framework;
using Autodesk.GenerativeToolkit.Analyse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestServices;
using Autodesk.DesignScript.Geometry;

namespace Autodesk.GenerativeToolkit.Analyse.Tests
{
    [TestFixture]
    public class PathFindingTests : GeometricTestBase
    {
        // Checks if shortest path returns the right value
        // and its helper functions works
        [Test]
        public void ShortestPathTest()
        {
            // Create layout with a boundary and two internal obstacles
            List<Point> boundaryPoints = new List<Point>
            {
                Point.ByCoordinates(25,-25),
                Point.ByCoordinates(25,25),
                Point.ByCoordinates(-25,25),
                Point.ByCoordinates(-25,-25)
            };
            List<Point> internal1Points = new List<Point>
            {
                Point.ByCoordinates(-1,0),
                Point.ByCoordinates(-1,10),
                Point.ByCoordinates(1,10),
                Point.ByCoordinates(1,0)
            };
            List<Point> internal2Points = new List<Point>
            {
                Point.ByCoordinates(11,0),
                Point.ByCoordinates(11,10),
                Point.ByCoordinates(13,10),
                Point.ByCoordinates(13,0)
            };
            Polygon boundary = Polygon.ByPoints(boundaryPoints);
            List<Polygon> internalPolygons = new List<Polygon>();
            new List<List<Point>> { internal1Points, internal2Points }.ForEach(lst => internalPolygons.Add(Polygon.ByPoints(lst)));

            // Create origin and desitination point
            Point originPoint = Point.ByCoordinates(-12, 5);
            Point destination = Point.ByCoordinates(17, 3);

            // Create visibility graph used to calculate the shortest path
            Visibility visibilityGraph = PathFinding.CreateVisibilityGraph(new List<Polygon> { boundary }, internalPolygons);
            
            // Check if the visibility graph is created properly
            Assert.IsTrue(!visibilityGraph.Equals(null));

            // Create shortest path
            var result = PathFinding.ShortestPath(visibilityGraph, originPoint, destination);

            // Check if the result of the Shortest path is a dictionary containing the keys "path" and "length"
            Assert.IsTrue(result.Keys.Contains("path"));
            Assert.IsTrue(result.Keys.Contains("length"));

            // Check if the length of the path is correct
            var length = (double)result["length"];
            Assert.AreEqual(35.225, Math.Round(length,3));

            // Check if the PathFinding.Lines returns the correct amount of lines
            var lines = PathFinding.Lines((BaseGraph)result["path"]);
            Assert.AreEqual(5, lines.Count());
        }
    }
}