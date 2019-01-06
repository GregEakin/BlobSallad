﻿using BlobSallad;
using NUnit.Framework;

namespace BlobSalladTests
{
    public class JointTests
    {
        private Bones _joint;
        private PointMass _pointMassA;
        private PointMass _pointMassB;

        [Test]
        public void CtorBodyJointTest()
        {
            var cxA = 41.0;
            var cyA = 43.0;
            var massA = 4.0;
            _pointMassA = new PointMass(cxA, cyA, massA);

            var cxB = 71.0;
            var cyB = 67.0;
            var massB = 1.0;
            _pointMassB = new PointMass(cxB, cyB, massB);

            // The blob can change shape by plus or minus 5%
            var low = 0.95;
            var high = 1.05;
            _joint = new Bones(_pointMassA, _pointMassB, low, high);

            Assert.AreEqual(36.498, _joint.ShortLimit, 0.01);
            Assert.AreEqual(40.340, _joint.LongLimit, 0.01);
        }

        [Test]
        public void ScaleTest()
        {
            double cxA = 41;
            double cyA = 43;
            var massA = 4.0;
            _pointMassA = new PointMass(cxA, cyA, massA);

            var cxB = 71.0;
            var cyB = 67.0;
            var massB = 1.0;
            _pointMassB = new PointMass(cxB, cyB, massB);

            var low = 0.95;
            var high = 1.05;
            _joint = new Bones(_pointMassA, _pointMassB, low, high);
            _joint.Scale(10.0);

            Assert.AreEqual(364.978, _joint.ShortLimit, 0.01);
            Assert.AreEqual(403.397, _joint.LongLimit, 0.01);
        }
    }
}