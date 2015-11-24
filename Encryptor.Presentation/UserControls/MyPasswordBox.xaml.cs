using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GalaSoft.MvvmLight;

namespace Encryptor.Presentation
{
    public partial class MyPasswordBox
    {
        public MyPasswordBox()
        {
            this.Loaded += (sender, args) => this.Grid.DataContext = this.vm;

            this.InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public static readonly DependencyProperty PasswordProperty = DependencyProperty.Register(
            "Password", typeof (string), typeof (MyPasswordBox),
            new PropertyMetadata(default(string), PropertyChangedCallback));

        private readonly MyPasswordBoxViewModel vm = new MyPasswordBoxViewModel();

        private static void PropertyChangedCallback(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var myPasswordBox = (MyPasswordBox) dependencyObject;
            var newValue = (string) dependencyPropertyChangedEventArgs.NewValue;

            if (myPasswordBox.Password != newValue)
                myPasswordBox.Password = newValue;
        }

        public string Password
        {
            get { return (string) this.GetValue(PasswordProperty); }
            set
            {
                this.SetValue(PasswordProperty, value);

                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Password"));
                if (this.PasswordBox.Password != value)
                    this.PasswordBox.Password = value;
            }
        }

        private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = (PasswordBox) sender;

            this.WatermarkTextBlock.Visibility = String.IsNullOrEmpty(passwordBox.Password)
                ? Visibility.Visible
                : Visibility.Collapsed;

            if (this.Password != passwordBox.Password)
            {
                this.Password = passwordBox.Password;

                this.vm.SetCalcEntropy(passwordBox.Password);
            }
        }

        public class MyPasswordBoxViewModel : ViewModelBase
        {
            private double offsetTransparent;
            private double offsetWhite;
            private int entropyBit;

            public MyPasswordBoxViewModel()
            {
                if (this.IsInDesignMode)
                {
                    this.SetCalcEntropy("qwert");
                }
            }

            public int EntropyBit
            {
                get { return this.entropyBit; }
                set { this.Set(ref this.entropyBit, value); }
            }

            public double OffsetTransparent
            {
                get { return this.offsetTransparent; }
                set
                {
                    this.Set(ref this.offsetTransparent, value);
                    this.OffsetWhite = (value + 0.05);
                }
            }

            public double OffsetWhite
            {
                get { return this.offsetWhite; }
                set { this.Set(ref this.offsetWhite, value); }
            }

            private static readonly HashSet<string> EntropySets = new HashSet<string>
            {
                "abcdefghijklmnopqrszkvwxyz",
                "ABCDEFGHIJKLMNOPQRSZKVWXYZ",
                "0123456789",
                @"!""#$%&()*+,-./[]_@:;=?",
                @"<>~^'\ ",
                "äöüÄÖÜ"
            };

            public void SetCalcEntropy(string password)
            {
                var letters = 0;
                foreach (var entropySet in EntropySets)
                {
                    var hit = password.Any(c => entropySet.Any(c1 => c1 == c));
                    if (hit)
                        letters += entropySet.Length;
                }

                var entropy = (int) (password.Length*Math.Log(letters));

                if (entropy > 0)
                {
                    this.OffsetTransparent = (double) entropy/256;
                    this.EntropyBit = entropy;
                }
                else
                {
                    this.OffsetTransparent = 0;
                    this.EntropyBit = 0;
                }
            }
        }
    }
}