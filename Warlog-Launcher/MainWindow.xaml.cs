using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using Warlog_Launcher;

namespace Warlog_Launcher
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        WebClient webClient;               // Our WebClient that will be doing the downloading for us
        Stopwatch sw = new Stopwatch();

        public MainWindow()
        {
            InitializeComponent();

            textureCheck.IsChecked = Properties.Settings.Default.textureHD;

            //Condition lors du premier lancement du launcher afin de savoir où ce situe le jeu
            if (Properties.Settings.Default.game_location == "none")
            {
                // Create OpenFileDialog 
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();



                // Set filter for file extension and default file extension 
                dlg.DefaultExt = "wow.exe";
                dlg.Filter = "World Of Warcraft|wow.exe";


                // Display OpenFileDialog by calling ShowDialog method 
                Nullable<bool> result = dlg.ShowDialog();

                if(result == true)
                {
                    // Open document 
                    string filename = dlg.FileName;

                    string[] fileSplit = filename.Split('\\');
                    int length = fileSplit.Length;

                    string test = filename.Replace(fileSplit[length - 1], "");

                    Properties.Settings.Default.game_location = test;
                    Properties.Settings.Default.Save();
                }
            }

            //Vérification des empreintes MD5

            //Création du dossier s'il n'existe pas
            if(Directory.Exists(Environment.ExpandEnvironmentVariables("%appdata%/WarlogLauncher").ToString()) == false)
            {
                Directory.CreateDirectory(Environment.ExpandEnvironmentVariables("%appdata%/WarlogLauncher/").ToString());
            }

            webClient = new WebClient();
            //Téléchargement du patch
            webClient.DownloadFile(Properties.Settings.Default.download_location+"patch.json", Environment.ExpandEnvironmentVariables("%appdata%/WarlogLauncher/").ToString()+"patch.json");

            //Lecture du patch
            using(StreamReader reader = new StreamReader(Environment.ExpandEnvironmentVariables("%appdata%/WarlogLauncher/").ToString() + "patch.json"))
            {
                string content = reader.ReadToEnd();
                //JSON -> MODEL
                List<Warlog_Launcher_Model> _Model = JsonConvert.DeserializeObject<List<Warlog_Launcher_Model>>(content);
                
                foreach(Warlog_Launcher_Model param in _Model)
                {
                    var test = Properties.Settings.Default.game_location + "Data/" + param.filename;

                    //Vérifie si le fichier
                    if (File.Exists(Properties.Settings.Default.game_location + "Data/"+param.filename))
                    {
                        //Lecture de l'empreinte SHA1 du fichier local
                        FileStream fileStream = File.OpenRead(@Properties.Settings.Default.game_location + "Data/" + param.filename);
                        string[] chksum = BitConverter.ToString(SHA1.Create().ComputeHash(fileStream)).Split('-');
                        string restOfArray = string.Join("", chksum);
                        fileStream.Close();
                        //Est différent de celui sur le serveur ? 
                        if (restOfArray.ToLower() != param.sha1.ToLower())
                        {
                            //débute le téléchargement
                            webClient.DownloadFile(Properties.Settings.Default.download_location + param.filename, Properties.Settings.Default.game_location + "Data/" + param.filename);
                        } 
                    } else
                    {
                        //Télécharger le fichier
                        webClient.DownloadFile(Properties.Settings.Default.download_location + param.filename, Properties.Settings.Default.game_location + "Data/" + param.filename);
                    }
                }

                reader.Close();
            }



            location.Content = Properties.Settings.Default.game_location;
        }

        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();



            // Set filter for file extension and default file extension 
            dlg.DefaultExt = "wow.exe";
            dlg.Filter = "World Of Warcraft|wow.exe";


            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();


            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;

                string[] fileSplit = filename.Split('\\');
                int length = fileSplit.Length;

                string test = filename.Replace(fileSplit[length - 1], "");

                Properties.Settings.Default.game_location = test;
                Properties.Settings.Default.Save();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (textureCheck.IsChecked == true)
            {


                webClient = new WebClient();
                webClient.DownloadFile(Properties.Settings.Default.download_location + "textureHD.json", Environment.ExpandEnvironmentVariables("%appdata%/WarlogLauncher/").ToString() + "textureHD.json");

                using (StreamReader reader = new StreamReader(Environment.ExpandEnvironmentVariables("%appdata%/WarlogLauncher/").ToString() + "textureHD.json"))
                {
                    string content = reader.ReadToEnd();
                    //JSON -> MODEL
                    List<Warlog_Launcher_Model> _Model = JsonConvert.DeserializeObject<List<Warlog_Launcher_Model>>(content);

                    if(_Model.Count == 0)
                    {
                        //Lance le jeu
                        Process.Start(Properties.Settings.Default.game_location + "Wow.exe");
                    }

                    foreach (Warlog_Launcher_Model param in _Model)
                    {
                        var test = Properties.Settings.Default.game_location + "Data/" + param.filename;

                        //Vérifie si le fichier
                        if (File.Exists(Properties.Settings.Default.game_location + "Data/" + param.filename))
                        {
                            //Lecture de l'empreinte SHA1 du fichier local
                            FileStream fileStream = File.OpenRead(@Properties.Settings.Default.game_location + "Data/" + param.filename);
                            string[] chksum = BitConverter.ToString(SHA1.Create().ComputeHash(fileStream)).Split('-');
                            string restOfArray = string.Join("", chksum);
                            fileStream.Close();
                            //Est différent de celui sur le serveur ? 
                            if (restOfArray.ToLower() != param.sha1.ToLower())
                            {
                                //débute le téléchargement
                                using (webClient = new WebClient())
                                {
                                    webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                                    webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                                    string urlAddress = Properties.Settings.Default.download_location + param.filename;
                                    // The variable that will be holding the url address (making sure it starts with http://)
                                    Uri URL = urlAddress.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ? new Uri(urlAddress) : new Uri("http://" + urlAddress);

                                    // Start the stopwatch which we will be using to calculate the download speed
                                    sw.Start();

                                    try
                                    {
                                        // Start downloading the file
                                        webClient.DownloadFileAsync(URL, Properties.Settings.Default.game_location + "Data/" + param.filename);
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show(ex.Message);
                                    }
                                }
                            }
                        }
                        else
                        {
                            //débute le téléchargement
                            using (webClient = new WebClient())
                            {
                                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                                string urlAddress = Properties.Settings.Default.download_location + param.filename;
                                // The variable that will be holding the url address (making sure it starts with http://)
                                Uri URL = urlAddress.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ? new Uri(urlAddress) : new Uri("http://" + urlAddress);

                                // Start the stopwatch which we will be using to calculate the download speed
                                sw.Start();

                                try
                                {
                                    // Start downloading the file
                                    webClient.DownloadFileAsync(URL, Properties.Settings.Default.game_location + "Data/" + param.filename);
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(ex.Message);
                                }
                            }
                        }
                    }

                    reader.Close();
                }
            } else
            {
                //Lance le jeu
                Process.Start(Properties.Settings.Default.game_location + "Wow.exe");
            }
        }

        // The event that will fire whenever the progress of the WebClient is changed
        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            // Calculate download speed and output it to labelSpeed.
            speedConnexion.Content = string.Format("{0} Mo/s", ((e.BytesReceived / 1024d / sw.Elapsed.TotalSeconds)/1000).ToString("0.00"));

            // Update the progressbar percentage only when the value is not the same.
            progressBar.Value = e.ProgressPercentage;

            // Show the percentage on our label.
            percent.Content = e.ProgressPercentage.ToString() + "%";
        }


        // The event that will trigger when the WebClient is completed
        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            // Reset the stopwatch.
            sw.Reset();

            if (e.Cancelled == true)
            {
                MessageBox.Show("Error");
            }

            //Lancer le jeu
            Process.Start(Properties.Settings.Default.game_location+"Wow.exe");
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.textureHD = true;
            Properties.Settings.Default.Save();
        }

        private void CheckBox_UnChecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.textureHD = false;
            Properties.Settings.Default.Save();
        }
    }
}
