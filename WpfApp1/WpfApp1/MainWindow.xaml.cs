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

    namespace WpfApp1
    {
        /// <summary>
        /// Interaction logic for MainWindow.xaml
        /// </summary>
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
            public string year = "2023";
        public int timespent = 0;
            // Your CLU project name and deployment name.
            static string cluProjectName = "testeLUIS2";
            static string cluDeploymentName = "LUISTESTE";
            string filePath = @"C:\Users\joao_\OneDrive\Ambiente de Trabalho\Estágio_DevScope\WpfApp1\jsonviewer.json";
            public int id = 0;

            private readonly IntentRecognizer intentRecognizer;
            private bool isRecognizing;

            public SpeechConfig config = SpeechConfig.FromSubscription(speechKey, speechRegion);

            public class Appointment
            {
                public string? Subject { get; set; }
                public string? Organizer { get; set; }
                public long Start { get; set; }
                public long End { get; set; }
                public bool Private { get; set; }
            }

            public class MeetingRoom
            {
                public int id { get; set; } = 0;
                public string? Roomlist { get; set; }
                public string? Name { get; set; }
                public string? RoomAlias { get; set; }
                public string? Email { get; set; }
                public List<Appointment> Appointments { get; set; } = new List<Appointment>();
                public bool Busy { get; set; }
            }

            public static DateTimeOffset GetData(long data)
            {
                DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(data);
                DateTime dateTime = dateTimeOffset.UtcDateTime;

                return dateTime;
            }
            public static void GetDisiredRoom(string filePath, int ID ,string day, string month, string year, string hour,int timespent ,SpeechConfig config)
            {
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);

                    List<MeetingRoom> rooms = JsonConvert.DeserializeObject<List<MeetingRoom>>(json);
                for (int i = 0; i < rooms.Count; i++)
                {

                    rooms[i].id = i + 1;


                }
                MessageBox.Show(ID.ToString());
                

                MeetingRoom room = rooms.FirstOrDefault(r => r.id == ID);

                    if (room != null)
                    {
                  //  MessageBox.Show(" deu");

                    if (DateTimeOffset.TryParseExact($"{day}/{month}/{year} {hour}", "d/MMMM/yyyy H:m", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset dateTimeOffset))
                        {
                      //  MessageBox.Show("deu");
                        Console.WriteLine(dateTimeOffset);
                            Appointment apoint = new Appointment
                            {
                                Subject = "João Gomes",
                                Organizer = "João Gomes",
                                Start = dateTimeOffset.ToUnixTimeMilliseconds(),  // DateTimeOffset.ParseExact("27/5/2023 02:28", "d/M/yyyy HH:mm ", CultureInfo.InvariantCulture).ToUnixTimeMilliseconds(),
                                End = dateTimeOffset.AddHours(timespent).ToUnixTimeMilliseconds(),
                                Private = false
                            };
                            room.Appointments.Add(apoint);
                            string updatejson = JsonConvert.SerializeObject(rooms, Formatting.Indented);
                            File.WriteAllText(filePath, updatejson);
                         SpeakWhatUserHasSopken($"The room has been booked for {day} of {month} at {year}.", config);
                       // Console.WriteLine("Novo compromisso adicionado");
                       // MessageBox.Show("Novo compromisso adicionado");
                    }
                    else
                    {
                        MessageBox.Show("Não deu");
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

                    foreach(var a in rooms)
                {
                    foreach(var b in a.Appointments)
                    {
                        MessageBox.Show((GetData(b.Start).ToString("MMMM").ToLower() == month.ToLower()).ToString());
                    }
                }
               
                //  desiredStart = "2"; // O valor de "Start" que você deseja pesquisar
                //   string desiredRoomName = "Arkanoid Room (1..3)"; // O nome da sala que você deseja pesquisar
               List<MeetingRoom> room = rooms.Where(r => r.Appointments.Exists(a => GetData(a.Start).Day.ToString("d") == day && GetData(a.Start).ToString("MMMM").ToLower() == month.ToLower() 
               && GetData(a.Start).ToString("yyyy") == year && GetData(a.Start).ToString("H:mm") == hour
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
                           foreach(var app in findrooms.Appointments)
                            {
                                var combinedData = new
                                {
                                    RoomId = findrooms.id,
                                    RoomName = findrooms.Name,
                                    RoomEmail = findrooms.Email,
                                    app.Subject,
                                    app.Organizer,
                                    StartHour = GetData(app.Start).ToString("HH:mm"),
                                    StartDay = GetData(app.Start).ToString("dd/MMMM/yyyy"),
                                    EndHour = GetData(app.End).ToString("HH:mm"),
                                    EndDay = GetData(app.End).ToString("dd/MMMM/yyyy"),
                                    Private = app.Private.ToString()
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
        public static List<object> GetOriginalData(string filePath)
        {
            string json = File.ReadAllText(filePath);
            List<MeetingRoom> rooms = JsonConvert.DeserializeObject<List<MeetingRoom>>(json);
            for (int i = 0; i < rooms.Count; i++)
            {
                rooms[i].id = i + 1;
            }
            List<object> items = new List<object>();
            foreach (var room in rooms)
            {

                foreach (var appointment in room.Appointments)
                {
                    var combinedData = new
                    {
                        RoomId = room.id,
                        RoomName = room.Name,
                        appointment.Organizer,
                        StartHour = GetData(appointment.Start).ToString("HH:mm"),
                        StartDay = GetData(appointment.Start).ToString("dd/MMMM/yyyy"),
                        EndHour = GetData(appointment.End).ToString("HH:mm"),
                        EndDay = GetData(appointment.End).ToString("dd/MMMM/yyyy"),
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


            data.ItemsSource = GetOriginalData(filePath);


            //data.ItemsSource = GetDisiredRoomData(filePath, "2", items);


            var audioConfig = AudioConfig.FromDefaultMicrophoneInput();

                //    speechRecognizer = new SpeechRecognizer(config, audioConfig);
                this.intentRecognizer = new IntentRecognizer(config, audioConfig);

                var cluModel = new ConversationalLanguageUnderstandingModel(
                    languageKey,
                    languageEndpoint,
                    cluProjectName,
                    cluDeploymentName);
                var collection = new LanguageUnderstandingModelCollection();
                collection.Add(cluModel);
                intentRecognizer.ApplyLanguageModels(collection);
                intentRecognizer.Recognized += SpeechRecognizer_Recognized;


                teste();



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
                                          //  MessageBox.Show(category);
                                        switch (category)
                                        {
                                            case "reservation":
                                                roomcategory = category;
                                                if(dayText == null)
                                                    {
                                                        await SpeakWhatUserHasSopken("Wich Day ?", config);
                                                    }
                                                

                                                // MessageBox.Show("wich day ?");
                                                break;

                                            case "day":

                                                Match match = Regex.Match(text, @"\d+");
                                                if (match.Success)
                                                {
                                                   
                                                    dayText = Int32.Parse(match.Value).ToString();
                                                        DayLabel.Content = $"Day: {dayText}";
                                                    if(monthtext == null)
                                                    {
                                                        await SpeakWhatUserHasSopken("Wich month ?", config);
                                                    }
                                                        
                                                    //   MessageBox.Show("wich month ?");
                                                }
                                                break;

                                            case "month":
                                                monthtext = text;
                                                MonthLabel.Content = $"Month: {monthtext}";
                                              //  Console.WriteLine(monthtext.Replace(".", ""));
                                                if(hourText == null)
                                                {
                                                    await SpeakWhatUserHasSopken("Wich hour ?", config);
                                                }
                                                //   MessageBox.Show("wich hour ?");
                                                break;

                                            case "hour":
                                                
                                                hourText = text;
                                                HourLabel.Content = $"Hour: {hourText}";
                                                    // MessageBox.Show(hourText);
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
                                                                await SpeakWhatUserHasSopken("Wich hour ?", config);
                                                            }
                                                        }
                                                        
                                                    }
                                                    
                                                if (dayText != null && monthtext != null && hourText != null)
                                                {
                                                    Console.WriteLine($"The room has been booked for {dayText} of {monthtext} at {hourText}.");
                                                        data.ItemsSource = GetDisiredRoomData(filePath, dayText, monthtext, year, hourText);
                                                        await SpeakWhatUserHasSopken("choose one id ?", config);
                                                      
                                                     
                                                }
                                                    break;
                                                case "ID":
                                                    Match m = Regex.Match(text, @"\d+");
                                                    if (m.Success)
                                                    {

                                                        //MessageBox.Show(id.ToString());
                                                        if (id == 0)
                                                        {
                                                            id = Int32.Parse(m.Value);
                                                            await SpeakWhatUserHasSopken("How manu hour you want to spent in the meeting ?", config);
                                                            break;
                                                        }
                                                        else
                                                        {
                                                            timespent = Int32.Parse(m.Value);
                                                            GetDisiredRoom(filePath, id, dayText, monthtext, year, hourText, timespent ,config);
                                                            //   MessageBox.Show("wich month ?");

                                                            dayText = null;
                                                            monthtext = null;
                                                            hourText = null;
                                                            id = 0;
                                                            timespent = 0;
                                                            DayLabel.Content = $"Day: ";
                                                            MonthLabel.Content = $"Month: ";
                                                            HourLabel.Content = $"Hour: ";
                                                            data.ItemsSource = GetOriginalData(filePath);
                                                        }
                                                           
                                                        

                                                    }
                                                    break;
                                                default:
                                                    await SpeakWhatUserHasSopken("Invalid Inputs.", config);
                                                    break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }


                    // Retorne o valor que deseja atribuir a 'value'
                    return new object[] { roomcategory, dayText, hourText, monthtext, year };
                });

            }
            public async void teste()
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

            private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
          
            }

            private void Button_Click(object sender, RoutedEventArgs e)
            {
                var microphoneMute = new NAudio.CoreAudioApi.MMDeviceEnumerator();
                if (button1.Content.ToString() == "MicrofoneOff")
                {
                    button1.Content = "MicrofoneOn";
                    MicrophoneMute(false, microphoneMute);
                    SpeakWhatUserHasSopken("You can speack now", config);
                }
                else
                {
                    button1.Content = "MicrofoneOff";
                    SpeakWhatUserHasSopken("Microfone Muted", config);
                    MicrophoneMute(true, microphoneMute);

                }
           
            }
        }
    }
