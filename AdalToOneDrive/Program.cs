using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AdalToOneDrive
{
    class Program
    {
        static void Main(string[] args)
        {
            //Task.Run(async () =>
            //{
                authenticateAsync();
            //}).Wait();
        }

        private static void authenticateAsync()
        {

            var tenantId = "b7471dd6-decc-4868-9d86-02e711af554e";
            var clientId = "2e408c1e-789b-42e3-bf23-0b7b9e437c9f";
            var clientKey = "EiZ9GIc/UhOdQXDdFtvBiPVMRcj354Wyy8x2gRb8DHY=";
            //var resourceId = "https://qpickit-my.sharepoint.com";
            var resourceId = "https://graph.microsoft.com";
            var authorityUrl = "https://login.windows.net/{0}";

            AuthenticationContext authContext = new AuthenticationContext(String.Format(authorityUrl, tenantId));
            ClientCredential clientCredentials = new ClientCredential(clientId, clientKey);
            var accessToken = authContext.AcquireTokenAsync(resourceId, clientCredentials).Result;

            // https://qpickit-my.sharepoint.com/personal/ivan_qpickit_com/Documents/Hi.txt

            var url = "https://graph.microsoft.com/v1.0/users/ivan@qpickit.com/drive/root:/hi.txt";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.AccessToken);
            
            HttpClient client = new HttpClient();
            HttpResponseMessage response = client.SendAsync(request).Result;
            var content = response.Content.ReadAsStringAsync().Result;

            Console.Write(content);

            Console.ReadLine();

            // разбираешь тут контент.. в нем должно быть:

            //content-type: application/json;odata.metadata=minimal;odata.streaming=true;IEEE754Compatible=false;charset=utf-8
            //cache-control: private
            //client-request-id: 41274804-854f-4264-94cf-13437814ab11
            //request-id: 41274804-854f-4264-94cf-13437814ab11
            //Status Code: 200

            //{
            //    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('04e0e608-7df0-4d68-a9ca-d34b6ef2082c')/drive/root/$entity",
            //    "@microsoft.graph.downloadUrl": "https://qpickit-my.sharepoint.com/personal/ivan_qpickit_com/_layouts/15/download.aspx?guestaccesstoken=GB4aiuiQT6haXwjdEmJN5Ie0UrfqdOtt%2bG0X5C2KEkk%3d&docid=089f2f948fcd645c895efd40c4ffb2908&expiration=2016-11-18T00%3a30%3a49.000Z&userid=3&authurl=True&NeverAuth=True",
            //    "createdBy": {
            //        "user": {
            //            "id": "04e0e608-7df0-4d68-a9ca-d34b6ef2082c",
            //            "displayName": "Ivan Krupskiy"
            //        }
            //    },
            //    "createdDateTime": "2016-11-17T22:51:09Z",
            //    "eTag": "\"{89F2F948-FCD6-45C8-95EF-D40C4FFB2908},1\"",
            //    "id": "01B7BVBDKI7HZITVX4ZBCZL36UBRH7WKII",
            //    "lastModifiedBy": {
            //        "user": {
            //            "id": "04e0e608-7df0-4d68-a9ca-d34b6ef2082c",
            //            "displayName": "Ivan Krupskiy"
            //        }
            //    },
            //    "lastModifiedDateTime": "2016-11-17T22:51:09Z",
            //    "name": "Hi.txt",
            //    "webUrl": "https://qpickit-my.sharepoint.com/personal/ivan_qpickit_com/Documents/Hi.txt",
            //    "cTag": "\"c:{89F2F948-FCD6-45C8-95EF-D40C4FFB2908},1\"",
            //    "file": {
            //        "hashes": {
            //            "quickXorHash": "SEgDCILAhjLgoQx5A0EIAAAAAAA="
            //        }
            //    },
            //    "parentReference": {
            //        "driveId": "b!2twqMXGwxUqQI82y2ZjslRUf0UJ8gMhIlXcnsE8DqEZ09Y0PqaooTqbMex78lyLl",
            //        "id": "01B7BVBDN6Y2GOVW7725BZO354PWSELRRZ",
            //        "path": "/drive/root:"
            //    },
            //    "size": 11
            //}

            // Добываешь @microsoft.graph.downloadUrl

            // и скачиваешь файл без всякой авторизации..

            // полезные ссылки:

            // https://graph.microsoft.io/en-us/graph-explorer#/ - попробуй вставить туда https://graph.microsoft.com/v1.0/users/ivan@qpickit.com/drive/root:/hi.txt

            // https://graph.microsoft.io/en-us/docs/api-reference/v1.0/resources/onedrive - документация..
            // у меня почему-то не заработало https://graph.microsoft.com/v1.0/users/ivan@qpickit.com/drive/root:/hi.txt:/content
        }
    }
}
