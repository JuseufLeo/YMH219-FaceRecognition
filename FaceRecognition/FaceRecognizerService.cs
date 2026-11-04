using OpenCvSharp;
using OpenCvSharp.Face;

namespace FaceRecognition
{
    /// <summary>Bir yüz için tanıma sonucu.</summary>
    public record Prediction(string Label, double Distance, bool IsKnown)
    {
        /// <summary>Uzaklık değerini 0-100 arası basit bir güven yüzdesine çevirir (uzaklık küçükse güven yüksek).</summary>
        public int ConfidencePercent => (int)Math.Clamp(100 - Distance, 0, 100);
    }

    /// <summary>
    /// LBPH (Local Binary Patterns Histograms) yüz tanıyıcısını sarmalar: örneklerle eğitir ve tahmin yapar.
    /// </summary>
    public class FaceRecognizerService : IDisposable
    {
        /// <summary>Bu uzaklığın üstündeki tahminler "Bilinmiyor" sayılır.</summary>
        public double UnknownThreshold { get; set; } = 70;

        private LBPHFaceRecognizer recognizer;
        private Dictionary<int, string> names = new Dictionary<int, string>();

        public bool IsTrained => recognizer != null;
        public int LabelCount => names.Count;

        /// <summary>Veri setindeki tüm örneklerle modeli eğitir. En az 2 kişi gerekir.</summary>
        public int Train(FaceDataset dataset)
        {
            var (images, ids, labelNames) = dataset.LoadAll();
            try
            {
                if (labelNames.Count < 2)
                    throw new InvalidOperationException(
                        "Eğitim için en az 2 farklı etiket (kişi) gerekir. Önce örnek ekleyin.");

                recognizer?.Dispose();
                recognizer = LBPHFaceRecognizer.Create();
                recognizer.Train(images, ids);
                names = labelNames;
                return images.Count;
            }
            finally
            {
                foreach (Mat m in images) m.Dispose();
            }
        }

        /// <summary>Hazırlanmış (200x200 gri) yüz için tahmin yapar.</summary>
        public Prediction Predict(Mat preparedFace)
        {
            if (!IsTrained)
                throw new InvalidOperationException("Model henüz eğitilmedi.");
            recognizer.Predict(preparedFace, out int id, out double distance);
            bool known = distance <= UnknownThreshold && names.ContainsKey(id);
            return new Prediction(known ? names[id] : "Bilinmiyor", distance, known);
        }

        public void Dispose() => recognizer?.Dispose();
    }
}
