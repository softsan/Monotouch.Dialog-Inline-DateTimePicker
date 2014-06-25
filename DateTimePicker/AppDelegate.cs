namespace DateTimePicker
{
    #region Namespace
    using DateTimePicker.Controls;
    
    using MonoTouch.Dialog;
    using MonoTouch.Foundation;
    using MonoTouch.UIKit;
    using System;
    #endregion 

    /// <summary>
    /// The UIApplicationDelegate for the application. This class is responsible for launching the
    /// User Interface of the application, as well as listening (and optionally responding) to 
    /// application events from iOS.
    /// </summary>
    [Register("AppDelegate")]
    public partial class AppDelegate : UIApplicationDelegate
    {
        /// <summary>
        /// class-level declarations 
        /// </summary>
        private UIWindow window;

        /// <summary>
        /// Navigation controller
        /// </summary>
        private UINavigationController navigationController;

        /// <summary>
        /// This method is invoked when the application has loaded and is ready to run. In this 
        ///  method you should instantiate the window, load the UI into it and then make the window
        /// visible.
        /// </summary>
        /// <param name="app">object of UIApplication</param>
        /// <param name="options">NSDictionary object</param>
        /// <returns>True if windows loaded successfully</returns>
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            // create a new window instance based on the screen size
            this.window = new UIWindow(UIScreen.MainScreen.Bounds);

            DialogViewController dialogViewController = new DialogViewController(this.GenerateData());
            this.navigationController = new UINavigationController(dialogViewController);

            // If you have defined a view, add it here:
            this.window.RootViewController = this.navigationController;

            // make the window visible
            this.window.MakeKeyAndVisible();

            return true;
        }

        /// <summary>
        /// Generate form 
        /// </summary>
        /// <returns>Monotouch dialog root element with added section </returns>
        private RootElement GenerateData()
        {
            RootElement root = new RootElement("Date time demo");
            Section section = new Section()
                        {
                            new InlineDateTimePicker("Date only", DateTime.Today, UIDatePickerMode.Date),              
                            new InlineDateTimePicker("Date and Time", DateTime.Now, UIDatePickerMode.DateAndTime),
                            new InlineDateTimePicker("Time only", DateTime.Now, UIDatePickerMode.Time)
                        };
            
            root.Add(section);

            // ****  Important -> This line is very important to open the datetimelement inline.
            root.UnevenRows = true; 
            return root;
        }
    }
}