using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Devices.Enumeration;
using Windows.Storage;
using Windows.Media.MediaProperties;
using System.Diagnostics;
using Windows.System.Threading;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace BackgroundRecord
{
    public sealed class StartupTask : IBackgroundTask
    {
        MediaCapture _mediaCapture;
        MediaEncodingProfile profile;
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            // 
            // TODO: Insert code to perform background work
            //
            // If you start any asynchronous methods here, prevent the task
            // from closing prematurely by using BackgroundTaskDeferral as
            // described in http://aka.ms/backgroundtaskdeferral
            //
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();

            taskInstance.Canceled += TaskInstance_Canceled;
            taskInstance.Task.Completed += Task_Completed;
            var captureDevs = await DeviceInformation.FindAllAsync(DeviceClass.AudioCapture);

            if (captureDevs != null && captureDevs.Count > 0)
            {
                _mediaCapture = new MediaCapture();

                var audioDev = captureDevs.FirstOrDefault();
                MediaCaptureInitializationSettings settings = new MediaCaptureInitializationSettings()
                {
                    AudioDeviceId = audioDev.Id,
                    StreamingCaptureMode = StreamingCaptureMode.Audio
                };

                await _mediaCapture.InitializeAsync(settings);

                Debug.WriteLine("start new recording process");
                StorageFile file = await KnownFolders.MusicLibrary.CreateFileAsync("background.wav", CreationCollisionOption.GenerateUniqueName);
                profile = MediaEncodingProfile.CreateWav(AudioEncodingQuality.High);
                profile.Audio.ChannelCount = 1;
                profile.Audio.SampleRate = 16000;

                await _mediaCapture.StartRecordToStorageFileAsync(profile, file);

                //ThreadPoolTimer timer = ThreadPoolTimer.CreatePeriodicTimer(Time_Tick, TimeSpan.FromMinutes(5));

                Debug.WriteLine("record processing");

               // await Task.Factory.StartNew(async () =>
               //{
               //    //while (true)
               //    {
               //        try
               //        {

               //            Debug.WriteLine("start new recording process");
               //            StorageFile file = await KnownFolders.MusicLibrary.CreateFileAsync("background.wav", CreationCollisionOption.GenerateUniqueName);
               //            MediaEncodingProfile profile = MediaEncodingProfile.CreateWav(AudioEncodingQuality.High);
               //            profile.Audio.ChannelCount = 1;
               //            profile.Audio.SampleRate = 16000;

               //            await _mediaCapture.StartRecordToStorageFileAsync(profile, file);

               //            Debug.WriteLine("record processing");

               //        }
               //        catch(Exception ex)
               //        {
               //            Debug.WriteLine(ex.Message);
               //        }

               //    }
               //});
            }
            //deferral.Complete();
        }

        private async void Time_Tick(ThreadPoolTimer timer)
        {
            await _mediaCapture.StopRecordAsync();

            Debug.WriteLine("Record stopped");
            StorageFile file = await KnownFolders.MusicLibrary.CreateFileAsync("background.wav", CreationCollisionOption.GenerateUniqueName);

            Debug.WriteLine("start new recording process");
            await _mediaCapture.StartRecordToStorageFileAsync(profile, file);
        }

        private void Task_Completed(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {

        }

        private async void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            try
            {
                Debug.WriteLine("task canceled");
                await _mediaCapture.StopRecordAsync();
                Debug.WriteLine("Record stopped");
            }
            catch (Exception)
            {
            }
        }
    }
}
