using LunaVK.Core.DataObjects;
using LunaVK.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaVK.Core.Library
{
    public class OutboundGeoAttachment : IOutboundAttachment
    {
        private string _geoDescription = string.Empty;

        public double Latitude { get; private set; }
        public double Longitude { get; private set; }

#region IOutboundAttachment
        public OutboundAttachmentUploadState UploadState { get { return OutboundAttachmentUploadState.Completed; } set { } }

        public void Upload(Action completionCallback, Action<double> progressCallback = null)
        {
            completionCallback();
        }

        public bool IsUploadAttachment
        {
            get { return false; }
        }
#endregion

        public string IconSource { get { return "\xE819"; } }

        public string Title
        {
            get
            {
                return "Место";//Conversation_WallPost;
            }
        }

        public string Subtitle
        {
            get
            {
                return this._geoDescription;
            }
        }

        
        public OutboundGeoAttachment(double latitude, double longitude)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.FetchDescription();
        }

        public OutboundGeoAttachment()
        {
        }

        private void FetchDescription()
        {/*
            MapsService.Current.ReverseGeocodeToAddress(this.Latitude, this.Longitude, (Action<BackendResult<string, ResultCode>>)(res =>
            {
                if (res.ResultCode != ResultCode.Succeeded)
                    return;
                this.GeoDescription = res.ResultData;
            }));*/
        }

        public VKAttachment GetAttachment()
        {
            throw new NotImplementedException();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(1);
            writer.Write(this.Latitude);
            writer.Write(this.Longitude);
            writer.WriteString(this._geoDescription);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            this.Latitude = reader.ReadDouble();
            this.Longitude = reader.ReadDouble();
            this._geoDescription = reader.ReadString();

            if (string.IsNullOrEmpty(this._geoDescription))
                this.FetchDescription();
        }
    }
}
