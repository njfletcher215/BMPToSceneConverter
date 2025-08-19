using Godot;
using System;
using System.IO;

// TODO this should open immediately, THEN start generating the scene.

// TODO annotate
[Tool]
public partial class BMPToSceneConverterPreviewWindow : Window {
    [Export] private TextureRect inputImagePreview;
    [Export] private CanvasItemPreview outputScenePreview;
    [Export] private FileDialog saveDialog;

    private PackedScene scene;

    // TODO this should only be initialized once
    public void Initialize(string inputFilePath, string mappingFilePath) {
        this.LoadInputPreviewImage(inputFilePath);
        this.GenerateScene(inputFilePath, mappingFilePath);
    }

    public override void _Ready() {
        this.saveDialog.FileSelected += (path) => {
            ResourceSaver.Save(this.scene, path);
            GD.Print($"Saved scene to: {path}");  // TODO this should be something other than a log; a popup window or something...
            this.OnCancel();
        };
     }

    /// <summary>
    /// Load the input image file as a texture and display it on the input image preview.
    /// </summary>
    /// <param name="inputFilePath">The path to the input file.</param>
    private void LoadInputPreviewImage(string inputFilePath) {
        Image inputFilePreviewImage = Image.LoadFromFile(inputFilePath);
        if (inputFilePreviewImage == null) GD.PushError($"Failed to load image: {inputFilePath}");  // TODO this should open an error popup or display it in the preview area instead

        ImageTexture inputFilePreviewTexture = new ImageTexture();
        inputFilePreviewTexture.SetImage(inputFilePreviewImage);

        this.inputImagePreview.Texture = inputFilePreviewTexture;
    }


    private void OnCancel() {
        this.EmitSignal(SignalName.CloseRequested);
    }

    // TODO any error should open a popup with an error message. Honestly, a success should too.
    /// <summary>
    /// Generate the scene from the input image and mapping files.
    /// </summary>
    /// <param name="inputFilePath">The path to the input image file.</param>
    /// <param name="mappingFilePath">The path to the mapping file.</param>
    private void GenerateScene(string inputFilePath, string mappingFilePath) {
        if (string.IsNullOrEmpty(inputFilePath) ||
            string.IsNullOrEmpty(mappingFilePath))
            return;  // TODO this should open a popup with an error message

        var mapping = ResourceLoader.Load<BMPToSceneConverterMapping>(mappingFilePath);
        if (mapping == null) {
            GD.PrintErr("Failed to load mapping: ", mappingFilePath);
            return;
        }

        Node2D root = new Node2D();
        root.Name = "Level";
        byte[,] indexMap = ReadBmpIndexedPixels(inputFilePath);
        for (int x = 0; x < indexMap.GetLength(0); x++) {
            for (int y = 0; y < indexMap.GetLength(1); y++) {
                int index = indexMap[x, y];
                if (index < 0 || index >= mapping.prefabs.Length)
                    continue;
                PackedScene prefab = mapping.prefabs[index];
                if (prefab == null)
                    continue;

                Node2D node = prefab.Instantiate<Node2D>();
                node.Position = mapping.offset + new Vector2(x * mapping.pixelSize.X, y * mapping.pixelSize.Y);
                root.AddChild(node);
                node.Owner = root;
            }
        }

        this.scene = new PackedScene();
        GD.Print(this.scene.Pack(root));
        GD.Print(this.scene);
        GD.Print(root.GetChildCount());

        this.outputScenePreview.Preview(root);
    }

    /// <summary>
    /// Read raw pixel indices from a palettized BMP.
    /// </summary>
    /// <returns>A 2D array of pixel indices.</returns>
    public static byte[,] ReadBmpIndexedPixels(string path) {
        using FileStream fs = File.OpenRead(path);
        using BinaryReader reader = new BinaryReader(fs);

        // BMP Header
        reader.ReadBytes(10); // Skip signature and file size, etc.
        int pixelDataOffset = reader.ReadInt32(); // Usually 54 for uncompressed BMPs

        int dibHeaderSize = reader.ReadInt32();
        int width = reader.ReadInt32();
        int height = reader.ReadInt32();
        reader.ReadInt16(); // Color planes
        int bitsPerPixel = reader.ReadInt16();
        int compression = reader.ReadInt32();

        if (compression != 0)
            throw new NotSupportedException("Compressed BMP not supported.");
        if (bitsPerPixel != 4 && bitsPerPixel != 8)
            throw new NotSupportedException("Only 4-bit and 8-bit BMPs are supported.");

        reader.BaseStream.Seek(pixelDataOffset, SeekOrigin.Begin);

        byte[,] pixels = new byte[width, height];

        int rowSizeBytes = ((width * bitsPerPixel + 31) / 32) * 4;
        for (int y = height - 1; y >= 0; y--) { // BMP rows are bottom-up
            byte[] row = reader.ReadBytes(rowSizeBytes);
            for (int x = 0; x < width; x++) {
                if (bitsPerPixel == 8) {
                    pixels[x, y] = row[x];
                } else if (bitsPerPixel == 4) {
                    int byteIndex = x / 2;
                    bool highNibble = x % 2 == 0;
                    pixels[x, y] = highNibble ? (byte)(row[byteIndex] >> 4) : (byte)(row[byteIndex] & 0x0F);
                }
            }
        }

        return pixels;
    }
}

