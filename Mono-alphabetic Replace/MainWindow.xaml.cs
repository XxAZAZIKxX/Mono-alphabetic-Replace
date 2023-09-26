using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Math = System.Math;

namespace Mono_alphabetic_Replace;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private bool wasAdded;
    private const string UkranianAlphabet = "абвгґдеєжзиіїйклмнопрстуфхцчшщьюя";

    internal char[] OriginalAlphabet
    {
        get => GetAlphabetFromStackPanel(OriginalAlphabetStackPanel);
        set
        {
            var textBoxes = value.Select(p => GetLetterTextBox(char.ToUpper(p)));
            OriginalAlphabetStackPanel.Children.Clear();
            foreach (var textBox in textBoxes)
            {
                OriginalAlphabetStackPanel.Children.Add(textBox);
            }
        }
    }

    internal char[] EditedAlphabet
    {
        get => GetAlphabetFromStackPanel(EditedAlphabetStackPanel);
        set
        {
            var textBoxes = value.Select(p => GetLetterTextBox(char.ToUpper(p)));
            EditedAlphabetStackPanel.Children.Clear();
            foreach (var textBox in textBoxes)
            {
                EditedAlphabetStackPanel.Children.Add(textBox);
            }
        }
    }

    private char[] GetAlphabetFromStackPanel(Panel stackPanel)
    {
        var list = new List<char>();
        foreach (var child in stackPanel.Children)
        {
            var text = (child as TextBox)?.Text;
            if (text is null) continue;
            list.Add(char.ToUpper(text.Length > 0 ? text[0] : ' '));
        }

        return list.ToArray();
    }

    private static TextBox GetLetterTextBox(char letter)
    {
        var letterTextBox = new TextBox
        {
            MaxLength = 1,
            Background = new SolidColorBrush(Colors.Transparent),
            Text = $"{letter}",
            Foreground = new SolidColorBrush(Colors.AliceBlue),
            Margin = new Thickness(0, 0, 5, 0),
            Width = 18,
            Height = 22,
            VerticalContentAlignment = VerticalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            CaretBrush = new SolidColorBrush(Colors.AliceBlue)
        };
        letterTextBox.TextChanged += (_, _) =>
        {
            if (letterTextBox.Text.Length != letterTextBox.MaxLength) return;
            var nextTextBox = NextTextBox(letterTextBox);
            nextTextBox?.Focus();
            nextTextBox?.SelectAll();
        };
        letterTextBox.PreviewKeyDown += (_, args) =>
        {
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (args.Key)
            {
                case Key.Back when string.IsNullOrEmpty(letterTextBox.Text) is false:
                    return;
                case Key.Back:
                {
                    var previousTextBox = PreviousTextBox(letterTextBox);
                    previousTextBox?.Focus();
                    previousTextBox?.SelectAll();
                    break;
                }
                case Key.Right:
                {
                    var nextTextBox = NextTextBox(letterTextBox);
                    nextTextBox?.Focus();
                    nextTextBox?.SelectAll();
                    break;
                }
                case Key.Left:
                {
                    var previousTextBox = PreviousTextBox(letterTextBox);
                    previousTextBox?.Focus();
                    previousTextBox?.SelectAll();
                    break;
                }
            }
        };
        return letterTextBox;
    }

    private static TextBox? NextTextBox(TextBox current)
    {
        if (current.Parent is not StackPanel parent) return null;
        var currentIndex = parent.Children.IndexOf(current);
        if (currentIndex == -1 || currentIndex + 1 >= parent.Children.Count) return null;
        return parent.Children[currentIndex + 1] as TextBox;
    }

    private static TextBox? PreviousTextBox(TextBox current)
    {
        if (current.Parent is not StackPanel parent) return null;
        var currentIndex = parent.Children.IndexOf(current);
        if (currentIndex == -1 || currentIndex - 1 < 0) return null;
        return parent.Children[currentIndex - 1] as TextBox;
    }



    public MainWindow() => InitializeComponent();

    private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        ResetButtonClick(null!, null!);
        WindowLabel.Content = Title;
    }

    private void ResetButtonClick(object sender, RoutedEventArgs e)
    {
        var originalAlphabet = UkranianAlphabet.ToCharArray();
        OriginalAlphabet = originalAlphabet;
        EditedAlphabet = originalAlphabet;
        wasAdded = false;
    }

    private void ReplaceButtonClick(object sender, RoutedEventArgs e)
    {
        var text = OriginalRichTextBox.GetAllTextFromRichTextBox();
        var originalAlphabet = OriginalAlphabet;
        var editedAlphabet = EditedAlphabet;
        editedAlphabet = editedAlphabet.Take(originalAlphabet.Length).ToArray();
        var replacementMap = new Dictionary<char, char>();
        for (var i = 0; i < originalAlphabet.Length; i++)
        {
            replacementMap[char.ToLower(originalAlphabet[i])] = char.ToLower(editedAlphabet[i]);
        }

        var editedText = new StringBuilder(text.Length);
        foreach (var ch in text)
        {
            if (replacementMap.TryGetValue(char.ToLower(ch), out var replacement))
            {
                editedText.Append(char.IsUpper(ch) ? char.ToUpper(replacement) : replacement);
            }
            else
                editedText.Append(ch);
        }

        EditedRichTextBox.Document = new FlowDocument();
        EditedRichTextBox.AppendText(editedText.ToString());
    }

    private void OriginalRichTextBox_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        ChangeRichBoxFontSizeKeyboard(e);
    }

    private void ChangeRichBoxFontSizeKeyboard(KeyEventArgs e)
    {
        if ((Keyboard.Modifiers & ModifierKeys.Control) == 0) return;
        switch (e.Key)
        {
            case Key.OemPlus:
                OriginalRichTextBox.FontSize += 2;
                EditedRichTextBox.FontSize = OriginalRichTextBox.FontSize;
                e.Handled = true;
                break;
            case Key.OemMinus:
                if (OriginalRichTextBox.FontSize - 2 <= 0) break;
                OriginalRichTextBox.FontSize -= 2;
                EditedRichTextBox.FontSize = OriginalRichTextBox.FontSize;
                e.Handled = true;
                break;
        }
    }

    private void OriginalRichTextBox_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        ChangeRichBoxFontSizeMouseWheel(e);
    }

    private void ChangeRichBoxFontSizeMouseWheel(MouseWheelEventArgs e)
    {
        if ((Keyboard.Modifiers & ModifierKeys.Control) == 0) return;
        if (e.Delta > 0)
            OriginalRichTextBox.FontSize += 2;
        else
        {
            if (OriginalRichTextBox.FontSize - 2 <= 0) return;
            OriginalRichTextBox.FontSize -= 2;
        }

        EditedRichTextBox.FontSize = OriginalRichTextBox.FontSize;
        e.Handled = true;
    }

    private void EditedRichTextBox_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        ChangeRichBoxFontSizeMouseWheel(e);
    }

    private void EditedRichTextBox_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        ChangeRichBoxFontSizeKeyboard(e);
    }

    private void TopPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            MaximizeButton_OnMouseLeftButtonDown(this, e);
            e.Handled = true;
            return;
        }

        try
        {
            DragMove();
            e.Handled = true;
        }
        catch
        {
            /*ignored*/
        }
    }

    private void ExitButton_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) => Environment.Exit(0);

    private void MaximizeButton_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private void MinimizeButton_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) =>
        WindowState = WindowState.Minimized;

    private void LeftBottomThumb_OnDragDelta(object sender, DragDeltaEventArgs e)
    {
        if (Width - e.HorizontalChange > MinWidth)
        {
            Left += e.HorizontalChange;
            Width -= e.HorizontalChange;
        }

        if (Height + e.VerticalChange > MinHeight)
        {
            Height += e.VerticalChange;
        }
    }

    private void RightBottomThumb_OnDragDelta(object sender, DragDeltaEventArgs e)
    {
        if (Width + e.HorizontalChange > MinWidth)
        {
            Width += e.HorizontalChange;
        }

        if (Height + e.VerticalChange > MinHeight)
        {
            Height += e.VerticalChange;
        }
    }

    private void Swap_OnClick(object sender, RoutedEventArgs e) =>
        (OriginalAlphabet, EditedAlphabet) = (EditedAlphabet, OriginalAlphabet);

    private void EditButton_OnClick(object sender, RoutedEventArgs e)
    {
        var fastAlphabetEditWindow = new FastAlphabetEditWindow(this);
        fastAlphabetEditWindow.ShowDialog();
    }

    private void MixButton_OnClick(object sender, RoutedEventArgs e) => EditedAlphabet = Shuffle(EditedAlphabet);

    private static T[] Shuffle<T>(T[] array)
    {
        var rng = new Random();
        var newArray = new T[array.Length];
        Array.Copy(array, newArray, array.Length);
        var n = newArray.Length;
        while (n > 1)
        {
            n--;
            var k = rng.Next(n + 1);
            (newArray[k], newArray[n]) = (newArray[n], newArray[k]);
        }

        return newArray;
    }

    private void AddSpaceButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (wasAdded) return;
        wasAdded = true;
        OriginalAlphabet = OriginalAlphabet.Append(' ').ToArray();
        EditedAlphabet = OriginalAlphabet;
    }

    private void ClearButton_OnClick(object sender, RoutedEventArgs e)
    {
        foreach (var uiElement in EditedAlphabetStackPanel.Children)
        {
            if (uiElement is not TextBox textBox) continue;
            textBox.Text = string.Empty;
        }
    }

    private void RichTextBox_SelectionChanged(object sender, RoutedEventArgs e)
    {
        if (sender is not RichTextBox currentTextBox) return;
        var otherTextBox = currentTextBox == OriginalRichTextBox ? EditedRichTextBox : OriginalRichTextBox;

        if (currentTextBox.GetOffsetFromEndToStartContent() is -2) return;
        if (currentTextBox.Length() != otherTextBox.Length()) return;
        if (currentTextBox.Selection.IsEmpty)
        {
            otherTextBox.ResetForeground();
            return;
        }

        //
        var selectionStart = currentTextBox.Selection.Start;
        var selectionEnd = currentTextBox.Selection.End;
        if (selectionStart is null || selectionEnd is null) return;

        //
        var documentContentStart = currentTextBox.Document.ContentStart;
        var paragraph = new Paragraph();
        var text = otherTextBox.GetAllTextFromRichTextBox();
        var offsetFromStartToStart = selectionStart.GetOffsetToPosition(documentContentStart).GetAbs() - 2;
        var offsetFromStartToEnd = selectionEnd.GetOffsetToPosition(documentContentStart).GetAbs() - 2;
        if (offsetFromStartToEnd > text.Length) return;
        paragraph.Inlines.Add(text[..offsetFromStartToStart]);
        var run = new Run(text[offsetFromStartToStart..offsetFromStartToEnd])
        {
            Background = new SolidColorBrush(Colors.DodgerBlue)
        };
        paragraph.Inlines.Add(run);
        paragraph.Inlines.Add(text[offsetFromStartToEnd..]);
        otherTextBox.Document = new FlowDocument(paragraph);
    }


    private void OriginalRichTextBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
        EditedRichTextBox.ResetForeground();
    }

    private void EditedRichTextBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
        OriginalRichTextBox.ResetForeground();
    }
}

internal static class RichTextBoxExtension
{
    public static string GetAllTextFromRichTextBox(this RichTextBox richTextBox)
    {
        var text = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd).Text;
        var lastIndexOf = text.LastIndexOf("\r\n", StringComparison.Ordinal);
        if (lastIndexOf != -1) text = text[..lastIndexOf];
        return text;
    }

    public static int GetOffsetFromEndToStartContent(this RichTextBox richTextBox)
    {
        return richTextBox.Document.ContentEnd.GetOffsetToPosition(richTextBox.Document.ContentStart);
    }

    public static int Length(this RichTextBox richTextBox)
    {
        return richTextBox.GetAllTextFromRichTextBox().Length;
    }

    public static void ResetForeground(this RichTextBox richTextBox)
    {
        var text = richTextBox.GetAllTextFromRichTextBox();
        richTextBox.Document = new FlowDocument();
        richTextBox.AppendText(text);
    }
}

internal static class IntExtension
{
    public static int GetAbs(this int value) => Math.Abs(value);
}