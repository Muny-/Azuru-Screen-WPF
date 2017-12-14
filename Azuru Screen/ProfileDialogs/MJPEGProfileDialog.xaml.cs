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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ASU
{
    /// <summary>
    /// Interaction logic for Client.xaml
    /// </summary>
    public partial class MJPEGProfileDialog : Window
    {
        public Dictionary<string, MJPEGConnectionProfile> Profiles = new Dictionary<string, MJPEGConnectionProfile>();

        public MJPEGConnectionProfile SelectedProfile;

        public MJPEGProfileDialog()
        {
            InitializeComponent();

            profileSelection.SelectedIndex = 0;

            LoadProfiles();

            //MessageBox.Show(new MJPEGConnectionProfile("http://192.168.1.11:8080/videofeed.mjpeg", true, "admin", "Password992039").ToJSON());
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (profileSelection.SelectedIndex != -1)
            {
                MJPEGConnectionProfile prof = Profiles[profileSelection.SelectedValue.ToString()];

                URLTextbox.Text = prof.Address;
                usernameTextbox.Text = prof.Username;
                passwordTextbox.Password = prof.Password;
                UseAuthenticationCheckbox.IsChecked = prof.IsAuthenticated;
            }
        }

        void UpdateProfileList()
        {
            profileSelection.Items.Clear();

            foreach(KeyValuePair<string, MJPEGConnectionProfile> pair in Profiles)
            {
                profileSelection.Items.Add(pair.Key);
            }
        }

        static string ConfigurationFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Azuru Sharing Utility/";
        static string ConfigurationFile = ConfigurationFolder + "mjpegprofiles.json";

        void CheckConfigFile()
        {
            if (Directory.Exists(ConfigurationFolder))
            {
                if (File.Exists(ConfigurationFile))
                {

                }
                else
                {
                    File.WriteAllText(ConfigurationFile, "{\"Most Recent\": " + new MJPEGConnectionProfile("Most Recent", "").ToJSON() + "}");
                }
            }
            else
            {
                Directory.CreateDirectory(ConfigurationFolder);
                CheckConfigFile();
            }
        }

        void LoadProfiles()
        {
            CheckConfigFile();

            dynamic profiles = DynamicJson.Parse(File.ReadAllText(ConfigurationFile));

            Profiles.Clear();
            

            foreach(KeyValuePair<string, dynamic> pair in profiles)
            {
                Profiles.Add(pair.Value.name, new MJPEGConnectionProfile(pair.Value.name, pair.Value.address, (bool)pair.Value.is_authenticated, pair.Value.username, pair.Value.password));
            }

            UpdateProfileList();

            profileSelection.SelectedIndex = 0;
        }

        void SaveProfiles()
        {
            CheckConfigFile();

            string json = "{";

            for (int i = 0; i < Profiles.Count; i++)
            {

                KeyValuePair<string, MJPEGConnectionProfile> pair = Profiles.ElementAt(i);

                json += "\"" + pair.Key + "\": " + pair.Value.ToJSON();

                if (i < Profiles.Count-1)
                    json += ", ";
            }

            json += "}";

            File.WriteAllText(ConfigurationFile, json);
        }

        void AddProfile()
        {
            if (ProfileNameTextbox.Text != "" && (Profiles.Where(val => val.Key == ProfileNameTextbox.Text).Count() < 1))
            {
                NewProfileGrid.Visibility = System.Windows.Visibility.Hidden;

                Profiles.Add(ProfileNameTextbox.Text, new MJPEGConnectionProfile(ProfileNameTextbox.Text, ""));

                URLTextbox.Text = "";
                usernameTextbox.Text = "";
                passwordTextbox.Password = "";
                UseAuthenticationCheckbox.IsChecked = false;

                ProfileNameTextbox.Text = "";

                UpdateProfileList();

                profileSelection.SelectedIndex = profileSelection.Items.Count - 1;
            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                AddProfile();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AddProfile();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            NewProfileGrid.Visibility = System.Windows.Visibility.Visible;
            ProfileNameTextbox.Focus();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (profileSelection.SelectedIndex != 0)
            {
                Profiles.Remove(profileSelection.SelectedValue.ToString());
                UpdateProfileList();
                profileSelection.SelectedIndex = 0;
            }
        }

        private void UseAuthenticationCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                KeyValuePair<string, MJPEGConnectionProfile> pair = Profiles.ElementAt(profileSelection.SelectedIndex);
                pair.Value.IsAuthenticated = true;
                
            }
            catch
            {

            }
            AuthenticationGroup.IsEnabled = true;
        }

        private void UseAuthenticationCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                KeyValuePair<string, MJPEGConnectionProfile> pair = Profiles.ElementAt(profileSelection.SelectedIndex);
                pair.Value.IsAuthenticated = false;
                
            }
            catch
            {

            }
            AuthenticationGroup.IsEnabled = false;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            
            SelectedProfile = Profiles.ElementAt(profileSelection.SelectedIndex).Value;

            if (SelectedProfile != Profiles["Most Recent"])
                Profiles["Most Recent"] = new MJPEGConnectionProfile("Most Recent", SelectedProfile.Address, SelectedProfile.IsAuthenticated, SelectedProfile.Username, SelectedProfile.Password);

            SaveProfiles();


            this.DialogResult = true;

        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            
        }

        private void usernameTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                KeyValuePair<string, MJPEGConnectionProfile> pair = Profiles.ElementAt(profileSelection.SelectedIndex);
                pair.Value.Username = usernameTextbox.Text;
            }
            catch
            {

            }
        }

        private void URLTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                KeyValuePair<string, MJPEGConnectionProfile> pair = Profiles.ElementAt(profileSelection.SelectedIndex);

                pair.Value.Address = URLTextbox.Text;
            }
            catch
            {

            }
        }

        private void passwordTextbox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                KeyValuePair<string, MJPEGConnectionProfile> pair = Profiles.ElementAt(profileSelection.SelectedIndex);
                pair.Value.Password = passwordTextbox.Password;
            }
            catch
            {

            }
        }
    }
}
