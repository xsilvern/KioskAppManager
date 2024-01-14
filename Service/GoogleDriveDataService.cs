using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using KioskAppServer.ViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace KioskAppServer.Service
{
    public class GoogleDriveDataService
    {
        public readonly string _imageFolderName = "KioskImages";
        private UserCredential _userCredential;
        private DriveService _driveService;

        public GoogleDriveDataService()
        {
            
        }

        private async Task<UserCredential> AuthorizeGoogleDriveAsync()
        {
            using (var stream = new FileStream("client_secrets.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = "token.json";//토큰 생성해서 로컬에 저장 후 재로그인 필요 없도록 만들기
                return await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    new[] { DriveService.Scope.Drive },
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true));
            }
        }

        public async Task InitializeDriveService()
        {
            _userCredential = await AuthorizeGoogleDriveAsync();
            if (_userCredential != null)
            {
                _driveService = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = _userCredential,
                    ApplicationName = "WPF Drive API",
                });
                
            }
            else
            {
                MessageBox.Show("구글 계정 연동 실패");
            }
        }
        public string ConvertBitmapImageToBase64(BitmapImage bitmapImage)
        {
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));

            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Save(ms);
                byte[] imageBytes = ms.ToArray();
                return Convert.ToBase64String(imageBytes);
            }
        }
        public async Task<KioskDataSet> GetKioskDataFromDriveAsync(string fileName)
        {
            await InitializeDriveService();
            var listRequest = _driveService.Files.List();
            listRequest.Q = $"name = '{fileName}' and mimeType = 'application/json'";
            var files = listRequest.Execute();

            if (files.Files != null && files.Files.Count > 0)
            {
                var file = files.Files[0];
                var request = _driveService.Files.Get(file.Id);
                using (var memoryStream = new MemoryStream())
                {
                    // 스트림을 사용하여 파일 다운로드
                    request.Download(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    using (var reader = new StreamReader(memoryStream))
                    {
                        string jsonContent = reader.ReadToEnd();
                        return JsonConvert.DeserializeObject<KioskDataSet>(jsonContent);
                    }
                }
            }
            else
            {
                MessageBox.Show("구글 드라이브에 파일이 존재하지 않아 새로 제작합니다.");

                var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = fileName,
                    MimeType = "application/json"
                };

                KioskDataSet kioskData = new KioskDataSet();
                string jsonContent = JsonConvert.SerializeObject(kioskData, Formatting.Indented);

                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonContent)))
                {
                    var request = _driveService.Files.Create(fileMetadata, stream, "application/json");
                    request.Fields = "id";
                    request.Upload();
                }

                return kioskData;
            }
        }

        public void UploadToDrive<T>(string fileName,T uploadData)
        {
            //현재는 KioskData를 저장시킨다
            string jsonContent = JsonConvert.SerializeObject(uploadData, Formatting.Indented);

            // 기존 파일 검색
            var listRequest = _driveService.Files.List();
            listRequest.Q = $"name = '{fileName}'";
            var files = listRequest.Execute().Files;
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = fileName,
                MimeType = "application/json"
            };

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonContent)))
            {
                if (files != null && files.Count > 0)
                {
                    FilesResource.UpdateMediaUpload request;
                    // 기존 파일이 있는 경우, 해당 파일을 업데이트, id로 구분
                    var existingFileId = files[0].Id;
                    request = _driveService.Files.Update(fileMetadata, existingFileId, stream, "application/json");
                    request.Upload();
                }
                else
                {
                    FilesResource.CreateMediaUpload request;
                    // 새 파일 업로드
                    request = _driveService.Files.Create(fileMetadata, stream, "application/json");
                    request.Fields = "id";
                    request.Upload();
                }
            }
        }

        //폴더가 없으면 생성하고 있으면 그것의 ID 반환하는 메서드
        public async Task<string> CreateFolderIfNotExistAsync(string folderName)
        {
            //MimeType을 이용해서 쿼리문 검색하기
            var searchQuery = $"mimeType = 'application/vnd.google-apps.folder' and name = '{folderName}'";
            var listRequest = _driveService.Files.List();
            listRequest.Q = searchQuery;
            var files = await listRequest.ExecuteAsync();

            if (files.Files != null && files.Files.Count > 0)
            {
                return files.Files[0].Id;
            }
            else
            {
                var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = folderName,
                    MimeType = "application/vnd.google-apps.folder"
                };
                var createRequest = _driveService.Files.Create(fileMetadata);
                createRequest.Fields = "id";
                var folder = await createRequest.ExecuteAsync();
                return folder.Id;
            }
        }
        //기존에 Base64를 이용한 Json 변환으로 저장했더니 데이터가 너무 커져서 경로로 대체.
        public async Task<string> UploadImageAsync(string imagePath)
        {
            string folderId = await CreateFolderIfNotExistAsync(_imageFolderName);

            // 파일 확장자에 따른 MIME 타입
            string mimeType = GetMimeType(Path.GetExtension(imagePath).ToLower());

            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = Path.GetFileName(imagePath),
                Parents = new List<string> { folderId }
            };

            FilesResource.CreateMediaUpload request;
            using (var stream = new FileStream(imagePath, FileMode.Open))
            {
                request = _driveService.Files.Create(fileMetadata, stream, mimeType);
                request.Fields = "id";
                await request.UploadAsync();
            }

            var file = request.ResponseBody;
            return file.Id;
        }

        // MIME 타입을 결정
        private string GetMimeType(string extension)
        {
            switch (extension)
            {
                case ".jfif":// JFIF 파일은 JPEG과 같은 MIME 타입을 사용한다고 함
                case ".jpeg":
                case ".jpg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                default:
                    throw new ArgumentException("Unsupported type");
            }
        }

        public async Task DeleteImageAsync(string fileId)
        {
            await _driveService.Files.Delete(fileId).ExecuteAsync();
        }
    }
}
