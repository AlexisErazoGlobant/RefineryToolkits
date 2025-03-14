﻿using Autodesk.DesignScript.Geometry;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using TestServices;

namespace Autodesk.RefineryToolkits.SpacePlanning.Analyze.Tests
{
    [TestFixture]
    public partial class VisibilityTests : GeometricTestBase
    {
        private const string percentageVisibleOutputPort = "percentageVisible";
        private const string visibleItemsOutputPort = "visibleItems";

        // Points
        private Polygon pointObstacles;
        private List<Point> samplePoints;
        private Point pointOrigin;

        // Lines
        private List<Curve> lines;
        private Point originZero;
        private List<Polygon> boundaryPoly;

        [SetUp]
        public void BeforeTest()
        {
            // for Points
            boundaryPoly = [Rectangle.ByWidthLength(50, 50) as Polygon];
            pointObstacles = Rectangle.ByWidthLength(15, 15) as Polygon;
            samplePoints = [];
            foreach (int n in Enumerable.Range(16, 10))
            {
                foreach (int i in Enumerable.Range(-25, 10))
                {
                    samplePoints.Add(Point.ByCoordinates(n, i));
                }
            }
            pointOrigin = Point.ByCoordinates(-20, -2);

            // for Lines
            lines = boundaryPoly[0].Explode().Cast<Curve>().ToList();
            originZero = Point.ByCoordinates(0, 0);
        }

        #region Points

        /// <summary>
        /// Check Visible Points dictionary output is correct
        /// </summary>
        [Test]
        public void OfPoints_DicionaryOutputTest()
        {
            var result = Visibility.OfPointsFromOrigin(
                pointOrigin,
                samplePoints,
                boundaryPoly,
                [pointObstacles]);

            Assert.IsTrue(result.Keys.Contains(percentageVisibleOutputPort));
            Assert.IsTrue(result.Keys.Contains(visibleItemsOutputPort));
        }

        /// <summary>
        /// Check if VisiblePoints returns the right score of visible points in a layout
        /// with one obstacle and a grid of 
        /// </summary>
        [Test]
        public void OfPoints_FromOriginTest()
        {
            var expectedVisibilityPercentage = 57d;
            var result = Visibility.OfPointsFromOrigin(
                pointOrigin,
                samplePoints,
                boundaryPoly,
                [pointObstacles]);

            var visiblePointsScore = (double)result[percentageVisibleOutputPort];

            Assert.AreEqual(expectedVisibilityPercentage, Math.Round(visiblePointsScore, 2));
        }

        #endregion

        #region Lines

        /// <summary>
        /// Check views to outside dictionary output is correct
        /// </summary>
        [Test]
        public void OfLines_DictionaryOutputTest()
        {
            var result = Visibility.OfLinesFromOrigin(
                originZero,
                lines,
                boundaryPoly,
                []);

            // Check if output of node is a Dictionary that contains the correct output ports
            Assert.IsTrue(result.Keys.Contains(percentageVisibleOutputPort));
            Assert.IsTrue(result.Keys.Contains(visibleItemsOutputPort));
        }

        /// <summary>
        /// Checks if the output score is correct in a layout with no obstacles blocking the views
        /// </summary>
        [Test]
        public void OfLines_CheckIfOutputScoreIsCorrectWithNoObstrutions()
        {
            var expectedVisibilityPercentage = 100d;
            var result = Visibility.OfLinesFromOrigin(
                originZero,
                lines,
                boundaryPoly,
                []);

            // Check if the score output is 1.0
            // as there are no obstacles blocking the views to outside
            var viewScore = (double)result[percentageVisibleOutputPort];
            Assert.AreEqual(expectedVisibilityPercentage, viewScore);
        }

        /// <summary>
        /// Checks that internal obstacels in the layout are detected
        /// </summary>
        [Test]
        public void OfLines_CheckIfViewsToOutsideDetectsObstaclesInLayout()
        {
            var notExpectedVisibilityPercentage = 100d;
            Polygon internalPoly = Rectangle.ByWidthLength(5, 5) as Polygon;
            Point newOrigin = originZero.Translate(10) as Point;

            var result = Visibility.OfLinesFromOrigin(
                newOrigin,
                lines,
                boundaryPoly,
                [internalPoly]);

            var viewScore = (double)result[percentageVisibleOutputPort];
            Assert.Less(viewScore, notExpectedVisibilityPercentage);
            Assert.AreNotEqual(notExpectedVisibilityPercentage, viewScore);
        }

        #endregion
    }
}