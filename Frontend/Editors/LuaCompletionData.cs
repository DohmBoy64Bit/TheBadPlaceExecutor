using Avalonia.Media;
using Avalonia.Media.Imaging;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using System;

namespace Frontend.Editors;

public class LuaCompletionData : ICompletionData
{
    public string Text { get; set; }
    public object Content { get; set; }
    public object Description { get; set; }
    public double Priority { get; set; }
    public IImage? Image { get; set; }

    public LuaCompletionData(string text, string caption, string description, double priority = 0)
    {
        Text = text;
        Content = caption;
        Description = description;
        Priority = priority;
    }

    public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
    {
        textArea.Document.Replace(completionSegment, Text);
    }
}
