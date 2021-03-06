﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Microsoft.Kinect;
using System.ComponentModel;

using System.Windows.Media.Animation;
using WpfAnimatedGif;


namespace Bitness
{
    /// <summary>
    /// Interaction logic for FloorWindow.xaml
    /// </summary>
    public partial class FloorWindow : Window, INotifyPropertyChanged
    {
        /// <summary>
        /// Drawing image that we will display
        /// </summary>
        private DrawingImage imageSource;

        /// <summary>
        /// Drawing group for body rendering output
        /// </summary>
        private DrawingGroup drawingGroup;


        private readonly Brush drawBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));

        /// <summary>
        /// Small bit of text in the bottom right corner of the screen
        /// </summary>
        private string statusText = "Nothing has happened yet!";

        public const double OFFSET = 0.5f;
        public const double X_IN = 3.5f;
        public const double Y_IN = 2f;

        /// <summary>
        /// INotifyPropertyChangedPropertyChanged event to allow window controls to bind to changeable data
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;


        /// <summary>
        /// Gets the bitmap to display
        /// </summary>
        public ImageSource ImageSource
        {
            get
            {
                return this.imageSource;
            }
        }

        public string StatusText
        {
            get
            {
                return this.statusText;
            }
            set
            {
                if (this.statusText != value)
                {
                    this.statusText = value;

                    // notify any bound elements that the text has changed
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("StatusText"));
                    }
                }
            }
        }

        public FloorWindow()
        {
            this.drawingGroup = new DrawingGroup();
            this.imageSource = new DrawingImage(this.drawingGroup);

            // use the window object as the view model in this simple example
            this.DataContext = this;

            InitializeComponent();

            Canvas.SetLeft(blueFloorGif, 0.0);
            Canvas.SetTop(blueFloorGif, 0.0);


        }

        /// <summary>
        /// given a camera space point it maps it to a pixel position on the floor
        /// </summary>
        /// <param name="p">CameraSpacePoint in distance of meters</param>
        public Point MapDepthPointToFloor(CameraSpacePoint p)
        {

            // check if in bounds of screen
            if (p.Z > OFFSET &&
                p.Z < (OFFSET + Y_IN) &&
                p.X < (X_IN / 2) &&

                p.X > -(X_IN / 2))
            {
                // modify point origin from kinect to origin of screen (still meters)
                double fy = (OFFSET + Y_IN) - p.Z;
                double fx = (X_IN / 2) + p.X;

                // convert into pixels
                double px = (fx / X_IN) * this.Width;
                double py = (fy / Y_IN) * this.Height;

                return new Point(px - 55, py);
            }

            return new Point(0.0, 0.0);
        }

        public void DrawTopDownView(Player redPlayer, Player bluePlayer)
        {
            using (DrawingContext dc = this.drawingGroup.Open())
            {
                // Draw a transparent background to set the render size
                dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, this.Width, this.Height));

                if (redPlayer.state == Player.State.SYNCED)
                {
                    Body body = redPlayer.body;
                    Point p = MapDepthPointToFloor(body.Joints[JointType.Head].Position);

                    if (p.X != 0 && p.Y != 0)
                    {
                        Canvas.SetLeft(redFloorGif, p.X);
                        Canvas.SetTop(redFloorGif, p.Y);
                    }
                }

                if (bluePlayer.state == Player.State.SYNCED)
                {
                    Body body = bluePlayer.body;
                    Point p = MapDepthPointToFloor(body.Joints[JointType.Head].Position);

                    if (p.X != 0 && p.Y != 0)
                    {
                        Canvas.SetLeft(blueFloorGif, p.X);
                        Canvas.SetTop(blueFloorGif, p.Y);
                    }
                }
            }
        }
    }
}
