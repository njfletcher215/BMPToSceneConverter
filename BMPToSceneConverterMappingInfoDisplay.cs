using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// TODO I need methods to unset / clear

/// <summary>
/// A display for a BMPToSceneConverter mapping. Shows the offset, pixel size, and color mappings.
/// </summary>
[Tool]
public partial class BMPToSceneConverterMappingInfoDisplay : Container {
    [Export] private SimpleFormatStringLabel offsetLabel;
    [Export] private SimpleFormatStringLabel pixelSizeLabel;

    /// <summary>
    /// The list of color mapping displays. Parsed from direct children of this object.
    /// </summary>
    private BMPToSceneConverterColorMappingDisplay[] colorMappingDisplays;

    public static Color[] GetPalette(string bmpFilePath) {
        using FileStream fs = File.OpenRead(bmpFilePath);
        using BinaryReader reader = new BinaryReader(fs);

        // Skip BMP file header (14 bytes)
        reader.BaseStream.Seek(14, SeekOrigin.Begin);

        int dibHeaderSize = reader.ReadInt32(); // Typically 40 (BITMAPINFOHEADER)
        reader.BaseStream.Seek(10, SeekOrigin.Current); // Skip rest of DIB header up to biBitCount

        int bitsPerPixel = reader.ReadInt16();
        int compression = reader.ReadInt32();
        int imageSize = reader.ReadInt32();
        int xPpm = reader.ReadInt32();
        int yPpm = reader.ReadInt32();
        int colorsUsed = reader.ReadInt32();
        reader.ReadInt32(); // importantColors

        if (bitsPerPixel != 4)
            throw new NotSupportedException("Only 4-bit BMPs are supported.");
        if (compression != 0)
            throw new NotSupportedException("Compressed BMPs are not supported.");

        if (colorsUsed == 0)
            colorsUsed = 16; // Default for 4-bit BMP

        Color[] palette = new Color[colorsUsed];

        // Palette starts immediately after the DIB header
        long paletteOffset = 14 + dibHeaderSize;
        reader.BaseStream.Seek(paletteOffset, SeekOrigin.Begin);

        for (int i = 0; i < colorsUsed; i++) {
            byte blue = reader.ReadByte();
            byte green = reader.ReadByte();
            byte red = reader.ReadByte();
            byte reserved = reader.ReadByte(); // Usually unused
            palette[i] = new Color(red / 255.0f, green / 255.0f, blue / 255.0f, 1.0f);
        }

        return palette;
    }

    public override void _Ready() {
        this.colorMappingDisplays = this.GetChildren().OfType<BMPToSceneConverterColorMappingDisplay>().ToArray();
        for (int i = 0; i < this.colorMappingDisplays.Length; i++) this.colorMappingDisplays[i].Initialize(i);
    }

    /// <summary>
    /// Clear the display.
    /// </summary>
    public void Clear() {
        this.offsetLabel.SetValue("x", null);
        this.offsetLabel.SetValue("y", null);
        this.pixelSizeLabel.SetValue("x", null);
        this.pixelSizeLabel.SetValue("y", null);
        foreach (BMPToSceneConverterColorMappingDisplay colorMappingDisplay in this.colorMappingDisplays) colorMappingDisplay.Clear();
    }

    /// <summary>
    /// Update the display with the given offset, pixel size, color, and/or prefab information.
    /// </summary>
    /// <param name="offset">The offset of the mapping.</param>
    /// <param name="pixelSize">The pixel size of the mapping.</param>
    /// <param name="colors">The colors in the mapping (in order).</param>
    /// <param name="prefabs">The prefabs in the mapping (in order).</param>
    /// <exception cref="ArgumentException">Thrown when more <paramref name="colors" /> or <paramref name="prefabs" /> are passed than color mapping displays available.</exception>
    public void Update(Vector2? offset = null, Vector2? pixelSize = null, Color[] colors = null, PackedScene[] prefabs = null) {
        if (colors != null && colors.Length > this.colorMappingDisplays.Length)
            throw new ArgumentException("Too many colors provided -- not enough displays attached.", nameof(colors));
        if (prefabs != null && prefabs.Length > this.colorMappingDisplays.Length)
            throw new ArgumentException("Too many prefabs provided -- not enough displays attached.", nameof(prefabs));
        if (offset != null) {
            this.offsetLabel.SetValue("x", offset.Value.X);
            this.offsetLabel.SetValue("y", offset.Value.Y);
        }
        if (pixelSize != null) {
            this.pixelSizeLabel.SetValue("x", pixelSize.Value.X);
            this.pixelSizeLabel.SetValue("y", pixelSize.Value.Y);
        }
        if (colors != null)
            for (int i = 0; i < colors.Length; i++)
                this.colorMappingDisplays[i].Update(color: colors[i]);
        if (prefabs != null)
            for (int i = 0; i < prefabs.Length; i++)
                this.colorMappingDisplays[i].Update(prefab: prefabs[i]);
    }

    /// <summary>
    /// Update the display with the offset, pixel size, and/or prefab information found in the given mapping.
    /// </summary>
    /// <param name="mapping">The mapping with the offset, pixel size, and/or prefab information.</param>
    public void Update(BMPToSceneConverterMapping mapping) {
        this.Update(offset: mapping.offset, pixelSize: mapping.pixelSize, prefabs: mapping.prefabs);
    }

    /// <summary>
    /// Update the display with the offset, pixel size, and/or prefab information found in the mapping at the given file path,
    /// and/or the colors found in the BMP file at the given file path.
    /// </summary>
    /// <param name="mappingFilePath">The path to the file with the offset, pixel size, and/or prefab information.</param>
    /// <param name="bmpFilePath">The path to the BMP file with the colors.</param>
    public void Update(string mappingFilePath = null, string bmpFilePath = null) {
        if (mappingFilePath != null) this.Update(ResourceLoader.Load<BMPToSceneConverterMapping>(mappingFilePath));
        if (bmpFilePath != null) this.Update(colors: BMPToSceneConverterMappingInfoDisplay.GetPalette(bmpFilePath));
    }
}
