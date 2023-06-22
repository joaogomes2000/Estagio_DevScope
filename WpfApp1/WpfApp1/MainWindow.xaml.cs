using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Intent;
using System.Text.Json;
using System.Text.RegularExpressions;
using NAudio.CoreAudioApi;
using System.IO;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Globalization;
using WpfApp1.Contructor;
using System.Windows.Forms;
using static WpfApp1.MainWindow;
using static WpfApp1.Contructor.constructor;
using System.Net.Http;
using System.Diagnostics.CodeAnalysis;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {       
        static string languageKey = "399cdcf52ced406aa740500f8be2bef8";
        static string languageEndpoint = "https://joaogomesluis.cognitiveservices.azure.com/";
        static string speechKey = "3335277541e047589ad1f59ea420b924";
        static string speechRegion = "westeurope";
        public string? roomcategory = null;
        public string? dayText = null;
        public string? hourText = null;
        public string? monthtext = null;
        public DateTime now = DateTime.Now;
        public Dictionary<string, string> daysDictionary = new Dictionary<string, string>();
            
        public int timespent = 0;
        // Your CLU project name and deployment name.
        static string cluProjectName = "testeLUIS2";
        static string cluDeploymentName = "LUISTESTE";
        string filePath = @"C:\Users\joao_\OneDrive\Ambiente de Trabalho\Estágio_DevScope\WpfApp1\jsonviewer.json";
        public int id = 0;

        private readonly IntentRecognizer intentRecognizer;
        private bool isRecognizing;

        public SpeechConfig config = SpeechConfig.FromSubscription(speechKey, speechRegion);

        public static DateTimeOffset GetData(long data)
        {
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(data).AddHours(1);
            DateTime dateTime = dateTimeOffset.UtcDateTime;

            return dateTime;
        }
        public static async void GetDisiredRoom(string filePath, int ID ,string day, string month, string year, string hour,int timespent ,SpeechConfig config)
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);

                List<MeetingRoom> rooms = JsonConvert.DeserializeObject<List<MeetingRoom>>(json);
                for (int i = 0; i < rooms.Count; i++)
                {

                    rooms[i].id = i + 1;

                 
                }
             
                MeetingRoom room = rooms.FirstOrDefault(r => r.id == ID);
                if (room != null)
                {
                    if (DateTimeOffset.TryParseExact($"{day}/{month}/{year} {hour}", "d/MMMM/yyyy h:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset dateTimeOffset))
                    {
                        System.Windows.MessageBox.Show(dateTimeOffset.ToUnixTimeMilliseconds().ToString());
                        Appointment apoint = new Appointment
                        {
                            Subject = "João Gomes",
                            Organizer = "João Gomes",
                            Start = dateTimeOffset.ToUnixTimeMilliseconds(),  // DateTimeOffset.ParseExact("27/5/2023 02:28", "d/M/yyyy HH:mm ", CultureInfo.InvariantCulture).ToUnixTimeMilliseconds(),
                            End = dateTimeOffset.AddHours(timespent).ToUnixTimeMilliseconds(),
                            Private = false
                        };
                        //room.Appointments.Insert(0,apoint);
                        room.Appointments.Add(apoint);
                        string updatejson = JsonConvert.SerializeObject(rooms, Formatting.Indented);
                        File.WriteAllText(filePath, updatejson);
                      
                            SpeakWhatUserHasSopken($"The room has been booked for {day} of {month} at {year}.", config);
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Não deu");
                    }
                }
                    else
                    {
                       // MessageBox.Show("Não deu");
                       Console.WriteLine("Sala encontradas:");
                    }
            }
            else
            {
               // MessageBox.Show("Não deu");
                Console.WriteLine("O arquivo não existe: " + filePath);
            }
        }



        public static List<object> GetDisiredRoomData(string filePath, string day , string month, string year, string hour  )
        {
           
            List<object> items = new List<object>();
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);

                List<MeetingRoom> rooms = JsonConvert.DeserializeObject<List<MeetingRoom>>(json);
                    // int count = 0;
                for (int i = 0; i < rooms.Count; i++)
                {
                   
                    rooms[i].id = i + 1;
                }
              
                List<MeetingRoom> room = rooms.Where(r => r.Appointments.Exists(a => GetData(a.Start).Day.ToString("d") == day && GetData(a.Start).ToString("MMMM").ToLower() == month.ToLower() 
                && GetData(a.Start).ToString("yyyy") == year && GetData(a.Start).ToString("h:mm tt") == hour
                ) ).ToList();
                List<MeetingRoom> avaliableroom = rooms.Except(room).ToList();
              
                if (room.Count >= 1)
                {

                    Console.WriteLine("Sala ocupadas:");
                }
                if (avaliableroom.Count > 0)
                {
                    Console.WriteLine($"Sala disponiveis para o dia {day}:");
                    foreach (MeetingRoom findrooms in avaliableroom)
                    {
                        if(findrooms.Appointments.Count > 0)
                        {
                            foreach (var app in findrooms.Appointments)
                            {
                                var combinedData = new
                                {
                                    RoomId = findrooms.id,
                                    RoomName = findrooms.Name,
                                    app.Organizer,
                                    StartHour = GetData(app.Start).ToString("hh:mm tt"),
                                    StartDay = GetData(app.Start).ToString("dd/MMMM/yyyy"),
                                    EndHour = GetData(app.End).ToString("hh:mm tt"),
                                    EndDay = GetData(app.End).ToString("dd/MMMM/yyyy"),
                                    Busy = findrooms.Busy.ToString()
                                };
                                items.Add(combinedData);

                            }
                        }
                        else
                        {
                            var combinedData = new
                            {
                                RoomId = findrooms.id,
                                RoomName = findrooms.Name,
                                Organizer = "",
                                StartHour = "",
                                StartDay = "",
                                EndHour = "",
                                EndDay = "",
                                Busy = findrooms.Busy.ToString()
                            };
                            items.Add(combinedData);
                        }

                        }
                }
            }
                return items;
        }
        public static void MicrophoneMute(bool muted, NAudio.CoreAudioApi.MMDeviceEnumerator microphoneMute)
        {
            var commDevice = microphoneMute.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications);
            commDevice.AudioEndpointVolume.Mute = muted;
        }

        async static Task SpeakWhatUserHasSopken(string whatHasUserSpoken, SpeechConfig speechConfig) // = what has user spoken
        {
            var microphoneMute = new NAudio.CoreAudioApi.MMDeviceEnumerator();
            using (var speechSynthesizer = new Microsoft.CognitiveServices.Speech.SpeechSynthesizer(speechConfig))
            {
                MicrophoneMute(true, microphoneMute);
                // commDevice.AudioEndpointVolume.Mute = true;
                var speechSynthesisResult = await speechSynthesizer.SpeakTextAsync(whatHasUserSpoken);
                // commDevice.AudioEndpointVolume.Mute = false;
                MicrophoneMute(false, microphoneMute);
            }
        }

        public static async Task<List<object>> GetFirstData(string filePath)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", @"eyJFbWFpbCI6ImpvYW8uZ29tZXNAZGV2c2NvcGUubmV0In0=|1686405113|64YB_cjTVJuL8a9XRjxfowVdBSA=");

                string cookieValue = "eyJFbWFpbCI6ImpvYW8uZ29tZXNAZGV2c2NvcGUubmV0In0=|1686405113|64YB_cjTVJuL8a9XRjxfowVdBSA=";
                var cookieUri = new Uri("https://rooms-dev.aad-bonfim.devscope.net/");
                httpClient.DefaultRequestHeaders.Add("Cookie", $"_oauth2_proxy={cookieValue}");



                var response = await httpClient.GetAsync("https://rooms-dev.aad-bonfim.devscope.net/api/rooms");


                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                    List<MeetingRoom> rooms = JsonConvert.DeserializeObject<List<MeetingRoom>>(responseContent);

                    for (int i = 0; i < rooms.Count; i++)
                    {
                        rooms[i].id = i + 1;
                    }
                    string updatejson = JsonConvert.SerializeObject(rooms, Formatting.Indented);
                    File.WriteAllText(filePath, updatejson);
                }
            }
            return null;
        }
        public static List<object> GetOriginalData(string filePath)
        {

            string json = File.ReadAllText(filePath);
            List<MeetingRoom> rooms = JsonConvert.DeserializeObject<List<MeetingRoom>>(json);

            List<object> items = new List<object>();
            foreach (var room in rooms)
            {
                if (room.Appointments.Count > 0)
                {
                    foreach (var appointment in room.Appointments)
                    {
                        var combinedData = new
                        {
                            RoomId = room.id,
                            RoomName = room.Name,
                            appointment.Organizer,
                            StartHour = GetData(appointment.Start).ToString("hh:mm tt"),
                            StartDay = GetData(appointment.Start).ToString("dd/MMMM/yyyy"),
                            EndHour = GetData(appointment.End).ToString("hh:mm tt"),
                            EndDay = GetData(appointment.End).ToString("dd/MMMM/yyyy"),
                            Busy = room.Busy.ToString()
                        };
                        items.Add(combinedData);
                    }
                }
                        else
                        {
                            var combinedData = new
                            {
                                RoomId = room.id,
                                RoomName = room.Name,
                                Organizer = "",
                                StartHour = "",
                                StartDay = "",
                                EndHour = "",
                                EndDay = "",
                                Busy = room.Busy.ToString()
                            };
                            items.Add(combinedData);
                        }
            }

            return items;
        }
       
        public MainWindow()
        {
            var microphoneMute = new NAudio.CoreAudioApi.MMDeviceEnumerator();
            MicrophoneMute(true, microphoneMute);
            InitializeComponent();
            daysDictionary["the second day."] = "the 2nd day";
            daysDictionary["the first day."] = "the 1st day";
            daysDictionary["the third day."] = "the 3rd day";

            InicializedDataASync();
            var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
            this.intentRecognizer = new IntentRecognizer(config, audioConfig);

            var cluModel = new ConversationalLanguageUnderstandingModel(
                languageKey,
                languageEndpoint,
                cluProjectName,
                cluDeploymentName);
            var collection = new LanguageUnderstandingModelCollection
            {
                cluModel
            };

            intentRecognizer.ApplyLanguageModels(collection);

            intentRecognizer.Recognized += SpeechRecognizer_Recognized;
            RecognizeIntents();

               
            }

        public async void InicializedDataASync()
        {
           await GetFirstData(filePath);
            data.ItemsSource =  GetOriginalData(filePath);

            ICollectionView view = CollectionViewSource.GetDefaultView(data.ItemsSource);
            // Sort the DataGrid by a specific column
            view.SortDescriptions.Clear();
            view.SortDescriptions.Add(new SortDescription("RoomId", ListSortDirection.Ascending));
        }

            private async void SpeechRecognizer_Recognized(object? sender,
       IntentRecognitionEventArgs e)
            {
                object value = await Dispatcher.InvokeAsync(async () => {
                // MessageBox.Show("Cheguei");


                var recognitionResult = e.Result;
                if (recognitionResult.Reason == ResultReason.RecognizedIntent)
                {
                    Console.WriteLine($"RECOGNIZED: Text={recognitionResult.Text}");
                    Console.WriteLine($"    Intent Id: {recognitionResult.IntentId}.");
                    Console.WriteLine($"    Language Understanding JSON: {recognitionResult.Properties.GetProperty(PropertyId.LanguageUnderstandingServiceResponse_JsonResult)}");

                    // Extrai o JSON do resultado do serviço de compreensão de linguagem
                    var json = recognitionResult.Properties.GetProperty(PropertyId.LanguageUnderstandingServiceResponse_JsonResult) as string;

                    // Faz o parsing do JSON para um objeto JsonDocument
                    using (JsonDocument document = JsonDocument.Parse(json))
                    {
                        // Acessa a propriedade 'result' para obter os resultados
                        if (document.RootElement.TryGetProperty("result", out JsonElement result))
                        {

                            // Acessa a propriedade 'prediction' para obter as previsões
                            if (result.TryGetProperty("prediction", out JsonElement prediction))
                            {
                                // Acessa a propriedade 'entities' para obter as entidades
                                if (prediction.TryGetProperty("entities", out JsonElement entities) && entities.ValueKind == JsonValueKind.Array && entities.GetArrayLength() > 0)
                                {    
                                    foreach (var entity in entities.EnumerateArray())
                                    {
                                        // Obtém os valores das propriedades da entidade
                                        var category = entity.GetProperty("category").GetString();
                                        string text = entity.GetProperty("text").GetString();

                                            if(recognitionResult.Text.ToLower() == "the first day." || recognitionResult.Text.ToLower() == "the second day." || recognitionResult.Text.ToLower() == "the third day.")
                                            {
                                                category = "day";
                                                text = daysDictionary[recognitionResult.Text.ToLower()];
                                            }
                                          //  MessageBox.Show(category);
                                        switch (category)
                                        {
                                            case "reservation":
                                                roomcategory = category;
                                                break;

                                            case "day":

                                                Match match = Regex.Match(text, @"\d+");
                                                if (match.Success)
                                                {
                                                   
                                                    dayText = Int32.Parse(match.Value).ToString();
                                                        DayLabel.Content = $"Day: {dayText}";
                                                }
                                                break;

                                            case "month":
                                                monthtext = text;
                                                MonthLabel.Content = $"Month: {text}";
                                                break;

                                            case "hour":
                                                
                                                hourText = text;
                                                HourLabel.Content = $"Hour: {hourText}";

                                                    break;
                                                case "ID":
                                                    Match m = Regex.Match(text, @"\d+");
                                                    if (m.Success)
                                                    {
                                                        if (id == 0)
                                                        {
                                                            id = Int32.Parse(m.Value);
                                                            await SpeakWhatUserHasSopken("How manu hour you want to spent in the meeting ?", config);
                                                            break;
                                                        }
                                                        else
                                                        {
                                                            timespent = Int32.Parse(m.Value);
                                                            GetDisiredRoom(filePath, id, dayText, monthtext, now.Year.ToString(), hourText, timespent ,config);
                                                            
                                                            DayLabel.Content = $"Day: ";
                                                            MonthLabel.Content = $"Month: ";
                                                            HourLabel.Content = $"Hour: ";
                                                            data.ItemsSource =  GetOriginalData(filePath);
                                                        }
                                                           
                                                        

                                                    }
                                                    break;
                                                default:
                                                    await SpeakWhatUserHasSopken("Invalid Inputs.", config);
                                                    break;
                                            }
                                        }
                                        while (true)
                                        {
                                            if (dayText == null)
                                            {
                                                await SpeakWhatUserHasSopken("Wich Day ?", config);
                                                break;
                                            }
                                            else
                                            {
                                                if (monthtext == null)
                                                {
                                                    await SpeakWhatUserHasSopken("Wich month ?", config);
                                                    break;
                                                }
                                                else
                                                {
                                                    if (hourText == null)
                                                    {
                                                        await SpeakWhatUserHasSopken("Wich Hour ?", config);
                                                        break;
                                                    }
                                                }
                                            }

                                            if (dayText != null && monthtext != null && hourText != null)
                                            {
                                                if(id == 0)
                                                {
                                                    data.ItemsSource = GetDisiredRoomData(filePath, dayText, monthtext, now.Year.ToString(), hourText);
                                                    await SpeakWhatUserHasSopken("choose one id ?", config);
                                                    break;
                                                }
                                                break;

                                            }
                                            if (dayText != null && monthtext != null && hourText != null && id != 0 && timespent != 0)
                                            {
                                                dayText = null;
                                                monthtext = null;
                                                hourText = null;
                                                id = 0;
                                                timespent = 0;
                                                break;
                                            }
                                        }
                                       
                                       
                                    }
                                }
                            }
                        }
                    }


                   
                });

            }
            public async void RecognizeIntents()
            {
                if (!isRecognizing)
                {
                    await intentRecognizer.StartContinuousRecognitionAsync();

             
                    isRecognizing = true;
             
                }
                else
                {
                    await intentRecognizer.StopContinuousRecognitionAsync();

                    isRecognizing = false;
                
                }
            }

            private async void Button_Click(object sender, RoutedEventArgs e)
            {
                var microphoneMute = new NAudio.CoreAudioApi.MMDeviceEnumerator();
                if (button1.Content.ToString() == "MicrofoneOff")
                {
                    button1.Content = "MicrofoneOn";
                    MicrophoneMute(false, microphoneMute);
                 await   SpeakWhatUserHasSopken("You can speack now", config);
                }
                else
                {
                    button1.Content = "MicrofoneOff";
                  
                  await  SpeakWhatUserHasSopken("Microfone Muted", config);
                    MicrophoneMute(true, microphoneMute);

                }
           
            }
        }
}
