﻿using BlobSallad;
using NUnit.Framework;

namespace BlobSalladTests
{
    public class PointTests
    {
        [Test]
        public void ctorTest()
        {
            Point point = new Point(23.0, 31.0);
            Assert.AreEqual(23.0, point.getX());
            Assert.AreEqual(31.0, point.getY());
        }
    }
}