using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Encryptor.FileOperator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace Encryptor.Presentation.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private string passwordFirst;
        private string passwordSecond;
        private bool hideFileName;
        private double progressValue;
        private string progressStatus;
        private string filePath;
        private bool showDropPanel;
        private bool isEncryptionMode;
        private bool isEnabled = true;

        public MainViewModel()
        {
            if (this.IsInDesignMode)
            {
                this.ProgressStatus = "encrypting";
                this.ProgressValue = 75;
                this.FilePath = @"c:\files\temp.txt";
                this.ShowDropPanel = false;
                this.isEncryptionMode = true;
                this.PasswordFirst = "qwert";
            }
            else
            {
                this.GoCommand = new RelayCommand(this.Go, this.CanExecute);
                this.HelpCommand = new RelayCommand(this.Help);
                this.isEncryptionMode = true;
            }
        }
        private void Help()
        {
            try
            {
                var myProcess = new Process
                {
                    StartInfo =
                    {
                        FileName = "https://github.com/dhcgn/Encryptor/wiki"
                    }
                };
                myProcess.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public RelayCommand HelpCommand { get; set; }

        private async void Go()
        {
            this.IsEnabled = false;

            if (this.isEncryptionMode)
            {
                await this.Encrypt();
            }
            else
            {
                await this.Decrypt();
            }

            this.IsEnabled = true;
        }

        public bool IsEnabled
        {
            get { return this.isEnabled; }
            set { this.Set(ref this.isEnabled, value); }
        }

        private Task<bool> Encrypt()
        {
            if (new FileInfo(this.FilePath).Length > 2000000000)
            {
                MessageBox.Show("File is to too big. For the moment files up to 2GB can be encrypted", "Error");
                return null;
            }

            return Task.Factory.StartNew(() =>
            {
                Engine.Encryptor.Encrypt(this.FilePath, this.PasswordFirst, this.HideFileName, this.StatusCallback);
                return true;
            });
        }

        private Task<bool> Decrypt()
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    Engine.Encryptor.Decrypt(this.FilePath, this.PasswordFirst, this.StatusCallback);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "Exception");
                    return false;
                }
                return true;
            });
        }

        public void StatusCallback(double percent, string message)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.ProgressValue = percent;
                this.ProgressStatus = message;
            }));
        }

        public RelayCommand GoCommand { get; set; }

        public string FilePath
        {
            get { return this.filePath; }
            set
            {
                this.Set(ref this.filePath, value);
                this.RaisePropertyChanged(() => this.FileName);
            }
        }

        public string FileName => Path.GetFileName(this.FilePath);

        private bool CanExecute()
        {
            if (String.IsNullOrWhiteSpace(this.PasswordFirst)) return false;

            if (this.isEncryptionMode)
            {
                if (String.IsNullOrWhiteSpace(this.PasswordSecond)) return false;
                if (this.PasswordFirst != this.PasswordSecond) return false;
            }

            if (String.IsNullOrWhiteSpace(this.FilePath)) return false;

            return true;
        }

        public bool HideFileName
        {
            get { return this.hideFileName; }
            set { this.Set(ref this.hideFileName, value); }
        }

        public bool ShowDropPanel
        {
            get { return this.showDropPanel; }
            set { this.Set(ref this.showDropPanel, value); }
        }

        public string PasswordFirst
        {
            get { return this.passwordFirst; }
            set
            {
                this.Set(ref this.passwordFirst, value);
                this.GoCommand?.RaiseCanExecuteChanged();
            }
        }

        public string PasswordSecond
        {
            get { return this.passwordSecond; }
            set
            {
                this.Set(ref this.passwordSecond, value);
                this.GoCommand?.RaiseCanExecuteChanged();
            }
        }

        public double ProgressValue
        {
            get { return this.progressValue; }
            set { this.Set(ref this.progressValue, value); }
        }

        public string ProgressStatus
        {
            get { return this.progressStatus; }
            set { this.Set(ref this.progressStatus, value); }
        }

        public void DropFiles(string[] files)
        {
            if (files.Count() == 1 && !File.GetAttributes(files[0]).HasFlag(FileAttributes.Directory))
            {
                if (files[0].EndsWith(Encryptor.Engine.Encryptor.FileExtension))
                    this.DecryptionMode = true;
                else
                    this.EncryptionMode = true;

                this.FilePath = files[0];
            }
            else if (MessageBox.Show("Not a single file was dropped, do you want to create a ZipArchiv", "Question",
                    MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var zipArchiv = Path.Combine(Path.GetDirectoryName(files[0]),Guid.NewGuid() + ".zip");
                Zip.CreateZipArchiv(files, zipArchiv);
            }
            else
            {
                this.FilePath = null;
            }
            this.GoCommand?.RaiseCanExecuteChanged();
        }

        public bool EncryptionMode
        {
            get { return this.isEncryptionMode; }
            set
            {
                this.Set(ref this.isEncryptionMode, value);
                this.RaisePropertyChanged(() => this.DecryptionMode);
                this.GoCommand?.RaiseCanExecuteChanged();
            }
        }

        public bool DecryptionMode
        {
            get { return !this.isEncryptionMode; }
            set
            {
                this.Set(ref this.isEncryptionMode, !value);
                this.RaisePropertyChanged(() => this.EncryptionMode);
                this.GoCommand?.RaiseCanExecuteChanged();
            }
        }
    }
}