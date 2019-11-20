# OpenCvAutomator
A C# library to perform Windows automation using OpenCV computer vision.

It uses OpenCV (Emgu.CV) for the computer vision functionality and WindowsInput to control the mouse and keyboard. It's very 
similar to Sikuli, but doesn't use Java/Jython and it doesn't have an IDE to generate its scripts. You simply pass image 
file paths to the methods that accept them (I've only tested using paths relative to the library itself but it is likely that 
absolute paths will also work). It is intended that this library would be used to automate sections of tests that cannot be 
automated with tools like Microsoft Coded UI, TestStack.White or Selenium. It should be noted that testing using image recognition 
should be used as a last resort as changes to the layout or appearance of the UI will require updates to the tests, unlike the 
aforementioned automation tools that can get handles to the UI objects by various means.
