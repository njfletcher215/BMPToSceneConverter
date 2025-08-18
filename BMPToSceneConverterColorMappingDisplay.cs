using System;
using Godot;

// TODO I also need methods to unset / clear

/// <summary>
/// A display for a BMPToSceneConverter color mapping. Shows the color, prefab, and prefab file path.
/// </summary>
[Tool]
public partial class BMPToSceneConverterColorMappingDisplay : Container {
    [Export] private TextureRect colorDisplay;
    [Export] private SimpleFormatStringLabel indexLabel;
    [Export] private CanvasItemPreview prefabPreview;

    private bool initialized = false;

    /// <summary>
    /// Initialize the display with the given index.
    /// </summary>
    /// <param name="index">The index of this display.</param>
    public void Initialize(int index) {
        if (this.initialized) return;
        this.indexLabel.SetValue("index", index);
        this.indexLabel.Hide();
        this.initialized = true;
    }

    /// <summary>
    /// Clear the color and prefab from the display.
    /// </summary>
    public void Clear() {
        this.indexLabel.Hide();
        this.colorDisplay.Texture = null;
        this.prefabPreview.Texture = null;
    }

    /// <summary>
    /// Update the display with the given color and/or prefab.
    /// </summary>
    /// <param name="color">The color of the color mapping.</param>
    /// <param name="prefab">The prefab of the color mapping.</param>
    public void Update(Color? color = null, PackedScene prefab = null) {
        GD.Print("Hello");
        GD.Print(color);
        GD.Print(prefab);
        if (color != null || prefab != null) this.indexLabel.Show();
        if (color != null) {
            if (this.colorDisplay.Texture == null) {
                Image colorImage = Image.CreateEmpty(1, 1, false, Image.Format.Rgba8);
                colorImage.Fill(color.Value);

                ImageTexture colorTexture = new ImageTexture();
                colorTexture.SetImage(colorImage);

                this.colorDisplay.Texture = colorTexture;
                this.colorDisplay.StretchMode = TextureRect.StretchModeEnum.Scale;
            }
            else if ((this.colorDisplay.Texture as ImageTexture)?.GetImage()?.GetPixel(0, 0) != color) {
                this.colorDisplay.Texture = GD.Load<Texture2D>(BMPToSceneConverterPlugin.QUESTION_MARK_IMAGE_PATH);
                this.colorDisplay.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
            }
        }
        if (prefab != null) this.prefabPreview.Preview(prefab);
    }
}
