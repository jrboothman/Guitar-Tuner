using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FftSharp;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Plugin.AudioRecorder;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Essentials;
using MathNet.Numerics.IntegralTransforms;
using System.Reflection;

namespace Guitar_Tuner.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DropDTuning : ContentPage
    {

        private readonly AudioPlayer audioPlayer = new AudioPlayer();

        double RecordedFrequency;
        double Min;
        double Max;
        bool StringSelected = false;

        public DropDTuning()
        {
            InitializeComponent();
        }

        protected async void LEButtonClicked(object sender, EventArgs e)
        {
            TuneInfoText.Text = "";
            Max = 146.8;
            Min = 146.2;
            StringSelected = true;
        }
        protected async void AButtonClicked(object sender, EventArgs e)
        {
            TuneInfoText.Text = "";
            Max = 164.8;
            Min = 164.2;
            StringSelected = true;
        }
        protected async void DButtonClicked(object sender, EventArgs e)
        {
            TuneInfoText.Text = "";
            Max = 291.8;
            Min = 291.2;
            StringSelected = true;
        }
        protected async void GButtonClicked(object sender, EventArgs e)
        {
            TuneInfoText.Text = "";
            Max = 97.8;
            Min = 97.2;
            StringSelected = true;
        }
        protected async void BButtonClicked(object sender, EventArgs e)
        {
            TuneInfoText.Text = "";
            Max = 123.8;
            Min = 123.2;
            StringSelected = true;
        }
        protected async void HEButtonClicked(object sender, EventArgs e)
        {
            TuneInfoText.Text = "";
            Max = 164.8;
            Min = 164.2;
            StringSelected = true;
        }

        protected async void TuneButtonClicked(object sender, EventArgs e)
        {
            if (StringSelected == false)
            {
                TuneInfoText.Text = "Select a String";
            }
            else
            {
                var audioProcessor = new AudioProcessor(audioPlayer);

                audioProcessor.StartRecording();

                // Wait for the recording to finish
                await Task.Delay(5500);

                // Get the latest smoothed frequency value
                double smoothedFrequency = audioProcessor.GetSmoothedFrequency();

                // Set the RecordedFrequency variable
                RecordedFrequency = smoothedFrequency;

                // Now you can use the RecordedFrequency variable as needed
                Console.WriteLine($"Recorded frequency: {RecordedFrequency} Hz");

                if (RecordedFrequency == 0)
                {
                    TuneInfoText.Text = "No Sound Detected";
                }
                else if (RecordedFrequency < Min)
                {
                    TuneInfoText.Text = "Tune Higher";
                }
                else if (RecordedFrequency > Max)
                {
                    TuneInfoText.Text = "Tune Lower";
                }
                else
                {
                    TuneInfoText.Text = "Tuned";
                }
            }
        }

        public class AudioProcessor
        {
            private AudioRecorderService audioRecorder;
            private Task recordingTask;
            private const int SampleRate = 44100;
            private Complex[] fftResult;
            private List<byte> audioBuffer;
            private AudioPlayer audioPlayer;

            private const double MinFrequency = 50.0;
            private const double MaxFrequency = 1319.0;
            private const int SmoothingWindowSize = 3;

            private Queue<double> smoothedFrequencies = new Queue<double>();

            public AudioProcessor(AudioPlayer Player)
            {
                audioBuffer = new List<byte>();
                audioRecorder = new AudioRecorderService
                {
                    StopRecordingOnSilence = false, // Keep recording until explicitly stopped
                    PreferredSampleRate = SampleRate,
                };
                audioPlayer = Player;

            }

            private const int MaxRecordingDurationInSeconds = 3; // Set a reasonable maximum duration

            public void StartRecording()
            {
                audioRecorder.AudioInputReceived += OnAudioInputReceived;

                recordingTask = audioRecorder.StartRecording();

                // Schedule a task to stop recording after the specified duration
                Task.Delay(TimeSpan.FromSeconds(MaxRecordingDurationInSeconds)).ContinueWith(_ => StopRecording());
            }

            public double GetSmoothedFrequency()
            {
                return smoothedFrequencies.Count > 0 ? smoothedFrequencies.Peek() : 0.0;
            }

            public void StopRecording()
            {
                // Stop the recording explicitly when needed
                audioRecorder.StopRecording();
            }

            private async void OnAudioInputReceived(object sender, string audioFilePath)
            {
                try
                {
                    Console.WriteLine($"Audio file path: {audioFilePath}");


                    // Verify if the file exists
                    if (File.Exists(audioFilePath))
                    {
                        // Load audio data from the file
                        var audioData = await LoadAudioDataFromFile(audioFilePath);

                        // Add the new audio data to the buffer
                        audioBuffer.AddRange(audioData);

                        // Process the buffer if it reaches a certain size
                        if (audioBuffer.Count >= SampleRate * 2) // Adjust the duration as needed
                        {
                            // Perform FFT on the audio data
                            try
                            {
                                fftResult = PerformFFT(audioBuffer.ToArray());

                                // Process the FFT result as needed
                                ProcessFFTResult(fftResult);
                            }
                            catch (Exception fftException)
                            {
                                Console.WriteLine($"FFT Error: {fftException.Message}");
                            }

                            // Clear the buffer after processing
                            audioBuffer.Clear();
                        }
                    }
                    else
                    {
                        Console.WriteLine("File not found.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
            public async Task<byte[]> LoadAudioDataFromFile(string filePath)
            {
                try
                {
                    // Check if the file exists
                    if (File.Exists(filePath))
                    {
                        // Read the audio file into a byte array
                        byte[] audioData;
                        using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                        {
                            audioData = new byte[fileStream.Length];
                            await fileStream.ReadAsync(audioData, 0, (int)fileStream.Length);
                        }
                        return audioData;
                    }
                    else
                    {
                        Console.WriteLine("File not found (audio).");
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    // Handle exceptions
                    Console.WriteLine($"Error loading audio file: {ex.Message}");
                    return null;
                }
            }


            private Complex[] PerformFFT(byte[] audioData)
            {
                try
                {
                    // Find the nearest or next power of 2 for the length of audioData
                    int fftLength = 1;
                    while (fftLength < audioData.Length)
                    {
                        fftLength *= 2;
                    }

                    // Resize audioData to the new length
                    Array.Resize(ref audioData, fftLength);

                    var complexAudioData = audioData.Select(b => new Complex(b, 0)).ToArray();

                    // Apply windowing function
                    var window = FftSharp.Window.Hanning(complexAudioData.Length);
                    for (int i = 0; i < complexAudioData.Length; i++)
                    {
                        complexAudioData[i] *= window[i];
                    }

                    // Perform FFT using FftSharp
                    FftSharp.Transform.FFT(complexAudioData);

                    Console.WriteLine("FFT Result: " + string.Join(", ", complexAudioData.Select(c => c.Magnitude)));

                    return complexAudioData;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"FFT Error: {ex.Message}");
                    throw; // Re-throw the exception to propagate it to the caller
                }
            }

            private void ProcessFFTResult(Complex[] fftResult)
            {
                // Get the fundamental frequency information
                var fundamentalFrequencyInfo = GetFundamentalFrequencyInfo(fftResult);

                if (fundamentalFrequencyInfo.HasValue)
                {
                    double sampleRate = SampleRate;
                    int maxIndex = fundamentalFrequencyInfo.Value;

                    // Calculate the frequency using the correct scaling
                    double frequency = maxIndex * sampleRate / fftResult.Length;

                    // Filter out frequencies outside the valid range
                    if (frequency >= MinFrequency && frequency <= MaxFrequency)
                    {
                        // Apply smoothing
                        double smoothedFrequency = SmoothFrequency(frequency);


                        Console.WriteLine($"unsmoothed frequency: {frequency} Hz");
                        Console.WriteLine($"Detected frequency: {smoothedFrequency} Hz");

                        string audioFilePath = audioRecorder.GetAudioFilePath();



                        // Play audio file after frequency detection
                        PlayAudioFile(audioFilePath);
                    }
                }
                else
                {
                    Console.WriteLine("No valid fundamental frequency found.");
                }
            }

            private void PlayAudioFile(string filePath)
            {
                if (File.Exists(filePath))
                {
                    audioPlayer.Play(filePath);
                }
                else
                {
                    Console.WriteLine("Audio file not found for playback.");
                }
            }

            private double SmoothFrequency(double frequency)
            {
                smoothedFrequencies.Enqueue(frequency);

                // Keep the window size fixed
                while (smoothedFrequencies.Count > SmoothingWindowSize)
                {
                    smoothedFrequencies.Dequeue();
                }

                // Calculate the average of the frequencies in the window
                double averageFrequency = smoothedFrequencies.Average();

                return averageFrequency;
            }

            private int? GetFundamentalFrequencyInfo(Complex[] fftResult)
            {
                int windowSize = 512; // Adjust the window size as needed
                double magnitudeThreshold = 5.0; // Adjust the magnitude threshold as needed

                // Convert the FFT result to magnitudes
                double[] magnitudes = fftResult.Select(c => c.Magnitude).ToArray();

                // Apply a windowing function to the magnitudes
                double[] window = MathNet.Numerics.Window.Hann(magnitudes.Length).ToArray();
                for (int i = 0; i < magnitudes.Length; i++)
                {
                    magnitudes[i] *= window[i];
                }

                // Find the index of the maximum magnitude within the specified frequency range
                int maxIndex = GetMaxMagnitudeIndex(magnitudes, windowSize, magnitudeThreshold);

                // If a valid peak is found, convert the index to a frequency bin and return it
                if (maxIndex != -1)
                {
                    // Correct the frequency calculation
                    int frequencyBin = maxIndex;
                    if (frequencyBin > fftResult.Length / 2)
                    {
                        frequencyBin = fftResult.Length - frequencyBin;
                    }
                    return frequencyBin; // Return frequency bin instead of frequency
                }
                else
                {
                    return null; // If no valid peaks are found, return null
                }
            }

            private int GetMaxMagnitudeIndex(double[] magnitudes, int windowSize, double magnitudeThreshold)
            {
                for (int i = windowSize; i < magnitudes.Length - windowSize; i++)
                {
                    bool isMaxima = true;

                    // Check if the magnitude at index i is a local maximum
                    for (int j = i - windowSize; j <= i + windowSize; j++)
                    {
                        if (magnitudes[i] < magnitudes[j])
                        {
                            isMaxima = false;
                            break;
                        }
                    }

                    // If the magnitude is a local maximum and above the threshold, return its index
                    if (isMaxima && magnitudes[i] > magnitudeThreshold)
                    {
                        return i;
                    }
                }

                return -1; // If no valid peaks are found, return -1
            }


        }
    }
}