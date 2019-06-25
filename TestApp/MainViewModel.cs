using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.ViewModel;

namespace TestApp
{
    public class MainViewModel : NotificationObject
    {
        private string _text = @"
確認お願いします。
ありがとうございます。
以上宜しくお願い致します。
 
HONG MOLD TECH
徐知弘";

        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
                RaisePropertyChanged(() => Text);
            }
        }
    }
}