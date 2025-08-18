# BMPToSceneConverter

A Godot plugin providing a tool to create .tscns from BMP files by creating a grid of prefab instances, with each prefab mapping to a specific color.

This plugin has been designed specifically for 4-bit (16-color) palette-indexed BMPs. There is currently no support for any other bit count.

## Installation

First, install dependencies. Simply clone the [CommonGodotUI](https://github.com/njfletcher215/CommonGodotUI) repo into `res://addons/` (these are simply library files, so you can place them anywhere in your project folder -- but `addons/CommonGodotUI` is the recommended location).
```bash
# from your project's root directory
git clone git@github.com:njfletcher215/CommonGodotUI.git addons/CommonGodotUI/
```
Then, clone this repo into `res://addons/` (this repo *must* be placed in `addons` and named `BMPToSceneConverter`, or the Godot editor will not be able to find it).
```bash
# from your project's root directory
git clone git@github.com:njfletcher215/BMPToSceneConverter.git addons/BMPToSceneConverter/
```

## Basic Usage

Create an instance of `BMPToSceneConverterMapping` (in the Godot editor, right click on the FileSystem, then 'Create New...' > 'Resource' or 'New Resource...'). Configure the mapping according to your preferences:

> **offset**: The offset of the grid within the output scene.
> **pixelSize**: The size of each grid square, aka the distance that a single pixel in the .bmp maps to. Note that the size of each grid
> square is completely independent from the size of the prefabs.
> **prefabs**: The prefabs to use in the scene. Each color in the .bmp file will be mapped directly to the prefab at the same index in this
> array. If the index in this array is empty, no prefab will be placed.

Create a 4-bit palette-indexed BMP using any image editing software. I recommend a pixel-art software such as [Aseprite](https://www.aseprite.org/).

The converter can be found under 'Project' > 'Tools'.

## Questions or Issues?

Feel free to open an issue on GitHub if something's not working or not clear.
Alternatively, you can contact the developer at njfletcher215@gmail.com

