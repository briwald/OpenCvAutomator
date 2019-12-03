# OpenCvAutomator
A C# library to perform Windows automation using OpenCV computer vision.

It uses OpenCV (Emgu.CV) for the computer vision functionality and WindowsInput to control the mouse and keyboard. It's very 
similar to Sikuli, but doesn't use Java/Jython and it doesn't have an IDE to generate its scripts. You simply pass image 
file paths to the methods that accept them. I've only tested using paths relative to the library itself, but it is likely that 
absolute paths will also work. 

It is intended that this library would be used to automate sections of tests that cannot be 
automated with tools like Microsoft Coded UI, TestStack.White or Selenium. It should be noted that testing using image recognition 
should be used as a last resort as changes to the layout or appearance of the UI will require updates to the tests, unlike the 
aforementioned automation tools that can get handles to the UI objects by various means. If OpenCvAutomator is being used on a 
build server, make sure to match the desktop backgrounds and screen resolutions so that the image matching works correctly.
## Usage
Add a reference to OpenCvAutomator to your project. Instantiate an Automator and call the various available methods.

    Automator a = new Automator(0.75)   //0.75 is the image matching accuracy that is passed to OpenCV (0-1)
    a.ShowDesktop();                    //Presses Windows key + d
    a.Click("path\\to\\image.png");     //Left mouse click on image.png
    a.Type("text");                     //Type some text at the current cursor location
    a.MoveMouse(30, -10);               //Moves the mouse 30 pixels on the X axis, -10 pixels on the Y axis
    a.Click();                          //Left click the mouse at the current location
    a.Wait("path\\to\\image2.png", 10); //Wait a maximum of 10 seconds for image2.png to appear on screem
