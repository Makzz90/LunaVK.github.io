using LunaVK.Core;
using LunaVK.Core.Framework;
using LunaVK.Core.Library;
using LunaVK.Core.Utils;
using LunaVK.Framework;
using LunaVK.Library;
using LunaVK.UC;
using LunaVK.ViewModels;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LunaVK.Pages
{
    public sealed partial class DiagnosticsPage : PageBase
    {
        private bool _isSending;

        public DiagnosticsPage()
        {
            this.InitializeComponent();
            base.DataContext = new SettingsViewModel();
            base.Title = LocalizedStrings.GetString("NewSettings_Diagnostics/Title");
            this.Loaded += DiagnosticsPage_Loaded;
            this.Unloaded += DiagnosticsPage_Unloaded;
        }

        private SettingsViewModel VM
        {
            get { return base.DataContext as SettingsViewModel; }
        }
        private void DiagnosticsPage_Unloaded(object sender, RoutedEventArgs e)
        {
            Logger.Instance.LogAdded -= this.OnLogAdded;
        }

        private void DiagnosticsPage_Loaded(object sender, RoutedEventArgs e)
        {
            base.InitializeProgressIndicator();
            //this._logs.Text = Logger.Instance.GetLog();
            Logger.Instance.LogAdded += this.OnLogAdded;
            //this._btnSendData.IsEnabled = Logger.Instance.IsLogFileExists;
        }

        private void OnLogAdded(string msg)
        {
            Execute.ExecuteOnUIThread(() => 
            {
                //this._logs.Text += "\n";
                //this._logs.Text += msg;
                this._logs.Items.Insert(0,msg.Trim());
            });
        }

        private void SendData_OnClicked(object sender, RoutedEventArgs e)
        {
            if (this._isSending)
                return;
            this._isSending = true;
            this.VM.SetInProgress(true);

            string stroageText = Logger.Instance.ReadLogFromStorage();
            string logText = Logger.Instance.GetLog;

            AppsService.Instance.SendLog(stroageText + "||" + logText, (result) =>
            {
                Execute.ExecuteOnUIThread(() =>
                {
                    if (result == true)
                    {
                        Logger.Instance.DeleteLogFromIsolatedStorage();
                        this._logs.Items.Clear();
                    }

                    this._isSending = false;
                    this.VM.SetInProgress(false);
                    //ResultCode resultCode = result.ResultCode;
                    
                    GenericInfoUC.ShowBasedOnResult(LocalizedStrings.GetString("LogSent"));
                    this._btnSendData.IsEnabled = false;
                });
            });
        }
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this._logs.Items.Clear();

            string text = Logger.Instance.GetLog;
            string[] splitted = text.Split(new char[] { '\n' });
            foreach (var item in splitted)
            {
                this._logs.Items.Insert(0, item.Trim());
            }
        }
        
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this._logs.Items.Clear();

            //this._logs.Text = Logger.Instance.ReadLogFromStorage();
            string text = Logger.Instance.ReadLogFromStorage();
            string[] splitted = text.Split(new char[] { '\n' });
            foreach(var item in splitted)
            {
                this._logs.Items.Insert(0,item.Trim());
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Logger.Instance.DeleteLogFromIsolatedStorage();
            this._logs.Items.Clear();
            //
            //
            var container = ApplicationData.Current.LocalSettings.CreateContainer("Settings2", ApplicationDataCreateDisposition.Always);
            object value;
            string o = "";
            if (container.Values.TryGetValue("Data", out value))
            {
                o = (string)value;
            }
            else
            {
                o = "Not exists";
            }
            this._logs.Items.Add(o);
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView lv = sender as ListView;
            if (lv.SelectedIndex == 0)
                CustomFrame.Instance.Navigate(typeof(Pages.Debug.TestResponse));
            else if (lv.SelectedIndex == 1)
                CustomFrame.Instance.Navigate(typeof(Pages.Debug.TestEmoji));
            else if (lv.SelectedIndex == 2)
                CustomFrame.Instance.Navigate(typeof(Pages.Debug.TestStickersKeywords));
            else if (lv.SelectedIndex == 3)
                CustomFrame.Instance.Navigate(typeof(Pages.Debug.TestRawNotification));
            else if (lv.SelectedIndex == 4)
                CustomFrame.Instance.Navigate(typeof(Pages.Debug.ViewColors));
        }

        private void _logs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView lv = sender as ListView;
            lv.SelectionMode = ListViewSelectionMode.None;
            lv.SelectionMode = ListViewSelectionMode.Single;
        }

        private void ListView_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            ListView lv = sender as ListView;
            if (lv.SelectedIndex == 0)
                PushNotifications.Instance.RegisterTasks();
            if (lv.SelectedIndex == 1)
                PushNotifications.Instance.UnRegisterTasks();
        }
    }
}
