using System;
using System.IO;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using NAudio.CoreAudioApi;
using static System.Net.Mime.MediaTypeNames;

class Program
{
    // This example requires environment variables named "SPEECH_KEY" and "SPEECH_REGION"
    static string speechKey = "f837b896af1646f58390f977edcfeed4";
    static string speechRegion = "northeurope";

    // static void OutputSpeechSynthesisResult(SpeechSynthesisResult speechSynthesisResult, string recognizedText)
    //{
    //    switch (speechSynthesisResult.Reason)
    //    {
    //        case ResultReason.SynthesizingAudioCompleted:
    //            Console.WriteLine($"Speech synthesized for text: [{recognizedText}]");
    //            break;
    //        case ResultReason.Canceled:
    //            var cancellation = SpeechSynthesisCancellationDetails.FromResult(speechSynthesisResult);
    //            Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

    //            if (cancellation.Reason == CancellationReason.Error)
    //            {
    //                Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
    //                Console.WriteLine($"CANCELED: ErrorDetails=[{cancellation.ErrorDetails}]");
    //                Console.WriteLine($"CANCELED: Did you set the speech resource key and region values?");
    //            }
    //            break;
    //        default:
    //            break;
    //    }
    //}

    async static Task SpeakWhileEar(string whatHasUserSpoken) // = what has user spoken
    {
        using var enumerator = new NAudio.CoreAudioApi.MMDeviceEnumerator();
        var commDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications);
       
        var speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
        speechConfig.SpeechSynthesisVoiceName = "en-US-JennyNeural";
        using (var speechSynthesizer = new Microsoft.CognitiveServices.Speech.SpeechSynthesizer(speechConfig))
        {
            commDevice.AudioEndpointVolume.Mute = true;
            var speechSynthesisResult = await speechSynthesizer.SpeakTextAsync(whatHasUserSpoken);
            commDevice.AudioEndpointVolume.Mute = false;
        }
    }


    async static Task Main(string[] args)
    {
        var speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
        speechConfig.SpeechRecognitionLanguage = "en-US";

        using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
        using  var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);
        var stopRecognition = new TaskCompletionSource<int>();
        speechConfig.SpeechSynthesisVoiceName = "en-US-JennyNeural";

        // Handler for the Recognized event, which is raised when the speech recognizer transcribes speech
        speechRecognizer.Recognized += async (s, whatHasUserSpoken) =>
        {
            if (whatHasUserSpoken.Result.Reason == ResultReason.RecognizedSpeech)
            {
                Console.WriteLine($"Recognized: {whatHasUserSpoken.Result.Text}");
                await SpeakWhileEar(whatHasUserSpoken.Result.Text);
                Console.WriteLine("Speak something >");
            }     
        };



        // Handler for the Canceled event, which is raised when the speech recognition is canceled
        speechRecognizer.Canceled += (s, e) =>
        {
            Console.WriteLine($"CANCELED: Reason={e.Reason}");

            if (e.Reason == CancellationReason.Error)
            {
                Console.WriteLine($"CANCELED: ErrorCode={e.ErrorCode}");
                Console.WriteLine($"CANCELED: ErrorDetails={e.ErrorDetails}");
                Console.WriteLine($"CANCELED: Did you set the speech resource key and region values?");
            }

            stopRecognition.TrySetResult(0);
        };

        // Handler for the SessionStopped event, which is raised when the speech recognition session is stopped
        speechRecognizer.SessionStopped += (s, e) =>
        {
            Console.WriteLine("\n    Session stopped event.");
            stopRecognition.TrySetResult(0);
        };

         await speechRecognizer.StartContinuousRecognitionAsync();

        // Get text from the console and synthesize to the default speaker.
        Console.WriteLine("Speak something >");

        // Waits for completion. Use Task.WaitAny to keep the task rooted.
        Task.WaitAny(new[] { stopRecognition.Task });
    }
}
        // Synthesize
