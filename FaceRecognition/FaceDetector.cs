using OpenCvSharp;
using Size = OpenCvSharp.Size;

namespace FaceRecognition
{
    /// <summary>
    /// OpenCV'nin Haar Cascade sınıflandırıcısıyla görüntüdeki yüzleri bulur.
    /// </summary>
    public class FaceDetector : IDisposable
    {
        private readonly CascadeClassifier cascade;

        /// <summary>Cascade dosyası yoksa veya okunamazsa FileNotFoundException fırlatır.</summary>
        public FaceDetector(string cascadePath)
        {
            if (!File.Exists(cascadePath))
                throw new FileNotFoundException(
                    "Yüz algılama modeli (Haar Cascade) bulunamadı:\n" + cascadePath, cascadePath);
            cascade = new CascadeClassifier(cascadePath);
            if (cascade.Empty())
                throw new FileNotFoundException("Yüz algılama modeli okunamadı:\n" + cascadePath, cascadePath);
        }

        /// <summary>Renkli görüntüdeki tüm yüzlerin dikdörtgenlerini döndürür (büyükten küçüğe).</summary>
        public Rect[] Detect(Mat image)
        {
            using Mat gray = ToGray(image);
            Cv2.EqualizeHist(gray, gray);   // ışık farklarını azalt
            Rect[] faces = cascade.DetectMultiScale(gray, 1.1, 5, HaarDetectionTypes.ScaleImage, new Size(60, 60));
            return faces.OrderByDescending(r => r.Width * r.Height).ToArray();
        }

        /// <summary>Yüz bölgesini tanıma için standart hâle getirir: gri, 200x200, histogram eşitlenmiş.</summary>
        public static Mat PrepareFace(Mat image, Rect face)
        {
            using Mat gray = ToGray(image);
            using Mat crop = new Mat(gray, face);
            Mat resized = new Mat();
            Cv2.Resize(crop, resized, new Size(200, 200));
            Cv2.EqualizeHist(resized, resized);
            return resized;
        }

        private static Mat ToGray(Mat image)
        {
            Mat gray = new Mat();
            if (image.Channels() == 1) image.CopyTo(gray);
            else Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);
            return gray;
        }

        public void Dispose() => cascade.Dispose();
    }
}
