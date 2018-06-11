using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace IOTCoreMasterApp.DataModel
{
    public class TestLog
    {
        private Windows.Storage.StorageFile sampleFile;



        private StringBuilder _sb = new StringBuilder();

        public TestLog(string sFileName)
        {
            GetFile(sFileName);
        }

        private async void GetFile(string sFileName)
        {
            try
            {
                //檢查檔案是否存在                

                sampleFile = await KnownFolders.VideosLibrary.CreateFileAsync(sFileName, CreationCollisionOption.ReplaceExisting);

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async void WriteText(string sMessage)
        {
            try
            {

                //await Windows.Storage.FileIO.WriteTextAsync(sampleFile, sMessage);
                await Windows.Storage.FileIO.AppendTextAsync(sampleFile, sMessage);




                //讀取驗證
                /*                
                await ReadText()
                */
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 讀取驗證，是否有成功寫入
        /// </summary>
        /// <param name="sb"></param>
        /// <returns></returns>
        public async Task ReadText()
        {
            _sb.Clear();

            using (IRandomAccessStream stream = await sampleFile.OpenReadAsync())
            {
                using (StreamReader rd = new StreamReader(stream.AsStreamForRead(), System.Text.Encoding.UTF8))
                {
                    _sb.Append(rd.ReadToEnd());
                }
            }

        }
    }
}
