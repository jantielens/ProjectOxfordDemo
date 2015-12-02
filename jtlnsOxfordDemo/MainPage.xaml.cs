using Microsoft.ProjectOxford.Emotion;
using Microsoft.ProjectOxford.Face;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace jtlnsOxfordDemo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        MediaCapture mc;
        EmotionServiceClient OxfordEmo = new EmotionServiceClient("YOURKEY");
        FaceServiceClient OxfordFace = new FaceServiceClient("YOURKEY");
        public MainPage()
        {
            this.InitializeComponent();
            wv.Navigate(new Uri("http://www.spam.com"));
            InitCamera();
        }

        async void InitCamera()
        {
            mc = new MediaCapture();
            var cameras = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
            var camera = cameras.First();
            var settings = new MediaCaptureInitializationSettings() { VideoDeviceId = camera.Id };
            await mc.InitializeAsync(settings);
            ce.Source = mc;
            await mc.StartPreviewAsync();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var ms = new MemoryStream();
            var ms2 = new MemoryStream();
            await mc.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), ms.AsRandomAccessStream());
            await mc.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), ms2.AsRandomAccessStream());
            ms.Position = 0L;
            ms2.Position = 0L;

            var detectedEmotions = await OxfordEmo.RecognizeAsync(ms2);
            if (detectedEmotions != null && detectedEmotions.Length > 0)
            {
                var Face = detectedEmotions[0];
                var s = Face.Scores;
                gAnger.Value = s.Anger;
                gContempt.Value = s.Contempt;
                gDisgust.Value = s.Disgust;
                gFear.Value = s.Fear;
                gHappiness.Value = s.Happiness;
                gNeutral.Value = s.Neutral;
                gSadness.Value = s.Sadness;
                gSurprise.Value = s.Surprise;
            }

            var detectedFaces = await OxfordFace.DetectAsync(ms, false, true, true, false);
            if (detectedFaces.Length > 0 && detectedEmotions.Length > 0)
            {
                tbAge.Text = detectedFaces[0].Attributes.Age.ToString();
                tbGender.Text = detectedFaces[0].Attributes.Gender;

                if (detectedFaces[0].Attributes.Gender == "female")
                {
                    if (detectedFaces[0].Attributes.Age < 25)
                        wv.Navigate(new Uri("https://www.jbc.be/nl/k3-zoekt-k3?utm_source=websitestudio100&utm_medium=banner&utm_term=k3%20kleedje&utm_content=iconisch%20k3%20jurkje&utm_campaign=studio100k33"));
                    else
                        wv.Navigate(new Uri("http://www.amazon.com/Girls-Think-Everything-Ingenious-Inventions/dp/0618195637/ref=sr_1_2?ie=UTF8&qid=1448372694&sr=8-2&keywords=female+scientist"));
                }
                else
                {
                    if (detectedEmotions[0].Scores.Surprise < .4)
                        wv.Navigate(new Uri("http://www.microsoft.com/surface/en-us/devices/surface-pro-4?ocid=OCTEVENT_MSCOM"));
                    else
                        wv.Navigate(new Uri("http://www.microsoft.com/surface/en-us/devices/surface-book"));
                }
            }
            else
            {
                gAnger.Value = .5;
                gContempt.Value = .5;
                gDisgust.Value = .5;
                gDisgust.Value = .5;
                gHappiness.Value = .5;
                gNeutral.Value = .5;
                gSadness.Value = .5;
                gSurprise.Value = .5;
                tbAge.Text = "?";
                tbGender.Text = "?";
                wv.Navigate(new Uri("http://www.spam.com"));
            }
        }
    }
}
