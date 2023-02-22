using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace KeyPad
{
    /// <summary>
    /// Interaction logic for KeypadNum.xaml
    /// </summary>
    /// 
    public partial class KeypadNum : Window,INotifyPropertyChanged
    {

        private string _result;
        public string Result
        {
            get { return _result; }
            private set { _result = value; this.OnPropertyChanged("Result"); }
        }


        public KeypadNum(TextBox owner, Window wndOwner)
        {
            InitializeComponent();
            this.Owner = wndOwner;
            this.DataContext = this;
            Result = "";
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }


        private void Button_TouchDown(object sender, TouchEventArgs e)
        {
            Button button = sender as Button;
            switch (button.CommandParameter.ToString())
            {


                case "RETURN":
                    this.DialogResult = true;
                    break;

                case "BACK":
                    if (Result.Length > 0)
                        Result = Result.Remove(Result.Length - 1);
                    break;

                default:
                    Result += button.Content.ToString();
                    break;
            }
        }

        private void buttonEsc_TouchDown(object sender, TouchEventArgs e)
        {
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            switch (button.CommandParameter.ToString())
            {


                case "RETURN":
                    this.DialogResult = true;
                    break;

                case "BACK":
                    if (Result.Length > 0)
                        Result = Result.Remove(Result.Length - 1);
                    break;

                default:
                    Result += button.Content.ToString();
                    break;
            }
        }

        private void buttonEsc_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
