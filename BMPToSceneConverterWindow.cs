using Godot;
using System;
using System.IO;

/// <summary>
/// The control panel for the Plugin.
/// </summary>
// TODO annotate and otherwise clean up
// TODO test with new color mapping displays
// TODO inputFilePathsLabel should probably be scrollable
//      and the mapping info grid could be too
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

    /// <summary>
    /// Initialize this with empty data.
    /// </summary>
    private void Initialize() {
        this.InputFilePaths = new string[0];
        this.previewsClosed = 0;
        this.MappingFilePath = null;
    }

    public override void _Ready() {
        this.inputDialog.FilesSelected += (paths) => {
            this.InputFilePaths = paths;
            if (!string.IsNullOrEmpty(this.MappingFilePath))
                foreach (string inputFilePath in this.InputFilePaths)
                    this.OpenPreviewWindow(inputFilePath, this.MappingFilePath);
        };
        // TODO this needs to display the preview information
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
        PreviewWindow previewWindow =
            GD.Load<PackedScene>(Plugin.PREVIEW_WINDOW_SCENE_PATH)
            .Instantiate<PreviewWindow>();
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
        this.Initialize();  // TODO I don't think this is working... its also not the correct name -- I only use 'initialize' for things that happen once.
        this.Hide();
    }
}
