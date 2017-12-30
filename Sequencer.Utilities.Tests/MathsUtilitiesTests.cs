﻿using System;
using NUnit.Framework;

namespace Sequencer.Utilities.Tests
{
    [TestFixture]
    public class MathsUtilitiesTests
    {
        private static readonly object[] TickPositionCases =
        {
            new object[] { 0, 1, 0 },
            new object[] { 0.5, 1, 1 },
            new object[] { 1, 1, 1 },
            new object[] { 90, 96, 96 },
            new object[] { 48, 96, 96 },
            new object[] { 47, 96, 0 },
            new object[] { 190, 96, 192 },
            new object[] { 193, 96, 192 },
            new object[] { 960, 96, 960 },
            new object[] { 980, 96, 960 },
            new object[] { 970, 96, 960 },
            new object[] { 27, 12, 24 }
        };

        private static readonly object[] ClampCases =
        {
            new object[] { 1, 0, 1, 1 },
            new object[] { -1, 0, 1, 0 },
            new object[] { 2, 0, 1, 1 },
            new object[] { 0, 0, 1, 0 },
            new object[] { -1, -2, 2, -1 },
            new object[] { 9, 0, 10, 9 },
            new object[] { 13, 0, 10, 10 }
        };
        
        [Test]
        public void NearestValue_MultipleMustBeGreaterThan0()
        {
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            Assert.Throws<ArgumentException>(() => MathsUtilities.NearestValue(0, 0));

            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            Assert.Throws<ArgumentException>(() => MathsUtilities.NearestValue(0, -1));
        }

        [Test]
        [TestCaseSource(nameof(TickPositionCases))]
        public void NearestValueTests(double value, int multiple, int expectedValue)
        {
            int actualValue = MathsUtilities.NearestValue(value, multiple);

            Assert.AreEqual(expectedValue, actualValue);
        }

        [Test]
        [TestCaseSource(nameof(ClampCases))]
        public void ClampTests(int value, int min, int max, int expected)
        {
            int actual = value.Clamp(min, max);

            Assert.AreEqual(expected, actual);
        }
    }
}