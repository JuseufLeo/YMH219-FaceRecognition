using OpenCvSharp;

namespace FaceRecognition
{
    /// <summary>
    /// Yerel yüz örneklerini yönetir. Her kişi (etiket) için FaceData/&lt;etiket&gt;/ klasöründe
    /// 200x200 gri yüz resimleri saklanır. Bu klasör GitHub'a yüklenmez.
    /// </summary>
    public class FaceDataset
    {
        public string RootFolder { get; }

        public FaceDataset(string rootFolder)
        {
            RootFolder = rootFolder;
            Directory.CreateDirectory(rootFolder);
        }

        /// <summary>Bir kişiye yeni bir yüz örneği ekler ve dosya yolunu döndürür.</summary>
        public string AddSample(string label, Mat face)
        {
            label = CleanLabel(label);
            if (label.Length == 0)
                throw new ArgumentException("Etiket (kişi adı) boş olamaz.");
            string folder = Path.Combine(RootFolder, label);
            Directory.CreateDirectory(folder);
            string path = Path.Combine(folder, $"{DateTime.Now:yyyyMMdd_HHmmss_fff}.png");
            Cv2.ImWrite(path, face);
            return path;
        }

        /// <summary>Etiketler ve her birindeki örnek sayısı.</summary>
        public Dictionary<string, int> GetLabelCounts() =>
            Directory.GetDirectories(RootFolder)
                     .ToDictionary(Path.GetFileName, d => Directory.GetFiles(d, "*.png").Length);

        /// <summary>Eğitim için tüm örnekleri yükler. Etiketlere 0,1,2... şeklinde numara verir.</summary>
        public (List<Mat> Images, List<int> Ids, Dictionary<int, string> Names) LoadAll()
        {
            var images = new List<Mat>();
            var ids = new List<int>();
            var names = new Dictionary<int, string>();
            int id = 0;
            foreach (string folder in Directory.GetDirectories(RootFolder).OrderBy(f => f))
            {
                string[] files = Directory.GetFiles(folder, "*.png");
                if (files.Length == 0) continue;
                names[id] = Path.GetFileName(folder);
                foreach (string file in files)
                {
                    Mat img = Cv2.ImRead(file, ImreadModes.Grayscale);
                    if (img.Empty()) { img.Dispose(); continue; }
                    images.Add(img);
                    ids.Add(id);
                }
                id++;
            }
            return (images, ids, names);
        }

        public void DeleteLabel(string label)
        {
            string folder = Path.Combine(RootFolder, label);
            if (Directory.Exists(folder))
                Directory.Delete(folder, true);
        }

        private static string CleanLabel(string label)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                label = label.Replace(c.ToString(), "");
            return label.Trim();
        }
    }
}
