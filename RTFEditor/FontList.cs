using System;
using System.Collections.ObjectModel;
using System.Drawing.Text;
using System.Windows.Media;

namespace RTFEditor
{
    /// <summary>
    /// Erzeugt eine String Liste aller auf dem System installierten Schriftarten
    /// </summary>
    internal class FontList : ObservableCollection<string>
    {
        public FontList()
        {
            //foreach (FontFamily f in Fonts.SystemFontFamilies)
            //{
            //    this.Add(f.ToString());
            //}
            using (InstalledFontCollection fonts = new InstalledFontCollection())
            {
                foreach (var family in fonts.Families)
                {
                    this.Add(family.Name);
                }
            }
        }
    }
}