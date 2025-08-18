using Godot;
using System;
using System.Collections.Immutable;

/// <summary>
/// An editor plugin providing a tool to convert bitmaps to scenes, by mapping each non-transparent pixel to a prefab (by color).
/// Depends on CommonGodotUI.
/// </summary>
[Tool]
public partial class BMPToSceneConverterPlugin : EditorPlugin {
    private static readonly ImmutableArray<(string, string)> DEPENDENCIES = [
        ("CommonGodotUI", "Repo can be found at https://github.com/njfletcher215/CommonGodotUI. You may need to re-install the plugin after installing this dependency."),
    ];

    public const string PREVIEW_WINDOW_SCENE_PATH = "res://addons/BMPToSceneConverter/BMPToSceneConverterPreviewWindow.tscn";
    public const string TOOL_WINDOW_SCENE_PATH = "res://addons/BMPToSceneConverter/BMPToSceneConverterWindow.tscn";
    public const string QUESTION_MARK_IMAGE_PATH = "res://addons/BMPToSceneConverter/question.png";

    public const string TOOL_TITLE = "BMP to Scene Converter";

    private Window toolWindow;

    public override void _EnterTree() {
        // verify dependencies
        bool dependencyNotPresent = false;
        foreach ((string dependency, string furtherInstructions) in BMPToSceneConverterPlugin.DEPENDENCIES) {
            if (!BMPToSceneConverterPlugin.IsDependencyPresent(dependency)) {
                dependencyNotPresent = true;
                GD.PushError($"Dependency not found! ({dependency}) should be placed in res://addons/. {furtherInstructions}");
            }
        }
        if (dependencyNotPresent) return;

        // pre-load the window hidden
        this.toolWindow = GD.Load<PackedScene>(BMPToSceneConverterPlugin.TOOL_WINDOW_SCENE_PATH).Instantiate<Window>();
        this.toolWindow.Hide();
        EditorInterface.Singleton.GetBaseControl().AddChild(this.toolWindow);

        // add menu item to Project > Tools
        this.AddToolMenuItem(BMPToSceneConverterPlugin.TOOL_TITLE, Callable.From(OnToolMenuPressed));
    }

    public override void _ExitTree() {
        this.RemoveToolMenuItem(BMPToSceneConverterPlugin.TOOL_TITLE);

        if (this.toolWindow != null && this.toolWindow.IsInsideTree()) {
            this.toolWindow.QueueFree();
        }
    }

    /// <summary>
    /// Show the tool window.
    /// </summary>
    private void OnToolMenuPressed() {
        if (this.toolWindow != null) {
            this.toolWindow.PopupCentered();
        }
    }

    /// <summary>
    /// Check for the given dependency.
    /// </summary>
    /// <param name="dependency">The name of the expected dependency (specifically, the name of the dependency root folder).</param>
    private static bool IsDependencyPresent(string dependency) {
        DirAccess? dependencyDirectory = DirAccess.Open($"res://addons/{dependency}");
        bool dependencyPresent = dependencyDirectory != null;
        dependencyDirectory?.Dispose();
        return dependencyPresent;
    }
}

