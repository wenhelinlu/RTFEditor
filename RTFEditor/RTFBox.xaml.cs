using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO;
using ColorPicker;
using System.Runtime.InteropServices;
using Rtf2HtmlMod;

namespace RTFEditor
{
    /// <summary>
    /// Interaktionslogik für "RTFBox.xaml"
    /// </summary>
    public partial class RTFBox
    {
        public static readonly DependencyProperty TextProperty =
          DependencyProperty.Register("Text", typeof(string), typeof(RTFBox),
          new PropertyMetadata(string.Empty));

        public string Text
        {
            get { return GetValue(TextProperty) as string; }
            set
            {
                SetValue(TextProperty, value);
            }
        }

        #region BindingContent Dependency Property

        private string myBindingContent = string.Empty;

        /// <summary>
        /// 转换后的Html文本
        /// </summary>
        public string BindingHtml
        {
            get { return (string)GetValue(BindingHtmlProperty); }
            set { SetValue(BindingHtmlProperty, value); }
        }

        public static readonly DependencyProperty BindingHtmlProperty =
            DependencyProperty.Register("BindingHtml", typeof(string), typeof(RTFBox),
                new FrameworkPropertyMetadata(string.Empty, new PropertyChangedCallback(OnBindingHtmlChanged)));

        private static void OnBindingHtmlChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            RTFBox editor = (RTFBox)sender;

            var v = (string)e.NewValue;
            string rtf = MarkupConverter.HtmlToRtfConverter.ConvertHtmlToRtf(v);
            editor.SetRTF(rtf);
        }

        private void NotifyBindingHtmlChanged()
        {
            //if (myBindingContent != this.ContentHtml)
            //{
            //    BindingContent = this.ContentHtml;
            //}

            //将rtf格式转换成html文本
            var rtf = GetRTF();
            var htmlResult = RtfToHtmlConverter.RtfToHtml(rtf);
            BindingHtml = htmlResult.Html;
        }

        #endregion BindingContent Dependency Property

        /// <summary>
        /// Konstruktor - initialisiert alle graphischen Komponenten
        /// </summary>
        public RTFBox()
        {
            this.InitializeComponent();

            FontColorContextMenu.Opened += FontColorContextMenu_Opened;
            FontColorContextMenu.Closed += FontColorContextMenu_Closed;

            BackColorContextMenu.Opened += BackColorContextMenu_Opened;
            BackColorContextMenu.Closed += BackColorContextMenu_Closed;

            FontColorPicker.SelectedColorChanged += FontColorPicker_SelectedColorChanged;
            BackColorPicker.SelectedColorChanged += BackColorPicker_SelectedColorChanged;
        }

        #region Variablen

        private bool dataChanged = false; // ungespeicherte Textänderungen

        private string privateText = null; // Inhalt der RTFBox im txt-Format

        public string text
        {
            get
            {
                TextRange range = new TextRange(RichTextControl.Document.ContentStart, RichTextControl.Document.ContentEnd);
                return range.Text;
            }
            set
            {
                privateText = value;
            }
        }

        private string zeilenangabe; // aktuelle Zeile der Cursorposition
        private int privAktZeile = 1;

        public int aktZeile
        {
            get { return privAktZeile; }
            set
            {
                privAktZeile = value;
                zeilenangabe = "Ze: " + value;
                LabelZeileNr.Content = zeilenangabe;
            }
        }

        private string spaltenangabe; // aktuelle Spalte der Cursorposition
        private int privAktSpalte = 1;

        public int aktSpalte
        {
            get { return privAktSpalte; }
            set
            {
                privAktSpalte = value;
                spaltenangabe = "Sp: " + value;
                LabelSpalteNr.Content = spaltenangabe;
            }
        }

        #endregion Variablen

        #region ControlHandler

        //
        // Nach dem Laden des Control
        //
        private void RTFEditor_Loaded(object sender, RoutedEventArgs e)
        {
            // Schrifttypen- und -größen-Initialisierung
            TextRange range = new TextRange(RichTextControl.Selection.Start, RichTextControl.Selection.End);
            Fonttype.SelectedValue = range.GetPropertyValue(FlowDocument.FontFamilyProperty).ToString();
            Fontheight.SelectedValue = range.GetPropertyValue(FlowDocument.FontSizeProperty).ToString();

            // aktuelle Zeilen- und Spaltenpositionen angeben
            aktZeile = Zeilennummer();
            aktSpalte = Spaltennummer();

            RichTextControl.Focus();
        }

        #endregion ControlHandler

        #region private ToolBarHandler

        //
        // ToolStripButton Open wurde gedrückt
        //
        private void ToolStripButtonOpen_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SetRTF("");

            // Configure open file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = ""; // Default file name
            dlg.DefaultExt = ".rtf"; // Default file extension
            dlg.Filter = "RichText documents (.rtf)|*.rtf|Text documents (.txt)|*.txt"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                string path = dlg.FileName;

                // Open document
                TextRange range;
                FileStream fStream;

                if (File.Exists(path))
                {
                    range = new TextRange(RichTextControl.Document.ContentStart, RichTextControl.Document.ContentEnd);
                    fStream = new FileStream(path, FileMode.OpenOrCreate);
                    range.Load(fStream, DataFormats.Rtf);
                    fStream.Close();
                }
            }
        }

        //
        // ToolStripButton Print wurde gedrückt
        //
        private void ToolStripButtonPrint_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Configure printer dialog box
            PrintDialog dlg = new PrintDialog();
            dlg.PageRangeSelection = PageRangeSelection.AllPages;
            dlg.UserPageRangeEnabled = true;

            // Show and process save file dialog box results
            if (dlg.ShowDialog() == true)
            {
                //use either one of the below
                // dlg.PrintVisual(RichTextControl as Visual, "printing as visual");
                dlg.PrintDocument((((IDocumentPaginatorSource)RichTextControl.Document).DocumentPaginator), "printing as paginator");
            }
        }

        //
        // ToolStripButton Strikeout wurde gedrückt
        // (läßt sich nicht durch Command in XAML abarbeiten)
        //
        private void ToolStripButtonStrikeout_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // TODO: Ereignishandlerimplementierung hier einfügen.
            TextRange range = new TextRange(RichTextControl.Selection.Start, RichTextControl.Selection.End);

            TextDecorationCollection tdc = (TextDecorationCollection)RichTextControl.Selection.GetPropertyValue(Inline.TextDecorationsProperty);
            if (tdc == null || !tdc.Equals(TextDecorations.Strikethrough))
            {
                tdc = TextDecorations.Strikethrough;
            }
            else
            {
                tdc = new TextDecorationCollection();
            }
            range.ApplyPropertyValue(Inline.TextDecorationsProperty, tdc);
        }

        //
        // ToolStripButton Textcolor wurde gedrückt
        //
        private void ToolStripButtonTextcolor_Click(object sender, RoutedEventArgs e)
        {
            //ColorDialog colorDialog = new ColorDialog();
            ////colorDialog.Owner = this;
            //if ((bool)colorDialog.ShowDialog())
            //{
            //    TextRange range = new TextRange(RichTextControl.Selection.Start, RichTextControl.Selection.End);

            //    range.ApplyPropertyValue(FlowDocument.ForegroundProperty, new SolidColorBrush(colorDialog.SelectedColor));
            //}
        }

        //
        // ToolStripButton Backgroundcolor wurde gedrückt
        //
        private void ToolStripButtonBackcolor_Click(object sender, RoutedEventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            //colorDialog.Owner = this;
            if ((bool)colorDialog.ShowDialog())
            {
                TextRange range = new TextRange(RichTextControl.Selection.Start, RichTextControl.Selection.End);
                range.ApplyPropertyValue(FlowDocument.BackgroundProperty, new SolidColorBrush(colorDialog.SelectedColor));
            }
        }

        //
        // ToolStripButton Subscript wurde gedrückt
        // (läßt sich auch durch Command in XAML machen.
        // Damit dann aber ein wirkliches Subscript angezeigt wird, muß die benutzte Font OpenType sein:
        // http://msdn.microsoft.com/en-us/library/ms745109.aspx#variants
        // Um auch für alle anderen Schriftarten Subscript zu realisieren, läßt sich stattdessen die Baseline Eigenschaft verändern)
        //
        private void ToolStripButtonSubscript_Click(object sender, RoutedEventArgs e)
        {
            var currentAlignment = RichTextControl.Selection.GetPropertyValue(Inline.BaselineAlignmentProperty);

            BaselineAlignment newAlignment = ((BaselineAlignment)currentAlignment == BaselineAlignment.Subscript) ? BaselineAlignment.Baseline : BaselineAlignment.Subscript;
            RichTextControl.Selection.ApplyPropertyValue(Inline.BaselineAlignmentProperty, newAlignment);
        }

        //
        // ToolStripButton Superscript wurde gedrückt
        // (läßt sich auch durch Command in XAML machen.
        // Damit dann aber ein wirkliches Superscript angezeigt wird, muß die benutzte Font OpenType sein:
        // http://msdn.microsoft.com/en-us/library/ms745109.aspx#variants
        // Um auch für alle anderen Schriftarten Superscript zu realisieren, läßt sich stattdessen die Baseline Eigenschaft verändern)
        //
        private void ToolStripButtonSuperscript_Click(object sender, RoutedEventArgs e)
        {
            var currentAlignment = RichTextControl.Selection.GetPropertyValue(Inline.BaselineAlignmentProperty);

            BaselineAlignment newAlignment = ((BaselineAlignment)currentAlignment == BaselineAlignment.Superscript) ? BaselineAlignment.Baseline : BaselineAlignment.Superscript;
            RichTextControl.Selection.ApplyPropertyValue(Inline.BaselineAlignmentProperty, newAlignment);
        }

        //
        // Textgröße wurde vom Benutzer verändert
        //
        private void Fontheight_DropDownClosed(object sender, EventArgs e)
        {
            string fontHeight = (string)Fontheight.SelectedItem;

            if (fontHeight != null)
            {
                RichTextControl.Selection.ApplyPropertyValue(System.Windows.Controls.RichTextBox.FontSizeProperty, fontHeight);
                RichTextControl.Focus();
            }
        }

        //
        // andere Font wurde vom Benutzer ausgewählt
        //
        private void Fonttype_DropDownClosed(object sender, EventArgs e)
        {
            string fontName = (string)Fonttype.SelectedItem;

            if (fontName != null)
            {
                RichTextControl.Selection.ApplyPropertyValue(System.Windows.Controls.RichTextBox.FontFamilyProperty, fontName);
                RichTextControl.Focus();
            }
        }

        //
        // Alignmentstatus anpassen
        //
        private void ToolStripButtonAlignLeft_Click(object sender, RoutedEventArgs e)
        {
            if (ToolStripButtonAlignLeft.IsChecked == true)
            {
                ToolStripButtonAlignCenter.IsChecked = false;
                ToolStripButtonAlignRight.IsChecked = false;
            }
        }

        //
        // Alignmentstatus anpassen
        //
        private void ToolStripButtonAlignCenter_Click(object sender, RoutedEventArgs e)
        {
            if (ToolStripButtonAlignCenter.IsChecked == true)
            {
                ToolStripButtonAlignLeft.IsChecked = false;
                ToolStripButtonAlignRight.IsChecked = false;
            }
        }

        //
        // Alignmentstatus anpassen
        //
        private void ToolStripButtonAlignRight_Click(object sender, RoutedEventArgs e)
        {
            if (ToolStripButtonAlignRight.IsChecked == true)
            {
                ToolStripButtonAlignCenter.IsChecked = false;
                ToolStripButtonAlignLeft.IsChecked = false;
            }
        }

        #endregion private ToolBarHandler

        #region private RichTextBoxHandler

        //
        // Formatierungen des markierten Textes anzeigen
        //
        private void RichTextControl_SelectionChanged(object sender, RoutedEventArgs e)
        {
            // markierten Text holen
            TextRange selectionRange = new TextRange(RichTextControl.Selection.Start, RichTextControl.Selection.End);

            if (selectionRange.GetPropertyValue(FontWeightProperty).ToString() == "Bold")
            {
                ToolStripButtonBold.IsChecked = true;
            }
            else
            {
                ToolStripButtonBold.IsChecked = false;
            }

            if (selectionRange.GetPropertyValue(FontStyleProperty).ToString() == "Italic")
            {
                ToolStripButtonItalic.IsChecked = true;
            }
            else
            {
                ToolStripButtonItalic.IsChecked = false;
            }

            if (selectionRange.GetPropertyValue(Inline.TextDecorationsProperty) == TextDecorations.Underline)
            {
                ToolStripButtonUnderline.IsChecked = true;
            }
            else
            {
                ToolStripButtonUnderline.IsChecked = false;
            }

            if (selectionRange.GetPropertyValue(Inline.TextDecorationsProperty) == TextDecorations.Strikethrough)
            {
                ToolStripButtonStrikeout.IsChecked = true;
            }
            else
            {
                ToolStripButtonStrikeout.IsChecked = false;
            }

            if (selectionRange.GetPropertyValue(FlowDocument.TextAlignmentProperty).ToString() == "Left")
            {
                ToolStripButtonAlignLeft.IsChecked = true;
            }

            if (selectionRange.GetPropertyValue(FlowDocument.TextAlignmentProperty).ToString() == "Center")
            {
                ToolStripButtonAlignCenter.IsChecked = true;
            }

            if (selectionRange.GetPropertyValue(FlowDocument.TextAlignmentProperty).ToString() == "Right")
            {
                ToolStripButtonAlignRight.IsChecked = true;
            }

            // Sub-, Superscript Buttons setzen
            try
            {
                switch ((BaselineAlignment)selectionRange.GetPropertyValue(Inline.BaselineAlignmentProperty))
                {
                    case BaselineAlignment.Subscript:
                        ToolStripButtonSubscript.IsChecked = true;
                        ToolStripButtonSuperscript.IsChecked = false;
                        break;

                    case BaselineAlignment.Superscript:
                        ToolStripButtonSubscript.IsChecked = false;
                        ToolStripButtonSuperscript.IsChecked = true;
                        break;

                    default:
                        ToolStripButtonSubscript.IsChecked = false;
                        ToolStripButtonSuperscript.IsChecked = false;
                        break;
                }
            }
            catch (Exception)
            {
                ToolStripButtonSubscript.IsChecked = false;
                ToolStripButtonSuperscript.IsChecked = false;
            }

            // Get selected font and height and update selection in ComboBoxes
            Fonttype.SelectedValue = selectionRange.GetPropertyValue(FlowDocument.FontFamilyProperty).ToString();
            Fontheight.SelectedValue = selectionRange.GetPropertyValue(FlowDocument.FontSizeProperty).ToString();

            // Ausgabe der Zeilennummer
            aktZeile = Zeilennummer();

            // Ausgabe der Spaltennummer
            aktSpalte = Spaltennummer();
        }

        //
        // wurden Textänderungen gemacht
        //
        private void RichTextControl_TextChanged(object sender, TextChangedEventArgs e)
        {
            dataChanged = true;
        }

        //
        // Tastendruck erzeugt ein neues Zeichen in der gewählten Font
        //
        private void RichTextControl_KeyDown(object sender, KeyEventArgs e)
        {
            dataChanged = true;

            string fontName = (string)Fonttype.SelectedValue;
            string fontHeight = (string)Fontheight.SelectedValue;
            TextRange range = new TextRange(RichTextControl.Selection.Start, RichTextControl.Selection.End);

            range.ApplyPropertyValue(TextElement.FontFamilyProperty, fontName);
            range.ApplyPropertyValue(TextElement.FontSizeProperty, fontHeight);
        }

        //
        // Tastenkombinationen auswerten
        //
        private void RichTextControl_KeyUp(object sender, KeyEventArgs e)
        {
            // Ctrl + B
            if ((Keyboard.Modifiers == ModifierKeys.Control) && (e.Key == Key.B))
            {
                if (ToolStripButtonBold.IsChecked == true)
                {
                    ToolStripButtonBold.IsChecked = false;
                }
                else
                {
                    ToolStripButtonBold.IsChecked = true;
                }
            }

            // Ctrl + I
            if ((Keyboard.Modifiers == ModifierKeys.Control) && (e.Key == Key.I))
            {
                if (ToolStripButtonItalic.IsChecked == true)
                {
                    ToolStripButtonItalic.IsChecked = false;
                }
                else
                {
                    ToolStripButtonItalic.IsChecked = true;
                }
            }

            // Ctrl + U
            if ((Keyboard.Modifiers == ModifierKeys.Control) && (e.Key == Key.U))
            {
                if (ToolStripButtonUnderline.IsChecked == true)
                {
                    ToolStripButtonUnderline.IsChecked = false;
                }
                else
                {
                    ToolStripButtonUnderline.IsChecked = true;
                }
            }

            // Ctrl + O
            if ((Keyboard.Modifiers == ModifierKeys.Control) && (e.Key == Key.O))
            {
                ToolStripButtonOpen_Click(sender, e);
            }
        }

        #endregion private RichTextBoxHandler

        #region private Funktionen

        //
        // Gibt die Zeilennummer der aktuellen Cursorposition zurück
        //
        private int Zeilennummer()
        {
            TextPointer caretLineStart = RichTextControl.CaretPosition.GetLineStartPosition(0);
            TextPointer p = RichTextControl.Document.ContentStart.GetLineStartPosition(0);
            int currentLineNumber = 1;

            // Vom Anfang des RTF-Box Inhaltes wird vorwärts solange weitergezählt, bis die aktuelle Cursorposition erreicht ist.
            while (true)
            {
                if (caretLineStart.CompareTo(p) < 0)
                {
                    break;
                }
                int result;
                p = p.GetLineStartPosition(1, out result);
                if (result == 0)
                {
                    break;
                }
                currentLineNumber++;
            }
            return currentLineNumber;
        }

        //
        // Gibt die Spaltennummer der aktuellen Cursorposition zurück
        private int Spaltennummer()
        {
            TextPointer caretPos = RichTextControl.CaretPosition;
            TextPointer p = RichTextControl.CaretPosition.GetLineStartPosition(0);
            int currentColumnNumber = Math.Max(p.GetOffsetToPosition(caretPos) - 1, 0);

            return currentColumnNumber;
        }

        #endregion private Funktionen

        #region öffentliche Funktionen

        //
        // Alle Daten löschen
        //
        public void Clear()
        {
            dataChanged = false;
            RichTextControl.Document.Blocks.Clear();
        }

        //
        // Inhalt der RichTextBox als RTF setzen
        //
        public void SetRTF(string rtf)
        {
            TextRange range = new TextRange(RichTextControl.Document.ContentStart, RichTextControl.Document.ContentEnd);

            // Exception abfangen für StreamReader und MemoryStream, ArgumentException abfangen für range.Load bei rtf=null oder rtf=""
            try
            {
                // um die Load Methode eines TextRange Objektes zu benutzen wird ein MemoryStream Objekt erzeugt
                using (MemoryStream rtfMemoryStream = new MemoryStream())
                {
                    using (StreamWriter rtfStreamWriter = new StreamWriter(rtfMemoryStream))
                    {
                        rtfStreamWriter.Write(rtf);
                        rtfStreamWriter.Flush();
                        rtfMemoryStream.Seek(0, SeekOrigin.Begin);

                        range.Load(rtfMemoryStream, DataFormats.Rtf);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        //
        // RTF Inhalt der RichTextBox als RTF-String zurückgeben
        //
        public string GetRTF()
        {
            TextRange range = new TextRange(RichTextControl.Document.ContentStart, RichTextControl.Document.ContentEnd);

            // Exception abfangen für StreamReader und MemoryStream
            try
            {
                // um die Load Methode eines TextRange Objektes zu benutzen wird ein MemoryStream Objekt erzeugt
                using (MemoryStream rtfMemoryStream = new MemoryStream())
                {
                    using (StreamWriter rtfStreamWriter = new StreamWriter(rtfMemoryStream))
                    {
                        range.Save(rtfMemoryStream, DataFormats.Rtf);

                        rtfMemoryStream.Flush();
                        rtfMemoryStream.Position = 0;
                        StreamReader sr = new StreamReader(rtfMemoryStream);
                        return sr.ReadToEnd();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion öffentliche Funktionen

        #region TODO

        private float zoomFaktor = 1.0F; // Zoom Faktor der RTFBox

        // für das Scrollen bei Drag&Drop Operationen
        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);

        //private const long WM_USER = &H400;
        private const long WM_USER = 1024;

        private void SliderZoom_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //SendMessage(hwnd, EM_SETZOOM, 10, 10);

            int result = SendMessage(new System.Windows.Interop.WindowInteropHelper(Application.Current.MainWindow).Handle, (int)WM_USER + 255, (int)e.NewValue * 10, 10);
        }

        // In der StatusBar Ausgabe von Grossschreibweise, Num Lock, Zoom

        #endregion TODO

        #region 文字颜色、背景色设置

        private void ToggleFontColor_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement fxElement = sender as FrameworkElement;
            if (fxElement != null && FontColorContextMenu != null)
            {
                FontColorContextMenu.PlacementTarget = fxElement;
                FontColorContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                FontColorContextMenu.IsOpen = true;
            }
        }

        private void ToggleBackColor_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement fxElement = sender as FrameworkElement;
            if (fxElement != null && BackColorContextMenu != null)
            {
                BackColorContextMenu.PlacementTarget = fxElement;
                BackColorContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                BackColorContextMenu.IsOpen = true;
            }
        }

        /// <summary>
        /// 打开文字颜色选色板
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FontColorContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            FontColorPicker.Reset();
            ToggleFontColor.IsChecked = true;
        }

        /// <summary>
        /// 关闭文字颜色选色板
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FontColorContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            ToggleFontColor.IsChecked = false;
        }

        /// <summary>
        /// 设置文字颜色
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FontColorPicker_SelectedColorChanged(object sender, PropertyChangedEventArgs<Color> e)
        {
            TextRange range = new TextRange(RichTextControl.Selection.Start, RichTextControl.Selection.End);
            range.ApplyPropertyValue(FlowDocument.ForegroundProperty, new SolidColorBrush(e.NewValue));
        }

        /// <summary>
        /// 打开背景颜色选色板
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackColorContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            BackColorPicker.Reset();
            ToggleBackColor.IsChecked = true;
        }

        /// <summary>
        /// 关闭背景颜色选色板
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackColorContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            ToggleBackColor.IsChecked = false;
        }

        /// <summary>
        /// 设置背景色
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackColorPicker_SelectedColorChanged(object sender, PropertyChangedEventArgs<Color> e)
        {
            TextRange range = new TextRange(RichTextControl.Selection.Start, RichTextControl.Selection.End);
            range.ApplyPropertyValue(FlowDocument.BackgroundProperty, new SolidColorBrush(e.NewValue));
        }

        #endregion 文字颜色、背景色设置

        private void Exporttohtml_Click(object sender, RoutedEventArgs e)
        {
            string rtf = GetRTF();
            var htmlOutput = @"D:\test21.html";
            var contentUriPrefix = Path.GetFileNameWithoutExtension(htmlOutput);
            var htmlResult = RtfToHtmlConverter.RtfToHtml(rtf, contentUriPrefix);
            htmlResult.WriteToFile(htmlOutput);
        }

        private void RichTextControl_LostFocus(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(this.NotifyBindingHtmlChanged));
        }
    }
}