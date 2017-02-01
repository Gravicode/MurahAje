using System;
using Microsoft.Bot.Builder.FormFlow;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using System.Configuration;
using Microsoft.Bot.Builder.Dialogs;

namespace MurahAje.Bot
{
    [Serializable]
    public class OrderDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("Selamat datang bos, sudah mau order ?");
            var OrderFormDialog = FormDialog.FromForm<OrderQuery>(OrderQuery.BuildForm, FormOptions.None);
            context.Call(OrderFormDialog, this.ResumeAfterOrderFormDialog);
        }
        
        private async Task ResumeAfterOrderFormDialog(IDialogContext context, IAwaitable<OrderQuery> result)
        {
            try
            {
                var hasil = await result;
                
            }
            catch (FormCanceledException ex)
            {
                string reply;

                if (ex.InnerException == null)
                {
                    reply = "Bos membatalkan order, keluar dari dialog.";
                }
                else
                {
                    reply = $"Sorry terjadi kesalahan bos :( Detail: {ex.InnerException.Message}";
                }

                await context.PostAsync(reply);
            }
            finally
            {
                context.Done<object>(null);
            }
        }
      
    }

    [Serializable]
    public enum TipeOrder
    {
        [Terms("makan & minuman", "barang bekas", "tempat nginep")]
        kuliner = 1,
        garagesale,
        placetostay
    }
    [Serializable]
    //[Template(TemplateUsage.NotUnderstood, "Ane ga paham \"{0}\".", "Coba lagi ya, ane tidak dapat nilai \"{0}\".")]
    public class OrderQuery
    {
        public DateTime TanggalOrder;
        public string NoOrder;
        [Prompt("Siapa namanya bos ? {||}")]
        public string Nama;

        [Prompt("Boleh minta alamatnya ? {||}")]
        public string Alamat;

        [Prompt("Berapa no telponnya ? {||}")]
        public string Telpon;

        [Prompt("Emailnya boleh dunk? {||}")]
        public string Email;

        [Prompt("Mo pesen apaan ? {||}")]
        public TipeOrder TipePesanan;

        [Prompt("Coba tulis kode barang dan jumlah-nya bos, pisahin pake koma kalau lebih dari satu (contoh: K001=1, K002=4) {||}")]
        public string Pesanan;


        public static IForm<OrderQuery> BuildForm()
        {

            OnCompletionAsyncDelegate<OrderQuery> processOrder = async (context, state) =>
            {
                await Task.Run(() =>
                {
                    state.NoOrder = $"OR-{DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss")}";
                    state.TanggalOrder = DateTime.Now;
                    // Retrieve storage account from connection string.
                    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                        ConfigurationManager.AppSettings["StorageConnectionString"]);

                    // Create the queue client.
                    CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

                    // Retrieve a reference to a queue.
                    CloudQueue queue = queueClient.GetQueueReference("order");

                    // Create the queue if it doesn't already exist.
                    queue.CreateIfNotExists();

                    // Create a message and add it to the queue.
                    CloudQueueMessage message = new CloudQueueMessage(JsonConvert.SerializeObject(state));
                    queue.AddMessage(message);
                    Console.WriteLine("Push data ke que");
                }
                 );
            };
            var builder = new FormBuilder<OrderQuery>(false);
            var form = builder
                        .Field(nameof(Nama))
                        .Field(nameof(Alamat))
                        .Field(nameof(Telpon))
                        .Field(nameof(Email))
                        .Field(nameof(Pesanan))
                        /*
                        .Field(nameof(Jumlah), validate:
                            async (state, value) =>
                            {
                                var result = new ValidateResult { IsValid = true, Value = value, Feedback = "barang tersedia" };
                                var jml = int.Parse(value.ToString());
                                var stok = datas.StockBarang[state.Pesanan];
                                if (jml <= 0)
                                {
                                    result.Feedback = $"Serius dulu lah bos belanjanya, masa beli sejumlah {jml} ?";
                                    result.IsValid = false;
                                }
                                else
                                if (jml > stok)
                                {
                                    result.Feedback = $"Board {state.Pesanan} stoknya hanya tinggal {stok} buah";
                                    result.IsValid = false;
                                    //result.Value = 0;
                                }
                                return result;


                            })*/
                        .Confirm(async (state) =>
                        {
                            var pesan = $"Pesanan {state.TipePesanan.ToString()} bos {state.Nama} adalah {state.Pesanan}, sudah ok bos ?";
                            return new PromptAttribute(pesan);
                        })
                        .Message("Makasih dah order disini bos, segera kami proses!")
                        .OnCompletion(processOrder)
                        .Build();
            return form;
        }
    }
}