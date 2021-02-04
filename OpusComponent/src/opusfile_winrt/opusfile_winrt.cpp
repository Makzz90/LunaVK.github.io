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

#include "opusfile_winrt.h"
#include <iostream>
#include <fstream>

#define writeint(buf, base, val) do { buf[base + 3] = ((val) >> 24) & 0xff; \
buf[base + 2]=((val) >> 16) & 0xff; \
buf[base + 1]=((val) >> 8) & 0xff; \
buf[base] = (val) & 0xff; \
} while(0)

namespace Opusfile {

	namespace WindowsRuntime {

		const ::OpusFileCallbacks op_winrt_callbacks = {
			&OggOpusFile::read_func,
			&OggOpusFile::seek_func,
			&OggOpusFile::tell_func,
			&OggOpusFile::close_func
		};


		OggOpusFile::OggOpusFile()
			: of_(nullptr), file_stream_(nullptr), file_reader_(nullptr)
		{
		}

		OggOpusFile::~OggOpusFile()
		{
			Free();
		}

		bool OggOpusFile::IsValid::get()
		{
			return nullptr != of_ && nullptr != file_stream_ && nullptr != file_reader_;
		}

		void OggOpusFile::Open(Windows::Storage::Streams::IRandomAccessStream^ fileStream)
		{
			Open(fileStream, nullptr);
		}

		void OggOpusFile::Open(Windows::Storage::Streams::IRandomAccessStream^ fileStream, Windows::Storage::Streams::IBuffer^ initial)
		{
			file_stream_ = fileStream;
			file_reader_ = ref new Windows::Storage::Streams::DataReader(file_stream_);

			uint8 *initial_data = nullptr;
			size_t initial_size = 0;
			if (initial) {
				initial_data = get_array(initial);
				initial_size = initial->Length;
			}

			int error = 0;
			of_ = ::op_open_callbacks((void *)this, &op_winrt_callbacks, initial_data, initial_size, &error);

			if (0 != error) {
				Free();
				switch (error) {
				case OP_EFAULT:
					throw ref new Platform::FailureException();
				case OP_EIMPL:
					throw ref new Platform::NotImplementedException();
				case OP_EINVAL:
				case OP_ENOTFORMAT:
					throw ref new Platform::InvalidArgumentException();
				default:
					throw ref new Platform::COMException(error);
				}
			}
		}

		void OggOpusFile::Free()
		{
			if (of_) {
				(void)::op_free(of_);
				of_ = nullptr;
			}

			if (file_reader_) {
				(void)file_reader_->DetachStream();
				delete file_reader_;
			}

			file_reader_ = nullptr;
			file_stream_ = nullptr;
		}

		bool OggOpusFile::Seekable()
		{
			_ASSERTE(IsValid);
			return !!(::op_seekable(of_));
		}

		int OggOpusFile::LinkCount()
		{
			_ASSERTE(IsValid);
			return ::op_link_count(of_);
		}

		opus_uint32 OggOpusFile::Serialno()
		{
			return Serialno(-1);
		}

		opus_uint32 OggOpusFile::Serialno(int li)
		{
			_ASSERTE(IsValid);
			return ::op_serialno(of_, li);
		}

		int OggOpusFile::ChannelCount()
		{
			return ChannelCount(-1);
		}

		int OggOpusFile::ChannelCount(int li)
		{
			_ASSERTE(IsValid);
			return ::op_channel_count(of_, li);
		}

		opus_int64 OggOpusFile::RawTotal()
		{
			return RawTotal(-1);
		}

		opus_int64 OggOpusFile::RawTotal(int li)
		{
			_ASSERTE(IsValid);
			return ::op_raw_total(of_, li);
		}

		ogg_int64_t OggOpusFile::PcmTotal()
		{
			return PcmTotal(-1);
		}

		ogg_int64_t OggOpusFile::PcmTotal(int li)
		{
			_ASSERTE(IsValid);
			return ::op_pcm_total(of_, li);
		}

		OpusHead^ OggOpusFile::Head()
		{
			return Head(-1);
		}

		OpusHead^ OggOpusFile::Head(int li)
		{
			_ASSERTE(IsValid);
			const ::OpusHead *head = ::op_head(of_, li);
			return ref new OpusHead(head);
		}

		OpusTags^ OggOpusFile::Tags()
		{
			return Tags(-1);
		}

		OpusTags^ OggOpusFile::Tags(int li)
		{
			_ASSERTE(IsValid);
			const ::OpusTags *tags = ::op_tags(of_, li);
			return ref new OpusTags(tags);
		}

		int OggOpusFile::CurrentLink()
		{
			_ASSERTE(IsValid);
			return ::op_current_link(of_);
		}

		opus_int32 OggOpusFile::Bitrate()
		{
			return Bitrate(-1);
		}

		opus_int32 OggOpusFile::Bitrate(int li)
		{
			_ASSERTE(IsValid);
			return ::op_bitrate(of_, li);
		}

		opus_int32 OggOpusFile::BitrateInstant()
		{
			_ASSERTE(IsValid);
			return ::op_bitrate_instant(of_);
		}

		opus_int64 OggOpusFile::RawTell()
		{
			_ASSERTE(IsValid);
			return ::op_raw_tell(of_);
		}

		ogg_int64_t OggOpusFile::PcmTell()
		{
			_ASSERTE(IsValid);
			return ::op_pcm_tell(of_);
		}

		void OggOpusFile::SetGainOffset(GainType gainType, opus_int32 gainOffsetQ8)
		{
			_ASSERTE(IsValid);
			int ret = ::op_set_gain_offset(of_, (int)gainType, gainOffsetQ8);
			if (ret < 0) {
				if (OP_EINVAL == ret)
					throw ref new Platform::InvalidArgumentException("gainType");
				throw ref new Platform::COMException(ret);
			}
		}

		void OggOpusFile::SetDitherEnabled(bool enabled)
		{
			_ASSERTE(IsValid);
			::op_set_dither_enabled(of_, enabled ? TRUE : FALSE);
		}

		Windows::Storage::Streams::IBuffer^ OggOpusFile::Read(int bufSize)
		{
			return Read(bufSize, NULL);
		}

		Windows::Storage::Streams::IBuffer^ OggOpusFile::Read(int bufSize, int *li)
		{
			_ASSERTE(IsValid);

			std::vector<opus_int16> pcm(bufSize);
			int ret = ::op_read(of_, &pcm.front(), bufSize, li);

			if (ret < 0) {
				if (OP_EFAULT == ret)
					throw ref new Platform::FailureException();
				if (OP_EIMPL == ret)
					throw ref new Platform::NotImplementedException();
				throw ref new Platform::COMException(ret);
			}
			if (0 == ret)
				return ref new Windows::Storage::Streams::Buffer(0);

			int channels = ::op_channel_count(of_, li != NULL ? *li : -1);

			return pack_sample(&pcm.front(), ret * channels);
		}

		Windows::Storage::Streams::IBuffer^ OggOpusFile::ReadStereo(int bufSize)
		{
			_ASSERTE(IsValid);

			std::vector<opus_int16> pcm(bufSize);
			int ret = ::op_read_stereo(of_, &pcm.front(), bufSize);

			if (ret < 0) {
				if (OP_EFAULT == ret)
					throw ref new Platform::FailureException();
				if (OP_EIMPL == ret)
					throw ref new Platform::NotImplementedException();
				throw ref new Platform::COMException(ret);
			}
			if (0 == ret)
				return ref new Windows::Storage::Streams::Buffer(0);

			const int channels = 2; // Since the decoded data is stereo

			return pack_sample(&pcm.front(), ret * channels);
		}

		void OggOpusFile::RawSeek(opus_int64 byteOffset)
		{
			_ASSERTE(IsValid);

			int ret = ::op_raw_seek(of_, byteOffset);
			if (0 != ret) {
				if (OP_EINVAL == ret)
					throw ref new Platform::InvalidArgumentException();
				throw ref new Platform::COMException(ret);
			}
		}

		void OggOpusFile::PcmSeek(ogg_int64_t pcmOffset)
		{
			_ASSERTE(IsValid);

			int ret = ::op_pcm_seek(of_, pcmOffset);
			if (0 != ret) {
				if (OP_EINVAL == ret)
					throw ref new Platform::InvalidArgumentException();
				throw ref new Platform::COMException(ret);
			}
		}


		int OggOpusFile::read_func(void *stream, unsigned char *ptr, int nbytes)
		{
			OggOpusFile^ instance = reinterpret_cast<OggOpusFile^>(stream);
			_ASSERTE(instance && instance->file_reader_);
			if (nbytes > 0) {
				unsigned count = perform_synchronously(instance->file_reader_->LoadAsync(nbytes));
				if (count > 0) {
					instance->file_reader_->ReadBytes(Platform::ArrayReference<uint8>(reinterpret_cast<uint8 *>(ptr), count));
					return count;
				}
			}
			return 0;
		}

		int OggOpusFile::seek_func(void *stream, opus_int64 offset, int whence)
		{
			OggOpusFile^ instance = reinterpret_cast<OggOpusFile^>(stream);
			_ASSERTE(instance && instance->file_stream_);
			switch (whence) {
			case SEEK_CUR:
				instance->file_stream_->Seek(instance->file_stream_->Position + (uint64)offset);
				break;
			case SEEK_END:
				instance->file_stream_->Seek(instance->file_stream_->Size - (uint64)offset);
				break;
			case SEEK_SET:
				instance->file_stream_->Seek((uint64)offset);
				break;
			default:
				throw ref new Platform::InvalidArgumentException("whence");
			}
			return 0;
		}

		opus_int64 OggOpusFile::tell_func(void *stream)
		{
			OggOpusFile^ instance = reinterpret_cast<OggOpusFile^>(stream);
			_ASSERTE(instance && instance->file_stream_);
			return (long)instance->file_stream_->Position;
		}

		int OggOpusFile::close_func(void *stream)
		{
			OggOpusFile^ instance = reinterpret_cast<OggOpusFile^>(stream);
			_ASSERTE(instance && instance->file_reader_);
			(void)instance->file_reader_->DetachStream();
			delete instance->file_reader_;
			instance->file_reader_ = nullptr;
			return 0;
		}


		Platform::Array<OpusPictureTag^>^ OpusPictureTag::Parse(OpusTags^ opusTags)
		{
			std::vector<OpusPictureTag^> pics;
			int i = 0;

			while (true) {
				char *comment = opusTags->GetComment(i++);
				if (nullptr == comment) break;
				if (0 != opus_tagncompare("METADATA_BLOCK_PICTURE", 22, comment)) continue;

				OpusPictureTag^ pic = ref new OpusPictureTag();
				int ret = ::opus_picture_tag_parse(pic->src_, comment);
				if (OP_EFAULT == ret)
					throw ref new Platform::OutOfMemoryException();
				if (0 == ret) pics.push_back(pic);
			}

			return Platform::ArrayReference<OpusPictureTag^>(pics.data(), pics.size());
		}










		//
		int OggOpusFile::writeFrame(const Platform::Array<uint8_t>^ framePcmBytes, unsigned int frameByteCount)
		{
			int cur_frame_size = frame_size;
			_packetId++;

			opus_int32 nb_samples = frameByteCount / 2;
			total_samples += nb_samples;
			if (nb_samples < frame_size) {
				op.e_o_s = 1;
			}
			else {
				op.e_o_s = 0;
			}

			int nbBytes = 0;

			if (nb_samples != 0) {
				uint8_t *paddedFrameBytes = framePcmBytes->Data;
				int freePaddedFrameBytes = 0;

				if (nb_samples < cur_frame_size) {
					paddedFrameBytes = (uint8_t *)malloc(cur_frame_size * 2);
					freePaddedFrameBytes = 1;
					memcpy(paddedFrameBytes, framePcmBytes->Data, frameByteCount);
					memset(paddedFrameBytes + nb_samples * 2, 0, cur_frame_size * 2 - nb_samples * 2);
				}

				nbBytes = opus_encode(_encoder, (opus_int16 *)paddedFrameBytes, cur_frame_size, _packet, max_frame_bytes / 10);
				if (freePaddedFrameBytes) {
					free(paddedFrameBytes);
					paddedFrameBytes = NULL;
				}

				if (nbBytes < 0) {
					//	LOGE("Encoding failed: %s. Aborting.", opus_strerror(nbBytes));
					return 0;
				}

				enc_granulepos += cur_frame_size * 48000 / coding_rate;
				size_segments = (nbBytes + 255) / 255;
				min_bytes = min(nbBytes, min_bytes);
			}

			while ((((size_segments <= 255) && (last_segments + size_segments > 255)) || (enc_granulepos - last_granulepos > max_ogg_delay)) && ogg_stream_flush_fill(&os, &og, 255 * 255)) {
				if (ogg_page_packets(&og) != 0) {
					last_granulepos = ogg_page_granulepos(&og);
				}

				last_segments -= og.header[26];
				int writtenPageBytes = writeOggPage(&og, _fileOs);
				if (writtenPageBytes != og.header_len + og.body_len) {
					//		LOGE("Error: failed writing data to output stream");
					return 0;
				}
				bytes_written += writtenPageBytes;
				pages_out++;
			}

			op.packet = (unsigned char *)_packet;
			op.bytes = nbBytes;
			op.b_o_s = 0;
			op.granulepos = enc_granulepos;
			if (op.e_o_s) {
				op.granulepos = ((total_samples * 48000 + rate - 1) / rate) + header.preskip;
			}
			op.packetno = 2 + _packetId;
			ogg_stream_packetin(&os, &op);
			last_segments += size_segments;

			while ((op.e_o_s || (enc_granulepos + (frame_size * 48000 / coding_rate) - last_granulepos > max_ogg_delay) || (last_segments >= 255)) ? ogg_stream_flush_fill(&os, &og, 255 * 255) : ogg_stream_pageout_fill(&os, &og, 255 * 255)) {
				if (ogg_page_packets(&og) != 0) {
					last_granulepos = ogg_page_granulepos(&og);
				}
				last_segments -= og.header[26];
				int writtenPageBytes = writeOggPage(&og, _fileOs);
				if (writtenPageBytes != og.header_len + og.body_len) {
					//			LOGE("Error: failed writing data to output stream");
					return 0;
				}
				bytes_written += writtenPageBytes;
				pages_out++;
			}

			return 1;
		}

		int OggOpusFile::initRecorder(Platform::String ^ path)
		{
			cleanupRecorder();

			if (path->IsEmpty())
				return 0;

			auto lol = _wfopen_s(&_fileOs, path->Data(), L"wb");//	_fileOs = fopen(path, "wb");
			if (!_fileOs)
			{

				return -lol;
			}

			inopt.rate = rate;
			inopt.gain = 0;
			inopt.endianness = 0;
			inopt.copy_comments = 0;
			inopt.rawmode = 1;
			inopt.ignorelength = 1;
			inopt.samplesize = 16;
			inopt.channels = 1;
			inopt.skip = 0;

			comment_init(&inopt.comments, &inopt.comments_length, opus_get_version_string());

			if (rate > 24000) {
				coding_rate = 48000;
			}
			else if (rate > 16000) {
				coding_rate = 24000;
			}
			else if (rate > 12000) {
				coding_rate = 16000;
			}
			else if (rate > 8000) {
				coding_rate = 12000;
			}
			else {
				coding_rate = 8000;
			}

			if (rate != coding_rate) {
				printf("Invalid rate");
				return -1;
			}

			header.channels = 1;
			header.channel_mapping = 0;
			header.input_sample_rate = rate;
			header.gain = inopt.gain;
			header.nb_streams = 1;

			int result = OPUS_OK;
			_encoder = opus_encoder_create(coding_rate, 1, OPUS_APPLICATION_AUDIO, &result);
			if (result != OPUS_OK) {
				printf("Error cannot create encoder: %s", opus_strerror(result));
				return result;
			}

			min_bytes = max_frame_bytes = (1275 * 3 + 7) * header.nb_streams;
			_packet = (uint8_t*)malloc(max_frame_bytes);

			result = opus_encoder_ctl(_encoder, OPUS_SET_BITRATE(bitrate));
			if (result != OPUS_OK) {
				printf("Error OPUS_SET_BITRATE returned: %s", opus_strerror(result));
				return result;
			}

#ifdef OPUS_SET_LSB_DEPTH
			result = opus_encoder_ctl(_encoder, OPUS_SET_LSB_DEPTH(max(8, min(24, inopt.samplesize))));
			if (result != OPUS_OK) {
				printf("Warning OPUS_SET_LSB_DEPTH returned: %s", opus_strerror(result));
			}
#endif

			opus_int32 lookahead;
			result = opus_encoder_ctl(_encoder, OPUS_GET_LOOKAHEAD(&lookahead));
			if (result != OPUS_OK) {
				//			LOGE("Error OPUS_GET_LOOKAHEAD returned: %s", opus_strerror(result));
				return result;
			}

			inopt.skip += lookahead;
			header.preskip = (int)(inopt.skip * (48000.0 / coding_rate));
			inopt.extraout = (int)(header.preskip * (rate / 48000.0));

			if (ogg_stream_init(&os, rand()) == -1) {
				//		LOGE("Error: stream init failed");
				return result;
			}

			unsigned char header_data[100];
			int packet_size = opus_header_to_packet(&header, header_data, 100);
			op.packet = header_data;
			op.bytes = packet_size;
			op.b_o_s = 1;
			op.e_o_s = 0;
			op.granulepos = 0;
			op.packetno = 0;
			ogg_stream_packetin(&os, &op);

			while ((result = ogg_stream_flush(&os, &og))) {
				if (!result) {
					break;
				}

				int pageBytesWritten = writeOggPage(&og, _fileOs);
				if (pageBytesWritten != og.header_len + og.body_len) {
					//			LOGE("Error: failed writing header to output stream");
					return 0;
				}
				bytes_written += pageBytesWritten;
				pages_out++;
			}

			comment_pad(&inopt.comments, &inopt.comments_length, comment_padding);
			op.packet = (unsigned char *)inopt.comments;
			op.bytes = inopt.comments_length;
			op.b_o_s = 0;
			op.e_o_s = 0;
			op.granulepos = 0;
			op.packetno = 1;
			ogg_stream_packetin(&os, &op);

			while ((result = ogg_stream_flush(&os, &og))) {
				if (result == 0) {
					break;
				}

				int writtenPageBytes = writeOggPage(&og, _fileOs);
				if (writtenPageBytes != og.header_len + og.body_len) {
					//			LOGE("Error: failed writing header to output stream");
					return 0;
				}

				bytes_written += writtenPageBytes;
				pages_out++;
			}

			free(inopt.comments);

			return 1;
		}


		void OggOpusFile::cleanupRecorder()
		{
			if (_encoder) {
				opus_encoder_destroy(_encoder);
				_encoder = 0;
			}

			ogg_stream_clear(&os);

			if (_packet) {
				free(_packet);
				_packet = 0;
			}

			if (_fileOs) {
				fclose(_fileOs);
				_fileOs = 0;
			}

			_packetId = -1;
			bytes_written = 0;
			pages_out = 0;
			total_samples = 0;
			enc_granulepos = 0;
			size_segments = 0;
			last_segments = 0;
			last_granulepos = 0;
			memset(&os, 0, sizeof(ogg_stream_state));
			memset(&inopt, 0, sizeof(oe_enc_opt));
			memset(&header, 0, sizeof(OpusHead));
			memset(&op, 0, sizeof(ogg_packet));
			memset(&og, 0, sizeof(ogg_page));
		}

		void OggOpusFile::comment_init(char **comments, int *length, const char *vendor_string)
		{
			// The 'vendor' field should be the actual encoding library used
			int vendor_length = strlen(vendor_string);
			int user_comment_list_length = 0;
			int len = 8 + 4 + vendor_length + 4;
			char *p = (char *)malloc(len);
			memcpy(p, "OpusTags", 8);
			writeint(p, 8, vendor_length);
			memcpy(p + 12, vendor_string, vendor_length);
			writeint(p, 12 + vendor_length, user_comment_list_length);
			*length = len;
			*comments = p;
		}

		int OggOpusFile::writeOggPage(ogg_page *page, FILE *os) {
			int written = fwrite(page->header, sizeof(unsigned char), page->header_len, os);
			written += fwrite(page->body, sizeof(unsigned char), page->body_len, os);
			return written;
		}

		int OggOpusFile::opus_header_to_packet(const OpusHeader *h, unsigned char *packet, int len) {
			int i;
			Packet p;
			unsigned char ch;

			p.data = packet;
			p.maxlen = len;
			p.pos = 0;
			if (len < 19) {
				return 0;
			}
			if (!write_chars(&p, (const unsigned char *)"OpusHead", 8)) {
				return 0;
			}

			ch = 1;
			if (!write_chars(&p, &ch, 1)) {
				return 0;
			}

			ch = h->channels;
			if (!write_chars(&p, &ch, 1)) {
				return 0;
			}

			if (!write_uint16(&p, h->preskip)) {
				return 0;
			}

			if (!write_uint32(&p, h->input_sample_rate)) {
				return 0;
			}

			if (!write_uint16(&p, h->gain)) {
				return 0;
			}

			ch = h->channel_mapping;
			if (!write_chars(&p, &ch, 1)) {
				return 0;
			}

			if (h->channel_mapping != 0) {
				ch = h->nb_streams;
				if (!write_chars(&p, &ch, 1)) {
					return 0;
				}

				ch = h->nb_coupled;
				if (!write_chars(&p, &ch, 1)) {
					return 0;
				}

				/* Multi-stream support */
				for (i = 0; i < h->channels; i++) {
					if (!write_chars(&p, &h->stream_map[i], 1)) {
						return 0;
					}
				}
			}

			return p.pos;
		}

		int OggOpusFile::write_chars(Packet *p, const unsigned char *str, int nb_chars)
		{
			int i;
			if (p->pos > p->maxlen - nb_chars)
				return 0;
			for (i = 0; i < nb_chars; i++)
				p->data[p->pos++] = str[i];
			return 1;
		}

		int OggOpusFile::write_uint32(Packet *p, ogg_uint32_t val) {
			if (p->pos > p->maxlen - 4) {
				return 0;
			}
			p->data[p->pos] = (val) & 0xFF;
			p->data[p->pos + 1] = (val >> 8) & 0xFF;
			p->data[p->pos + 2] = (val >> 16) & 0xFF;
			p->data[p->pos + 3] = (val >> 24) & 0xFF;
			p->pos += 4;
			return 1;
		}

		int OggOpusFile::write_uint16(Packet *p, ogg_uint16_t val) {
			if (p->pos > p->maxlen - 2) {
				return 0;
			}
			p->data[p->pos] = (val) & 0xFF;
			p->data[p->pos + 1] = (val >> 8) & 0xFF;
			p->pos += 2;
			return 1;
		}

		void OggOpusFile::comment_pad(char **comments, int* length, int amount) {
			if (amount > 0) {
				char *p = *comments;
				// Make sure there is at least amount worth of padding free, and round up to the maximum that fits in the current ogg segments
				int newlen = (*length + amount + 255) / 255 * 255 - 1;
				p = (char *)realloc(p, newlen);
				for (int i = *length; i < newlen; i++) {
					p[i] = 0;
				}
				*comments = p;
				*length = newlen;
			}
		}
		//
		//

	}

}
