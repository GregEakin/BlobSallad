﻿using ApprovalTests.Reporters;
using ApprovalTests.Wpf;
using BlobSallad;
using NUnit.Framework;
using System.Threading;
using System.Windows.Controls;
using Environment = BlobSallad.Environment;

namespace BlobSalladTests
{
    [UseReporter(typeof(DiffReporter), typeof(ClipboardReporter))]
    [Apartment(ApartmentState.STA)]
    public class BlobCollectiveTests
    {
        [Test]
        public void CtorTest()
        {
            var collective = new BlobCollective(71.0, 67.0, 4);

            Assert.AreEqual(4, collective.MaxNum);
            Assert.AreEqual(1, collective.NumActive);
        }

        [Test]
        public void SplitTest()
        {
            var canvas = new Canvas { Width = 200, Height = 200 };

            var collective = new BlobCollective(1.0, 1.0, 4);
            collective.Split();
            Assert.AreEqual(2, collective.NumActive);
            collective.Draw(canvas, 100.0);

            var wpf = new ContentControl { Content = canvas };
            WpfApprovals.Verify(wpf);
        }

        [Test]
        public void FindLargestTest()
        {
            var collective = new BlobCollective(1.0, 1.0, 4);
            collective.Split();
            collective.Split();

            var motherBlob = collective.FindLargest(null);
            Assert.AreEqual(0.300, motherBlob.Radius, 0.01);
        }

        [Test]
        public void FindSmallestTest()
        {
            var collective = new BlobCollective(1.0, 1.0, 4);
            collective.Split();
            collective.Split();

            var largest = collective.FindLargest(null);

            // Find one of the two smallest blobs.
            var smallest1 = collective.FindSmallest(null);
            Assert.AreNotSame(largest, smallest1);
            Assert.AreEqual(0.225, smallest1.Radius, 0.01);

            // Find the other smallest blob.
            var smallest2 = collective.FindSmallest(smallest1);
            Assert.AreNotSame(largest, smallest2);
            Assert.AreNotSame(smallest1, smallest2);
            Assert.AreEqual(0.225, smallest2.Radius, 0.01);
        }

        [Test]
        public void FindClosestTest()
        {
            var environment = new Environment(0.2, 0.2, 2.6, 1.6);

            var collective = new BlobCollective(1.0, 1.0, 4);
            collective.Split();
            collective.Split();

            collective.Move(1.0);
            collective.Sc(environment);

            var largest = collective.FindLargest(null);
            var smallest1 = collective.FindSmallest(null);
            var smallest2 = collective.FindSmallest(smallest1);

            var closest = collective.FindClosest(largest);
            Assert.AreSame(smallest2, closest);
        }

        [Test]
        public void JoinTest()
        {
            var canvas = new Canvas { Width = 200, Height = 200 };

            var collective = new BlobCollective(1.0, 1.0, 4);
            collective.Split();
            collective.Join();
            Assert.AreEqual(1, collective.NumActive);
            collective.Draw(canvas, 100.0);

            var wpf = new ContentControl { Content = canvas };
            WpfApprovals.Verify(wpf);
        }

        [Test]
        public void SelectBlobMissTest()
        {
            var collective = new BlobCollective(1.0, 1.0, 4);
            collective.FindClosest(2.0, 2.0);
            Assert.IsNull(collective.SelectedBlob);
        }

        [Test]
        public void SelectBlobHitTest()
        {
            var collective = new BlobCollective(1.0, 1.0, 4);
            collective.FindClosest(1.0, 1.1);
            Assert.NotNull(collective.SelectedBlob);
            Assert.True(collective.SelectedBlob.Selected);
        }

        [Test]
        public void DrawTest()
        {
            var canvas = new Canvas { Width = 200, Height = 200 };

            var collective = new BlobCollective(1.0, 1.0, 4);
            collective.Draw(canvas, 100.0);

            var wpf = new ContentControl { Content = canvas };
            WpfApprovals.Verify(wpf);
        }
    }
}