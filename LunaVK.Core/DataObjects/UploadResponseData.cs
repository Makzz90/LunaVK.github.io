namespace LunaVK.Core.DataObjects
{
    public class UploadResponseData
    {
        public string response { get; set; }

        public string server { get; set; }

        public string hash { get; set; }

        public string photo { get; set; }

        public string audio { get; set; }
    }

    public class UploadPhotoResponseData
    {
        public string server { get; set; }

        public string photo { get; set; }

        public string photos_list { get; set; }

        public string hash { get; set; }

        public string aid { get; set; }

        public int uid { get; set; }

        public int gid { get; set; }
    }

    public class UploadDocResponseData
    {
        public string file { get; set; }

        public string error { get; set; }
    }

    public class UploadVideoResponseData
    {
        public int size { get; set; }

        public int owner_id { get; set; }

        public uint video_id { get; set; }

        public string video_hash { get; set; }

        public string error { get; set; }
    }

    public class UploadStoryResponseData
    {
        public Resp response { get; set; }

        public string _sig { get; set; }

        public class Resp
        {
            public VKStory story { get; set; }
        }
    }
}
