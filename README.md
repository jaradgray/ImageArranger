# Image Arranger
Image Arranger is a Windows desktop app that scales and arranges images to fit optimally within the app's window. It is written in C# using WPF/.NET.

![Screenshot](/screenshot.jpg)

## Features
- Recognizes most common image files (.bmp, .gif, .jpeg/.jpg, .png).
- Includes drag-and-drop functionality.
- Rearranges images when the window is resized.

## Using Image Arranger
- Download and run [src/ImageArranger/bin/Release/ImageArranger.exe](/src/ImageArranger/bin/Release/ImageArranger.exe) (requires a Windows PC with the .NET framework installed).
- Left-click and select images or drag and drop images into the app's window.
- Right-click for options to add, remove one, or remove all images.

## What I Learned
- Familiarity with C# and WPF.
- Designing and implementing a non-trivial data structure in [src/ImageArranger/DynamicGrid.cs](/src/ImageArranger/DynamicGrid.cs).
- Algorithm design and implementation.
  - The packing algorithm used is a modification of the one explained [here](https://www.codeproject.com/Articles/210979/Fast-optimizing-rectangle-packing-algorithm-for-bu).
