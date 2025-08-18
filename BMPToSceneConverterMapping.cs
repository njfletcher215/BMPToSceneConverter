using Godot;
using System;

/// <summary>
/// Mapping information for the BMP to Scene Converter.
/// </summary>
[Tool]
public partial class BMPToSceneConverterMapping : Resource {
    /// <summary>
    /// Where in the output scene the converter should start placing prefabs.
    /// </summary>
    [Export]
    public Vector2 offset;
    /// <summary>
    /// The distance that a single pixel should map to.
    /// </summary>
    [Export]
    public Vector2 pixelSize;

    /// <summary>
    /// The prefabs to use in the scene.
    /// The index of each BMP color maps directly to this array.
    /// If the index is empty, no prefab is placed.
    /// </summary>
    [Export]
    public PackedScene[] prefabs = new PackedScene[16];
}
