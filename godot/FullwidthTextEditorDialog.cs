using System;
using Ome.Engine.Frontend.Shell;
using Ome.Engine.Profiles;
using Ome.Engine.Sys4;
using Godot;

/// <summary>Godot presentation for AGERc command 10's blocking editor, in the shell's language
/// (<see cref="ShellBoxes"/>) with the game's own words, and its accept by the game's rule
/// (<see cref="GameTextEntry"/>).</summary>
public partial class FullwidthTextEditorDialog : Window
{
    private readonly LineEdit _edit = new() { MaxLength = 255 };
    private readonly Label _error = new()
    {
        AutowrapMode = TextServer.AutowrapMode.WordSmart,
        CustomMinimumSize = new Vector2(0, 28),
    };
    private readonly ShellStrings _strings;
    private readonly GameTextEntry _entry;
    private bool _completed;

    public event Action<bool, string>? EditCompleted;

    public FullwidthTextEditorDialog(ShellStrings strings, GameTextEntry entry)
    {
        _entry = entry;
        strings = strings.WithGameText(entry.Text);
        _strings = strings;
        Title = strings[ShellBoxes.NameEntryTitle];
        Exclusive = true;
        Transient = true;
        Unresizable = true;

        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", 16);
        margin.AddThemeConstantOverride("margin_top", 16);
        margin.AddThemeConstantOverride("margin_right", 16);
        margin.AddThemeConstantOverride("margin_bottom", 16);
        AddChild(margin);
        margin.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);

        var column = new VBoxContainer();
        margin.AddChild(column);
        column.AddChild(new Label { Text = strings[ShellBoxes.NameEntryPrompt] });
        _edit.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        column.AddChild(_edit);
        _error.AddThemeColorOverride("font_color", new Color(1.0f, 0.35f, 0.35f));
        column.AddChild(_error);

        var actions = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.End };
        var cancel = new Button { Text = strings[ShellBoxes.NameEntryCancel] };
        var accept = new Button { Text = strings[ShellBoxes.NameEntryAccept] };
        actions.AddChild(cancel);
        actions.AddChild(accept);
        column.AddChild(actions);

        accept.Pressed += TryAccept;
        cancel.Pressed += Cancel;
        _edit.TextSubmitted += _ => TryAccept();
        CloseRequested += Cancel;
    }

    public void Open(string initialText)
    {
        _completed = false;
        _error.Text = "";
        _edit.Text = initialText;
        PopupCentered(new Vector2I(430, 160));
        _edit.GrabFocus();
        _edit.SelectAll();
    }

    private void TryAccept()
    {
        TextEntryCheck check = _entry.Check(_edit.Text);
        if (check.Error != FullwidthTextValidationError.None)
        {
            _error.Text = check.Error == FullwidthTextValidationError.TooLong
                ? _strings[ShellBoxes.NameEntryTooLong]
                : _strings[ShellBoxes.NameEntryHalfWidth];
            _edit.GrabFocus();
            return;
        }
        Complete(true, check.Text);
    }

    private void Cancel() => Complete(false, _edit.Text);

    private void Complete(bool accepted, string text)
    {
        if (_completed) return;
        _completed = true;
        Hide();
        EditCompleted?.Invoke(accepted, text);
    }
}
