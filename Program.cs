using System.Text;
using System.Text.Json;

namespace DiscordDrive;

internal static class Program
{
    //https://discord.com/api/webhooks/1069851364311699516/qEUdynnbzizprfGRDTiJwzCIhyMP4A1UMunssjz23ZLqBBuy60BfN9hNeCOui0GuNSXK
    private static string WebhookURL = "";
    private static HttpClient httpClient = new HttpClient();
    const string WebhookFile = "WebhookURL.txt";
    static void Main()
    {
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Title = "DiscordDrive - Store your files on Discord for free! - Made by Kye#5000, github.com/kyeondiscord";
        Console.WriteLine("DiscordDrive - Store your files on Discord for free!");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Made by Kye#5000, github.com/kyeondiscord");

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("Choose an option:");
        Console.WriteLine("1. Upload a file");
        Console.WriteLine("2. Download a file");
        Console.WriteLine("3. Hash a file");

        string option = Console.ReadLine();
        switch (option)
        {
            case "1":
                {
                    if (File.Exists(WebhookFile))
                    {
                        WebhookURL = File.ReadAllText(WebhookFile);
                    }
                    else
                    {
                        Console.Clear();
                        Console.WriteLine("Enter Discord Webhook URL:");
                        WebhookURL = Console.ReadLine();
                        File.WriteAllText(WebhookFile, WebhookURL);
                    }

                    Console.WriteLine("Uploading File to " + WebhookURL);

                    string filename = OpenFileNameNative.OpenFileDialogue();
                    if (filename != null)
                    {
                        DiscordFile file = UploadFile(File.OpenRead(filename), Path.GetFileName(filename));
                        if (file.Chunks.Count > 0)
                        {
                            Directory.CreateDirectory("DiscordDrive\\Uploaded");
                            string filejson = JsonSerializer.Serialize(file, new JsonSerializerOptions() { WriteIndented = true });
                            File.WriteAllText(Path.Combine("DiscordDrive\\Uploaded", file.FileName + ".json"), filejson);
                        }
                    }
                }
                break;
            case "2":
                {
                    string filename = OpenFileNameNative.OpenFileDialogue("Discord Drive File\0*.json*\0");

                    if (filename != null)
                    {
                        DiscordFile fileToDownload = JsonSerializer.Deserialize<DiscordFile>(File.ReadAllText(filename))!;
                        Console.WriteLine("Loaded file: " + fileToDownload.FileName + " with " + fileToDownload.Chunks.Count + " chunks. " + fileToDownload.FileSize + " bytes.");
                        DownloadFile(fileToDownload);

                        string downloadedFile = Path.Combine("DiscordDrive\\Downloaded", fileToDownload.FileName);
                        if (File.Exists(downloadedFile))
                        {
                            string hash = Hashing.SHA256CheckSum(downloadedFile);
                            Console.WriteLine("SHA256 Checksum: " + hash);//fix this hash not wanting to fking work
                            if (hash == fileToDownload.SHA256)
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("File checksums match!");
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("File checksums DO NOT MATCH!");
                            }
                            Console.ResetColor();
                        }
                    }
                }

                break;
            case "3":
                {
                    string filename = OpenFileNameNative.OpenFileDialogue();

                    if (filename != null)
                    {
                        string hash = Hashing.SHA256CheckSum(filename);
                        Console.WriteLine("SHA256 Checksum: " + hash);
                    }
                    break;
                }
            default:
                Console.WriteLine("Invalid option.");
                break;
        }

        Console.WriteLine("Press any key to exit.");
        Console.ReadKey();
    }

    static DiscordFile UploadFile(Stream fileStream, string FileName)
    {
        DiscordFile file = new();
        Console.WriteLine($"Uploading {FileName}");
        file.FileName = FileName;
        file.FileSize = fileStream.Length;
        file.SHA256 = Hashing.SHA256CheckSum(fileStream);
        var chunks = SplitStream(fileStream, 1000000 * 8);
        Console.WriteLine($"Filesize is {file.FileSize} bytes with {chunks.Count} chunks");
        Console.WriteLine($"SHA256 Checksum: {file.SHA256}");
        for (int i = 0; i < chunks.Count; i++)
        {

            MultipartFormDataContent form = new MultipartFormDataContent
            {
                { new ByteArrayContent(chunks[i], 0, chunks[i].Length), "file1", (i + 1).ToString() }
            };

            if (i == 0)
            {
                string payload = "{ \"embeds\": [ { \"title\": \"File Upload: %filename% | %filesize% Bytes | %chunks% Chunk(s)\", \"color\": 2913013 } ] }";
                payload = payload.Replace("%filename%", file.FileName);
                payload = payload.Replace("%filesize%", file.FileSize.ToString());
                payload = payload.Replace("%chunks%", chunks.Count().ToString());
                form.Add(new StringContent(payload), "payload_json");
            }

            HttpResponseMessage response = httpClient.PostAsync(WebhookURL, form).GetAwaiter().GetResult();
            if (response.IsSuccessStatusCode)
            {
                MessageModal.Root msg = JsonSerializer.Deserialize<MessageModal.Root>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                Console.WriteLine("Successfully uploaded chunk " + (i + 1) + "/" + chunks.Count);
                string url = msg.attachments[0].url;

                if (i == 0)
                {

                    //https://cdn.discordapp.com/attachments/1069851340924276736/1069895919480344576/Part1
                    // into
                    //https://cdn.discordapp.com/attachments/1069851340924276736/


                    string result = url.Substring(0, url.LastIndexOf('/'));
                    result = result.Substring(0, result.LastIndexOf('/') + 1);

                    file.BaseURL = result;
                }
                string[] urlsplit = url.Split('/');
                string Route = urlsplit[5] + "/" + urlsplit[6];
                file.Chunks.Add(new Chunk(i, Route));
            }
            else
            {
                Console.WriteLine("Failed to upload chunk " + i + ", error code " + response.StatusCode);
            }
        }
        return file;
    }

    static void DownloadFile(DiscordFile discordFile)
    {
        Directory.CreateDirectory("DiscordDrive\\Downloaded");
        using FileStream newFile = File.Create(Path.Combine("DiscordDrive\\Downloaded", discordFile.FileName));
        foreach (Chunk chunk in discordFile.Chunks.OrderBy(x => x.PartNumber))
        {
            byte[] curChunk = httpClient.GetByteArrayAsync(discordFile.BaseURL + chunk.Route).GetAwaiter().GetResult();
            Console.WriteLine("Downloaded chunk " + (chunk.PartNumber + 1) + "/" + discordFile.Chunks.Count);
            newFile.Write(curChunk, 0, curChunk.Length);
        }

        newFile.Flush();

        Console.WriteLine("Downloaded file!");
    }


    static List<byte[]> SplitStream(Stream stream, int chunkSize)
    {
        List<byte[]> result = new List<byte[]>();

        for (int i = 0; i < stream.Length; i += chunkSize)
        {
            stream.Position = i;
            if (i + chunkSize > stream.Length)
            {
                byte[] chunk = new byte[stream.Length - i];

                stream.Read(chunk, 0, (int)(stream.Length - i));
                result.Add(chunk);
            }
            else
            {
                byte[] chunk = new byte[chunkSize];
                stream.Read(chunk, 0, chunkSize);
                result.Add(chunk);
            }
        }
        return result;
    }
}