/* opusfile_winrt - Opus Codec for Windows Runtime
 * Copyright (C) 2014-2015  Alexander Ovchinnikov
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 *
 * - Redistributions of source code must retain the above copyright
 * notice, this list of conditions and the following disclaimer.
 *
 * - Redistributions in binary form must reproduce the above copyright
 * notice, this list of conditions and the following disclaimer in the
 * documentation and/or other materials provided with the distribution.
 *
 * - Neither the name of copyright holder nor the names of project's
 * contributors may be used to endorse or promote products derived from
 * this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * ``AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A
 * PARTICULAR PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL THE COPYRIGHT HOLDER
 * OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

#include "opusfile.h"
#include "internal.h"

namespace Opusfile {

	namespace WindowsRuntime {

		public ref class OpusHead sealed {
		public:
			property int Version {
				int get() { return src_->version; }
			}

			property int ChannelCount {
				int get() { return src_->channel_count; }
			}

			property unsigned PreSkip {
				unsigned get() { return src_->pre_skip; }
			}

			property opus_uint32 InputSampleRate {
				opus_uint32 get() { return src_->input_sample_rate; }
			}

			property int OutputGain {
				int get() { return src_->output_gain; }
			}

			property int MappingFamily {
				int get() { return src_->mapping_family; }
			}

			property int StreamCount {
				int get() { return src_->stream_count; }
			}

			property int CoupledCount {
				int get() { return src_->coupled_count; }
			}

			property Platform::Array<uint8>^ Mapping {
				Platform::Array<uint8>^ get() {
					return mapping_ ? mapping_ : (mapping_ =
						Platform::ArrayReference<uint8>(const_cast<uint8 *>(src_->mapping), OPUS_CHANNEL_COUNT_MAX));
				}
			}

		internal:
			OpusHead(const ::OpusHead *src) : src_(src) { }

		private:
			const ::OpusHead *src_;

			Platform::Array<uint8>^ mapping_;
		};


		public ref class OpusTags sealed {
		public:
			property Platform::String^ Vendor {
				Platform::String^ get() { return vendor_ ? vendor_ : (vendor_ = string_from_utf8(src_->vendor)); }
			}

			property Platform::Array<Platform::String^>^ Comments {
				Platform::Array<Platform::String^>^ get() {
					return user_comments_ ? user_comments_ : (user_comments_ = InitializeUserComments());
				}
			}

		internal:
			OpusTags(const ::OpusTags *src) : src_(src) { }

			char *GetComment(int index) const {
				if (index < 0 || index > src_->comments) return nullptr;
				return src_->user_comments[index];
			}

		private:
			inline Platform::Array<Platform::String^>^ InitializeUserComments() {
				Platform::Array<Platform::String^>^ arr = ref new Platform::Array<Platform::String^>((unsigned)src_->comments);
				for (unsigned i = 0; i < arr->Length; i++) {
					arr[i] = string_from_utf8(src_->user_comments[i], src_->comment_lengths[i] + 1);
				}
				return arr;
			}

			const ::OpusTags *src_;

			Platform::String^ vendor_;
			Platform::Array<Platform::String^>^ user_comments_;
		};


		public enum struct OpusPictureType {
			Other,
			FileIcon32x32,
			FileIconOther,
			CoverFront,
			CoverBack,
			LeafletPage,
			Media,
			LeadArtist,
			Artist,
			Conductor,
			Band,
			Composer,
			Lyricist,
			RecordingLocation,
			DuringRecording,
			DuringPerformance,
			ScreenCapture,
			BrightColoredFish,
			Illustration,
			ArtistLogotype,
			PublisherLogotype
		};


		public enum struct OpusPictureFormat {
			Unknown = OP_PIC_FORMAT_UNKNOWN,
			Url = OP_PIC_FORMAT_URL,
			Jpeg = OP_PIC_FORMAT_JPEG,
			Png = OP_PIC_FORMAT_PNG,
			Gif = OP_PIC_FORMAT_GIF
		};


		public ref class OpusPictureTag sealed {
		public:
			virtual ~OpusPictureTag()
			{
				::opus_picture_tag_clear(src_);
				delete src_;
			}

			static Platform::Array<OpusPictureTag^>^ Parse(OpusTags^ opusTags);

			property OpusPictureType Type {
				OpusPictureType get() { return (OpusPictureType)(src_->type > 20 ? 0 : src_->type); }
			}

			property Platform::String^ MimeType {
				Platform::String^ get() {
					return mime_type_ ? mime_type_ : (mime_type_ = string_from_utf8(src_->mime_type));
				}
			}

			property Platform::String^ Description {
				Platform::String^ get() {
					return description_ ? description_ : (description_ = string_from_utf8(src_->description));
				}
			}

			property opus_uint32 Width {
				opus_uint32 get() { return src_->width; }
			}

			property opus_uint32 Height {
				opus_uint32 get() { return src_->height; }
			}

			property opus_uint32 Depth {
				opus_uint32 get() { return src_->depth; }
			}

			property opus_uint32 Colors {
				opus_uint32 get() { return src_->colors; }
			}

			property Platform::Array<uint8>^ Data {
				Platform::Array<uint8>^ get() {
					return data_ ? data_ : (data_ = Platform::ArrayReference<uint8>(src_->data, src_->data_length));
				}
			}

			property OpusPictureFormat Format {
				OpusPictureFormat get() { return (OpusPictureFormat)src_->format; }
			}

		private:
			OpusPictureTag() {
				src_ = new ::OpusPictureTag();
				::opus_picture_tag_init(src_);
			}

			::OpusPictureTag *src_;

			Platform::String^ mime_type_;
			Platform::String^ description_;
			Platform::Array<uint8>^ data_;
		};


		public enum struct GainType {
			HeaderGain = OP_HEADER_GAIN,
			TrackGain = OP_TRACK_GAIN,
			AbsoluteGain = OP_ABSOLUTE_GAIN
		};

		//
		typedef struct {
			void *readdata;
			opus_int64 total_samples_per_channel;
			int rawmode;
			int channels;
			long rate;
			int gain;
			int samplesize;
			int endianness;
			char *infilename;
			int ignorelength;
			int skip;
			int extraout;
			char *comments;
			int comments_length;
			int copy_comments;
		} oe_enc_opt;

		typedef struct {
			int version;
			int channels; /* Number of channels: 1..255 */
			int preskip;
			ogg_uint32_t input_sample_rate;
			int gain; /* in dB S7.8 should be zero whenever possible */
			int channel_mapping;
			/* The rest is only used if channel_mapping != 0 */
			int nb_streams;
			int nb_coupled;
			unsigned char stream_map[255];
		} OpusHeader;

		typedef struct {
			unsigned char *data;
			int maxlen;
			int pos;
		} Packet;
		//

		public ref class OggOpusFile sealed {
		public:
			OggOpusFile();
			virtual ~OggOpusFile();

			/* Call after open to check that the object was created
			 * successfully.  If not, use Open() to try again.
			 */
			property bool IsValid { bool get(); }

			void Open(Windows::Storage::Streams::IRandomAccessStream^ fileStream);
			void Open(Windows::Storage::Streams::IRandomAccessStream^ fileStream, Windows::Storage::Streams::IBuffer^ initial);
			void Free();

			bool Seekable();
			int LinkCount();
			opus_uint32 Serialno();
			opus_uint32 Serialno(int li);
			int ChannelCount();
			int ChannelCount(int li);
			opus_int64 RawTotal();
			opus_int64 RawTotal(int li);
			ogg_int64_t PcmTotal();
			ogg_int64_t PcmTotal(int li);
			OpusHead^ Head();
			OpusHead^ Head(int li);
			OpusTags^ Tags();
			OpusTags^ Tags(int li);
			int CurrentLink();
			opus_int32 Bitrate();
			opus_int32 Bitrate(int li);
			opus_int32 BitrateInstant();
			opus_int64 RawTell();
			ogg_int64_t PcmTell();

			void SetGainOffset(GainType gainType, opus_int32 gainOffsetQ8);
			void SetDitherEnabled(bool enabled);
			Windows::Storage::Streams::IBuffer^ Read(int bufSize);
			[Windows::Foundation::Metadata::DefaultOverloadAttribute]
			Windows::Storage::Streams::IBuffer^ Read(int bufSize, int *li);
			Windows::Storage::Streams::IBuffer^ ReadStereo(int bufSize);

			void RawSeek(opus_int64 byteOffset);
			void PcmSeek(ogg_int64_t pcmOffset);
			//
			int writeFrame(const Platform::Array<uint8_t>^ framePcmBytes, unsigned int frameByteCount);
			int initRecorder(Platform::String ^ path);
			void cleanupRecorder();
			//
		internal:
			static int read_func(void *stream, unsigned char *ptr, int nbytes);
			static int seek_func(void *stream, opus_int64 offset, int whence);
			static opus_int64 tell_func(void *stream);
			static int close_func(void *stream);

		private:
			::OggOpusFile *of_;
			Windows::Storage::Streams::IRandomAccessStream^ file_stream_;
			Windows::Storage::Streams::DataReader^ file_reader_;
			//
			ogg_int32_t _packetId;
			const opus_int32 frame_size = 960;
			opus_int64 total_samples;
			int size_segments;
			opus_int64 bytes_written;
			ogg_int64_t enc_granulepos;
			opus_int32 coding_rate = 16000;
			const opus_int32 rate = 16000;
			const opus_int32 bitrate = 16000;
			opus_int32 min_bytes;
			opus_int64 pages_out;
			int last_segments;
			int max_frame_bytes;
			const int max_ogg_delay = 0;
			ogg_int64_t last_granulepos;
			FILE *_fileOs = 0;
			OpusEncoder *_encoder = 0;
			uint8_t *_packet = 0;
			ogg_stream_state os;
			oe_enc_opt inopt;
			OpusHeader header;
			ogg_packet op;
			ogg_page og;
			const int comment_padding = 512;

			void comment_init(char **comments, int *length, const char *vendor_string);
			int writeOggPage(ogg_page *page, FILE *os);
			int opus_header_to_packet(const OpusHeader *h, unsigned char *packet, int len);
			int write_uint32(Packet *p, ogg_uint32_t val);
			int write_uint16(Packet *p, ogg_uint16_t val);
			int write_chars(Packet *p, const unsigned char *str, int nb_chars);
			void comment_pad(char **comments, int* length, int amount);
			//
		};
	}

}
