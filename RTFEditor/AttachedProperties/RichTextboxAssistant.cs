using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Markup;
using Rtf2HtmlMod;

namespace RTFEditor.AttachedProperties
{
    public static class RichTextboxAssistant
    {
        public static readonly DependencyProperty BoundDocument =
           DependencyProperty.RegisterAttached("BoundDocument", typeof(string), typeof(RichTextboxAssistant),
           new FrameworkPropertyMetadata(null,
               FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
               OnBoundDocumentChanged)
               );

        private static void OnBoundDocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RichTextBox box = d as RichTextBox;

            if (box == null)
                return;

            RemoveEventHandler(box);

            string newXAML = GetBoundDocument(d);

            box.Document.Blocks.Clear();

            if (!string.IsNullOrEmpty(newXAML))
            {
                using (MemoryStream xamlMemoryStream = new MemoryStream(Encoding.ASCII.GetBytes(newXAML)))
                {
                    ParserContext parser = new ParserContext();
                    parser.XmlnsDictionary.Add("", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
                    parser.XmlnsDictionary.Add("x", "http://schemas.microsoft.com/winfx/2006/xaml");
                    FlowDocument doc = new FlowDocument();
                    Section section = XamlReader.Load(xamlMemoryStream, parser) as Section;

                    box.Document.Blocks.Add(section);
                }
            }

            AttachEventHandler(box);
        }

        private static void RemoveEventHandler(RichTextBox box)
        {
            Binding binding = BindingOperations.GetBinding(box, BoundDocument);

            if (binding != null)
            {
                if (binding.UpdateSourceTrigger == UpdateSourceTrigger.Default ||
                    binding.UpdateSourceTrigger == UpdateSourceTrigger.LostFocus)
                {
                    box.LostFocus -= HandleLostFocus;
                }
                else
                {
                    box.TextChanged -= HandleTextChanged;
                }
            }
        }

        private static void AttachEventHandler(RichTextBox box)
        {
            Binding binding = BindingOperations.GetBinding(box, BoundDocument);

            if (binding != null)
            {
                if (binding.UpdateSourceTrigger == UpdateSourceTrigger.Default ||
                    binding.UpdateSourceTrigger == UpdateSourceTrigger.LostFocus)
                {
                    box.LostFocus += HandleLostFocus;
                }
                else
                {
                    box.TextChanged += HandleTextChanged;
                }
            }
        }

        private static void HandleLostFocus(object sender, RoutedEventArgs e)
        {
            RichTextBox box = sender as RichTextBox;

            string rtf = GetRTF(box);
            SetBoundDocument(box, rtf);
        }

        private static void HandleTextChanged(object sender, RoutedEventArgs e)
        {
            // TODO: TextChanged is currently not working!
            RichTextBox box = sender as RichTextBox;

            string rtf = GetRTF(box);
            SetBoundDocument(box, rtf);
        }

        public static string GetBoundDocument(DependencyObject dp)
        {
            var html = dp.GetValue(BoundDocument) as string;
            var xaml = string.Empty;

            if (!string.IsNullOrEmpty(html))
            {
                //xaml = HtmlToXamlConverter.ConvertHtmlToXaml(html, false);
                xaml = html;
            }

            return xaml;
        }

        public static void SetBoundDocument(DependencyObject dp, string value)
        {
            //var html = HtmlFromXamlConverter.ConvertXamlToHtml(xaml, false);
            var xaml = value;
            //var htmlOutput = @"D:\test21.html";
            //var contentUriPrefix = Path.GetFileNameWithoutExtension(htmlOutput);
            //var htmlResult = RtfToHtmlConverter.RtfToHtml(xaml, contentUriPrefix);
            ////htmlResult.WriteToFile(htmlOutput);
            //var html = htmlResult.Html;
            dp.SetValue(BoundDocument, xaml);
        }

        //
        // Inhalt der RichTextBox als RTF setzen
        //
        public static void SetRTF(RichTextBox richTextBox, string rtf)
        {
            TextRange range = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);

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
        public static string GetRTF(RichTextBox richTextBox)
        {
            TextRange range = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);

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
    }
}