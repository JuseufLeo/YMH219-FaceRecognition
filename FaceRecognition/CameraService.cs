using OpenCvSharp;

namespace FaceRecognition
{
    /// <summary>
    /// Kameradan arka planda sürekli kare okur ve her yeni kareyi olayla bildirir.
    /// </summary>
    public class CameraService : IDisposable
    {
        private VideoCapture capture;
        private CancellationTokenSource cts;
        private Task loopTask;

        public bool IsRunning { get; private set; }

        /// <summary>Yeni bir kare okunduğunda (karenin sahipliği olayı dinleyene geçer).</summary>
        public event EventHandler<Mat> FrameReady;
        /// <summary>Kamera beklenmedik şekilde durduğunda.</summary>
        public event EventHandler<string> Error;

        /// <summary>Kamerayı açar. Kamera yoksa InvalidOperationException fırlatır.</summary>
        public void Start(int cameraIndex = 0)
        {
            if (IsRunning) return;
            capture = new VideoCapture(cameraIndex, VideoCaptureAPIs.DSHOW);
            if (!capture.IsOpened())
            {
                capture.Dispose();
                capture = null;
                throw new InvalidOperationException(
                    "Kamera açılamadı.\nBilgisayarda kamera olduğundan, başka bir programın kullanmadığından ve " +
                    "Windows gizlilik ayarlarında kamera izni verildiğinden emin olun.");
            }
            capture.Set(VideoCaptureProperties.FrameWidth, 640);
            capture.Set(VideoCaptureProperties.FrameHeight, 480);

            cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            IsRunning = true;
            loopTask = Task.Run(() => CaptureLoop(token));
        }

        private void CaptureLoop(CancellationToken token)
        {
            int failures = 0;
            while (!token.IsCancellationRequested)
            {
                var frame = new Mat();
                if (!capture.Read(frame) || frame.Empty())
                {
                    frame.Dispose();
                    if (++failures > 50)
                    {
                        IsRunning = false;
                        Error?.Invoke(this, "Kameradan görüntü alınamıyor. Kamera bağlantısı kesilmiş olabilir.");
                        return;
                    }
                    Thread.Sleep(20);
                    continue;
                }
                failures = 0;
                FrameReady?.Invoke(this, frame);
                Thread.Sleep(30);   // yaklaşık 25 kare/saniye
            }
        }

        public void Stop()
        {
            if (!IsRunning && capture == null) return;
            cts?.Cancel();
            try { loopTask?.Wait(1000); } catch (AggregateException) { }
            capture?.Release();
            capture?.Dispose();
            capture = null;
            IsRunning = false;
        }

        public void Dispose() => Stop();
    }
}
