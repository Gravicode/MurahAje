using Microsoft.Bot.Builder.FormFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.FormFlow.Advanced;
using Microsoft.Bot.Builder.Resource;
using System.Resources;
using System.Text;
using System.Threading;
using Microsoft.Azure; // Namespace for CloudConfigurationManager
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Queue; // Namespace for Queue storage types
using System.Configuration;
using Newtonsoft.Json;

namespace MurahAje.Bot
{
    [Serializable]
    public class ReportDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("Sudah siap melapor bos ?");
            var ReportFormDialog = FormDialog.FromForm<Laporan>(Laporan.BuildForm, FormOptions.None);
            context.Call(ReportFormDialog, this.ResumeAfterReportFormDialog);
        }
        private async Task ResumeAfterReportFormDialog(IDialogContext context, IAwaitable<Laporan> result)
        {
            try
            {
                var hasil = await result;
                //do nothing
            }
            catch (FormCanceledException ex)
            {
                string reply;

                if (ex.InnerException == null)
                {
                    reply = "bos membatalkan laporan, dialog ditutup.";
                }
                else
                {
                    reply = $"Ada masalah teknis euy:( Detailnya: {ex.InnerException.Message}";
                }

                await context.PostAsync(reply);
            }
            finally
            {
                context.Done<object>(null);
            }
        }

    }
    public enum TipeLaporan
    {
        [Terms("kesalahan / bug", "saran", "kritik")]
        kesalahan = 1,
        saran,
        kritik
    }
    [Serializable]
    public class Laporan
    {
        public string NoLaporan;
        public DateTime TglLaporan;
        [Prompt("Siapa nama Bos ? {||}")]
        public string Nama;

        [Prompt("Berapa No. telponnya ? {||}")]
        public string Telpon;

        [Prompt("Alamat E-mail Bos ? {||}")]
        public string Email;

        [Prompt("Apa yang ingin disampaikan bos ? {||}")]
        public TipeLaporan TipeLaporan;

        [Prompt("Silakan masukan keterangan / laporan Bos.. {||}")]
        public string Keterangan;

        [Prompt("Terjadi di halaman apa (bisa bos copy linknya kemari) ? {||}")]
        public string Modul;

        [Prompt("Kapan Bos lihat format (year/month/date jam:menit) ? {||}")]
        public DateTime Waktu;

        [Prompt("Masukan perkiraan skala prioritas, 1 [tidak penting] - 10 [sangat penting] ? ")]
        public int SkalaPrioritas = 1;

        public static IForm<Laporan> BuildForm()
        {

            OnCompletionAsyncDelegate<Laporan> processReport = async (context, state) =>
            {
                await Task.Run(() =>
                {
                    state.NoLaporan = $"LP-{DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss")}";
                    state.TglLaporan = DateTime.Now;
                    // Retrieve storage account from connection string.
                    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                        ConfigurationManager.AppSettings["StorageConnectionString"]);

                    // Create the queue client.
                    CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

                    // Retrieve a reference to a queue.
                    CloudQueue queue = queueClient.GetQueueReference("laporan");

                    // Create the queue if it doesn't already exist.
                    queue.CreateIfNotExists();

                    // Create a message and add it to the queue.
                    CloudQueueMessage message = new CloudQueueMessage(JsonConvert.SerializeObject(state));
                    queue.AddMessage(message);
                    Console.WriteLine("Push data ke que");
                }
                );

            };
            var builder = new FormBuilder<Laporan>(false);
            var form = builder
                        .Field(nameof(Nama))
                        .Field(nameof(Telpon))
                        .Field(nameof(Email))
                        .Field(nameof(TipeLaporan))
                        .Field(nameof(Keterangan))
                        .Field(nameof(Modul))
                        .Field(nameof(Waktu))
                        .Field(nameof(SkalaPrioritas), validate:
                            async (state, value) =>
                            {
                                var result = new ValidateResult { IsValid = true, Value = value, Feedback = "ok, skala valid" };
                                var jml = int.Parse(value.ToString());
                                if (jml <= 0)
                                {
                                    result.Feedback = "Isilah dengan serius, prioritas minimal nilainya 1";
                                    result.IsValid = false;
                                }
                                else if (jml > 10)
                                {
                                    result.Feedback = "Jangan main-main dunk, prioritas tertinggi itu 10";
                                    result.IsValid = false;
                                }
                                return result;
                            })
                        .Confirm(async (state) =>
                        {
                            var pesan = $"Laporan dari {state.Nama} tentang {state.TipeLaporan.ToString()} sudah kami terima, apakah data ini sudah valid ?";
                            return new PromptAttribute(pesan);
                        })
                        .Message($"Terima kasih atas laporannya.")
                        .OnCompletion(processReport)
                        .Build();
            return form;
        }
    }
}