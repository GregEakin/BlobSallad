﻿// Found from https://blobsallad.se/
// Originally Written by: bjoern.lindberg@gmail.com
// Translated to C# by Greg Eakin

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BlobSallad
{
    public class Blob
    {
        public enum Eye
        {
            Open,
            Closed,
            Crossed
        }

        public enum Face
        {
            Smile,
            Open,
            Ooh
        }

        private readonly List<Stick> _sticks = new List<Stick>();
        private readonly List<PointMass> _pointMasses = new List<PointMass>();
        private readonly List<Joint> _joints = new List<Joint>();
        private readonly Random _random = new Random();

        private readonly Color _highlight = Colors.Pink;    // 255, 204, 204
        private readonly Color _normal = Colors.White;
        private Face _drawFaceStyle = Face.Smile;
        private Eye _drawEyeStyle = Eye.Open;

        public static int Mod(int x, int m)
        {
            return (x % m + m) % m;
        }

        public Blob(double x, double y, double radius, int numPointMasses)
        {
            if (x < 0.0 || y < 0.0)
                throw new ArgumentException("Can't have negative offsets for X and Y.");
            if (radius <= 0.0)
                throw new ArgumentException("Can't a a radius <= zero.");
            if (numPointMasses < 2)
                throw new ArgumentException("Not enough point masses.");

            X = x;
            Y = y;
            Radius = radius;

            for (var i = 0; i < numPointMasses; ++i)
            {
                var theta = (double) i * 2.0 * Math.PI / (double) numPointMasses;
                var cx = Math.Cos(theta) * radius + x;
                var cy = Math.Sin(theta) * radius + y;
                var mass = i < 2 ? 4.0 : 1.0;
                var pointMass = new PointMass(cx, cy, mass);
                _pointMasses.Add(pointMass);
            }

            MiddlePointMass = new PointMass(x, y, 1.0);

            for (var i = 0; i < numPointMasses; ++i)
            {
                var pointMassA = _pointMasses[i];
                var index = Mod(i + 1, numPointMasses);
                var pointMassB = _pointMasses[index];
                var stick = new Stick(pointMassA, pointMassB);
                _sticks.Add(stick);
            }

            var low = 0.95;
            var high = 1.05;
            for (var i = 0; i < numPointMasses; ++i)
            {
                var pointMassA = _pointMasses[i];
                var index = Mod(i + numPointMasses / 2 + 1, numPointMasses);
                var pointMassB = _pointMasses[index];
                var joint1 = new Joint(pointMassA, pointMassB, low, high);
                _joints.Add(joint1);
                var joint2 = new Joint(pointMassA, MiddlePointMass, high * 0.9, low * 1.1);
                _joints.Add(joint2);
            }
        }

        public double X { get; set; }

        public double Y { get; set; }

        public PointMass[] PointMasses => _pointMasses.ToArray();

        public Stick[] Sticks => _sticks.ToArray();

        public Joint[] Joints => _joints.ToArray();

        public PointMass MiddlePointMass { get; }

        public double Radius { get; private set; }

        public void AddBlob(Blob blob)
        {
            var dist = Radius + blob.Radius;
            var joint = new Joint(MiddlePointMass, blob.MiddlePointMass, 0.0, 0.0);
            joint.SetDist(dist * 0.95, 0.0);
            _joints.Add(joint);
        }

        public double XPos => MiddlePointMass.XPos;

        public double YPos => MiddlePointMass.YPos;

        public void Scale(double scaleFactor)
        {
            foreach (var joint in _joints)
                joint.Scale(scaleFactor);

            foreach (var stick in _sticks)
                stick.Scale(scaleFactor);

            Radius *= scaleFactor;
        }

        public void Move(double dt)
        {
            foreach (var pointMass in _pointMasses)
                pointMass.Move(dt);

            MiddlePointMass.Move(dt);
        }

        public void Sc(Environment env)
        {
            for (var j = 0; j < 4; ++j)
            {
                foreach (var pointMass in _pointMasses)
                {
                    var collision = env.Collision(pointMass.Pos, pointMass.PrevPos);
                    var friction = collision ? 0.75 : 0.01;
                    pointMass.Friction = friction;
                }

                foreach (var stick in _sticks)
                {
                    stick.Sc(env);
                }

                foreach (var joint in _joints)
                {
                    joint.Sc();
                }
            }
        }

        public Vector Force
        {
            get => MiddlePointMass.Force;
            set
            {
                foreach (var pointMass in _pointMasses)
                    pointMass.Force = value;

                MiddlePointMass.Force = value;
            }
        }

        public void AddForce(Vector force)
        {
            foreach (var pointMass in _pointMasses)
                pointMass.AddForce(force);

            MiddlePointMass.AddForce(force);
            var pointMass1 = _pointMasses[0];
            pointMass1.AddForce(force);
            pointMass1.AddForce(force);
            pointMass1.AddForce(force);
            pointMass1.AddForce(force);
        }

        public void MoveTo(double x, double y)
        {
            var blobPos = MiddlePointMass.Pos;
            x -= blobPos.X;
            y -= blobPos.Y;

            foreach (var pointMass in _pointMasses)
            {
                blobPos = pointMass.Pos;
                blobPos.AddX(x);
                blobPos.AddY(y);
            }

            blobPos = MiddlePointMass.Pos;
            blobPos.AddX(x);
            blobPos.AddY(y);
        }

        public bool Selected { get; set; }

        public void DrawEars(Canvas canvas, double scaleFactor)
        {
        }

        public void DrawEyesOpen(Canvas canvas, double scaleFactor, TransformGroup translateTransform)
        {
            {
                var radius = 0.12 * Radius * scaleFactor;
                var x = -0.15 * Radius * scaleFactor;
                var y = -0.20 * Radius * scaleFactor;
                var circle = new EllipseGeometry(new Point(x, y), radius, radius, translateTransform);
                var orangePath = new Path
                {
                    Fill = Brushes.White,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1.0,
                    Data = circle,
                };
                canvas.Children.Add(orangePath);
            }

            {
                var radius = 0.12 * Radius * scaleFactor;
                var x = 0.15 * Radius * scaleFactor;
                var y = -0.20 * Radius * scaleFactor;
                var circle = new EllipseGeometry(new Point(x, y), radius, radius, translateTransform);
                var orangePath = new Path
                {
                    Fill = Brushes.White,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1.0,
                    Data = circle,
                };
                canvas.Children.Add(orangePath);
            }

            {
                var radius = 0.06 * Radius * scaleFactor;
                var x = -0.15 * Radius * scaleFactor;
                var y = -0.17 * Radius * scaleFactor;
                var circle = new EllipseGeometry(new Point(x, y), radius, radius, translateTransform);
                var orangePath = new Path
                {
                    Fill = Brushes.Black,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1.0,
                    Data = circle,
                };
                canvas.Children.Add(orangePath);
            }

            {
                var radius = 0.06 * Radius * scaleFactor;
                var x = 0.15 * Radius * scaleFactor;
                var y = -0.17 * Radius * scaleFactor;
                var circle = new EllipseGeometry(new Point(x, y), radius, radius, translateTransform);
                var orangePath = new Path
                {
                    Fill = Brushes.Black,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1.0,
                    Data = circle,
                };
                canvas.Children.Add(orangePath);
            }
        }

        public void DrawEyesClosed(Canvas canvas, double scaleFactor, TransformGroup translateTransform)
        {
            {
                var radius = 0.12 * Radius * scaleFactor;
                var x = -0.15 * Radius * scaleFactor;
                var y = -0.20 * Radius * scaleFactor;
                var circle = new EllipseGeometry(new Point(x, y), radius, radius);
                var leftEye = new Path
                {
                    Fill = Brushes.White,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1.0,
                    Data = circle,
                    RenderTransform = translateTransform
                };
                canvas.Children.Add(leftEye);
            }

            {
                var radius = 0.12 * Radius * scaleFactor;
                var x = 0.15 * Radius * scaleFactor;
                var y = -0.20 * Radius * scaleFactor;
                var circle = new EllipseGeometry(new Point(x, y), radius, radius);
                var rightEye = new Path
                {
                    Fill = Brushes.White,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1.0,
                    Data = circle,
                    RenderTransform = translateTransform
                };
                canvas.Children.Add(rightEye);
            }

            {
                var startPointA = new Point(-0.25 * Radius * scaleFactor, -0.20 * Radius * scaleFactor);
                var pathFigureA = new PathFigure {StartPoint = startPointA};

                var pointA = new Point(-0.05 * Radius * scaleFactor, -0.20 * Radius * scaleFactor);
                var lineSegment1A = new LineSegment {Point = pointA};
                pathFigureA.Segments.Add(lineSegment1A);

                var startPointB = new Point(0.25 * Radius * scaleFactor, -0.20 * Radius * scaleFactor);
                var pathFigureB = new PathFigure {StartPoint = startPointB};

                var pointB = new Point(0.05 * Radius * scaleFactor, -0.20 * Radius * scaleFactor);
                var lineSegment1B = new LineSegment {Point = pointB};
                pathFigureB.Segments.Add(lineSegment1B);

                var pathGeometry = new PathGeometry {Figures = new PathFigureCollection {pathFigureA, pathFigureB}};

                var orangePath = new Path
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = 1.0,
                    Data = pathGeometry,
                    RenderTransform = translateTransform
                };

                canvas.Children.Add(orangePath);
            }
        }

        public void DrawSmile(Canvas canvas, double scaleFactor, TransformGroup translateTransform)
        {
            var point = new Point(0.25 * Radius * scaleFactor, 0.1 * Radius * scaleFactor);
            var size = new Size(0.25 * Radius * scaleFactor, 0.25 * Radius * scaleFactor);
            var arcSegment = new ArcSegment
            {
                Point = point,
                Size = size,
                IsLargeArc = true,
                SweepDirection = SweepDirection.Counterclockwise,
            };

            // pathGeometry = {M-34.5,13.8A27.6,27.6,0,1,0,34.5,13.8}
            var pathFigure = new PathFigure
            {
                StartPoint = new Point(-0.25 * Radius * scaleFactor, 0.1 * Radius * scaleFactor)
            };
            pathFigure.Segments.Add(arcSegment);

            var pathGeometry = new PathGeometry();
            pathGeometry.Figures.Add(pathFigure);

            var arcPath = new Path
            {
                Stroke = Brushes.Black,
                StrokeThickness = 2.0,
                Data = pathGeometry,
                Fill = Brushes.Transparent,
                RenderTransform = translateTransform
            };

            canvas.Children.Add(arcPath);
        }

        public void DrawOpenMouth(Canvas canvas, double scaleFactor, TransformGroup translateTransform)
        {
            var point = new Point(0.25 * Radius * scaleFactor, 0.1 * Radius * scaleFactor);
            var size = new Size(0.25 * Radius * scaleFactor, 0.25 * Radius * scaleFactor);
            var arcSegment = new ArcSegment
            {
                Point = point,
                Size = size,
                IsLargeArc = true,
                SweepDirection = SweepDirection.Counterclockwise,
            };

            // pathGeometry = {M-34.5,13.8A27.6,27.6,0,1,0,34.5,13.8}
            var pathFigure = new PathFigure
            {
                StartPoint = new Point(-0.25 * Radius * scaleFactor, 0.1 * Radius * scaleFactor)
            };
            pathFigure.Segments.Add(arcSegment);

            var pathGeometry = new PathGeometry();
            pathGeometry.Figures.Add(pathFigure);

            var arcPath = new Path
            {
                Stroke = Brushes.Black,
                StrokeThickness = 2.0,
                Data = pathGeometry,
                Fill = Brushes.Black,
                RenderTransform = translateTransform
            };

            canvas.Children.Add(arcPath);
        }

        public void DrawOohFace(Canvas canvas, double scaleFactor, TransformGroup translateTransform)
        {
            {
                var point = new Point(0.25 * Radius * scaleFactor, 0.1 * Radius * scaleFactor);
                var size = new Size(0.25 * Radius * scaleFactor, 0.25 * Radius * scaleFactor);
                var arcSegment = new ArcSegment
                {
                    Point = point,
                    Size = size,
                    IsLargeArc = true,
                    SweepDirection = SweepDirection.Counterclockwise,
                };

                // pathGeometry = {M-34.5,13.8A27.6,27.6,0,1,0,34.5,13.8}
                var pathFigure = new PathFigure
                {
                    StartPoint = new Point(-0.25 * Radius * scaleFactor, 0.1 * Radius * scaleFactor)
                };
                pathFigure.Segments.Add(arcSegment);

                var pathGeometry = new PathGeometry();
                pathGeometry.Figures.Add(pathFigure);

                var arcPath = new Path
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = 2.0,
                    Data = pathGeometry,
                    Fill = Brushes.Black,
                    RenderTransform = translateTransform
                };

                canvas.Children.Add(arcPath);
            }

            {
                var startPointA = new Point(-0.25 * Radius * scaleFactor, -0.3 * Radius * scaleFactor);
                var pathFigureA = new PathFigure {StartPoint = startPointA};

                var point1A = new Point(-0.05 * Radius * scaleFactor, -0.2 * Radius * scaleFactor);
                var lineSegment1A = new LineSegment {Point = point1A};
                pathFigureA.Segments.Add(lineSegment1A);

                var point2A = new Point(-0.25 * Radius * scaleFactor, -0.1 * Radius * scaleFactor);
                var lineSegment2A = new LineSegment {Point = point2A};
                pathFigureA.Segments.Add(lineSegment2A);

                var startPointB = new Point(0.25 * Radius * scaleFactor, -0.3 * Radius * scaleFactor);
                var pathFigureB = new PathFigure {StartPoint = startPointB};

                var point1B = new Point(0.05 * Radius * scaleFactor, -0.2 * Radius * scaleFactor);
                var lineSegment1B = new LineSegment {Point = point1B};
                pathFigureB.Segments.Add(lineSegment1B);

                var point2B = new Point(0.25 * Radius * scaleFactor, -0.1 * Radius * scaleFactor);
                var lineSegment2B = new LineSegment {Point = point2B};
                pathFigureB.Segments.Add(lineSegment2B);

                // Figures = {M-34.5,-41.4L-6.9,-27.6L-34.5,-13.8 M34.5,-41.4L6.9,-27.6L34.5,-13.8}
                var pathGeometry = new PathGeometry {Figures = new PathFigureCollection {pathFigureA, pathFigureB}};

                var orangePath = new Path
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = 2.0,
                    Data = pathGeometry,
                    RenderTransform = translateTransform
                };

                canvas.Children.Add(orangePath);
            }
        }

        public void UpdateFace()
        {
            if (_drawFaceStyle == Face.Smile && _random.NextDouble() < 0.05)
            {
                _drawFaceStyle = Face.Open;
            }
            else if (_drawFaceStyle == Face.Open && _random.NextDouble() < 0.1)
            {
                _drawFaceStyle = Face.Smile;
            }

            if (_drawEyeStyle == Eye.Open && _random.NextDouble() < 0.025)
            {
                _drawEyeStyle = Eye.Closed;
            }
            else if (_drawEyeStyle == Eye.Closed && _random.NextDouble() < 0.3)
            {
                _drawEyeStyle = Eye.Open;
            }
        }

        public void DrawFace(Canvas canvas, double scaleFactor, TransformGroup translateTransform)
        {
            if (MiddlePointMass.Velocity > 0.004)
            {
                DrawOohFace(canvas, scaleFactor, translateTransform);
            }
            else
            {
                if (_drawFaceStyle == Face.Smile)
                {
                    DrawSmile(canvas, scaleFactor, translateTransform);
                }
                else
                {
                    DrawOpenMouth(canvas, scaleFactor, translateTransform);
                }

                if (_drawEyeStyle == Eye.Open)
                {
                    DrawEyesOpen(canvas, scaleFactor, translateTransform);
                }
                else
                {
                    DrawEyesClosed(canvas, scaleFactor, translateTransform);
                }
            }
        }

        public PointMass this[int index]
        {
            get
            {
                index = Mod(index, _pointMasses.Count);
                return _pointMasses[index];
            }
        }

        public void DrawBody(Canvas canvas, double scaleFactor)
        {
            var pbzSeg = new PolyBezierSegment();
            for (var i = 0; i < PointMasses.Length; ++i)
            {
                var prevPointMass = PointMasses[Mod(i - 1, PointMasses.Length)];
                var currentPointMass = PointMasses[Mod(i, PointMasses.Length)];
                var nextPointMass = PointMasses[Mod(i + 1, PointMasses.Length)];
                var nextNextPointMass = PointMasses[Mod(i + 2, PointMasses.Length)];
                var tx = nextPointMass.XPos;
                var ty = nextPointMass.YPos;
                var cx = currentPointMass.XPos;
                var cy = currentPointMass.YPos;
                var px = cx * 0.5 + tx * 0.5;
                var py = cy * 0.5 + ty * 0.5;
                var nx = cx - prevPointMass.XPos + tx - nextNextPointMass.XPos;
                var ny = cy - prevPointMass.YPos + ty - nextNextPointMass.YPos;
                px += nx * 0.16;
                py += ny * 0.16;
                px *= scaleFactor;
                py *= scaleFactor;
                tx *= scaleFactor;
                ty *= scaleFactor;

                //        generalPath.curveTo(px, py, tx, ty, tx, ty);
                pbzSeg.Points.Add(new Point(px, py));
                pbzSeg.Points.Add(new Point(tx, ty));
                pbzSeg.Points.Add(new Point(tx, ty));
            }

            var myPathSegmentCollection = new PathSegmentCollection {pbzSeg};

            var startX = PointMasses[0].XPos * scaleFactor;
            var startY = PointMasses[0].YPos * scaleFactor;
            var pthFigure = new PathFigure
            {
                StartPoint = new Point(startX, startY),
                Segments = myPathSegmentCollection
            };

            var pthFigureCollection = new PathFigureCollection {pthFigure};

            var pthGeometry = new PathGeometry {Figures = pthFigureCollection};

            var color = Selected ? _highlight : _normal;

            var arcPath = new Path
            {
                Stroke = new SolidColorBrush(Colors.Black),
                StrokeThickness = 3,
                Data = pthGeometry,
                Fill = new SolidColorBrush(color)
            };

            canvas.Children.Add(arcPath);
        }

        public void DrawSimpleBody(Canvas canvas, double scaleFactor)
        {
            foreach (var stick in _sticks)
                stick.Draw(canvas, scaleFactor);

            foreach (var pointMass in _pointMasses)
                pointMass.Draw(canvas, scaleFactor);
        }

        public void Draw(Canvas canvas, double scaleFactor)
        {
            var transformGroup = new TransformGroup();

            var up = new Vector(0.0, -1.0);
            var ori = new Vector(_pointMasses[0].Pos);
            ori.Sub(MiddlePointMass.Pos);
            var ang = Math.Acos(ori.DotProd(up) / ori.Length());
            var radians = (ori.X < 0.0) ? -ang : ang;
            var theta = (180.0 / Math.PI) * radians;
            var rotateTransform = new RotateTransform(theta); //, MiddlePointMass.XPos, MiddlePointMass.YPos);
            transformGroup.Children.Add(rotateTransform);

            var tx = MiddlePointMass.XPos * scaleFactor;
            var ty = (MiddlePointMass.YPos - 0.35 * Radius) * scaleFactor;
            var translateTransform = new TranslateTransform(tx, ty);
            transformGroup.Children.Add(translateTransform);

            DrawBody(canvas, scaleFactor);
            UpdateFace();
            DrawFace(canvas, scaleFactor, transformGroup);
        }
    }
}