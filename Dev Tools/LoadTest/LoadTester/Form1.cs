using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using HtmlAgilityPack;
using Rock;

namespace LoadTester
{
    public partial class Form1 : Form
    {
        private static int clientCount;
        private static int requestDelayMS;
        private static int startOffsetMS;
        private static string url;
        private static bool downloadHeaderSrcElements;
        private static bool downloadBodySrcElements;
        private static ASCIIEncoding asciiEncoding;
        private static NameValueCollection postData;
        private static byte[] postBytes;
        private static ConcurrentBag<ChartData> chartResults;
        private static ConcurrentBag<Exception> exceptions;
        private static string statsText;
        private static long requestCount = 0;
        private static long responseCount = 0;
        private static int threadCount = 0;
        private static string requestMethod = "GET";
        private static bool keepRunning = false;
        private static int testDurationMS = 0;
        private static Stopwatch stopwatchTestDuration = null;
        private static List<Task> requestTasks = null;

        public Form1()
        {
            InitializeComponent();
        }

        public static readonly string[] UserAgentStrings = new string[] {
            "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 5.2; Trident/4.0)",
            "Mozilla/5.0 (Linux; Android 5.0; SM-G900V Build/LRX21T) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.83 Mobile Safari/537.36",
            "Mozilla/5.0 (iPhone; CPU iPhone OS 9_0_2 like Mac OS X) AppleWebKit/601.1.46 (KHTML, like Gecko) Mobile/13A452",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_11_2) AppleWebKit/601.3.9 (KHTML, like Gecko)",
            "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/48.0.2564.109 Safari/537.36",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_11_2) AppleWebKit/601.3.9 (KHTML, like Gecko) Version/9.0.2 Safari/601.3.9",
            "Safari/11601.4.4 CFNetwork/760.2.6 Darwin/15.3.0 (x86_64)",
            "Mozilla/5.0 (Windows NT 6.1; Trident/7.0; rv:11.0) like Gecko",
            "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.9.0.1) Gecko/2008070208 Firefox/3.0.1",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_10_5) AppleWebKit/601.4.4 (KHTML, like Gecko)",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_10_5) AppleWebKit/601.2.7 (KHTML, like Gecko) Version/9.0.1 Safari/601.2.7",
            "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.106 Safari/537.36",
            "Safari/11601.1.56 CFNetwork/760.0.5 Darwin/15.0.0 (x86_64)",
            "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/48.0.2564.97 Safari/537.36",
            "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)",
            "Mozilla/5.0 (iPhone; CPU iPhone OS 9_0_2 like Mac OS X) AppleWebKit/601.1.46 (KHTML, like Gecko) Mobile/13A452",
            "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.1; WOW64; Trident/6.0)",
            "Mozilla/5.0 (iPhone; CPU iPhone OS 9_1 like Mac OS X) AppleWebKit/600.1.4 (KHTML, like Gecko) CriOS/46.0.2490.73 Mobile/13B143 Safari/600.1.4",
            "Safari/10601.4.4 CFNetwork/720.5.7 Darwin/14.5.0 (x86_64)",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36 Edge/12.10240",
            "Mozilla/5.0 (Linux; Android 5.0; SM-G900V Build/LRX21T) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.83 Mobile Safari/537.36",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_10_5) AppleWebKit/601.1.56 (KHTML, like Gecko)",
            "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2490.71 Safari/537.36",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_10_5) AppleWebKit/600.8.9 (KHTML, like Gecko) Version/8.0.8 Safari/600.8.9",
            "Mozilla/5.0 (iPhone; CPU iPhone OS 9_2_1 like Mac OS X) AppleWebKit/601.1 (KHTML, like Gecko) CriOS/48.0.2564.104 Mobile/13D15 Safari/601.1.46",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_10_5) AppleWebKit/601.3.9 (KHTML, like Gecko) Version/9.0.2 Safari/601.3.9"
        };

        /// <summary>
        /// Handles the Click event of the btnStart control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void btnStart_Click( object sender, EventArgs e )
        {
            Debug.WriteLine( GC.GetTotalMemory( false ) / 1024 );
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();

            Debug.WriteLine( GC.GetTotalMemory( false ) / 1024 );
            keepRunning = true;
            lblStatus.Text = "RUNNING";
            chartResults = new ConcurrentBag<ChartData>();
            exceptions = new ConcurrentBag<Exception>();

            clientCount = tbClientCount.Text.AsInteger();
            requestDelayMS = tbRequestsDelayMS.Text.AsInteger();

            // spread out the start of the threads over the start window
            startOffsetMS = tbStartWindowMS.Text.AsInteger() / clientCount;

            url = tbUrl.Text;
            downloadHeaderSrcElements = cbDownloadHeaderSrcElements.Checked;
            downloadBodySrcElements = cbDownloadBodySrcElements.Checked;
            testDurationMS = (int)( tbTestDurationSecs.Text.AsDecimal() * 1000 );
            asciiEncoding = new ASCIIEncoding();
            postData = HttpUtility.ParseQueryString( string.Empty );
            foreach ( var line in tbPostBody.Lines )
            {
                var lineParts = line.Split( '=' );
                postData.Add( lineParts[0].Trim(), lineParts[1].Trim() );
            }

            postBytes = asciiEncoding.GetBytes( postData.ToString() );

            pgbRequestCount.Visible = testDurationMS == 0;
            pgbRequestCount.Maximum = clientCount;
            pgbRequestCount.Invalidate();

            pgbResponseCount.Visible = testDurationMS == 0;
            pgbResponseCount.Maximum = clientCount;
            pgbResponseCount.Invalidate();

            lblThreadCount.Text = "0";
            lblThreadCount.Visible = true;

            requestMethod = radPOST.Checked ? "POST" : "GET";

            backgroundWorker1.RunWorkerAsync();

            timer1.Enabled = true;
        }

        /// <summary>
        /// Runs the load test.
        /// </summary>
        /// <param name="bw">The bw.</param>
        private static void RunLoadTest( System.ComponentModel.BackgroundWorker bw )
        {
            stopwatchTestDuration = Stopwatch.StartNew();
            requestCount = 0;
            responseCount = 0;
            threadCount = 0;

            var requestUrl = new Uri( url );
            var baseUri = new Uri( requestUrl.Scheme + "://" + requestUrl.Host + ":" + requestUrl.Port.ToString() );

            var random = new Random();

            requestTasks = new List<Task>( clientCount );

            //Parallel.For( 0, clientCount, ( i ) =>
            for ( int i = 0; i < clientCount; i++ )
            {

                var task = new Task( () =>
                {
                    while ( true )
                    {
                        Interlocked.Increment( ref threadCount );
                        bw.ReportProgress( 0 );

                        requestCount++;
                        if ( !keepRunning )
                        {
                            return;
                        }

                        if ( testDurationMS > 0 )
                        {
                            if ( stopwatchTestDuration.ElapsedMilliseconds > testDurationMS )
                            {
                                return;
                            }
                        }

                        DoHttpRequest( baseUri, random );
                        responseCount++;

                        Interlocked.Decrement( ref threadCount );
                        bw.ReportProgress( 0 );

                        if ( testDurationMS == 0 )
                        {
                            break;
                        }
                        else
                        {
                            if ( stopwatchTestDuration.ElapsedMilliseconds > testDurationMS )
                            {
                                break;
                            }
                        }
                    }

                }, TaskCreationOptions.LongRunning );

                requestTasks.Add( task );
            }

            requestTasks.ForEach( a =>
            {
                a.Start();
                Thread.Sleep( startOffsetMS );
            } );


            if ( testDurationMS > 0 )
            {
                Task.WaitAll( requestTasks.ToArray(), testDurationMS );
            }
            else
            {
                Task.WaitAll( requestTasks.ToArray() );
            }

            stopwatchTestDuration.Stop();
            bw.ReportProgress( 0 );
            var results = chartResults.Select( a => a.YValue ).ToList();
            var totalTime = stopwatchTestDuration.Elapsed.TotalMilliseconds;
            var requestsPerMillisecond = requestCount / totalTime;

            var aveResponseTime = totalTime / requestCount;
            var responseLengths = chartResults.Select( a => a.ResponseLength ).Where( a => a > 0 ).ToList();
            var avgResponseLength = responseLengths.Count > 0 ? responseLengths.Average() : 0;
            try
            {
                statsText = string.Empty;
                if ( results.Count > 0 )
                {
                    statsText +=
    $@"
Response Time (ms)
 - Median/Mode/Avg/Max/Min 
 - {results.Median():0.0}/{results.Mode():0.0}/{results.Average():0.0}/{results.Max():0.0}/{results.Min():0.0}";
                }

                statsText += $@"
Total 
 - Requests: {results.Count()}
 - Time: {stopwatchTestDuration.Elapsed.TotalMilliseconds:0.0}ms
 - Exceptions: {exceptions.Count()}
---------------------
Average: {Math.Round( ( avgResponseLength / 1024 ), 0 )}KB responseLength
Requests/sec: {requestsPerMillisecond * 1000:0.0}
---------------------
";
                statsText = statsText.Trim();
            }
            catch ( Exception ex )
            {
                statsText = ex.Message;
            }

            statsText += Environment.NewLine + exceptions.Select( a => a.Message ).ToList().AsDelimited( Environment.NewLine );
        }

        private static void DoHttpRequest( Uri baseUri, Random random )
        {
            try
            {

                CookieContainer cookieContainer = new CookieContainer();

                HttpWebRequest clientRequest = GetClientRequest( url, random, postBytes, cookieContainer );

                var threadStopwatch = Stopwatch.StartNew();
                string pageLoadTimeMS = string.Empty;
                long responseLength = 0;

                var httpResponse = clientRequest.GetResponse() as HttpWebResponse;

                if ( httpResponse.ResponseUri != clientRequest.RequestUri )
                {
                    throw new Exception( "Redirected:" + httpResponse.ResponseUri );
                }

                if ( httpResponse.StatusCode != HttpStatusCode.OK )
                {
                    throw new Exception( "StatusCode:" + httpResponse.StatusCode.ToString() );
                }

                responseLength = 0;

                threadStopwatch.Stop();

                if ( ( downloadHeaderSrcElements || downloadBodySrcElements ) )
                {
                    responseLength = ProcessResponse( exceptions, baseUri, downloadHeaderSrcElements, downloadBodySrcElements, httpResponse );
                }

                threadStopwatch.Stop();
                chartResults.Add( new ChartData
                {
                    XValue = new DateTime( 2016, 1, 1 ).Add( stopwatchTestDuration.Elapsed ),
                    YValue = Math.Round( threadStopwatch.Elapsed.TotalMilliseconds, 3 ),
                    Series = string.Format( "ThreadId:{0}", Thread.CurrentThread.ManagedThreadId ),
                    ResponseLength = responseLength
                } );

                System.Threading.Thread.Sleep( requestDelayMS );
            }
            catch ( Exception ex )
            {
                exceptions.Add( ex );
            }
        }

        private static HttpWebRequest GetClientRequest( string url, Random random, byte[] postBytes, CookieContainer cookieContainer )
        {
            var clientRequest = HttpWebRequest.CreateHttp( url );
            clientRequest.Proxy = null;
            clientRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            clientRequest.CookieContainer = cookieContainer;
            clientRequest.UserAgent = UserAgentStrings[0];// random.Next( 0, UserAgentStrings.Count() - 1 )];
            clientRequest.Timeout = 60000;
            clientRequest.ReadWriteTimeout = 60000;
            clientRequest.Method = requestMethod;

            if ( requestMethod == "POST" )
            {

                clientRequest.ContentType = "application/x-www-form-urlencoded";
                clientRequest.ContentLength = postBytes.Length;

                using ( var postStream = clientRequest.GetRequestStream() )
                {
                    postStream.WriteTimeout = 30000;
                    postStream.WriteAsync( postBytes, 0, postBytes.Length ).ContinueWith( ( a ) =>
                    {
                        postStream.Flush();
                        postStream.Close();
                    } ).Wait();
                }
            }

            return clientRequest;
        }

        private static long ProcessResponse( ConcurrentBag<Exception> exceptions, Uri baseUri, bool downloadHeaderSrcElements, bool downloadBodySrcElements, HttpWebResponse httpResponse )
        {
            long responseLength;
            using ( var stream = httpResponse.GetResponseStream() )
            {
                using ( var reader = new StreamReader( stream ) )
                {
                    var responseHtml = reader.ReadToEnd();
                    responseLength = responseHtml.Length;
                    if ( downloadHeaderSrcElements || downloadBodySrcElements )
                    {
                        var htmlDoc = new HtmlAgilityPack.HtmlDocument();
                        htmlDoc.LoadHtml( responseHtml );
                        List<HtmlNode> nodesWithSrc = new List<HtmlNode>();

                        var headNode = htmlDoc?.DocumentNode?.Descendants( "head" )?.FirstOrDefault();
                        var bodyNode = htmlDoc?.DocumentNode?.Descendants( "body" )?.FirstOrDefault();

                        if ( downloadHeaderSrcElements && headNode != null )
                        {
                            nodesWithSrc.AddRange( headNode.DescendantsAndSelf()
                                .Where( a => a.NodeType == HtmlAgilityPack.HtmlNodeType.Element )
                                .Where( a => a.Attributes.Any( x => x.Name == "src" ) )
                                .ToList() );
                        }

                        if ( downloadBodySrcElements && bodyNode != null )
                        {
                            nodesWithSrc.AddRange( bodyNode.DescendantsAndSelf()
                                .Where( a => a.NodeType == HtmlAgilityPack.HtmlNodeType.Element )
                                .Where( a => a.Attributes.Any( x => x.Name == "src" ) )
                                .ToList() );
                        }

                        Parallel.ForEach(
                            nodesWithSrc,
                            ( srcNode ) =>
                            {
                                string srcRef = string.Empty;
                                try
                                {
                                    srcRef = srcNode.Attributes["src"].Value;
                                    if ( !srcRef.StartsWith( "//" ) && srcRef.StartsWith( "/" ) )
                                    {
                                        var srcUri = new Uri( baseUri, srcRef );
                                        var srcRequest = (HttpWebRequest)WebRequest.Create( srcUri );
                                        srcRequest.Proxy = null;
                                        srcRequest.Timeout = 1000;
                                        srcRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                                        var srcResponse = srcRequest.GetResponse();

                                        using ( var resultStream = srcResponse.GetResponseStream() )
                                        {
                                            using ( var resultReader = new StreamReader( resultStream ) )
                                            {
                                                var resultData = resultReader.ReadToEnd();
                                            }
                                        }
                                    }
                                }
                                catch ( Exception ex )
                                {
                                    exceptions.Add( new Exception( ex.Message + srcRef ) );
                                }
                            } );
                    }


                }
            }

            return responseLength;
        }

        /// <summary>
        /// Updates the progress bar.
        /// </summary>
        /// <param name="requestCount">The request count.</param>
        /// <param name="threadCount">The thread count.</param>
        private void UpdateProgressBar()
        {
            if ( InvokeRequired )
            {
                BeginInvoke( new Action( UpdateProgressBar ), new object[] { requestCount, threadCount } );
                return;
            }

            if ( pgbRequestCount.Visible )
            {
                pgbRequestCount.Value = (int)Interlocked.Read( ref requestCount );
            }

            if ( pgbResponseCount.Visible )
            {
                pgbResponseCount.Value = (int)Interlocked.Read( ref responseCount );
            }

            //if ( lblThreadCount.Text != threadCount.ToString() )
            {
                lblThreadCount.Text = $"Threads: {threadCount}\nRequests: {requestCount}\nResponses: {responseCount}";
                if (exceptions.Any())
                {
                    lblThreadCount.Text += $"\nExceptions:{exceptions.Count()}";
                }

                lblThreadCount.Refresh();
            }

            lblStatus.Text = $"RUNNING:{Math.Round( stopwatchTestDuration.Elapsed.TotalSeconds, 1 )}s";
        }

        /// <summary>
        /// 
        /// </summary>
        private class ChartData
        {
            public string Series { get; set; }

            public DateTime XValue { get; set; }

            public double YValue { get; set; }

            public long ResponseLength { get; set; }
        }

        /// <summary>
        /// Handles the Load event of the Form1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Form1_Load( object sender, EventArgs e )
        {
            lblThreadCount.Visible = false;
        }

        /// <summary>
        /// Handles the DoWork event of the backgroundWorker1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.DoWorkEventArgs"/> instance containing the event data.</param>
        private void backgroundWorker1_DoWork( object sender, System.ComponentModel.DoWorkEventArgs e )
        {
            RunLoadTest( backgroundWorker1 );
        }

        /// <summary>
        /// Handles the RunWorkerCompleted event of the backgroundWorker1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.RunWorkerCompletedEventArgs"/> instance containing the event data.</param>
        private void backgroundWorker1_RunWorkerCompleted( object sender, System.ComponentModel.RunWorkerCompletedEventArgs e )
        {
            timer1.Enabled = false;

            if ( e.Error != null )
            {
                lblStatus.Text = $"{e.Error.Message}, {e.Error.StackTrace}";
            }
            else
            {
                lblStatus.Text = "DONE";
            }

            keepRunning = false;

            chart1.Invalidate();
            lblThreadCount.Visible = false;

            pgbRequestCount.Value = pgbRequestCount.Maximum;
            pgbRequestCount.Hide();
            pgbResponseCount.Hide();

            chart1.Series.Clear();

            var seriesDictionary = new Dictionary<string, System.Windows.Forms.DataVisualization.Charting.Series>();
            var chartArea = chart1.ChartAreas[0];
            chartArea.AxisX.IntervalType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Milliseconds;
            chartArea.AxisX.Minimum = 0;
            if ( chartResults.Any() )
            {
                chartArea.AxisX.Maximum = chartResults.Max( a => a.XValue.TimeOfDay.TotalMilliseconds );
            }
            chartArea.AxisX.CustomLabels.Clear();
            double labelPosition = 0;
            var labelInterval = ( chartArea.AxisX.Maximum / 10 );
            while ( labelPosition < chartArea.AxisX.Maximum )
            {
                var label = new System.Windows.Forms.DataVisualization.Charting.CustomLabel();
                label.FromPosition = labelPosition;
                label.ToPosition = labelPosition + labelInterval;
                label.Text = string.Format( "@{0}s", Math.Round( label.ToPosition / 1000, 3 ) );
                label.GridTicks = System.Windows.Forms.DataVisualization.Charting.GridTickTypes.Gridline;
                chartArea.AxisX.CustomLabels.Add( label );

                labelPosition += labelInterval;
            }

            foreach ( string seriesName in chartResults.Select( a => a.Series ).Distinct() )
            {
                var series = new System.Windows.Forms.DataVisualization.Charting.Series
                {
                    Name = seriesName,
                    ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline,
                    Font = this.Font,
                    LabelToolTip = "#VALYms response @time #VALX{D}ms "
                };

                seriesDictionary.Add( seriesName, series );
                chart1.Series.Add( series );
            }

            foreach ( var item in chartResults.OrderBy( a => a.XValue ) )
            {
                var point = new System.Windows.Forms.DataVisualization.Charting.DataPoint();
                point.SetValueXY( item.XValue.TimeOfDay.TotalMilliseconds, item.YValue );
                point.ToolTip = string.Format( "{0}ms @ + {1}\n{2}bytes", item.YValue, item.XValue.TimeOfDay.TotalMilliseconds, item.ResponseLength );
                point.LabelToolTip = point.ToolTip;
                seriesDictionary[item.Series].Points.Add( point );
            }

            tbStats.Text = statsText;

            tbExceptions.Lines = exceptions.Select( a => a.Message + "@" +  a.StackTrace ).ToArray();
        }

        private void backgroundWorker1_ProgressChanged( object sender, System.ComponentModel.ProgressChangedEventArgs e )
        {
            UpdateProgressBar();
        }

        private void btnStop_Click( object sender, EventArgs e )
        {
            if ( keepRunning )
            {
                keepRunning = false;
                lblStatus.Text = "Stopping...";
            }
        }

        private void timer1_Tick( object sender, EventArgs e )
        {
            UpdateProgressBar();
        }

        private void tbUrl_TextChanged( object sender, EventArgs e )
        {

        }
    }
}
