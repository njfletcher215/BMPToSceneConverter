using Godot;
using System;
using System.IO;
using System.Threading.Tasks;

/// <summary>
/// A window previewing the scene which will be generated from a specific input image.
/// </summary>
[Tool]
public partial class BMPToSceneConverterPreviewWindow : Window {
    /// <summary>
    /// A simple guard to ensure this window is only initialized once.
    /// </summary>
    [Export] private bool initialized = false;

    [Export] private TextureRect inputImagePreview;
    [Export] private CanvasItemPreview outputScenePreview;
    [Export] private FileDialog saveDialog;

    private PackedScene scene;

    /// <summary>
    /// Initialize the window by loading the input image and generating the output scene.
    /// The output scene is generated asynchronously.
    /// </summary>
    /// <param name="inputFilePath">The image being used to generate the output scene.</param>
    /// <param name="mappingFilePath">The mapping being used to generate the output scene.</param>
    public async Task Initialize(string inputFilePath, string mappingFilePath) {
        if (!this.initialized) {
            this.LoadInputPreviewImage(inputFilePath);
            await Task.Run(() => this.GenerateScene(inputFilePath, mappingFilePath));
        }
        this.initialized = true;
    }

    public override void _Ready() {
        this.saveDialog.FileSelected += (path) => {
            ResourceSaver.Save(this.scene, path);
            this.Close();
        };
     }

    /// <summary>
    /// Close the window.
    /// </summary>
    private void Close() {
        this.EmitSignal(SignalName.CloseRequested);
    }

    /// <summary>
    /// Generate the scene from the input image and mapping files.
    /// </summary>
    /// <param name="inputFilePath">The path to the input image file.</param>
    /// <param name="mappingFilePath">The path to the mapping file.</param>
    private void GenerateScene(string inputFilePath, string mappingFilePath) {
        var mapping = ResourceLoader.Load<BMPToSceneConverterMapping>(mappingFilePath);

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

        this.outputScenePreview.Preview(root);
    }

    /// <summary>
    /// Load the input image file as a texture and display it on the input image preview.
    /// </summary>
    /// <param name="inputFilePath">The path to the input file.</param>
    private void LoadInputPreviewImage(string inputFilePath) {
        Image inputFilePreviewImage = Image.LoadFromFile(inputFilePath);

        ImageTexture inputFilePreviewTexture = new ImageTexture();
        inputFilePreviewTexture.SetImage(inputFilePreviewImage);

        this.inputImagePreview.Texture = inputFilePreviewTexture;
    }

    /// <summary>
    /// Simply close the window.
    /// </summary>
    private void OnCancel() {
        this.Close();
    }

    /// <summary>
    /// Read raw pixel indices from a palettized BMP.
    /// </summary>
    /// <param name="path">The path to the BMP file.</param>
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

