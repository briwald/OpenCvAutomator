using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using WindowsInput;
using WindowsInput.Native;

namespace OpenCvAutomator
{
    public class Automator
    {
        private InputSimulator inputSimulator;
        private IMouseSimulator mouse;
        private IKeyboardSimulator keyboard;
        private Image<Bgr, Byte> currentScreenshotImage;
        private Bitmap currentScreenshotBitmap;
        private Image<Bgr, byte> imageToSearchFor;
        private Image<Gray, float> searchResultImage;
        private double[] minValues, maxValues;
        private Point[] minLocations, maxLocations;
        private TemplateMatchingType matchingType;
        private Rectangle imageMatchRectangle;
        private Point centerOfImageMatchRectangle;
        private int screenMaxWidth;
        private int screenMaxHeight;
        private double matchingAccuracy;

        private bool ImageFound
        {
            get
            {
                return maxValues[0] > matchingAccuracy;
            }
        }

        /// <summary>
        /// A class that uses OpenCV and WindowsInput to automate Windows using computer vision
        /// </summary>
        /// <param name="matchingAccuracy">specifies the matching accuracy used on the result of OpenCV MatchTemplate (0-1). 0.75 seems adequate</param>
        public Automator(double matchingAccuracy)
        {
            screenMaxWidth = Screen.PrimaryScreen.Bounds.Width;
            screenMaxHeight = Screen.PrimaryScreen.Bounds.Height;
            inputSimulator = new InputSimulator();
            mouse = inputSimulator.Mouse;
            keyboard = inputSimulator.Keyboard;
            this.matchingAccuracy = matchingAccuracy;
            matchingType = TemplateMatchingType.CcoeffNormed;
        }

        /// <summary>
        /// Checks if a given image currently exists on screen
        /// </summary>
        /// <param name="filename">image file path</param>
        /// <returns>If the image exists on screen as bool</returns>
        public bool Exists(string filename)
        {
            FindImage(filename);

            if (ImageFound)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Waits for an image to appear on screen for a specified number of seconds
        /// </summary>
        /// <param name="filename">image file path</param>
        /// <param name="seconds">number of seconds</param>
        public void Wait(string filename, int seconds)
        {
            for (int x = 1; x <= seconds; x++)
            {
                try
                {
                    FindImage(filename);

                    if (ImageFound)
                    {
                        break;
                    }
                }
                catch(Exception)
                {
                    if (x == seconds)
                    {
                        currentScreenshotImage.Save("failure_" + GetTimestamp(DateTime.Now) + ".png");
                        throw new Exception(filename + " not found after " + seconds.ToString() + " seconds");
                    }

                    UpdateCurrentScreenshot();
                    Thread.Sleep(1000);
                }
            }
        }

        /// <summary>
        /// Moves the mouse over the center of an image if it is found on screen
        /// </summary>
        /// <param name="filename">image file path</param>
        public void Hover(string filename)
        {
            try
            {
                FindImage(filename);
                mouse.MoveMouseTo((double)centerOfImageMatchRectangle.X, (double)centerOfImageMatchRectangle.Y);
            }
            catch(Exception e)
            {
                currentScreenshotImage.Save("failure_" + GetTimestamp(DateTime.Now) + ".png");
                throw e;
            }
        }

        /// <summary>
        /// Moves the mouse in small increments relative to its current position (0,0 is the top left of the screen)
        /// </summary>
        /// <param name="pixelDeltaX"></param>
        /// <param name="pixelDeltaY"></param>
        public void MoveMouse(int pixelDeltaX, int pixelDeltaY)
        {
            mouse.MoveMouseBy(pixelDeltaX, pixelDeltaY);
        }

        /// <summary>
        /// Right clicks the mouse at the current location
        /// </summary>
        public void RightClick()
        {
            mouse.RightButtonClick();
        }

        /// <summary>
        /// Right clicks the mouse on an image if it exists on screen
        /// </summary>
        /// <param name="filename">image file path</param>
        public void RightClick(string filename)
        {
            try
            {
                FindImage(filename);
                mouse.MoveMouseTo((double)centerOfImageMatchRectangle.X, (double)centerOfImageMatchRectangle.Y);
                mouse.RightButtonClick();
            }
            catch(Exception e)
            {
                currentScreenshotImage.Save("failure_" + GetTimestamp(DateTime.Now) + ".png");
                throw e;
            }
        }

        /// <summary>
        /// Left clicks the mouse at the current location 
        /// </summary>
        public void Click()
        {
            mouse.LeftButtonClick();
        }

        /// <summary>
        /// Left clicks the mouse on an image if it exists on screen
        /// </summary>
        /// <param name="filename">image file path</param>
        public void Click(string filename)
        {
            try
            {
                FindImage(filename);
                mouse.MoveMouseTo((double)centerOfImageMatchRectangle.X, (double)centerOfImageMatchRectangle.Y);
                mouse.LeftButtonClick();
            }
            catch(Exception e)
            {
                currentScreenshotImage.Save("failure_" + GetTimestamp(DateTime.Now) + ".png");
                throw e;
            }
        }

        /// <summary>
        /// Double clicks the mouse at the current location
        /// </summary>
        public void DoubleClick()
        {
            mouse.LeftButtonDoubleClick();
        }

        /// <summary>
        /// Double clicks the mouse on an image if it exists on screen
        /// </summary>
        /// <param name="filename">image file path</param>
        public void DoubleClick(string filename)
        {
            try
            {
                FindImage(filename);
                mouse.MoveMouseTo((double)centerOfImageMatchRectangle.X, (double)centerOfImageMatchRectangle.Y);
                mouse.LeftButtonDoubleClick();
            }
            catch(Exception e)
            {
                currentScreenshotImage.Save("failure_" + GetTimestamp(DateTime.Now) + ".png");
                throw e;
            }
        }

        /// <summary>
        /// Types text at the current cursor location
        /// </summary>
        /// <param name="text">the text to type</param>
        public void Type(string text)
        {
            keyboard.Sleep(500);
            keyboard.TextEntry(text);
        }

        public void PressTab()
        {
            keyboard.Sleep(500);
            keyboard.KeyPress(VirtualKeyCode.TAB);
        }

        public void PressEnter()
        {
            keyboard.Sleep(500);
            keyboard.KeyPress(VirtualKeyCode.RETURN);
        }

        public void PressEscape()
        {
            keyboard.Sleep(500);
            keyboard.KeyPress(VirtualKeyCode.ESCAPE);
        }

        /// <summary>
        /// CTRL-A
        /// </summary>
        public void SelectAll()
        {
            keyboard.Sleep(500);
            keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_A);
        }

        /// <summary>
        /// CTRL-X
        /// </summary>
        public void Cut()
        {
            keyboard.Sleep(500);
            keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_X);
        }

        /// <summary>
        /// CTRL-C
        /// </summary>
        public void Copy()
        {
            keyboard.Sleep(500);
            keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_C);
        }

        /// <summary>
        /// CTRL-V
        /// </summary>
        /// <param name="text"></param>
        public void Paste(string text)
        {
            Thread t = new Thread((ThreadStart)(() => {
                Clipboard.SetText(text);
            }));

            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();

            keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_V);
        }

        /// <summary>
        /// WINDOWS_KEY-D
        /// </summary>
        public void ShowDesktop()
        {
            keyboard.Sleep(500);
            keyboard.ModifiedKeyStroke(VirtualKeyCode.LWIN, VirtualKeyCode.VK_D);
        }

        private void FindImage(string filename)
        {
            imageToSearchFor = new Image<Bgr, byte>(filename);
            UpdateCurrentScreenshot();
            searchResultImage = currentScreenshotImage.MatchTemplate(imageToSearchFor, matchingType);
            searchResultImage.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

            if (ImageFound)
            {
                imageMatchRectangle = new Rectangle(maxLocations[0], imageToSearchFor.Size);
                centerOfImageMatchRectangle = GetCenter(imageMatchRectangle);
            }
            else
            {
                throw new Exception(filename + " not found on screen");
            }
        }

        private String GetTimestamp(DateTime value)
        {
            return value.ToString("yyyy_dd__MM_HH_mm_ss");
        }

        private void UpdateCurrentScreenshot()
        {
            currentScreenshotBitmap = new Bitmap(screenMaxWidth, screenMaxHeight);
            Graphics g = Graphics.FromImage(currentScreenshotBitmap);
            g.CopyFromScreen(0, 0, 0, 0, Screen.PrimaryScreen.Bounds.Size);
            currentScreenshotImage = new Image<Bgr, byte>(currentScreenshotBitmap);
        }

        private Point GetCenter(Rectangle rect)
        {
            Point p = new Point(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2);
            int convertedPointX = Convert.ToInt32(p.X * (65535 / screenMaxWidth));
            int convertedPointY = Convert.ToInt32(p.Y * (65535 / screenMaxHeight));
            return new Point(convertedPointX, convertedPointY);
        }
    }
}