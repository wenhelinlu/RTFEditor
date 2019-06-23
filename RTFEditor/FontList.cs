using System;
using System.Collections.ObjectModel;
using System.Windows.Media;


namespace RTFEditor
{
    /// <summary>
    /// Erzeugt eine String Liste aller auf dem System installierten Schriftarten
    /// </summary>
    class FontList : ObservableCollection<string> 
    { 
        public FontList() 
        {
            foreach (FontFamily f in Fonts.SystemFontFamilies)
            {                
                this.Add(f.ToString());                
            }  
        }   
    }
}
