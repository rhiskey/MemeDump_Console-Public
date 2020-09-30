using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using VkNet;
using VkNet.Model.Attachments;
using VkNet.Abstractions;
using VkNet.Model;
using VkNet.Model.RequestParams;
using System.IO;

using System.Diagnostics;
using System.Net;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading;
using Tesseract;
using InstagramApiSharp.API;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Logger;
using InstagramApiSharp.API.Builder;
//using Discord;
//using Discord.WebSocket;
//using Discord.Webhook;
using System.Text.RegularExpressions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
//using DocumentFormat.OpenXml.Drawing;



namespace MemeDump_Console
{
    class Program
    {
        //Variables
        List<MediaAttachment> mediaattachment = new List<MediaAttachment>(); //вложения в нашу группу
        List<VkNet.Model.Attachments.Attachment> attachment = new List<VkNet.Model.Attachments.Attachment>();

        int startmin = 25;
        int endmin = 30;
        int mindelay = 5000;//milliseconds 5000/1000=5 sec
        int maxdelay = 15000;
        string fullphotoname;
        string filepath_archive = "posted_photos.txt"; 
        string text_archive = "posted_messages.txt"; 
        int erasecount = 10000; //1000; //удаляем когда превысит число символов (переделать в число строк)
        string groupsfilepath = "groupsid.txt"; //файл со списком пабликов для парсинга 
        string photofilename = "tempimage"; //скачанное фото
        long? groupid = -12345;
        ulong? ownerid3 = 12345;
        ulong? groupid3 = 12345;
        string vkLogin = "12345"; 
        string vkPassword = "pass";  
        string Token = "token"; //Standalone App
        string MessageToAttach;
        string firstpart = "https://api.vk.com/method/photos.get?owner_id=";
        //string countofposts = "50";
        string lastpart = "&album_id=wall&rev=0&extended=0&photo_sizes=0&count=50&access_token=INSERT_TOKEN_HERE&v=5.87";
        string filepath = "request.txt";
        private static IVkApi _api;
        string urlString = "https://api.telegram.org/bot{0}/sendMessage?chat_id={1}&text={2}";
        string apiToken = "12345:bottoken"; //bot father
        private string chatId = "-12345"; //https://api.telegram.org/bot<YourBOTToken>/getUpdates
        ulong PostCount = 20; //количество постов для парсинга
        string curTimeLong = DateTime.Now.ToLongTimeString(); //Результат: 8:49:10
        int hour;
        int min;
        bool f_postedall = false;
        bool f_start = false;
        string path = Directory.GetCurrentDirectory();
        string EventMessage;
        string myId = "12345";/
        private  IInstaApi InstaApi;


        public static void Main(string[] args)
        {
            ////Discord
            //new Program().MainAsync().GetAwaiter().GetResult();

            System.Threading.Thread.Sleep(5000);
            // Create an AutoResetEvent to signal the timeout threshold in the
            // timer callback has been reached.
            var autoEvent = new AutoResetEvent(false);

            //Число запусков потока
            var statusChecker = new StatusChecker(999999999);

            //Создает таймер, который проверяет статус через одну секунду и затем полседующие 1/4 секунды
            // Create a timer that invokes CheckStatus after one second, 
            // and every 1/4 second thereafter.
            Console.WriteLine("{0:h:mm:ss.fff} Creating timer. Launching in 1 sec with every 1hour thereafter\nPress Enter to start...",
                              DateTime.Now);
            //Console.ReadLine();
            var stateTimer = new Timer(statusChecker.CheckStatus,
                                       autoEvent, 1000, 3600000);
            //System.Threading.Thread.Sleep(1000);
            // When autoEvent signals, change the period to every half second.

            autoEvent.WaitOne();
            stateTimer.Change(0, 1800000);
            Console.WriteLine("\nChanging period to 30 minutes.\n");


            //When autoEvent signals the second time, dispose of the timer.
            autoEvent.WaitOne();
            stateTimer.Change(0, 3600000);
            Console.WriteLine("\nChanging period to .1 hrs \n");


            CancellationTokenSource tokenSource2 = new CancellationTokenSource();
            CancellationToken token = tokenSource2.Token;

        }


        public Program()
        {
            // It is recommended to Dispose of a client when you are finished
            // using it, at the end of your app's lifetime.
            //_client = new DiscordSocketClient();

            //_client.Log += LogAsync;
            //_client.Ready += ReadyAsync;
            //_client.MessageReceived += MessageReceivedAsync;
        }

        //Timer
        class StatusChecker
        {
            private int invokeCount;
            private int maxCount;

            public StatusChecker(int count)
            {
                invokeCount = 0;
                maxCount = count;
            }

            // This method is called by the timer delegate.
            public void CheckStatus(Object stateInfo)
            {
                AutoResetEvent autoEvent = (AutoResetEvent)stateInfo;
                Console.WriteLine("{0} Checking status {1,2}.",
                    DateTime.Now.ToString("h:mm:ss.fff"),
                    (++invokeCount).ToString());

                if (invokeCount == maxCount)
                {
                    // Reset the counter and signal the waiting thread.
                    invokeCount = 0;
                    autoEvent.Set();
                }


                Program P = new Program();


                Task stepone = P.ReadGroupsFileAsync(); //1 раз прогнали парсер

            }
        }



            //Discord
        public async Task MainAsync()
        {
            ////Working Hook but not what expected (CANT UPLOAD PHOTO)
            //using (var client = new DiscordWebhookClient("https://discordapp.com/api/webhooks/12345/INSERTTOKEN"))
            //{
            //    var embed = new EmbedBuilder
            //    {
            //        Title = "Test Embed",
            //        //ImageUrl=
            //        //ImageUrl= @"tempimage.jpg",
            //        //Url =,
            //        Description = "Test Description"

            //    };

            //    // Webhooks are able to send multiple embeds per message
            //    // As such, your embeds must be passed as a collection. 
            //    await client.SendMessageAsync(text: "Send a message to this webhook!", embeds: new[] { embed.Build() });
            //}
            ////---------------------------------------------

            //_client = new DiscordSocketClient();

            //_client.Log += LogAsync;

            // Some alternative options would be to keep your token in an Environment Variable or a standalone file.
            // var token = Environment.GetEnvironmentVariable("NameOfYourEnvironmentVariable");
            // var token = File.ReadAllText("token.txt");
            // var token = JsonConvert.DeserializeObject<AConfigurationClass>(File.ReadAllText("config.json")).Token;

            //// Tokens should be considered secret data, and never hard-coded.
            //var bot_Token = "  ";
            //await _client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable(bot_Token));
            //await _client.StartAsync();



        }

        ////Discord
        //private Task LogAsync(LogMessage log)
        //{
        //    Console.WriteLine(log.ToString());
        //    return Task.CompletedTask;
        //}
        //// The Ready event indicates that the client has opened a
        //// connection and it is now safe to access the cache.
        //private Task ReadyAsync()
        //{
        //    Console.WriteLine($"{_client.CurrentUser} is connected!");

        //    return Task.CompletedTask;
        //}

        private string CreateApiReq(string idpart)
        {
            string req = firstpart + idpart + lastpart;
            return req;
        }

        public string JsonReq(string url)
        {
            string json, response;
            // Создаём объект WebClient

            using (WebClient webClient = new WebClient())
            {

                // System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                try
                {
                    response = webClient.DownloadString(url);
                }
                catch (Exception)
                {
                   Console.WriteLine("Слишком много запросов! Повтори позднее");
                   // Console.ReadLine();
                    response = null;

                }
                json = JsonConvert.SerializeObject(response);

            }
            return json;
            //return page;
        }

        /// Возращает URL самой большой фотографиии из существующих
        public static string GetUrlOfBigPhoto(VkNet.Model.Attachments.Photo photo)
        {
            if (photo == null)
                return null;
            if (photo.Photo2560 != null)
                return photo.Photo2560.AbsoluteUri;
            if (photo.Photo1280 != null)
                return photo.Photo1280.AbsoluteUri;
            if (photo.Photo807 != null)
                return photo.Photo807.AbsoluteUri;
            if (photo.Photo604 != null)
                return photo.Photo604.AbsoluteUri;
            if (photo.Photo130 != null)
                return photo.Photo130.AbsoluteUri;
            if (photo.Photo75 != null)
                return photo.Photo75.AbsoluteUri;
            if (photo.Sizes?.Count > 0)
            {
                var bigSize = photo.Sizes[0];
                for (int i = 0; i < photo.Sizes.Count; i++)
                {
                    var photoSize = photo.Sizes[i];
                    if (photoSize.Height > bigSize.Height || photoSize.Width > bigSize.Width)
                        bigSize = photoSize;
                }
                return bigSize.Src.AbsoluteUri;
            }
            return null;
        }

        public void AddedFile(string info, string filename)
        {
            //если файл существует
            int charCount = File.ReadAllText(filename).Length;
            int linesCount = File.ReadAllLines(filename).Length;
            //Если число строк в файле больше или равно 10000
            if (linesCount >= erasecount)
            {
                //Нужно удалить начало файла
                File.WriteAllText(filename, String.Empty); //очистили файл
            }
            else
            {
                FileStream file1 = new FileStream(filename, FileMode.Append); //открытие файла на дозапись в конец файла
                StreamWriter writer = new StreamWriter(file1, Encoding.UTF8); //создаем «потоковый писатель» и связываем его с файловым потоком
                writer.WriteLine(info);
                //writer.Write(photos); //записываем в файл (раньше было json)
                writer.Close();
            }
        }

        private async Task ReadGroupsFileAsync(/*CancellationToken ct*/ /*CancellationToken cancellationToken*/)
        {
            f_start = true;

            var _api = new VkApi();
            //Авторизация
            _api.Authorize(new ApiAuthParams
            {
                Login = vkLogin,
                Password = vkPassword,
                AccessToken = Token
            });
            Console.WriteLine("Авториизация ВК успешна");
            //Console.ReadLine();
            int y = 0; //порядковый номер группы
            string[] lines = File.ReadAllLines(groupsfilepath);
            var last = File.ReadAllLines(groupsfilepath).Last();//последняя строка - группа
            var linescount = File.ReadAllLines(groupsfilepath).Length;//число строк

            List<MediaAttachment> vk_attachments = new List<MediaAttachment>();

            //Авторизация в инсте
            InstaLoginAsync();

            foreach (var K in lines) //для каждой группы получаем список записей
            {
                //InstaLoginAsync();
                long? id2 = Convert.ToInt64(K);
                WallGetObject wallposts = _api.Wall.Get(new WallGetParams
                {
                    OwnerId = id2,//Идентификатор пользователя или сообщества, со стены которого необходимо получить записи 
                                  //(по умолчанию — текущий пользователь). Обратите внимание, идентификатор сообщества в параметре owner_id
                                  //необходимо указывать со знаком “-“ — например, owner_id=-1 соответствует идентификатору сообщества ВКонтакте API (club1) целое число
                                  // Domain = id,  //- Короткий адрес пользователя или сообщества. строка
                                  //Filter=,  //all
                                  // Offset=, // Смещение, необходимое для выборки определенного подмножества записей. положительное число
                    Count = PostCount, //- Количество записей, которое необходимо получить (но не более 100). 
                    Extended = true
                    //Fields=Photo
                });
                var count = wallposts.TotalCount;

                Console.WriteLine("Количество постов в группе " + K + " = " + count);
                //Console.ReadLine();

                var postelem = wallposts.WallPosts; //Получили посты

                int j = 0;//число постов
                int numphot = 0; //номер фото

                for (j = 0; j < (int)PostCount; j++) //пока не дошли до макс значения постов одной группы
                {
                    string groupidWOmin2 = K.Replace("-", "");
                    var groups2 = _api.Groups.GetById(null, groupidWOmin2, null).FirstOrDefault();
                    string sourceLink2 = groups2.Name;
                                                      //Отправка источника записи
                    string sourcelb = sourceLink2;

                        Console.WriteLine("Parsing группы: " + sourcelb);


                    //текст к картинкам

                        //Проверить если есть фотки или только текст, постить
                        var att = postelem[j].Attachment; //Получили первое вложение
                        if ((postelem[j].MarkedAsAds != true) && (postelem[j].Text != null) && (att != null) && (att.Type == typeof(Photo))/*&&(postelem[j].PostType==post)*/)
                        {
                            //Удаление \n
                            string deleteenters = postelem[j].Text.Replace("\n", "");
                            //string deletespace = postelem[j].Text.Replace("\n", "");
                            MessageToAttach = deleteenters;

                            //получаем уникальный ID вложения
                            string postid = postelem[j].OwnerId + "_" + postelem[j].Id +"_"+ postelem[j].FromId; //postelem[j].id

                            //Проверка на повторы

                            //Downliad to json, десериализовать в класс
                            //Сравнить
                            string s = File.ReadAllText(text_archive); //Открываем файл с историей
                                                                       //if (s.IndexOf(MessageToAttach) != -1) //пока не дошли до конца ищем первое совпадение
                            if (s.IndexOf(MessageToAttach) != -1)
                            {
                            //Нашли текст уже был опубликован, значит пропускаем

                            Console.WriteLine("Нашли повтор текста.");

                            continue;
                            }

                            else
                            {
                            //Запомнили что опубликовали такое сообщение

                            AddedFile(MessageToAttach, text_archive);

                            //Отправка сообщения поста
                            if (string.IsNullOrEmpty(MessageToAttach) == false) //Если не пустое сообщение
                                {
                                if (MessageToAttach.IndexOf("[club") != -1 || 
                                    MessageToAttach.IndexOf("https://vk.cc/") != -1 ||
                                    MessageToAttach.IndexOf("https://teleg.run/") != -1 ||
                                    MessageToAttach.IndexOf("http://")!=-1 ||
                                    MessageToAttach.IndexOf("https://") != -1 ||
                                    MessageToAttach.IndexOf("Подробнее") != -1||
                                    MessageToAttach.IndexOf("подробнее") != -1||
                                    MessageToAttach.IndexOf("https") != -1||
                                    MessageToAttach.IndexOf("Похудела") != -1||
                                    MessageToAttach.IndexOf("шок") != -1|| MessageToAttach.IndexOf("Рисовать") != -1||
                                    MessageToAttach.IndexOf("научим") != -1||
                                    MessageToAttach.IndexOf("Подписывайтесь") != -1
                                    )
                                    continue;
        
                                string pattern = @"(http|ftp|https)://([\w_-]+(?:(?:\.[\w_-]+)+))([\w.,@?^=%&:/~+#-]*[\w@?^=%&/~+#-])?";
                                string vkpattern = @"^(https?:\/\/)?(www\.)?vk\.com\/(\w|\d)+?\/?$";
                                if (Regex.IsMatch(MessageToAttach, pattern, RegexOptions.IgnoreCase)|| Regex.IsMatch(MessageToAttach, vkpattern, RegexOptions.IgnoreCase))
                                {
                                    Console.WriteLine("Рекламная ссылка");
                                    continue;
                                }
            
                                Console.WriteLine("Отправка сообщения...");
 
                                var resp = await sendMessage(chatId, MessageToAttach);
             
                                Console.WriteLine("Отправлено: \n", MessageToAttach);
  
                                }

                            }

                        }
                    

                    foreach (var one in postelem)
                    {
                        if (numphot == (int)PostCount - 1) //если дошли числа постов, то обнуляем №фото, чтобы перезаписывать
                        {
                            numphot = 0;

                        }
                        List<string> photoname_ = new List<string>();


                        //Получить информацию о записи, первого вложения и текст
                       var atts = postelem[j].Attachments;
                        //var atts = i.Attachments;
                        int currphoto = 0;

                        ////Скачать все фото поста, затем добавить их в список и передать
                        //if (postelem[j].Attachments.Count > 1)
                        //{
                        //    int attcounter = 0;
                        //    foreach(var singleatt in postelem[j].Attachments)
                        //    {
                        //        if (singleatt.Type == typeof(Photo) && singleatt.Type != typeof(Link))
                        //        {
                        //            var url = GetUrlOfBigPhoto(singleatt.Instance as Photo);
                        //            //Если ссылка уже есть в файле истории, continue, если нет, то идем дальше
                        //            string s = File.ReadAllText(filepath_archive); //Открываем файл с историей
                        //            if (s.IndexOf(url) != -1) //пока не дошли до конца ищем первое совпадение
                        //            {
                        //                //Нашли фото уже был опубликован, значит пропускаем
                        //                Console.WriteLine("Нашли повтор фото...");
                        //                continue; // нет перехода на 2 уровня выше, нужно перескочить на след. пост
                        //                          //continue;
                        //            }
                        //            else
                        //            {
                        //                //Запомнили что опубликовали такую фотку
                        //                AddedFile(url, filepath_archive);
                        //                //InsertTO DB

                        //                //_api.Groups.GetById()
                        //                //скачали на ПК sourcelb - имя паблика
                        //                numphot = DownloadTempPhoto(url, j, sourcelb);
                        //                Console.WriteLine("Скачали и сохранили фото под № " + numphot);
                        //                // Console.ReadLine();

                        //                fullphotoname = photofilename + numphot + ".jpg";
                        //                photoname_.Add(fullphotoname);
                        //                attcounter++;
                        //            }
                        //        }
                        //    }
                        //    //как только вложений максимум для поста, остановить скачку и загрузить в инсту
                        //    NewAlbumUpload(photoname_, InstaApi);

                        //}
                        //else
                        //{
                        //  //fullphotoname = photofilename + numphot + ".jpg";
                        //  // UploadPhotoToProfile(fullphotoname, MessageToAttach, InstaApi);
                        //}


                        foreach (var at in postelem[j].Attachments) //для каждого вложения одного поста было att
                        //foreach (var at in i.Attachments)
                            {

                            if (at.Type == typeof(Photo) && at.Type != typeof(Link)) //если вложения = фото но не ссылка
                            {

                                //получили ссылку на фотку
                                var url = GetUrlOfBigPhoto(at.Instance as Photo);

                                //Если ссылка уже есть в файле истории, continue, если нет, то идем дальше
                                string s = File.ReadAllText(filepath_archive); //Открываем файл с историей
                                if (s.IndexOf(url) != -1) //пока не дошли до конца ищем первое совпадение
                                {
                                    //Нашли фото уже был опубликован, значит пропускаем
                                    Console.WriteLine("Нашли повтор фото...");
           
                                    continue; // нет перехода на 2 уровня выше, нужно перескочить на след. пост

                                }

                                else
                                {
                                    //Запомнили что опубликовали такую фотку
                                    AddedFile(url, filepath_archive);
             

                                    //_api.Groups.GetById()
                                    //скачали на ПК sourcelb - имя паблика
                                    numphot = DownloadTempPhoto(url, j, sourcelb);
                                    ////Пропускаем фото через OCR
                                    //try
                                    //{
                                    //    fullphotoname = photofilename + numphot + ".jpg";
                                    //    string ocrtext = TesseractTextFromPhoto(fullphotoname, null);
                                    //    ////Если на фото indexof("Spam")!=-1 то continue
                                    //    if (ocrtext.IndexOf("промокод") != -1 ||
                                    //        ocrtext.IndexOf("паблик") != -1 ||
                                    //        ocrtext.IndexOf("подписаться") != -1 ||
                                    //        ocrtext.IndexOf("кэшбек") != -1 ||
                                    //        ocrtext.IndexOf("высшее образование") != -1 ||
                                    //         ocrtext.IndexOf("егэ") != -1 ||
                                    //          ocrtext.IndexOf("40 профилей") != -1 ||
                                    //           ocrtext.IndexOf("очно") != -1 ||
                                    //            ocrtext.IndexOf("заочно") != -1 ||
                                    //             ocrtext.IndexOf("по своему расписанию") != -1 ||
                                    //              ocrtext.IndexOf("зачислнение каждый месяц") != -1 ||
                                    //               ocrtext.IndexOf("перспективы трудоустройства") != -1 ||
                                    //        ocrtext.IndexOf("тинькофф") != -1)
                                    //    {
                                    //        Console.WriteLine("Спам фраза:\n {0}", ocrtext);
                                    //        continue;
                                    //    }
                                    //}
                                    //catch(Exception ex)
                                    //{ }
                                    currphoto++;

                                    Console.WriteLine("Скачали и сохранили фото под № " + numphot);
                                    // Console.ReadLine();

                                    fullphotoname = photofilename + numphot + ".jpg";
                                    // fullphotoname = photofilename + sourcelb + ".jpg";

                                    //Залили на стену

                                    Console.WriteLine("Загружаем фото...");
                                    HttpResponseMessage result = await SendPhoto(chatId, fullphotoname, apiToken);
                            
                                    Console.WriteLine("Отправили в телегу");
                                    //Console.ReadLine();

                                    //Authorize Instagram
                                    //InstaLoginAsync();
                                    try
                                    {

                                        //////Send Photo To Story
                                        //UploadPhotoToStory(fullphotoname, "storyphoto", InstaApi);

                                        //UploadWithOptions(fullphotoname, "storyphoto", InstaApi);

                                        //Console.WriteLine("Отправили в инсту");
                                        ////Отправили ВК
                                        //var post = _api.Wall.Post(new WallPostParams
                                        //{
                                        //    OwnerId = groupid,
                                        //    FromGroup = true,
                                        //    Message = MessageToAttach,
                                        //    Attachments = vk_attachments
                                        //MessageToAttach = 
                                        //});


                                        //Повтор
                                        //string deleteenters = postelem[j].Text.Replace("\n", "");
                                        //MessageToAttach = deleteenters;
                                        string caption = postelem[j].Text;
                                       //string deletespace = postelem[j].Text.Replace("\n", "");

                                       //if (postelem[j].Attachments.Count == 1)
                                        UploadPhotoToProfile(fullphotoname, caption, InstaApi);
                                        //else UploadPhotoToProfile(fullphotoname, "мемчик", InstaApi);

                                            //Delay

                                            ////Discord
                                            //var channel_pic = _client.GetChannel(12345);

                                            //var guild = _client.Guilds.Single(g => g.Name == "Matrёшка");
                                            //var channel = guild.TextChannels.Single(ch => ch.Name == "пикчи");
                                            //await channel.SendFileAsync(fullphotoname, MessageToAttach);


                                            //await _client.channel.SendFileAsync("b1nzy.jpg",
                                            //embed: new EmbedBuilder { ImageUrl = "attachment://b1nzy.jpg" }.Build());
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("Невозможно залить фотку в инсту:\n {0}", ex.Message);
                                        
                                    }
                                    Random rnd = new Random();
                                    int number = rnd.Next(mindelay, maxdelay);

                                    //Добавить источник 
                                    //if (chb_addsource.Checked == true)
                                    //{

                                    //    int photocount = postelem[j].Attachments.Count; //число вложений фото

                                    //    if (photocount == 1)//если вложение одно
                                    //    {
                                    //        string groupidWOmin = K.Replace("-", "");
                                    //        var groups = _api.Groups.GetById(null, groupidWOmin, null).FirstOrDefault();
                                    //        if (groups != null)
                                    //        {

                                    //            string sourceLink = groups.Name;// + " @" + groups.Id;
                                    //                                            //Отправка источника записи
                                    //            string source = "Источник: " + sourceLink;

                                    //            await sendMessage(chatId, source);
                                    //        }
                                    //    }
                                    //    //если вложений несколько - один раз пишем источник в конце после последнего вложения
                                    //    if ((currphoto == photocount) && (photocount != 1))
                                    //    {
                                    //        //Получение ссылки на сообщество 
                                    //        string groupidWOmin = K.Replace("-", "");
                                    //        var groups = _api.Groups.GetById(null, groupidWOmin, null).FirstOrDefault();
                                    //        if (groups != null)
                                    //        {

                                    //            string sourceLink = groups.Name;// + " @" + groups.Id;
                                    //                                            //Отправка источника записи
                                    //            string source = "Источник: " + sourceLink;
                                    //            await sendMessage(chatId, source);
                                    //        }
                                    //    }
                                    //}
                                    numphot++;

                                    Console.WriteLine("Ждем  " + number / 1000 + " секунд");
                                    //Console.ReadLine();
                                    await Task.Delay(number);

                                }
                            }


                        }


                    }

                }
                y++;
                Console.WriteLine("Обработали пост № " + j);
                Console.WriteLine("Обработали строку № " + y + " Группа: " + id2);



            }

            Console.WriteLine("Обработаны все группы и посты!");

            f_postedall = true;
            return;

        }

        //public static string GetTextFromPhoto(Bitmap imgsource)
        //{
        //    var ocrtext = string.Empty;
        //    using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
        //    {
        //        using (var img = PixConverter.ToPix(imgsource))
        //        {
        //            using (var page = engine.Process(img))
        //            {
        //                ocrtext = page.GetText();
        //            }
        //        }
        //    }

        //    return ocrtext;
        //}

        public async void InstaLoginAsync()
        {
            var userSession = new UserSessionData
            {
                UserName = "login",
                Password = "password"
            };
            var delay = RequestDelay.FromSeconds(2, 2);
            InstaApi = InstaApiBuilder.CreateBuilder()
                .SetUser(userSession)
                .UseLogger(new DebugLogger(LogLevel.Exceptions))
                .SetRequestDelay(delay)
                .Build();
            const string stateFile = "state.bin";
            try
            {
                // load session file if exists
                if (File.Exists(stateFile))
                {
                    Console.WriteLine("Loading state from file");
                    using (var fs = File.OpenRead(stateFile))
                    {
                        InstaApi.LoadStateDataFromStream(fs);
                        // in .net core or uwp apps don't use LoadStateDataFromStream
                        // use this one:
                        // _instaApi.LoadStateDataFromString(new StreamReader(fs).ReadToEnd());
                        // you should pass json string as parameter to this function.
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            if (!InstaApi.IsUserAuthenticated)
            {
                // login
                Console.WriteLine($"Logging in as {userSession.UserName}");
                var logInResult = await InstaApi.LoginAsync();
                if (!logInResult.Succeeded)
                {
                    Console.WriteLine($"Unable to login: {logInResult.Info.Message}");
                    return;
                }
            }
            // save session in file
            var state = InstaApi.GetStateDataAsStream();
            // in .net core or uwp apps don't use GetStateDataAsStream.
            // use this one:
            // var state = _instaApi.GetStateDataAsString();
            // this returns you session as json string.
            using (var fileStream = File.Create(stateFile))
            {
                state.Seek(0, SeekOrigin.Begin);
                state.CopyTo(fileStream);
            }


        }

        //INSTAAPI
        public async Task NewAlbumUpload(List<string>photoList, IInstaApi InstaApi) //Несколько фото/видео
        {
            var album = new List<InstaAlbumUpload>();
            // IMPORTANT NOTE: only set one of ImageToUpload or VideoToUpload in InstaAlbumUpload class!
            // unless it will choose ImageToUpload automatically!.

            foreach (string singleimage in photoList)
            {
                // IMAGE 1
                album.Add(new InstaAlbumUpload
                {
                    ImageToUpload = new InstaImageUpload
                    {
                        // leave zero, if you don't know how height and width is it.
                        Height = 0,
                        Width = 0,
                        Uri = singleimage,
                        //// add user tags to your images
                        //UserTags = new List<InstaUserTagUpload>
                        //{
                        //new InstaUserTagUpload
                        //{
                        //    Username = "rmt4006",
                        //    X = 0.5,
                        //    Y = 0.5
                        //}
                        //}
                    }
                });
            }
            //// VIDEO 1
            //album.Add(new InstaAlbumUpload
            //{
            //    VideoToUpload = new InstaVideoUpload
            //    {
            //        // leave zero, if you don't know how height and width is it.
            //        Video = new InstaVideo(@"c:\video1.mp4", 0, 0),
            //        VideoThumbnail = new InstaImage(@"c:\video thumbnail 1.jpg", 0, 0),
            //        // Add user tag (tag people)
            //        UserTags = new List<InstaUserTagVideoUpload>
            //        {
            //            new InstaUserTagVideoUpload
            //            {
            //                Username = "rmt4006"
            //            }
            //        }
            //    }
            //});

            //// VIDEO 2
            //album.Add(new InstaAlbumUpload
            //{
            //    VideoToUpload = new InstaVideoUpload
            //    {
            //        // leave zero, if you don't know how height and width is it.
            //        Video = new InstaVideo(@"c:\video2.mp4", 0, 0),
            //        VideoThumbnail = new InstaImage(@"c:\video thumbnail 2.jpg", 0, 0)
            //    }
            //});

            //// IMAGE 2
            //album.Add(new InstaAlbumUpload
            //{
            //    ImageToUpload = new InstaImageUpload
            //    {
            //        // leave zero, if you don't know how height and width is it.
            //        Height = 0,
            //        Width = 0,
            //        Uri = @"c:\image2.jpg",
            //    }
            //});


            var result = await InstaApi.MediaProcessor.UploadAlbumAsync(album.ToArray(),
                "Hey, this my first album upload via InstagramApiSharp library.");

            // Above result will be something like this: IMAGE1, VIDEO1, VIDEO2, IMAGE2 [You can mix photos and videos together]

            Console.WriteLine(result.Succeeded
                ? $"Media created: {result.Value.Pk}, {result.Value.Caption}"
                : $"Unable to upload album: {result.Info.Message}");
        }

        public async void UploadPhotoToStory(string photofile, string caption, IInstaApi InstaApi)
        {
            //fullphotoname = photofilename + numphot + ".jpg";
            const int Hsize = 1920;
            const int Wsize = 1080;
            int quality = 100;
            string prefix = "resized_";

          //  using (Image<Rgba32> image = SixLabors.ImageSharp.Image.Load(photofile)) //open the file and detect the file type and decode it
            {
                //if (image.Width < Wsize || image.Height < Hsize)
                //{
                //    int W_coeff = Wsize / image.Width;
                //    int H_coeff = Hsize / image.Height;
                //    //W_coeff = MidpointRounding.AwayFromZero;
                //    //Округление до целого
                //    Decimal.Round(W_coeff);
                //    Decimal.Round(H_coeff);


                //    ////FILL Area Around Image With Grey РАМКА
                //    //image.Mutate(x =>
                //    //{
                //    //    x.Fill(Rgba32.DarkGray,
                //    //           new CoreRectangle(10, 10, 190, 140));
                //    //}); 

                //    //using (var destination = new Bitmap())
                //    //using (var graphics = Graphics.FromImage(destination))
                //    //{
                //    //    graphics.InterpolationMode = InterpolationMode.Default;
                //    //    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                //    //    graphics.FillRectangle(System.Drawing.Brushes.HotPink, new Rectangle(10, 10, 190, 140));

                //    //    //return destination.Size;
                //    //}

                //    // resize the image in place and return it for chaining
                //    //image.Mutate(x => x.Fill(Rgba32.HotPink, new CoreRectangle(10, 10, 190, 140)));
                //    // image is now in a file format agnostic structure in memory as a series of Rgba32 pixels
                //    image.Mutate(ctx => ctx.Resize(image.Width * W_coeff, image.Height * H_coeff)); // resize the image in place and return it for chaining
                //    image.Save(prefix+photofile); // based on the file extension pick an encoder then encode and write the data to disk
                //}

            } // dispose - releasing memory into a memory pool ready for the next image you wish to process
            var resizedphoto = prefix + photofile;
            var photo = resizedphoto.Split('\\').Last();
            // var image2 = new InstaImage { Uri = resizedphoto, /*Height = 1920*/ Height = 1920, Width = 1080 };
            var image = new InstaImage { Uri = photofile, /*Height = 1920*/ Height = 1920, Width = 1080 };

            var result = await InstaApi.StoryProcessor.UploadStoryPhotoAsync(image, prefix);
            Console.WriteLine(result.Succeeded
                ? $"Story created: {result.Value.Media.Pk}"
                : $"Unable to upload photo story: {result.Info.Message}");
        }

        public async void UploadWithOptions(string photofile, string caption, IInstaApi InstaApi)
        {
            // You can add hashtags or locations or poll questions to your photo/video stories!
            // Note that you must draw your hashtags/location names/poll questions in your image first and then upload it!

            var storyOptions = new InstaStoryUploadOptions();
            // Add hashtag
            storyOptions.Hashtags.Add(new InstaStoryHashtagUpload
            {
                X = 0.5, // center of image
                Y = 0.5, // center of image
                Z = 0,
                Width = 0.3148148,
                Height = 0.110367894,
                Rotation = 0,
                TagName = "meme"
            });

            //// Add poll question
            //storyOptions.Polls.Add(new InstaStoryPollUpload
            //{
            //    X = 0.5, // center of image
            //    Y = 0.5, // center of image
            //    Z = 0,
            //    Width = 0.3148148,
            //    Height = 0.110367894,
            //    Rotation = 0,
            //    Question = "Do you love IRAN?",
            //    Answer1 = "Are", // "YES" answer
            //    Answer2 = "Na" // "NO" answer
            //});

            //// Add location
            //var locationsResult = await InstaApi.LocationProcessor.SearchLocationAsync(0, 0, "Moscow");
            //var firstLocation = locationsResult.Value.FirstOrDefault();
            //var locationId = firstLocation.ExternalId;

            //storyOptions.Locations.Add(new InstaStoryLocationUpload
            //{
            //    X = 0.5, // center of image
            //    Y = 0.5, // center of image
            //    Z = 0,
            //    Width = 0.3148148,
            //    Height = 0.110367894,
            //    Rotation = 0,
            //    LocationId = locationId
            //});


            //// Mention people
            //storyOptions.Mentions.Add(new InstaStoryMentionUpload
            //{
            //    X = 0.5, // center of image
            //    Y = 0.5, // center of image
            //    Z = 0,
            //    Width = 0.7972222,
            //    Height = 0.21962096,
            //    Rotation = 0,
            //    Username = "rmt4006"
            //});

            //// Add story question
            //storyOptions.Questions.Add(new InstaStoryQuestionUpload
            //{
            //    X = 0.5, // center of image
            //    Y = 0.2, // center of image
            //    Z = 0,
            //    Width = 0.9507363,
            //    Height = 0.32469338000000003,
            //    Rotation = 0,
            //    Question = "ROFL?",
            //    BackgroundColor = "#ffffff", // #ffffff is white
            //    TextColor = "#000000" // #000000 is black
            //});

            var image = new InstaImage { Uri = photofile,Height=1080, Width=1080 };

            var result = await InstaApi.StoryProcessor.UploadStoryPhotoAsync(image, caption, storyOptions);
            // upload video
            //var result = await InstaApi.MediaProcessor.UploadVideoAsync(video, "ramtinak", storyOptions);
            Console.WriteLine(result.Succeeded
                ? $"Story created: {result.Value.Media.Pk}"
                : $"Unable to upload photo story: {result.Info.Message}");
        }


        public async Task UploadPhotoToProfile(string photofile, string caption, IInstaApi InstaApi)
        {
            var photo = photofile.Split('\\').Last();

            var mediaImage = new InstaImageUpload
            {
                // leave zero, if you don't know how height and width is it.
                Height = 0,
                Width = 0,
                //Uri = @"c:\someawesomepicture.jpg"
                Uri = @photofile
            };
            //// Add user tag (tag people)
            //mediaImage.UserTags.Add(new InstaUserTagUpload
            //{
            //    Username = "rmt4006",
            //    X = 0.5,
            //    Y = 0.5
            //});

           // string postmessage = caption + " #мемы #meme #помойкамемов #memes #memesdaily #memestagram";
            var result = await InstaApi.MediaProcessor.UploadPhotoAsync(mediaImage, caption);
            Console.WriteLine(result.Succeeded
                ? $"Media created: {result.Value.Pk}, {result.Value.Caption}"
                : $"Unable to upload photo: {result.Info.Message}");
        }

        //EXAMPLE from GIT
        public string TesseractTextFromPhoto(string photo, string[] args)
        {
            var testImagePath = photo;
            var ocrtext = string.Empty;

            try

            {

                using (var engine = new TesseractEngine(@"./tessdata", "rus", EngineMode.Default))

                {

                    using (var img = Pix.LoadFromFile(testImagePath))

                    {

                        using (var page = engine.Process(img))

                        {

                            var text = page.GetText();

                            Console.WriteLine("Mean confidence: {0}", page.GetMeanConfidence());

                            ocrtext = text;

                            Console.WriteLine("Text (GetText): \r\n{0}", text);

                            //Console.WriteLine("Text (iterator):");

                            //using (var iter = page.GetIterator())

                            //{

                            //    iter.Begin();



                            //    do

                            //    {

                            //        do

                            //        {

                            //            do

                            //            {

                            //                do

                            //                {

                            //                    if (iter.IsAtBeginningOf(PageIteratorLevel.Block))

                            //                    {

                            //                        Console.WriteLine("<BLOCK>");

                            //                    }



                            //                    Console.Write(iter.GetText(PageIteratorLevel.Word));

                            //                    Console.Write(" ");



                            //                    if (iter.IsAtFinalOf(PageIteratorLevel.TextLine, PageIteratorLevel.Word))

                            //                    {

                            //                        Console.WriteLine();

                            //                    }

                            //                } while (iter.Next(PageIteratorLevel.TextLine, PageIteratorLevel.Word));



                            //                if (iter.IsAtFinalOf(PageIteratorLevel.Para, PageIteratorLevel.TextLine))

                            //                {

                            //                    Console.WriteLine();

                            //                }

                            //            } while (iter.Next(PageIteratorLevel.Para, PageIteratorLevel.TextLine));

                            //        } while (iter.Next(PageIteratorLevel.Block, PageIteratorLevel.Para));

                            //    } while (iter.Next(PageIteratorLevel.Block));

                            //}

                        }

                    }

                }

            }

            catch (Exception e)

            {

                Trace.TraceError(e.ToString());

                Console.WriteLine("Unexpected Error: " + e.Message);

                Console.WriteLine("Details: ");

                Console.WriteLine(e.ToString());

            }


            return ocrtext;
        }
        private int DownloadTempPhoto(string photourl, int i, string name)
        {
            using (WebClient webClient = new WebClient())
            {
                try
                {

                    webClient.DownloadFile(photourl, photofilename + i + ".jpg");
                    webClient.Dispose();
                    //Bitmap bit2 = new Bitmap(photofilename + i + ".jpg");
                    //pictureBox1.Image = bit2;

                }
                catch (Exception)
                {

                   Console.WriteLine("Невозможно подключиться к серверу загрузки фото.\nПопробуйте добавить другую картинку");
                }

            }
            return i;
        }

        public async static Task<HttpResponseMessage> SendPhoto(string chatId, string filePath, string token)
        {
            var url = string.Format("https://api.telegram.org/bot{0}/sendPhoto", token);
            var fileName = filePath.Split('\\').Last();

            HttpResponseMessage response;
            using (var form = new MultipartFormDataContent())
            {
                form.Add(new StringContent(chatId.ToString(), Encoding.UTF8), "chat_id");

                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    form.Add(new StreamContent(fileStream), "photo", fileName);

                    using (var client = new HttpClient())
                    {
                       response= await client.PostAsync(url, form);
                    }
                }
            }
            //возвращать результат
            return response;
        }

        public async Task<Telegram.Bot.Types.Message> sendMessage(string destID, string text)
        {

            Telegram.Bot.Types.Message response = null;
            try
            {
               var bot = new Telegram.Bot.TelegramBotClient(apiToken);
               response=  await bot.SendTextMessageAsync(destID, text);
            }
            catch (Exception e)
            {
                Console.WriteLine("err sending message");
            }
            return response;
        }

       


    }
}
