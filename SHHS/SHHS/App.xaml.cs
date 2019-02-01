﻿using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using SHHS.View;
using SHHS.Controller;
using System.Globalization;
using System.Threading;

using SHHS.Model;
using Firebase.Database;
using Firebase.Auth;
using Plugin.DeviceInfo;
using System.Threading.Tasks;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace SHHS
{
    public partial class App : Application
    {

        public MainPage shhsMain;
        public CalenderPage shhsCalender;
        public SHHSEventManager shhsEventManager;
        public SettingPage shhsSetting;
        private const string notificationKey = "Notification";
        private const string soundKey = "Sound";
        private const string minutesKey = "Minutes";
        private const string UIDkey = "UID";
        private const string loginClient = "LoginToken";
        private const string loginToken = "FBToken";

        public string VersionNumber { get; set; }

        public string BuildNumber { get; set; }
        public bool isAdmin { get; set; }

        public App()
        {
            InitializeComponent();

            
            //Init Culture
            var userSelectedCulture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = userSelectedCulture;

            //Set Version
            VersionNumber = CrossDeviceInfo.Current.AppVersion;
            BuildNumber = CrossDeviceInfo.Current.AppBuild;


            //Init Page
            shhsMain = new MainPage();
            MainPage = new NavigationPage(shhsMain);
            shhsEventManager = new SHHSEventManager();
            Current = this;
            shhsSetting =  new SettingPage { Title = "Setting", Icon = "setting.png", BackgroundColor = Color.FromHex("#EFFACB") };









        }

        override protected async void OnStart()
        {
            // Handle when your app starts

            //If sign in fails

            var signInResults = await SHHSFirebaseLogin.SignIn();


            if (signInResults != null && signInResults.Equals(AuthErrorReason.UserDisabled.ToString())){
                await shhsMain.DisplayAlert(signInResults,"Your account has been disabled, default schedule has been set, please contact southhillscode@gmail.com for assistance", "Ok");
                shhsMain.scheduleManager.LocalJson();
                shhsMain.RefreshSchedule();
                
            } else if (signInResults != null && signInResults.Equals(AuthErrorReason.Undefined.ToString())) {
                await shhsMain.DisplayAlert(signInResults, "check your internet connection, default schedule has been set, you will not be able to get updated news or schedule", "Ok");
                shhsMain.scheduleManager.LocalJson();
                shhsMain.RefreshSchedule();

            }
            else if (signInResults != null)
            {
                var signuPResults = await SHHSFirebaseLogin.SignUp();

                if(signuPResults!= null) {
                    await shhsMain.DisplayAlert(signInResults, "An error has occured authenticating, default schedule has been set, you will not be able to get updated news or schedule", "Ok");
                    shhsMain.scheduleManager.LocalJson();
                    shhsMain.RefreshSchedule();

                }
            }


              
            



            //Gets all events from Firebase, and refresh their remaining time
            await shhsEventManager.InitalizeEventTable();
            shhsCalender = new CalenderPage { Title = "Calendar", Icon = "calendaricon.png" };
            //Must initalize eventManager before calender page
            shhsMain.Children.Add(shhsCalender);
            shhsMain.Children.Add(shhsSetting);
            await shhsEventManager.RefreshEvent();
            shhsCalender.SetDataSource(shhsEventManager.events);
            await shhsMain.ReInitMainPage();





        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }


        override protected async void OnResume()
        {
            // Handle when your app resumes
            shhsMain.RefreshSchedule();
            await shhsEventManager.RefreshEvent();
        }

        public FirebaseClient Client
        {
            get
            {
                if(AuthLink != null)
                    return new FirebaseClient("https://shhs-45632.firebaseio.com/", new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(AuthLink.FirebaseToken)});

         return null;
            }

                
            }




        public FirebaseAuthLink AuthLink { get; set; }

        public string FirbaseToken
        {
            get
            {
                if (Properties.ContainsKey(loginToken))
                    return (string)Properties[loginToken];

                return null;
            }
            set
            {
                Properties[loginToken] = value;
            }
        }
                
                
                
                
                
                
                
                public string UID
        {
            get
            {
                if (Properties.ContainsKey(UIDkey))
                    return (string)Properties[UIDkey];
                return null;
            }
            set
            {
                Properties[UIDkey] = value;
            }
        }




        public bool NotificationEnabled { 
            get {
                if (Properties.ContainsKey(notificationKey))
                    return (bool)Properties[notificationKey];
                return false;
            }
            set {
                Properties[notificationKey] = value; 
                if(shhsSetting!= null)
                shhsMain.scheduleManager.PushLocalNotifications();
            }
        }


        public bool SoundEnabled
        {

            get
            {

                if (Properties.ContainsKey(soundKey))
                    return (bool)Properties[soundKey];
                return false;

            }

            set
            {

                Properties[soundKey] = value;
                 if(shhsSetting!= null)
                shhsMain.scheduleManager.PushLocalNotifications();

            }


        }

        public string MinutesToSendNotification
        {

            get
            {

                if (Properties.ContainsKey(minutesKey))
                    return Properties[minutesKey].ToString();

                return "2";
            }

            set
            {

                Properties[minutesKey] = value;

                if(shhsSetting!= null)
                shhsMain.scheduleManager.PushLocalNotifications();
            }


        }
    }
}
