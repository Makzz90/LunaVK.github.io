using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace LunaVK.Pages.Debug
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class TestCall : Page
    {
        public TestCall()
        {
            this.InitializeComponent();
            //this._tb.Text = "{\"sessionGuid\":\"933f960cc7f04862101381fdbb146e5b\",\"signaling_data\":\"{\"audio\":{\"codecs\":[\"opus-uwb\",\"opus\",\"isac\",\"speex-wb\",\"speex\",\"g729\",\"pcma\",\"pcmu\",\"g722\",\"ilbc\"]},\"candidate\":[{\"generation\":0,\"ip\":\"192.168.1.87\",\"name\":\"audio_rtp\",\"network_name\":\"0\",\"password\":\"NVReTCG4PNXtW1ws\",\"port\":\"56637\",\"priority\":1.0,\"proto\":\"udp\",\"type\":\"local\",\"username\":\"78LO3%2BhByuyyH2E%2B\"}],\"fast_connect\":2,\"jb_flags\":3,\"peerList\":[],\"timeoutSec\":60,\"useragent\":{\"caps\":3847,\"state\":7,\"ua_ver\":\"VKM_PP_build_10\",\"voip_ver\":\"voip win32 release version:0.0.0.0 date:Dec 15 2018 01:44:05\"},\"video\":{\"cap\":{\"cmpl\":-6,\"fps\":24,\"height\":720,\"width\":1280},\"codecs\":[\"h264\",\"vp8\"]},\"zrtp-hash\":\"1.10 6cc56fb4b9e0392aeb69b3998008cde38490d17d3a3bd09c17ab1d92d1721a9f\"}\n\",\"type\":\"invite\",\"user_id\":\"375988312\",\"video\":false}";
            this._tb.Text = "{sessionGuid:933f960cc7f04862101381fdbb146e5b}";
        }

        /*longpoll
{{
    "sessionGuid": "44e23bf352bb00b1e6338f97584e0be1",
    "signaling_data": "{\"audio\":{\"codecs\":[\"opus-uwb\",\"opus\",\"isac\",\"speex-wb\",\"speex\",\"g729\",\"pcma\",\"pcmu\",\"g722\",\"ilbc\"]},\"candidate\":[{\"generation\":0,\"ip\":\"192.168.1.87\",\"name\":\"audio_rtp\",\"network_name\":\"0\",\"password\":\"hSPwMI0Ku/cwJJc4\",\"port\":\"62642\",\"priority\":1.0,\"proto\":\"udp\",\"type\":\"local\",\"username\":\"NYFQH29ShDMClbMR\"}],\"fast_connect\":2,\"jb_flags\":3,\"peerList\":[],\"timeoutSec\":60,\"useragent\":{\"caps\":3847,\"state\":7,\"ua_ver\":\"VKM_PP_build_10\",\"voip_ver\":\"voip win32 release version:0.0.0.0 date:Dec 15 2018 01:44:05\"},\"video\":{\"cap\":{\"cmpl\":-6,\"fps\":24,\"height\":720,\"width\":1280},\"codecs\":[\"h264\",\"vp8\"]},\"zrtp-hash\":\"1.10 0b2276ae6771261f6f4cb88a4538ac75fa5e6338098eaf6dccc3370146726bd5\"}\n",
    "type": "invite",
    "user_id": 375988312,
    "video": false,
    "msg_hash": "5c873d1f8a557",
    "first_name": "Тест",
    "last_name": "Тестович",
    "sex": 2,
    "photo_max": "https://vk.com/images/camera_200.png?ava=1",
    "photo_max_orig": "https://vk.com/images/camera_200.png?ava=1",
    "crop_rect": false,
    "verified": false
}}
                                     */

        /*
        message=
        {
            "sessionGuid":"933f960cc7f04862101381fdbb146e5b",
            "signaling_data":"{
                \"audio\":{
                    \"codecs\":[
                        \"opus-uwb\",
                        \"opus\",
                        \"isac\",
                        \"speex-wb\",
                        \"speex\",
                        \"g729\",
                        \"pcma\",
                        \"pcmu\",
                        \"g722\",
                        \"ilbc\"
                        ]
                   },
            \"candidate\":[
                {
                    \"generation\":0,
                    \"ip\":\"192.168.1.87\",
                    \"name\":\"audio_rtp\",
                    \"network_name\":\"0\",
                    \"password\":\"NVReTCG4PNXtW1ws\",
                    \"port\":\"56637\",
                    \"priority\":1.0,
                    \"proto\":\"udp\",
                    \"type\":\"local\",
                    \"username\":\"78LO3%2BhByuyyH2E%2B\"
                    }
             ],
             \"fast_connect\":2,
             \"jb_flags\":3,
             \"peerList\":[],
             \"timeoutSec\":60,
             \"useragent\":{
                \"caps\":3847,
                \"state\":7,
                \"ua_ver\":\"VKM_PP_build_10\",
                \"voip_ver\":\"voip win32 release version:0.0.0.0 date:Dec 15 2018 01:44:05\"
             },
             \"video\":{
             \"cap\":{\"cmpl\":-6,\"fps\":24,\"height\":720,\"width\":1280},\"codecs\":[\"h264\",\"vp8\"]},\"zrtp-hash\":\"1.10 6cc56fb4b9e0392aeb69b3998008cde38490d17d3a3bd09c17ab1d92d1721a9f\"}\n","type":"invite","user_id":"375988312","video":false}
        */

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["peer_id"] = "375988312";
            parameters["random_id"] = "1461185421";
            parameters["message"] = this._tb.Text;
            //device_id
            //group_id
        //    var temp = await Core.Network.RequestsDispatcher.GetResponse<int>("messages.sendVoipEvent", parameters);
            int i = 0;
        }

        private async void Button_Click2(object sender, RoutedEventArgs e)
        {
            string str = "return API.messages.sendVoipEvent({peer_id:375988312,random_id:1461185421,message:\"" + this._tb.Text + "\"});";
            //var temp = await Core.Network.RequestsDispatcher.Execute<Core.DataObjects.VKUser>(str,(jstr)=> {
            //    return jstr;
            //});
            //int i = 0;
        }


    }
}
