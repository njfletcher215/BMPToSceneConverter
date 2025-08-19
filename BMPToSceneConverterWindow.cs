using Godot;
using System;
using System.IO;

/// <summary>
/// The control panel for the Plugin.
/// </summary>
// TODO annotate and otherwise clean up
[Tool]
public partial class BMPToSceneConverterWindow : Window {
    [Export] private FileDialog inputDialog;
    [Export] private FileDialog mappingDialog;
    [Export] private SimpleFormatStringLabel inputFilePathsLabel;
    [Export] private SimpleFormatStringLabel mappingFilePathLabel;
    [Export] private BMPToSceneConverterMappingInfoDisplay mappingInfoDisplay;

    private string[] _inputFilePaths;
    private string _mappingFilePath;

    private string[] InputFilePaths {
        get {
            return this._inputFilePaths;
        }
        set {
            this._inputFilePaths = value;
            this.inputFilePathsLabel.SetValue("inputFilePaths", this._inputFilePaths);
            foreach (string inputFilePath in this._inputFilePaths)
                this.mappingInfoDisplay.Update(bmpFilePath: inputFilePath);
        }
    }

    private string MappingFilePath {
        get {
            return this._mappingFilePath;
        }
        set {
            this._mappingFilePath = value;
            this.mappingFilePathLabel.SetValue("mappingFilePath", this._mappingFilePath);
            this.mappingInfoDisplay.Update(mappingFilePath: this._mappingFilePath);
        }
    }

    private int previewsClosed;

    public override void _Ready() {
        this.inputDialog.FilesSelected += (paths) => {
            this.InputFilePaths = paths;
            if (!string.IsNullOrEmpty(this.MappingFilePath))
                foreach (string inputFilePath in this.InputFilePaths)
                    this.OpenPreviewWindow(inputFilePath, this.MappingFilePath);
        };
        this.mappingDialog.FileSelected += (path) => {
            this.MappingFilePath = path;
            if (this.InputFilePaths != null && this.InputFilePaths.Length > 0)
                foreach (string inputFilePath in this.InputFilePaths)
                    this.OpenPreviewWindow(inputFilePath, this.MappingFilePath);
        };
    }

    /// <summary>
    /// Open a preview window for the given input and mapping files.
    /// </summary>
    /// <param name="inputFilePaths">The path to the input file for the preview window.</param>
    /// <param name="mappingFilePath">The path to the mapping file for the preview window.</param>
    private void OpenPreviewWindow(string inputFilePaths, string mappingFilePath) {
        BMPToSceneConverterPreviewWindow previewWindow =
            GD.Load<PackedScene>(BMPToSceneConverterPlugin.PREVIEW_WINDOW_SCENE_PATH)
            .Instantiate<BMPToSceneConverterPreviewWindow>();
        previewWindow.Initialize(inputFilePaths, mappingFilePath);
        previewWindow.CloseRequested += () => this.OnPreviewWindowClosed();
        EditorInterface.Singleton.GetBaseControl().AddChild(previewWindow);
    }

    /// <summary>
    /// Mark a preview window as closed. When all preview windows are closed, close this as well.
    /// </summary>
    private void OnPreviewWindowClosed() {
        this.previewsClosed++;
        if (this.previewsClosed >= this.InputFilePaths.Length) this.EmitSignal(SignalName.CloseRequested);
    }

    /// <summary>
    /// Reset this, then hide it.
    /// </summary>
    private void OnCloseRequested() {
        this.Reset();  // TESTME
        this.Hide();
    }

    /// <summary>
    /// Reset this with empty data.
    /// </summary>
    private void Reset() {
        this.InputFilePaths = new string[0];  // TESTME
        this.previewsClosed = 0;
        this.MappingFilePath = null;
        this.mappingInfoDisplay.Clear();
    }
}
