using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Multi
{
    public class MultiClass
    {
        private string BaseIP = "192.168.1.";
        private int StartIP = 1;
        private int StopIP = 255;
        private string ip;
        private List<PingReply> replies { get; set; }
        public MultiClass()
        {
            replies = new List<PingReply>();
        }
        private int timeout = 100;
        private int nFound = 0;

        static object lockObj = new object();
        Stopwatch stopWatch = new Stopwatch();
        TimeSpan ts;
        //MultiClass()
        //{

        //}
        //MultiClass(List<PingReply> _replies)
        //{
        //    this.replies = _replies;
        //}
        public async void RunPingSweep_Async()
        {
            nFound = 0;

            var tasks = new List<Task>();

            stopWatch.Start();

            for (int i = StartIP; i <= StopIP; i++)
            {
                ip = BaseIP + i.ToString();

                System.Net.NetworkInformation.Ping p = new System.Net.NetworkInformation.Ping();
                var task = PingAndUpdateAsync(p, ip);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks).ContinueWith(t =>
            {
                stopWatch.Stop();
                ts = stopWatch.Elapsed;
                Console.WriteLine(nFound.ToString() + " devices found! Elapsed time: " + ts.ToString(), "Asynchronous");
                if(replies is not null)
                    foreach (var reply in replies)
                    {
                        Console.WriteLine($"{reply.Address} - {reply.RoundtripTime}");
                    }
            });
        }


        private async Task PingAndUpdateAsync(Ping ping, string ip)
        {
            var reply = await ping.SendPingAsync(ip, timeout);

            if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
            {
                lock (lockObj)
                {
                    nFound++;
                    replies.Add(reply);
                }
            }
        }
    }
}
