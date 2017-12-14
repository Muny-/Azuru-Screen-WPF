using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
    public partial class IPProfileDialog : Window
    {
        public Dictionary<string, IPConnectionProfile> Profiles = new Dictionary<string, IPConnectionProfile>();

        public IPConnectionProfile SelectedProfile;

        public IPProfileDialog()
        {
            InitializeComponent();

            profileSelection.SelectedIndex = 0;

            LoadProfiles();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (profileSelection.SelectedIndex != -1)
            {
                IPConnectionProfile prof = Profiles[profileSelection.SelectedValue.ToString()];

                AddressTextbox.Text = prof.Address.ToString();
                portBox.Value = prof.Port;
            }
        }

        void UpdateProfileList()
        {
            profileSelection.Items.Clear();

            foreach (KeyValuePair<string, IPConnectionProfile> pair in Profiles)
            {
                profileSelection.Items.Add(pair.Key);
            }
        }

        static string ConfigurationFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Azuru Sharing Utility/";
        static string ConfigurationFile = ConfigurationFolder + "ipprofiles.json";

        void CheckConfigFile()
        {
            if (Directory.Exists(ConfigurationFolder))
            {
                if (File.Exists(ConfigurationFile))
                {

                }
                else
                {
                    File.WriteAllText(ConfigurationFile, "{\"Most Recent\": " + new IPConnectionProfile("Most Recent", "127.0.0.1", 8076).ToJSON() + "}");
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
                
                Profiles.Add(pair.Value.name, new IPConnectionProfile(pair.Value.name, pair.Value.address, (int)pair.Value.port));
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

                KeyValuePair<string, IPConnectionProfile> pair = Profiles.ElementAt(i);

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

                Profiles.Add(ProfileNameTextbox.Text, new IPConnectionProfile(ProfileNameTextbox.Text, "", 8076));

                //URLTextbox.Text = "";
                AddressTextbox.Text = "";
                portBox.Value = 8076;

                ProfileNameTextbox.Text = "";

                UpdateProfileList();

                profileSelection.SelectedIndex = profileSelection.Items.Count - 1;
            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                NewProfileGrid.Visibility = System.Windows.Visibility.Hidden;
                ProfileNameTextbox.Text = "";
            }

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

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            
            SelectedProfile = Profiles.ElementAt(profileSelection.SelectedIndex).Value;

            if (SelectedProfile != Profiles["Most Recent"])
                Profiles["Most Recent"] = new IPConnectionProfile("Most Recent", SelectedProfile.Address, SelectedProfile.Port);

            SaveProfiles();


            this.DialogResult = true;

        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            
        }

        private void AddressTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                KeyValuePair<string, IPConnectionProfile> pair = Profiles.ElementAt(profileSelection.SelectedIndex);
                pair.Value.Address = AddressTextbox.Text;
            }
            catch
            {

            }
        }

        private void portBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                KeyValuePair<string, IPConnectionProfile> pair = Profiles.ElementAt(profileSelection.SelectedIndex);
                pair.Value.Port = (int)portBox.Value;
            }
            catch
            {

            }
        }
    }
}
