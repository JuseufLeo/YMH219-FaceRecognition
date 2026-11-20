using System.Text.Json;
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
        public double UnknownThreshold { get; set; } = 50;

        private LBPHFaceRecognizer recognizer;
        private Dictionary<int, string> names = new Dictionary<int, string>();

        public bool IsTrained => recognizer != null;
        public int LabelCount => names.Count;
        /// <summary>Modelin eğitildiği zaman (kayıttan yüklendiyse kayıttaki zaman).</summary>
        public DateTime? TrainedAt { get; private set; }
        /// <summary>Modelin eğitildiği toplam örnek sayısı.</summary>
        public int SampleCount { get; private set; }
        /// <summary>Etiket adları (kişiler).</summary>
        public IEnumerable<string> LabelNames => names.Values.OrderBy(n => n);

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
                TrainedAt = DateTime.Now;
                SampleCount = images.Count;
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

        // ---------- Modeli kaydetme / yükleme ----------

        /// <summary>Etiket adları ve eğitim bilgisi, modelin yanındaki JSON dosyasında saklanır.</summary>
        private class ModelInfo
        {
            public Dictionary<int, string> Names { get; set; }
            public DateTime TrainedAt { get; set; }
            public int SampleCount { get; set; }
        }

        private static string InfoPath(string modelPath) =>
            Path.Combine(Path.GetDirectoryName(modelPath) ?? "",
                         Path.GetFileNameWithoutExtension(modelPath) + "_labels.json");

        /// <summary>
        /// Eğitilmiş modeli diske yazar: LBPH histogramları .yml dosyasına (OpenCV'nin kendi biçimi),
        /// etiket adları ve eğitim bilgisi _labels.json dosyasına.
        /// </summary>
        public void Save(string modelPath)
        {
            if (!IsTrained)
                throw new InvalidOperationException("Kaydedilecek eğitilmiş model yok.");
            recognizer.Write(modelPath);
            var info = new ModelInfo { Names = names, TrainedAt = TrainedAt ?? DateTime.Now, SampleCount = SampleCount };
            File.WriteAllText(InfoPath(modelPath),
                JsonSerializer.Serialize(info, new JsonSerializerOptions { WriteIndented = true }));
        }

        /// <summary>
        /// Daha önce kaydedilmiş modeli yükler. Dosyalar yoksa false döner;
        /// dosyalar bozuksa InvalidDataException fırlatır.
        /// </summary>
        public bool TryLoad(string modelPath)
        {
            string infoPath = InfoPath(modelPath);
            if (!File.Exists(modelPath) || !File.Exists(infoPath))
                return false;

            ModelInfo info;
            try
            {
                info = JsonSerializer.Deserialize<ModelInfo>(File.ReadAllText(infoPath));
            }
            catch (JsonException ex)
            {
                throw new InvalidDataException("Model bilgi dosyası okunamadı: " + ex.Message);
            }
            if (info?.Names == null || info.Names.Count < 2)
                throw new InvalidDataException("Model bilgi dosyasında yeterli etiket yok.");

            LBPHFaceRecognizer loaded = LBPHFaceRecognizer.Create();
            try
            {
                loaded.Read(modelPath);
            }
            catch (OpenCVException ex)
            {
                loaded.Dispose();
                throw new InvalidDataException("Model dosyası okunamadı: " + ex.Message);
            }

            recognizer?.Dispose();
            recognizer = loaded;
            names = info.Names;
            TrainedAt = info.TrainedAt;
            SampleCount = info.SampleCount;
            return true;
        }

        public void Dispose() => recognizer?.Dispose();
    }
}
