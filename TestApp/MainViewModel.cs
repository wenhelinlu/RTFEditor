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
        private string _text = @"";

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